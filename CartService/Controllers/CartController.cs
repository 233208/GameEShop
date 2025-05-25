using CartService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;

namespace CartService.Controllers;

public class ProductDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

[Authorize]
[ApiController]
[Route("[controller]")]
public class CartController : ControllerBase
{
    private readonly IDistributedCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public CartController(IDistributedCache cache, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    private string GetCartKey(string userId) => $"cart:{userId}";

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest item)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        // Sprawdź czy produkt istnieje w katalogu
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("http://gameeshop_service:8080/api/product/" + item.ProductId);
        if (!response.IsSuccessStatusCode)
            return NotFound("Produkt nie istnieje.");

        var json = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var key = GetCartKey(userId);
        var cart = await GetCart(userId);
        var existing = cart.FirstOrDefault(x => x.ProductId == item.ProductId);
        if (existing != null)
            existing.Quantity += item.Quantity;
        else
            cart.Add(new CartItemDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Name = product?.Name ?? "(brak tytułu)"
            });

        await _cache.SetStringAsync(key, JsonSerializer.Serialize(cart));
        return Ok(cart);
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var cart = await GetCart(userId);

        var client = _httpClientFactory.CreateClient();
        var result = new List<CartItemDto>();

        foreach (var item in cart)
        {
            string? title = null;
            var response = await client.GetAsync("http://gameeshop_service:8080/api/product/" + item.ProductId);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                title = product?.Name;
            }

            result.Add(new CartItemDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Name = title ?? "(brak tytułu)"
            });
        }

        return Ok(result);
    }

    [HttpDelete("remove/{productId}")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var key = GetCartKey(userId);
        var cart = await GetCart(userId);
        cart.RemoveAll(x => x.ProductId == productId);
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(cart));
        return Ok(cart);
    }

    private async Task<List<CartItemDto>> GetCart(string userId)
    {
        var key = GetCartKey(userId);
        var json = await _cache.GetStringAsync(key);
        return json != null
            ? JsonSerializer.Deserialize<List<CartItemDto>>(json) ?? new List<CartItemDto>()
            : new List<CartItemDto>();
    }
}
