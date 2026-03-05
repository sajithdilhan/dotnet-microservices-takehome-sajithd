using OrderService.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.Dto;

public class OrderRequest
{
    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;
    [Required]
    public decimal Amount { get; set; } = 0;

    public static Order ToOrder(OrderRequest request)
    {
        return new Order(request.Amount, request.CustomerEmail);
    }
}