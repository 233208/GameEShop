using CartDomain.Models;


namespace CartService.Domain.Repositories;

public interface ICartRepository
{
    Task<Cart> GetCartAsync(string userId);
    Task<Cart> UpdateCartAsync(Cart cart);
    Task ClearCartAsync(string userId);
}