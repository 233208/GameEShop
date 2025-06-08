using Microsoft.AspNetCore.Mvc;
using User.Application.Services;
using User.Domain.Exceptions.Login;
using User.Domain.Models.Requests;

namespace UserService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly ILoginService _loginService;

    public LoginController(ILoginService loginService)
    {
        _loginService = loginService;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _loginService.LoginAsync(request.Username, request.Password);

            HttpContext.Session.SetString("Username", request.Username);
            HttpContext.Session.SetString("Token", token);

            return Ok(new { token });
        }
        catch (InvalidCredentialsException)
        {
            return Unauthorized();
        }
    }

    [HttpGet("get-session")]
    public IActionResult GetSession()
    {
        var username = HttpContext.Session.GetString("Username");
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(token))
        {
            return Unauthorized("No active session");
        }

        return Ok(new { Username = username, Token = token });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var token = HttpContext.Session.GetString("Token");
        if (!string.IsNullOrEmpty(token))
        {
            // Blacklist the token in Redis
            HttpContext.Session.Remove("Token");
            HttpContext.Session.Remove("Username");
        }

        return Ok("Logged out successfully");
    }
}