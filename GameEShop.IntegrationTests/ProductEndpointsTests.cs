using EShop.Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace EShop.IntegrationTests;

public class ProductEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnOkStatus_AndListOfProducts()
    {
        var response = await _client.GetAsync("/api/Product");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
        Assert.True(products.Count >= 3); 
        Assert.Contains(products, p => p.Name == "The Witcher 3: Wild Hunt");
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProduct_WhenIdExists()
    {
        var existingProductId = 1; 

        var response = await _client.GetAsync($"/api/Product/{existingProductId}");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);
        Assert.Equal(existingProductId, product.Id);
        Assert.Equal("The Witcher 3: Wild Hunt", product.Name);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        var nonExistentProductId = 999;

        var response = await _client.GetAsync($"/api/Product/{nonExistentProductId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}