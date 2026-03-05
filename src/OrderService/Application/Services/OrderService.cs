using OrderService.Application.Dto;
using OrderService.Application.Extensions;
using OrderService.Application.Interfaces;
using Shared.Contracts.Common;
using System.Net;

namespace OrderService.Application.Services;

public class OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger) : IOrderService
{
    public async Task<Result<IEnumerable<OrderResponse>>> GetAllOrders()
    {
        var orders = await orderRepository.GetAllOrders();
        if (orders is null)
        {
            logger.LogWarning("No orders found!");
            return Result<IEnumerable<OrderResponse>>.Failure(new Error((int)HttpStatusCode.BadRequest, "No orders found!"));
        }

        logger.LogInformation("Returning orders.");
        var dtos = orders.Select(o => o.ToResponse());
        return Result<IEnumerable<OrderResponse>>.Success(dtos);
    }

    public async Task<Result<int>> CreateOrder(OrderRequest request)
    {
        var order = OrderRequest.ToOrder(request);
        var result = await orderRepository.CreateOrder(order);
        if (result == 0)
        {
            logger.LogError("Failed to create order for customer:{CustomerEmail}", request.CustomerEmail);
            return Result<int>.Failure(new Error((int)HttpStatusCode.InternalServerError, "Failed to create order."));
        }

        logger.LogInformation("Order created successfully for customer:{CustomerEmail}", request.CustomerEmail);
        return Result<int>.Success(order.OrderId);
    }
}