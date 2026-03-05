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
            logger.LogInformation("No orders found!");
            return Result<IEnumerable<OrderResponse>>.Failure(new Error((int)HttpStatusCode.BadRequest, "No orders found!"));
        }

        logger.LogInformation("Returning {OrderCount} orders.", orders.Count);
        var dtos = orders.Select(o => o.ToResponse());
        return Result<IEnumerable<OrderResponse>>.Success(dtos);
    }
}
