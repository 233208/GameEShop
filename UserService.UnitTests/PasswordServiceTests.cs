using Microsoft.EntityFrameworkCore;
using Moq;
using User.Application.Producer;
using User.Application.Services;
using User.Domain.Models.Entities;
using User.Domain.Repositories;
using User.Domain.Utils;
using Xunit;
using System;
using System.Threading.Tasks;

namespace UserService.UnitTests;

public class PasswordServiceTests : IDisposable
{
    private readonly UserDbContext _dbContext; 
    private readonly Mock<IKafkaProducer> _mockKafkaProducer;
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {

        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;

        _dbContext = new UserDbContext(options);

        SeedDatabase();

        _mockKafkaProducer = new Mock<IKafkaProducer>();

        _passwordService = new PasswordService(_dbContext, _mockKafkaProducer.Object);
    }

    private void SeedDatabase()
    {
        var user = new User.Domain.Models.Entities.User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = PasswordHasher.Hash("oldPassword")
        };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges(); 
    }

    [Fact]
    public async Task RequestPasswordResetAsync_ShouldUpdatePasswordAndSendToKafka_WhenEmailExists()
    {
        var userEmail = "test@example.com";

        await _passwordService.RequestPasswordResetAsync(userEmail);

        var userAfter = await _dbContext.Users.FirstAsync(u => u.Email == userEmail);
        bool isNewPasswordCorrect = PasswordHasher.Verify("oldPassword", userAfter.PasswordHash);
        Assert.False(isNewPasswordCorrect);

        _mockKafkaProducer.Verify(p => p.SendMessageAsync("password-reset-topic", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_ShouldDoNothing_WhenEmailDoesNotExist()
    {
        var userEmail = "nonexistent@example.com";

        await _passwordService.RequestPasswordResetAsync(userEmail);

        _mockKafkaProducer.Verify(p => p.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}