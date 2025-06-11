using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using User.Application.Services;
using User.Domain.Models.Entities;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserModifyController : ControllerBase
    {
        private readonly IUserModifyService _userModifyService;

        public UserModifyController(IUserModifyService userModifyService)
        {
            _userModifyService = userModifyService;
        }

        // PUT: api/UserModify/self
        // Only the authenticated user can update their own email/password
        [HttpPut("self")]
        public async Task<ActionResult<User.Domain.Models.Entities.User>> UpdateOwnAccount([FromBody] UpdateOwnAccountRequest request)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var user = await _userModifyService.UpdateUserAsync(currentUserId, request.Email, request.Password, null);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        // PUT: api/UserModify/{id}/status
        // Only Admin or Employee can update IsActive
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<User.Domain.Models.Entities.User>> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
        {
            var user = await _userModifyService.UpdateUserAsync(id, null, null, request.IsActive);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        // DELETE: api/UserModify/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userModifyService.DeleteUserAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }

    // DTO for user updating their own account
    public class UpdateOwnAccountRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    // DTO for admin/employee updating IsActive
    public class UpdateUserStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
