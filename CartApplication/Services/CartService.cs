using CartApplication.DTO;
using CartService.Domain.Repositories;
using System.Text.Json;
using CartApplication.Producer;


namespace CartService.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IKafkaProducer _kafkaProducer;

    public CartService(ICartRepository cartRepository, IHttpClientFactory httpClientFactory, IKafkaProducer kafkaProducer)
    {
        _cartRepository = cartRepository;
        _httpClientFactory = httpClientFactory;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<IEnumerable<CartItemDTO>> AddItemToCartAsync(string userId, AddToCartRequest item)
    {
        // 1. Pobierz dane produktu, w tym stan magazynowy
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

        // 2. Pobierz aktualny koszyk
        var cart = await _cartRepository.GetCartAsync(userId);

        // 3. Sprawdź, ile tego produktu jest już w koszyku
        var currentQuantityInCart = cart.Items
            .FirstOrDefault(i => i.ProductId == item.ProductId)?.Quantity ?? 0;

        // 4. NOWA LOGIKA: Sprawdź, czy można dodać produkt
        if ((currentQuantityInCart + item.Quantity) > product.Stock)
        {
            // Rzuć wyjątek, jeśli stan magazynowy jest niewystarczający
            throw new InvalidOperationException($"Not enough items in stock for product '{product.Name}'. Available: {product.Stock}, In cart: {currentQuantityInCart}, Requested: {item.Quantity}.");
        }

        // 5. Jeśli walidacja przeszła pomyślnie, dodaj produkt
        cart.AddItem(item.ProductId, item.Quantity, product.Name);
        await _cartRepository.UpdateCartAsync(cart);

        return cart.Items.Select(i => new CartItemDTO { ProductId = i.ProductId, Name = i.Name, Quantity = i.Quantity });
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

        // Przygotuj wiadomość do Kafki
        var messagePayload = new
        {
            Email = userEmail,
            Items = cart.Items.Select(i => new { i.Name, i.Quantity }).ToList()
        };
        var message = JsonSerializer.Serialize(messagePayload);

        // Wyślij wiadomość i wyczyść koszyk
        await _kafkaProducer.SendMessageAsync("cart-finalization-topic", message);
        await _cartRepository.ClearCartAsync(userId);
    }
}