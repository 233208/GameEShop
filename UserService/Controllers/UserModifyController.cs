using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using User.Application.Services;
using System.ComponentModel.DataAnnotations; // Potrzebne dla DTO

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

        // NOWY ENDPOINT: Zmiana e-maila
        [HttpPut("self/email")]
        public async Task<IActionResult> UpdateOwnEmail([FromBody] UpdateEmailRequest request)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            try
            {
                var success = await _userModifyService.UpdateUserEmailAsync(currentUserId, request.NewEmail);
                if (!success) return NotFound("User not found.");
                return Ok(new { message = "Email updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message }); // 409 Conflict, np. gdy e-mail zajęty
            }
        }

        // NOWY ENDPOINT: Zmiana hasła
        [HttpPut("self/password")]
        public async Task<IActionResult> UpdateOwnPassword([FromBody] UpdatePasswordRequest request)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            try
            {
                var success = await _userModifyService.UpdateUserPasswordAsync(currentUserId, request.OldPassword, request.NewPassword);
                if (!success) return NotFound("User not found.");
                return Ok(new { message = "Password updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message }); // 400 Bad Request, np. gdy stare hasło jest błędne
            }
        }

        // Ten endpoint pozostaje bez zmian
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<User.Domain.Models.Entities.User>> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
        {
            var user = await _userModifyService.UpdateUserStatusAsync(id, request.IsActive);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        // Ten endpoint pozostaje bez zmian
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userModifyService.DeleteUserAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    

    // Definicje DTO, które dodaliśmy w kroku 1

    public class UpdateEmailRequest
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }

    public class UpdatePasswordRequest
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters long.")]
        public string NewPassword { get; set; }
    }

    public class UpdateUserStatusRequest
    {
        public bool IsActive { get; set; }
    }
}

// DTO for user updating their own email
public class UpdateEmailRequest
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }

    // DTO for user updating their own password
    public class UpdatePasswordRequest
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(5, ErrorMessage = "New password must be at least 5 characters long.")]
        public string NewPassword { get; set; }
    }

    // Ten DTO pozostaje bez zmian
    public class UpdateUserStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
