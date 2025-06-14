using CartApplication.DTO;

namespace CartService.Application.Services;

public interface ICartService
{
    Task<IEnumerable<CartItemDTO>> GetCartAsync(string userId);
    Task<IEnumerable<CartItemDTO>> AddItemToCartAsync(string userId, AddToCartRequest item);
    Task<IEnumerable<CartItemDTO>> RemoveItemFromCartAsync(string userId, int productId);
    Task FinalizeCartAsync(string userId, string userEmail);
}