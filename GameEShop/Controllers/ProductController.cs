using EShop.Application.Service;
using EShop.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EShopService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<List<Product>>> Get()
        {
            var result = await _productService.GetAllAsync();
            return Ok(result);
        }

        // GET api/Product/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> Get(int id)
        {
            var result = await _productService.GetAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        // POST api/Product
        [Authorize(Roles = "Employee,Admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> Post([FromBody] Product product)
        {
            var result = await _productService.AddAsync(product);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result); // Lepsza praktyka REST - zwraca 201 Created
        }

        // PUT api/Product/5
        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> Put(int id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("Product ID in URL must match Product ID in body.");
            }
            var result = await _productService.UpdateAsync(product);
            return Ok(result);
        }

        /// <summary>
        /// DTO for updating product status
        /// </summary>
        public class UpdateProductStatusRequest
        {
            [Required]
            public bool IsDeleted { get; set; }
        }

        // PATCH api/Product/5/status
        [Authorize(Roles = "Employee,Admin")]
        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateProductStatus(int id, [FromBody] UpdateProductStatusRequest request)
        {
            var product = await _productService.GetAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            product.Deleted = request.IsDeleted;
            var result = await _productService.UpdateAsync(product);
            return Ok(result);
        }

    }
}