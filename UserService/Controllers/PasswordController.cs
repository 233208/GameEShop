using Microsoft.AspNetCore.Mvc;
using User.Application.Services;

namespace UserService.Controllers;

public class PasswordResetRequest
{
    public string Email { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class PasswordController : ControllerBase
{
    private readonly IPasswordService _passwordService;

    public PasswordController(IPasswordService passwordService)
    {
        _passwordService = passwordService;
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return BadRequest("Email is required.");
        }

        await _passwordService.RequestPasswordResetAsync(request.Email);

        // Zawsze zwracaj tę samą, ogólną odpowiedź, aby chronić prywatność użytkowników.
        return Ok(new { message = "If an account with this email exists, password reset instructions have been sent." });
    }
}