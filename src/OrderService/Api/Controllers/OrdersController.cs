using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Interfaces;

namespace OrderService.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController(IOrderService orderService, ILogger<OrdersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOrders() //TODO: Implement pagination
    {
        logger.LogInformation("Getting orders.");
        var result = await orderService.GetAllOrders();

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.Message, statusCode: result.Error.Code);
        }

        return Ok(result.Value);
    }
}
