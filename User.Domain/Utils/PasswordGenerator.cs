using System.Security.Cryptography;

namespace User.Domain.Utils;

public static class PasswordGenerator
{
    private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    private const int DefaultLength = 12;

    public static string GenerateRandomPassword(int length = DefaultLength)
    {
        if (length <= 0)
            throw new ArgumentException("Password length must be positive.", nameof(length));

        var randomBytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var passwordChars = new char[length];
        for (int i = 0; i < length; i++)
        {
            passwordChars[i] = AllowedChars[randomBytes[i] % AllowedChars.Length];
        }

        return new string(passwordChars);
    }
}