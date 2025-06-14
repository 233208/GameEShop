using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using User.Application.Services;
using User.Domain.Models.Requests;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegisterController : ControllerBase
{
    private readonly IRegisterService _registerService;
    private readonly ILogger<RegisterController> _logger; 

    public RegisterController(IRegisterService registerService, ILogger<RegisterController> logger) 
    {
        _registerService = registerService;
        _logger = logger; 
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] User.Domain.Models.Requests.RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _registerService.RegisterAsync(request.Username, request.Email, request.Password);
            return Ok(new { message = "Registration successful." });
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "An error occurred during registration. Full exception: {Exception}", ex.ToString());

            return StatusCode(500, new { error = "An error occurred during registration. Check logs for details." });
        }
    }
}