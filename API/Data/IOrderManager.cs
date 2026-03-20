using API.DTOs;
using API.Misc;
using API.Pagination;

namespace API.Data;

public interface IOrderManager
{
    Task<ManagerResponse<PagedList<OrderDto>>> GetOrdersAsync (long userId, OrderListParams listParams);
    
    Task<ManagerResponse<OrderDto>> GetOrderAsync (long userId, long orderId);
    Task<ManagerResponse<OrderDto>> CreateOrderAsync(long userId, OrderDto orderDto);

    Task<ManagerResponse<OrderDto>> UpdateOrderAsync(long userId, OrderDto orderDto);
}