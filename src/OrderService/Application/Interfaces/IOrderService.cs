using OrderService.Application.Dto;
using Shared.Contracts.Common;

namespace OrderService.Application.Interfaces;

public interface IOrderService
{
    Task<Result<IEnumerable<OrderResponse>>> GetAllOrders();
    Task<Result<int>> CreateOrder(OrderRequest request);
}
