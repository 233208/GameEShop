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

// Implementujemy IDisposable, aby poprawnie czyścić bazę po testach
public class PasswordServiceTests : IDisposable
{
    private readonly UserDbContext _dbContext; // To już nie jest Mock, a prawdziwa instancja DbContext
    private readonly Mock<IKafkaProducer> _mockKafkaProducer;
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        // 1. Skonfiguruj opcje dla bazy danych w pamięci
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Używamy unikalnej nazwy, aby testy były odizolowane
            .Options;

        // 2. Stwórz prawdziwą instancję DbContext, która będzie działać w pamięci
        _dbContext = new UserDbContext(options);

        // 3. Wypełnij bazę w pamięci danymi startowymi
        SeedDatabase();

        // 4. Skonfiguruj pozostałe mocki (KafkaProducer bez zmian)
        _mockKafkaProducer = new Mock<IKafkaProducer>();

        // 5. Zainicjuj testowany serwis, przekazując mu prawdziwy (ale działający w pamięci) DbContext
        _passwordService = new PasswordService(_dbContext, _mockKafkaProducer.Object);
    }

    private void SeedDatabase()
    {
        // Ta metoda przygotowuje stan naszej bazy w pamięci przed każdym testem
        var user = new User.Domain.Models.Entities.User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = PasswordHasher.Hash("oldPassword")
        };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges(); // W przypadku bazy In-Memory, synchroniczny zapis jest w porządku
    }

    [Fact]
    public async Task RequestPasswordResetAsync_ShouldUpdatePasswordAndSendToKafka_WhenEmailExists()
    {
        // Arrange
        var userEmail = "test@example.com";

        // Act
        await _passwordService.RequestPasswordResetAsync(userEmail);

        // Assert
        // Sprawdzamy, czy hasło w naszej bazie w pamięci faktycznie się zmieniło
        var userAfter = await _dbContext.Users.FirstAsync(u => u.Email == userEmail);
        bool isNewPasswordCorrect = PasswordHasher.Verify("oldPassword", userAfter.PasswordHash);
        Assert.False(isNewPasswordCorrect); // Hasło nie powinno już pasować do starego

        // Weryfikacja mocka Kafki pozostaje bez zmian
        _mockKafkaProducer.Verify(p => p.SendMessageAsync("password-reset-topic", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_ShouldDoNothing_WhenEmailDoesNotExist()
    {
        // Arrange
        var userEmail = "nonexistent@example.com";

        // Act
        await _passwordService.RequestPasswordResetAsync(userEmail);

        // Assert
        // Weryfikacja mocka Kafki pozostaje bez zmian
        _mockKafkaProducer.Verify(p => p.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // Ta metoda czyści zasoby po każdym teście
    public void Dispose()
    {
        _dbContext.Dispose();
    }
}