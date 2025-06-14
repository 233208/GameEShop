namespace User.Application.Services;

public interface IUserModifyService
{
    Task<User.Domain.Models.Entities.User?> UpdateUserStatusAsync(int userId, bool isActive);
    Task<bool> UpdateUserEmailAsync(int userId, string newEmail);
    Task<bool> UpdateUserPasswordAsync(int userId, string oldPassword, string newPassword);
    Task<bool> DeleteUserAsync(int userId);
}