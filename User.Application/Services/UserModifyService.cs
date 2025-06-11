using Microsoft.EntityFrameworkCore;
using User.Domain.Repositories;
using User.Domain.Utils;

namespace User.Application.Services;

public class UserModifyService : IUserModifyService
{
    private readonly UserDbContext _dbContext;

    public UserModifyService(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User.Domain.Models.Entities.User?> UpdateUserAsync(
        int userId,
        string? email = null,
        string? password = null,
        bool? isActive = null)
    {
        var user = await _dbContext.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        if (!string.IsNullOrEmpty(email))
            user.Email = email;
        if (!string.IsNullOrEmpty(password))
            user.PasswordHash = PasswordHasher.Hash(password);
        if (isActive.HasValue)
            user.IsActive = isActive.Value;

        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return false;

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
