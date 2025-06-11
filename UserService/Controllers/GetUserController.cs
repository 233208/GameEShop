using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using User.Application.Services;
using User.Domain.Models.Response;

namespace UserService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GetUserController : ControllerBase
{
    private readonly IGetUserService _userService;

    public GetUserController(IGetUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("get-user-data")]
    public async Task<ActionResult<UserResponseDTO>> GetUserData()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var userDto = await _userService.GetUserAsync(userId);
            return Ok(userDto);
        }
        catch
        {
            return NotFound();
        }

    }
}
