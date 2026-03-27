using API.Misc;
using API.Pagination;
using CraftyCommon.DTOs;

namespace API.Data;

public interface IOrderManager
{
    Task<ManagerResponse<PagedList<OrderListItemDto>>> GetOrdersAsync (long userId, OrderListParams listParams);
    
    Task<ManagerResponse<OrderDto>> GetOrderAsync (long userId, long orderId);
    Task<ManagerResponse<OrderDto>> CreateOrderAsync(long userId, OrderDto orderDto);

    Task<ManagerResponse<OrderDto>> SetOrderStatusAsync(long userId, long orderId, OrderStatus newStatus);
    Task<ManagerResponse<OrderItemDto>> UpdateOrderItemStatusAsync(long userId, long orderItemId, OrderItemStatus newStatus);
}