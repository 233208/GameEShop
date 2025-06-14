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

    public async Task<bool> UpdateUserEmailAsync(int userId, string newEmail)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return false;

        if (await _dbContext.Users.AnyAsync(u => u.Email == newEmail))
        {
            throw new InvalidOperationException("Email is already taken.");
        }

        user.Email = newEmail;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserPasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return false;

        if (!PasswordHasher.Verify(oldPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Incorrect old password.");
        }

        user.PasswordHash = PasswordHasher.Hash(newPassword);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<User.Domain.Models.Entities.User?> UpdateUserStatusAsync(int userId, bool isActive) // Zmieniono `bool?` na `bool`
    {
        var user = await _dbContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        // Logika została uproszczona, ponieważ `isActive` zawsze będzie miało wartość
        user.IsActive = isActive;

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