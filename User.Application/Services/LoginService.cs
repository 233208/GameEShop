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
public class LoginService : ILoginService
{
    private readonly UserDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public LoginService(UserDbContext dbContext, IOptions<JwtSettings> jwtSettings)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        var user = await _dbContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);

        if (user == null)
            throw new InvalidCredentialsException();

        return GenerateToken(user);
    }

    private string GenerateToken(Users user)
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

        // Load RSA private key from PEM file
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
