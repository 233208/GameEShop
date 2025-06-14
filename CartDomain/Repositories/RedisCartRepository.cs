using CartDomain.Models;
using CartService.Domain.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CartDomain.Repositories;

public class RedisCartRepository : ICartRepository
{
    private readonly IDistributedCache _cache;
    private string GetCartKey(string userId) => $"cart:{userId}";

    public RedisCartRepository(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<Cart> GetCartAsync(string userId)
    {
        var json = await _cache.GetStringAsync(GetCartKey(userId));
        if (string.IsNullOrEmpty(json))
        {
            return new Cart(userId); 
        }
        var cart = JsonSerializer.Deserialize<Cart>(json);
        return cart ?? new Cart(userId);
    }

    public async Task<Cart> UpdateCartAsync(Cart cart)
    {
        await _cache.SetStringAsync(GetCartKey(cart.UserId), JsonSerializer.Serialize(cart));
        return await GetCartAsync(cart.UserId);
    }

    public async Task ClearCartAsync(string userId)
    {
        await _cache.RemoveAsync(GetCartKey(userId));
    }
}