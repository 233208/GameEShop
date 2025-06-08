namespace User.Application.Services;

public interface IJwtTokenService
{
    string GenerateToken(int userId, List<string> roles);
}