using MassTransit;
using OrderService.Application.Dto;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Idempotency;
using Shared.Contracts.Common;
using Shared.Contracts.Events;
using System.Net;

namespace OrderService.Application.Services;

public class OrderService(IOrderRepository orderRepository, IIdempotencyRepository idempotencyRepository,
    IPublishEndpoint publishEndpoint, ILogger<OrderService> logger) : IOrderService
{
    public async Task<Result<IEnumerable<OrderResponse>>> GetAllOrdersAsync()
    {
        var orders = await orderRepository.GetAllOrdersAsync();
        if (!orders.Any())
        {
            logger.LogWarning("No orders found!");
            return Result<IEnumerable<OrderResponse>>.Failure(new Error((int)HttpStatusCode.NotFound, "No orders found!"));
        }

        logger.LogInformation("Returning orders.");
        var dtos = orders.Select(p => OrderResponse.ToDto(p));
        return Result<IEnumerable<OrderResponse>>.Success(dtos);
    }

    public async Task<Result<int>> CreateOrderAsync(OrderRequest request, string idempotencyKey)
    {
        var existing = await idempotencyRepository.GetIdempotencyKeyAsync(idempotencyKey);

        if (existing != null)
        {
            logger.LogInformation("Duplicate request detected for key {Key}", idempotencyKey);
            return Result<int>.Success(int.Parse(existing.Response));
        }

        var order = OrderRequest.ToOrder(request);
        order = await orderRepository.CreateOrderAsync(order);
        if (order.OrderId == 0)
        {
            logger.LogError("Failed to create order for customer:{CustomerEmail}", request.CustomerEmail);
            return Result<int>.Failure(new Error((int)HttpStatusCode.InternalServerError, "Failed to create order."));
        }

        await idempotencyRepository.SaveAsync(new IdempotencyKey
        {
            CreatedAt = DateTime.UtcNow,
            Key = idempotencyKey,
            Response = order.OrderId.ToString()
        });

        await publishEndpoint.Publish(new OrderCreatedEvent(order.OrderId, order.Amount, order.CustomerEmail, order.OrderDate));

        logger.LogInformation("Order: {OrderId} created successfully for customer:{CustomerEmail}", order.OrderId, request.CustomerEmail);
        return Result<int>.Success(order.OrderId);
    }
}