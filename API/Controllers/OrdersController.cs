using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Extensions;
using API.Misc;
using API.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class OrdersController (IOrderManager orderManager) : BaseApiController
{    
    [HttpGet]
    public async Task<ActionResult<PagedList<OrderDto>>> GetOrders([FromQuery]OrderListParams listParams)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId = 0;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to view your orders.");
        }

        var orderResponse = await orderManager.GetOrdersAsync(userId, listParams);

        if (orderResponse.Data != null)
        {
            Response.AddPaginationHeader(orderResponse.Data);
        }

        return GetActionResult(orderResponse);
    }
    
    [HttpGet("{orderId}")]
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

    [HttpPut("{orderId}")]
    public async Task<ActionResult<OrderDto>> UpdateOrder(long orderId, OrderDto orderDto)
    {
        orderDto.Id = orderId;

        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId = 0;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to view your orders.");
        }

        return GetActionResult(await orderManager.UpdateOrderAsync(userId, orderDto));
    }
}