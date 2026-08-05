using System.Security.Claims;
using API.Data;
using API.Extensions;
using API.Misc;
using CraftyCommon.Pagination;
using CraftyCommon.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

public class OrdersController (IOrderManager orderManager) : BaseApiController
{    
    [HttpGet]
    [EnableRateLimiting(RateLimiters.UserRead)]
    public async Task<ActionResult<PagedList<OrderListItemDto>>> GetOrders([FromQuery]OrderListParams listParams)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId = 0;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to view your orders.");
        }

        var orderResponse = await orderManager.GetOrdersAsync(userId, listParams);

        return GetActionResult(orderResponse);
    }
    
    [HttpGet("{orderId:long}")]
    [EnableRateLimiting(RateLimiters.UserRead)]
    public async Task<ActionResult<OrderDto>> GetOrder(long orderId)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId = 0;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to view your orders.");
        }
        
        return GetActionResult(await orderManager.GetOrderAsync(userId, orderId));
    }

    [HttpPost]
    [EnableRateLimiting(RateLimiters.UserWrite)]
    public async Task<ActionResult<OrderDto>> CreateOrder(OrderDto orderDto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId = 0;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to view your orders.");
        }
        
        var response = await orderManager.CreateOrderAsync(userId, orderDto);
        if (response.ResponseType != ManagerResponseType.Ok || response.Data?.Id == null)
        {
            return GetActionResult(response);
        }

        /** In ASP.NET Core, using the suffix "Async" in function names can break some of the reflection
        * resolution in some ASP.NET functionality. If you use "Async" at the end of the functions they may work
        * for a long time until you use specific ASP.NET functionality, like CreatedAtAction
        **/
        return CreatedAtAction(nameof(GetOrder), new { orderId = response.Data!.Id }, orderDto);
    }

    [HttpPut("{orderId}/setStatus/{newStatus}")]
    [EnableRateLimiting(RateLimiters.UserWrite)]
    public async Task<ActionResult<OrderDto>> SetOrderStatus(long orderId, OrderStatus newStatus)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId = 0;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to cancel your orders.");
        }

        return GetActionResult(await orderManager.SetOrderStatusAsync(userId, orderId, newStatus));
    }

    [HttpPut("withOrderItem/{orderItemId}/setStatus/{newStatus}")]
    [EnableRateLimiting(RateLimiters.UserWrite)]
    public async Task<ActionResult<OrderItemDto>> UpdateOrderItemStatusAsync(long orderItemId, OrderItemStatus newStatus)
    {        
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId = 0;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to update order items.");
        }

        return GetActionResult(await orderManager.UpdateOrderItemStatusAsync(userId, orderItemId, newStatus));
    }
}