using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.Application.Services;
using User.Domain.Exceptions.Login;
using User.Domain.Models.Requests;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        protected ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }


        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = _loginService.Login(request.Username, request.Password);
                return Ok(new { token });
            }
            catch (InvalidCredentialsException)
            {
                return Unauthorized();
            }
        }

        [HttpGet]
        [Authorize]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AdminPage()
        {
            return Ok();
        }
        [HttpGet("set-session")]
        public IActionResult SetSession()
        {
            HttpContext.Session.SetString("UserName", "JohnDoe");
            return Ok("Session set");
        }

        [HttpGet("get-session")]
        public IActionResult GetSession()
        {
            var userName = HttpContext.Session.GetString("UserName");
            return Ok(userName ?? "No session value");
        }
    }

}