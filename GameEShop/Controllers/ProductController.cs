using EShop.Application.Service;
using EShop.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EShopService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/<ProductController>
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var result = await _productService.GetAllAsync();
            return Ok(result);
        }

        // GET api/<ProductController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var result = await _productService.GetAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // POST api/<ProductController>
        [Authorize(Roles = "Employee,Admin")]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Product product)
        {
            var result = await _productService.AddAsync(product);

            return Ok(result);
        }

        // PUT api/<ProductController>/5
        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] Product product)
        {
            var result = await _productService.UpdateAsync(product);

            return Ok(result);
        }

        // DELETE api/<ProductController>/5
        [Authorize(Roles = "Employee,Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var product = await _productService.GetAsync(id);
            product.Deleted = true;
            var result = await _productService.UpdateAsync(product);

            return Ok(result);
        }

        // PATCH api/<ProductController>/6
        [Authorize(Roles = "Employee,Admin")]
        [HttpPatch]
        public ActionResult Add([FromBody] Product product)
        {
            var result = _productService.Add(product);

            return Ok(result);
        }
        [HttpGet("get-session")]
        public IActionResult GetSession()
        {
            var username = HttpContext.Session.GetString("Username");
            var email = HttpContext.Session.GetString("Email");
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(token))
            {
                return Unauthorized("No active session");
            }

            return Ok(new { Username = username, Token = token, Email = email });
        }

    }
}