using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using User.Application.Producer;
using User.Domain.Repositories;
using User.Domain.Utils;

namespace User.Application.Services;

public class PasswordService : IPasswordService
{
    private readonly UserDbContext _dbContext;
    private readonly IKafkaProducer _kafkaProducer;

    public PasswordService(UserDbContext dbContext, IKafkaProducer kafkaProducer)
    {
        _dbContext = dbContext;
        _kafkaProducer = kafkaProducer;
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return;
        }

        var newPassword = PasswordGenerator.GenerateRandomPassword();

        user.PasswordHash = PasswordHasher.Hash(newPassword);
        await _dbContext.SaveChangesAsync();

        var messagePayload = new { user.Email, NewPassword = newPassword };
        var messageJson = JsonSerializer.Serialize(messagePayload);

        await _kafkaProducer.SendMessageAsync("password-reset-topic", messageJson);
    }
}