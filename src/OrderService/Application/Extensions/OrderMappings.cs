using OrderService.Application.Dto;
using OrderService.Domain.Entities;

namespace OrderService.Application.Extensions
{
    public static class OrderMappings
    {
        public static OrderResponse ToResponse(this Order order)
        {
            return new OrderResponse
            {
                OrderId = order.OrderId,
                Amount = order.Amount,
                CustomerEmail = order.CustomerEmail
            };
        }
    }
}
