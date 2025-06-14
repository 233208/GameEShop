namespace User.Application.Services;

public interface IPasswordService
{
    Task RequestPasswordResetAsync(string email);
}