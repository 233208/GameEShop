using Microsoft.EntityFrameworkCore;
using User.Domain.Exceptions.Login;
using User.Domain.Repositories;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using User.Application.Settings;
using System.Security.Cryptography;
using User.Domain.Models.Entities;
using System.Collections.Concurrent;
using User.Domain.Utils;
using User.Application.Producer;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

// Usunięto zbędne usingi dla KafkaMessage i JsonSerializer
// using NotificationService;
// using System.Text.Json; 

public class LoginService : ILoginService
{
    private readonly UserDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;
    private readonly IKafkaProducer _kafkaProducer;

    protected ConcurrentQueue<int> _userLoggedIdsQueue;

    public LoginService(UserDbContext dbContext, IOptions<JwtSettings> jwtSettings, IKafkaProducer kafkaProducer)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
        _userLoggedIdsQueue = new ConcurrentQueue<int>();
        _kafkaProducer = kafkaProducer;
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        var user = await _dbContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            throw new InvalidCredentialsException();

        _userLoggedIdsQueue.Enqueue(user.Id);


        var messageKey = user.Email;
        var messageValue = $"Witaj {user.Username}, zalogowales sie do GameEShop!";

        await _kafkaProducer.SendMessageAsync("after-login-email-topic", $"{messageKey}:{messageValue}");



        return GenerateToken(user);
    }

    private string GenerateToken(User.Domain.Models.Entities.User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        if (user.Roles != null)
        {
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }
        }

        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText("./data/private.key"));
        var creds = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
