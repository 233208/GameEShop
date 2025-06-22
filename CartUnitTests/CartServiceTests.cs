using Moq;
using Xunit;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartApplication.DTO;
using CartService.Domain.Repositories;
using CartDomain.Models;
using CartApplication.Producer;
using CartService.Application.Services;
using System.Net.Http;
using Moq.Protected;
using System.Threading;
using System;
using System.Linq;

namespace CartService.UnitTests;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _mockCartRepository;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IKafkaProducer> _mockKafkaProducer;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly ICartService _cartService;

    public CartServiceTests()
    {
        _mockCartRepository = new Mock<ICartRepository>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockKafkaProducer = new Mock<IKafkaProducer>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        _cartService = new CartService.Application.Services.CartService(
            _mockCartRepository.Object,
            _mockHttpClientFactory.Object,
            _mockKafkaProducer.Object,
            _mockHttpContextAccessor.Object
        );
    }

    // Pomocnicza metoda do mockowania odpowiedzi z HttpClient
    private Mock<HttpMessageHandler> SetupHttpClientMock(HttpResponseMessage response)
    {
        var mockMessageHandler = new Mock<HttpMessageHandler>();

        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var client = new HttpClient(mockMessageHandler.Object);
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

        return mockMessageHandler;
    }

    [Fact]
    public async Task AddItemToCartAsync_ShouldAddItem_WhenProductIsInStock()
    {
        // Arrange
        var userId = "testUser";
        var request = new AddToCartRequest { ProductId = 1, Quantity = 1 };
        var productDto = new ProductDto { Id = 1, Name = "Test Game", Stock = 10, Deleted = false };
        var emptyCart = new Cart(userId);

        _mockCartRepository.Setup(r => r.GetCartAsync(userId)).ReturnsAsync(emptyCart);
        _mockCartRepository.Setup(r => r.UpdateCartAsync(It.IsAny<Cart>()))
                           .Returns(Task.FromResult(new Cart(userId) { Items = { new CartItem { ProductId = 1, Quantity = 1, Name = "Test Game" } } }));


        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(productDto))
        };
        SetupHttpClientMock(response);

        // Act
        var result = await _cartService.AddItemToCartAsync(userId, request);

        // Assert
        _mockCartRepository.Verify(r => r.UpdateCartAsync(It.Is<Cart>(c =>
            c.UserId == userId &&
            c.Items.Count == 1 &&
            c.Items[0].ProductId == request.ProductId &&
            c.Items[0].Quantity == request.Quantity
        )), Times.Once);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Game", result.First().Name);
    }

    [Fact]
    public async Task AddItemToCartAsync_ShouldThrowException_WhenProductIsOutOfStock()
    {
        // Arrange
        var userId = "testUser";
        var request = new AddToCartRequest { ProductId = 1, Quantity = 5 };
        var productDto = new ProductDto { Id = 1, Name = "Test Game", Stock = 4, Deleted = false };

        _mockCartRepository.Setup(r => r.GetCartAsync(userId)).ReturnsAsync(new Cart(userId));
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(productDto))
        };
        SetupHttpClientMock(response);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _cartService.AddItemToCartAsync(userId, request));
        Assert.Contains("Not enough items in stock", exception.Message);
    }

    [Fact]
    public async Task FinalizeCartAsync_ShouldClearCartAndSendMessage_WhenSuccessful()
    {
        // Arrange
        var userId = "testUser";
        var userEmail = "test@example.com";
        var cart = new Cart(userId);
        cart.AddItem(1, 1, "Test Game");

        var productDetailDto = new ProductDetailDTO { Id = 1, Name = "Test Game", Stock = 10 };

        _mockCartRepository.Setup(r => r.GetCartAsync(userId)).ReturnsAsync(cart);

        var mockHttpContext = new DefaultHttpContext();
        mockHttpContext.Request.Headers["Authorization"] = "Bearer fake-token";
        _mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(mockHttpContext);

        var mockMessageHandler = new Mock<HttpMessageHandler>();

        // Konfiguracja dla zapytania GET (pobranie produktu)
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(productDetailDto))
            });

        // Konfiguracja dla zapytania PUT (aktualizacja produktu)
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var client = new HttpClient(mockMessageHandler.Object);
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

        // Act
        await _cartService.FinalizeCartAsync(userId, userEmail);

        // Assert
        _mockKafkaProducer.Verify(p => p.SendMessageAsync("cart-finalization-topic", It.IsAny<string>()), Times.Once);
        _mockCartRepository.Verify(r => r.ClearCartAsync(userId), Times.Once);
    }

    [Fact]
    public async Task FinalizeCartAsync_ShouldThrowException_WhenCartIsEmpty()
    {
        // Arrange
        var userId = "testUser";
        var userEmail = "test@example.com";
        var emptyCart = new Cart(userId);

        _mockCartRepository.Setup(r => r.GetCartAsync(userId)).ReturnsAsync(emptyCart);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _cartService.FinalizeCartAsync(userId, userEmail));
        Assert.Equal("Cart is empty.", exception.Message);
    }
}