using CartApplication.DTO;
using CartService.Domain.Repositories;
using System.Text.Json;
using CartApplication.Producer;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http; 


namespace CartService.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(ICartRepository cartRepository, IHttpClientFactory httpClientFactory, IKafkaProducer kafkaProducer, IHttpContextAccessor httpContextAccessor) 
    {
        _cartRepository = cartRepository;
        _httpClientFactory = httpClientFactory;
        _kafkaProducer = kafkaProducer;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<CartItemDTO>> AddItemToCartAsync(string userId, AddToCartRequest item)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync($"http://gameeshop:8080/api/product/{item.ProductId}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Product not found.");
        }

        var json = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (product == null)
        {
            throw new Exception("Could not retrieve product information.");
        }

        if (product.Deleted)
        {
            throw new InvalidOperationException("This product is no longer available.");
        }

        var cart = await _cartRepository.GetCartAsync(userId);

        var currentQuantityInCart = cart.Items
            .FirstOrDefault(i => i.ProductId == item.ProductId)?.Quantity ?? 0;

        if ((currentQuantityInCart + item.Quantity) > product.Stock)
        {
            throw new InvalidOperationException($"Not enough items in stock for product '{product.Name}'. Available: {product.Stock}, In cart: {currentQuantityInCart}, Requested: {item.Quantity}.");
        }

        cart.AddItem(item.ProductId, item.Quantity, product.Name);

        var updatedCart = await _cartRepository.UpdateCartAsync(cart);

        return updatedCart.Items.Select(i => new CartItemDTO { ProductId = i.ProductId, Name = i.Name, Quantity = i.Quantity });
    }
    public async Task<IEnumerable<CartItemDTO>> GetCartAsync(string userId)
    {
        var cart = await _cartRepository.GetCartAsync(userId);
        return cart.Items.Select(i => new CartItemDTO { ProductId = i.ProductId, Name = i.Name, Quantity = i.Quantity });
    }

    public async Task<IEnumerable<CartItemDTO>> RemoveItemFromCartAsync(string userId, int productId)
    {
        var cart = await _cartRepository.GetCartAsync(userId);
        cart.RemoveItem(productId);
        await _cartRepository.UpdateCartAsync(cart);

        return cart.Items.Select(i => new CartItemDTO { ProductId = i.ProductId, Name = i.Name, Quantity = i.Quantity });
    }
    public async Task FinalizeCartAsync(string userId, string userEmail)
    {
        var cart = await _cartRepository.GetCartAsync(userId);
        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException("Cart is empty.");
        }

        var client = _httpClientFactory.CreateClient();

        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("Authorization token not found in the request.");
        }
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);


        foreach (var item in cart.Items)
        {
            var productResponse = await client.GetAsync($"http://gameeshop:8080/api/product/{item.ProductId}");
            if (!productResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Product with ID {item.ProductId} not found. Cannot finalize order.");
            }

            var productJson = await productResponse.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductDetailDTO>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (product == null)
            {
                throw new Exception($"Failed to deserialize product with ID {item.ProductId}.");
            }

            if (product.Stock < item.Quantity)
            {
                throw new InvalidOperationException($"Not enough stock for product '{product.Name}'. Order cannot be finalized.");
            }
            product.Stock -= item.Quantity;


            var updateResponse = await client.PutAsJsonAsync($"http://gameeshop:8080/api/product/{item.ProductId}", product);
            updateResponse.EnsureSuccessStatusCode(); 
        }

        var messagePayload = new
        {
            Email = userEmail,
            Items = cart.Items.Select(i => new { i.Name, i.Quantity }).ToList()
        };
        var message = JsonSerializer.Serialize(messagePayload);


        await _kafkaProducer.SendMessageAsync("cart-finalization-topic", message);
        await _cartRepository.ClearCartAsync(userId);
    }
}