namespace User.Application.Services;

public interface IUserModifyService
{
    Task<User.Domain.Models.Entities.User?> UpdateUserAsync(
        int userId,
        string? email = null,
        string? password = null,
        bool? isActive = null
    );
    Task<bool> DeleteUserAsync(int userId);
}
