using API.Data.Configuration;
using API.DTOs;
using API.Entities;
using API.Misc;
using API.Pagination;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class OrderManager (DataContext context, ICraftyUserManager userManager) : IOrderManager
{
    public async Task<ManagerResponse<PagedList<OrderDto>>> GetOrdersAsync (long userId, OrderListParams listParams)
    {
        var user = await userManager.GetUserAsync(userId);
        
        if (user == null)
        {
            return new ManagerResponse<PagedList<OrderDto>>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A user with id {userId} does not exist."]
            };
        }

        var query = listParams.TypeFilter switch
        {
            OrderListType.SellOnly => context.Orders.Where(order => order.SellerId == userId),
            OrderListType.BuyOnly => context.Orders.Where(order => order.BuyerId == userId),
            _ => context.Orders.Where(order => order.BuyerId == userId || order.SellerId == userId)
        };

        if (listParams.ShowIncompleteOnly)
            query = query.Where(order => order.Status != OrderStatus.Delivered && order.Status != OrderStatus.Cancelled);

        query = listParams.OrderBy.ToLower() switch
        {
            "buyername" => listParams.IsOrderDescending ? 
                query.OrderByDescending(order => order.Buyer.FullName) 
                : query.OrderBy(order => order.Buyer.FullName),
            "sellerusername" => listParams.IsOrderDescending ? 
                query.OrderByDescending(order => order.Seller.UserName) 
                : query.OrderBy(order => order.Seller.UserName),
            _ => listParams.IsOrderDescending ? 
                query.OrderByDescending(order => order.OrderDate) 
                : query.OrderBy(order => order.OrderDate),
        };

        var resultQuery = query.Select(order => new OrderDto()
        {
            Id = order.Id,
            OrderDate = order.OrderDate.ToString("o"),
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            SellerId = order.Seller.Id,
            SellerUserName = order.Seller.UserName ?? "",
            BuyerId = order.Buyer.Id,
            BuyerName = order.Buyer.FullName,
            BuyerAddress = order.Buyer.Address
            // I intentionally leave out the OrderItem collection here since the list endpoint doesn't need it, and it would be a waste of resources to include it.            
        });

        return new ManagerResponse<PagedList<OrderDto>>(
            await PagedList<OrderDto>.CreateAsync(resultQuery, listParams.PageNumber, listParams.PageSize));        
    }    

    public async Task<ManagerResponse<OrderDto>> GetOrderAsync (long userId, long orderId)
    {
        var orderDto = await context.Orders
            .Include(order => order.Seller)
            .Include (order => order.Buyer)
            .Include (order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.Craft)
                    .ThenInclude(craft => craft.SearchImage)
            .Where(order => order.Id == orderId)
            .Select (order => new OrderDto(order))
            .SingleOrDefaultAsync();

        if (orderDto == null)
        {
            return new ManagerResponse<OrderDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"An order with id {orderId} does not exist."]
            };
        }

        if (orderDto?.SellerId != userId && orderDto?.BuyerId != userId)
        {            
            return new ManagerResponse<OrderDto>()
            {
                ResponseType = ManagerResponseType.Forbidden
            };
        }

        return new ManagerResponse<OrderDto>(orderDto);
    }

    public async Task<ManagerResponse<OrderDto>> CreateOrderAsync(long userId, OrderDto orderDto)
    {

        if (orderDto.BuyerId != userId)
        {
            return new ManagerResponse<OrderDto>() { ResponseType = ManagerResponseType.Forbidden };
        }

        var newOrder = new Order()
        {
            TotalPrice = orderDto.TotalPrice,
            Status = OrderStatus.Pending,
            SellerId = orderDto.SellerId,
            BuyerId = orderDto.BuyerId
        };
        
        if (DateOnly.TryParse(orderDto.OrderDate, out var createdAt))
        {
            newOrder.OrderDate = createdAt;
        }

        foreach (var orderItemDto in orderDto.OrderItems)
        {
            var newOrderItem = new OrderItem()
            {
                Quantity = orderItemDto.Quantity,
                PricePerCraft = orderItemDto.PricePerCraft,
                Discount = orderItemDto.Discount,
                CraftId = orderItemDto.CraftId,
            };
            newOrder.OrderItems.Add(newOrderItem);
        }

        var response = await userManager.AddOrderToUsersAsync(newOrder);

        if (response.Data == null)
        {
            return response;
        }
        
        if (await context.SaveChangesAsync() == 0)
        {
            return new ManagerResponse<OrderDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["Failed to create a new order."]
            };
        }

        return new ManagerResponse<OrderDto>(new OrderDto(newOrder));
    }

    public async Task<ManagerResponse<OrderDto>> UpdateOrderAsync(long userId, OrderDto orderDto)
    {        

        if (orderDto.SellerId != userId && orderDto.BuyerId != userId)
        {
            return new ManagerResponse<OrderDto>() { ResponseType = ManagerResponseType.Forbidden };
        }

        var order = await context.Orders.SingleOrDefaultAsync(order => order.Id == orderDto.Id);

        if (order == null)
        {
            return new ManagerResponse<OrderDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"An order with id {orderDto.Id} does not exist."]
            };
        }

        if (orderDto.Status == OrderStatus.Cancelled && orderDto.BuyerId != userId)
        {
            return new ManagerResponse<OrderDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["Only a buyer can cancel an order."]
            };
        }

        if (orderDto.SellerId != userId && 
            orderDto.Status != order.Status &&
            (orderDto.Status == OrderStatus.PaymentReceived ||
            orderDto.Status == OrderStatus.Pending ||
            orderDto.Status == OrderStatus.Shipped))
        {
            return new ManagerResponse<OrderDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"Only a seller can change the order's status to '{orderDto.Status}'"]
            };
        }
        
        // For the moment, I am only allowing the users to edit the status of orders.
        order.Status = orderDto.Status;

        if (await context.SaveChangesAsync() == 0)
        {            
            return new ManagerResponse<OrderDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"Failed to update order with id {orderDto.Id}"]
            };
        }

        return new ManagerResponse<OrderDto>(orderDto);
    }
}