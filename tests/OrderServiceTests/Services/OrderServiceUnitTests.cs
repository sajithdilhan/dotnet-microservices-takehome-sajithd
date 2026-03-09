using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Application.Dto;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Idempotency;
using Shared.Contracts.Events;
using ServiceType = OrderService.Application.Services.OrderService;

namespace OrderServiceTests.Services;

public class OrderServiceUnitTests
{
    [Fact]
    public async Task GetAllOrders_ReturnsSuccess_WhenRepositoryHasOrders()
    {
        // Arrange
        var order = new Order(10, "test@test.com") { OrderId = 1 };
        var orders = new List<Order> { order };

        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.GetAllOrdersAsync()).ReturnsAsync(orders);

        var mockIdempotency = new Mock<IIdempotencyRepository>();
        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockIdempotency.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.GetAllOrdersAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var list = result.Value!.ToList();
        Assert.Single(list);
        Assert.Equal(1, list[0].OrderId);
        Assert.Equal(order.Amount, list[0].Amount);
        Assert.Equal(order.CustomerEmail, list[0].CustomerEmail);
    }

    [Fact]
    public async Task GetAllOrders_ReturnsFailure_WhenRepositoryReturnsNull()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.GetAllOrdersAsync()).ReturnsAsync(Enumerable.Empty<Order>());

        var mockIdempotency = new Mock<IIdempotencyRepository>();
        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockIdempotency.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.GetAllOrdersAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error!.Code);
        Assert.Equal("No orders found!", result.Error.Message);
    }

    [Fact]
    public async Task CreateOrder_ReturnsFailure_WhenRepositoryReturnsOrderWithIdZero()
    {
        // Arrange
        var request = new OrderRequest { CustomerEmail = "test@test.com", Amount = 20 };
        var returnedOrder = new Order(request.Amount, request.CustomerEmail) { OrderId = 0 };

        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.CreateOrderAsync(It.IsAny<Order>())).ReturnsAsync(returnedOrder);

        var mockIdempotency = new Mock<IIdempotencyRepository>();
        mockIdempotency.Setup(r => r.GetIdempotencyKeyAsync(It.IsAny<string>())).ReturnsAsync((IdempotencyKey?)null);

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockIdempotency.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.CreateOrderAsync(request, "key-1");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(500, result.Error!.Code);
        Assert.Equal("Failed to create order.", result.Error.Message);
    }

    [Fact]
    public async Task CreateOrder_PublishesEventAndReturnsSuccess_WhenRepositoryCreatesOrder()
    {
        // Arrange
        var request = new OrderRequest { CustomerEmail = "test@test.com", Amount = 50 };
        var createdOrder = new Order(request.Amount, request.CustomerEmail) { OrderId = 5, OrderDate = DateTime.UtcNow };

        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.CreateOrderAsync(It.IsAny<Order>())).ReturnsAsync(createdOrder);

        var mockIdempotency = new Mock<IIdempotencyRepository>();
        mockIdempotency.Setup(r => r.GetIdempotencyKeyAsync(It.IsAny<string>())).ReturnsAsync((IdempotencyKey?)null);

        var mockPublish = new Mock<IPublishEndpoint>();
        mockPublish.Setup(p => p.Publish(It.IsAny<OrderCreatedEvent>(), default)).Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockIdempotency.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.CreateOrderAsync(request, "key-2");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
        mockIdempotency.Verify(r => r.SaveAsync(It.Is<IdempotencyKey>(k => k.Key == "key-2" && k.Response == "5")), Times.Once);
        mockPublish.Verify(p => p.Publish(It.Is<OrderCreatedEvent>(e => e.OrderId == 5 && e.Amount == request.Amount && e.CustomerEmail == request.CustomerEmail), default), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_ThrowsException_WhenRepositoryThrows()
    {
        // Arrange
        var request = new OrderRequest { CustomerEmail = "test@test.com", Amount = 15 };

        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.CreateOrderAsync(It.IsAny<Order>())).ThrowsAsync(new Exception("DB error"));

        var mockIdempotency = new Mock<IIdempotencyRepository>();
        mockIdempotency.Setup(r => r.GetIdempotencyKeyAsync(It.IsAny<string>())).ReturnsAsync((IdempotencyKey?)null);

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockIdempotency.Object, mockPublish.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.CreateOrderAsync(request, "key-3"));
    }

    [Fact]
    public async Task GetAllOrders_ReturnsNotFound_WhenRepositoryReturnsEmptyList()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.GetAllOrdersAsync()).ReturnsAsync(Enumerable.Empty<Order>());

        var mockIdempotency = new Mock<IIdempotencyRepository>();
        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockIdempotency.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.GetAllOrdersAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error!.Code);
        Assert.Equal("No orders found!", result.Error.Message);
    }

    [Fact]
    public async Task CreateOrder_ThrowsException_WhenPublishEndpointThrows()
    {
        // Arrange
        var request = new OrderRequest { CustomerEmail = "test@test.com", Amount = 30 };
        var createdOrder = new Order(request.Amount, request.CustomerEmail) { OrderId = 7, OrderDate = DateTime.UtcNow };

        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.CreateOrderAsync(It.IsAny<Order>())).ReturnsAsync(createdOrder);

        var mockIdempotency = new Mock<IIdempotencyRepository>();
        mockIdempotency.Setup(r => r.GetIdempotencyKeyAsync(It.IsAny<string>())).ReturnsAsync((IdempotencyKey?)null);

        var mockPublish = new Mock<IPublishEndpoint>();
        mockPublish.Setup(p => p.Publish(It.IsAny<OrderCreatedEvent>(), default)).ThrowsAsync(new Exception("Publish failed"));

        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockIdempotency.Object, mockPublish.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.CreateOrderAsync(request, "key-4"));
    }

    [Fact]
    public async Task CreateOrder_ReturnsCachedOrderId_WhenIdempotencyKeyExists()
    {
        // Arrange
        var request = new OrderRequest { CustomerEmail = "test@test.com", Amount = 100 };
        var existingKey = new IdempotencyKey
        {
            Key = "duplicate-key",
            Response = "999",
            CreatedAt = DateTime.UtcNow
        };

        var mockRepo = new Mock<IOrderRepository>();
        var mockIdempotency = new Mock<IIdempotencyRepository>();
        mockIdempotency.Setup(r => r.GetIdempotencyKeyAsync("duplicate-key")).ReturnsAsync(existingKey);

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockIdempotency.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.CreateOrderAsync(request, "duplicate-key");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(999, result.Value);
        mockRepo.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Never);
        mockPublish.Verify(p => p.Publish(It.IsAny<OrderCreatedEvent>(), default), Times.Never);
    }
}
