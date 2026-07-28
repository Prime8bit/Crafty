using API.Data.Configuration;
using API.Entities;
using API.Misc;
using CraftyCommon.Pagination;
using CraftyCommon.DTOs;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class OrderManager (
    DataContext context, 
    UserManager<User> userManager
    ) : IOrderManager
{
    public async Task<ManagerResponse<PagedList<OrderListItemDto>>> GetOrdersAsync (long userId, OrderListParams listParams)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            return new ManagerResponse<PagedList<OrderListItemDto>>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A user with id {userId} does not exist."]
            };
        }

        var query = listParams.TypeFilter switch
        {
            OrderListType.SellOnly => context.Orders
                .Include(order => order.Buyer)
                .Where(order => order.OrderItems
                .Any(orderItem => orderItem.SellerId == userId)),
            OrderListType.BuyOnly => context.Orders
                .Include(order => order.Buyer)
                .Where(order => order.BuyerId == userId),
            _ => context.Orders
                .Include(order => order.Buyer)
                .Where(order => order.BuyerId == userId 
                    || order.OrderItems.Any(orderItem => orderItem.SellerId == userId))
        };

        if (listParams.ShowIncompleteOnly)
            query = query.Where(order => order.Status != OrderStatus.Complete);

        query = listParams.OrderBy.ToLower() switch
        {
            "buyername" => listParams.IsOrderDescending ? 
                query.OrderByDescending(order => order.Buyer!.FullName) 
                : query.OrderBy(order => order.Buyer!.FullName),
            _ => listParams.IsOrderDescending ? 
                query.OrderByDescending(order => order.OrderDate) 
                : query.OrderBy(order => order.OrderDate),
        };

        var resultQuery = query.ProjectToType<OrderListItemDto>();

        return new ManagerResponse<PagedList<OrderListItemDto>>(
            await PagedList<OrderListItemDto>.CreateAsync(resultQuery, listParams.PageNumber, listParams.PageSize));        
    }    

    public async Task<ManagerResponse<OrderDto>> GetOrderAsync (long userId, long orderId)
    {
        var order = await context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return new ManagerResponse<OrderDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"Unable to find order with id {orderId}"]
            };
        }

        OrderDto? orderDto;
        if (order.BuyerId == userId)
        {
            orderDto = await context.Orders
                .Include (order => order.Buyer)
                    .ThenInclude(user => user!.ProfileImage)
                .Include (order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Seller)
                        .ThenInclude(user => user!.ProfileImage)
                .Include (order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Craft)
                        .ThenInclude(craft => craft!.SearchImage)
                .Where(order => order.Id == orderId)
                .ProjectToType<OrderDto>()
                .SingleOrDefaultAsync();
        }
        else
        {
            orderDto = await context.Orders
                .Include (order => order.Buyer!)
                    .ThenInclude(user => user.ProfileImage)
                .Include (order => order.OrderItems.Where(orderItem => orderItem.SellerId == userId))
                    .ThenInclude(orderItem => orderItem.Seller!)
                        .ThenInclude(user => user.ProfileImage)
                .Include (order => order.OrderItems.Where(orderItem => orderItem.SellerId == userId))
                    .ThenInclude(orderItem => orderItem.Craft!)
                        .ThenInclude(craft => craft.SearchImage)
                .Where(order => order.Id == orderId)
                .ProjectToType<OrderDto>()
                .SingleOrDefaultAsync();

            if (orderDto != null && orderDto.OrderItems.Count == 0)
            {
                return new ManagerResponse<OrderDto>()
                {
                    ResponseType = ManagerResponseType.Forbidden
                };
            }
        }

        if (orderDto == null)
        {
            return new ManagerResponse<OrderDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"An order with id {orderId} does not exist."]
            };
        }

        return new ManagerResponse<OrderDto>(orderDto);
    }

    public async Task<ManagerResponse<OrderDto>> CreateOrderAsync(long userId, OrderDto orderDto)
    {
        using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
            {
                var user = await userManager.Users
                    .Include(user => user.ProfileImage)
                    .Where (user => user.Id == userId)
                    .FirstOrDefaultAsync();
        
                if (user == null)
                {
                    return new ManagerResponse<OrderDto>()
                    {
                        ResponseType = ManagerResponseType.NotFound,
                        ErrorMessages = [$"A user with id {userId} does not exist."]
                    };
                }

                if (orderDto.BuyerId != userId)
                {
                    return new ManagerResponse<OrderDto>() { ResponseType = ManagerResponseType.Forbidden };
                }

                // If I had a payment processor, this is where I might use it.

                var newOrder = new Order()
                {
                    OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    ShippingName = orderDto.ShippingName == "" && user.FullName != null ? user.FullName : orderDto.ShippingName,
                    ShippingAddress = orderDto.ShippingAddress == "" && user.Address != null ? user.Address : orderDto.ShippingAddress,
                    BillingName = orderDto.BillingName == "" && user.FullName != null ? user.FullName : orderDto.BillingName,
                    BillingAddress = orderDto.BillingAddress == "" && user.Address != null ? user.Address : orderDto.BillingAddress,
                    Status = OrderStatus.PaymentReceived,
                    BuyerId = userId,
                };
                float totalPrice = 0f;

                foreach (var orderItemDto in orderDto.OrderItems)
                {
                    var orderItemResponse = await CreateOrderItemAsync(orderItemDto);
                    if (orderItemResponse.Data == null)
                    {
                        return new ManagerResponse<OrderDto>()
                        {
                            ResponseType = orderItemResponse.ResponseType,
                            ErrorMessages = orderItemResponse.ErrorMessages
                        };
                    }
                    // TODO (Nate) Implement discounts here.
                    totalPrice += orderItemResponse.Data.PricePerCraft; // * orderItemResponse.Data.Discount;
                    newOrder.OrderItems.Add(orderItemResponse.Data);
                }

                newOrder.TotalPrice = totalPrice;
                context.Orders.Add(newOrder);
                
                if (await context.SaveChangesAsync() == 0)
                {
                    await transaction.RollbackAsync();
                    return new ManagerResponse<OrderDto>()
                    {
                        ResponseType = ManagerResponseType.BadRequest,
                        ErrorMessages = ["Failed to create a new order."]
                    };
                }

                await transaction.CommitAsync();

                return new ManagerResponse<OrderDto>(newOrder.Adapt<OrderDto>());
            } catch (Exception ex)
            {
                await transaction.RollbackAsync();                
                return new ManagerResponse<OrderDto>()
                {
                    ResponseType = ManagerResponseType.BadRequest,
                    ErrorMessages = [ex.Message]
                };
            }
        }  
    }

    public async Task<ManagerResponse<OrderDto>> SetOrderStatusAsync(long userId, long orderId, OrderStatus newStatus)
    {
        var order = await context.Orders.SingleOrDefaultAsync(order => order.Id == orderId);

        if (order == null)
        {
            return new ManagerResponse<OrderDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"An order with id {orderId} does not exist."]
            };
        }

        if (order.BuyerId != userId)
        {
            return new ManagerResponse<OrderDto>() { ResponseType = ManagerResponseType.Forbidden };
        }

        if (newStatus == OrderStatus.None)
        {
            return new ManagerResponse<OrderDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"You cannot set the status of order {orderId} to None."]
            };
        }

        var result = new OrderDto()
        {
            Id = order.Id,
            OrderDate = order.OrderDate.ToString("o"),
            TotalPrice = order.TotalPrice,
            ShippingName = order.ShippingName,
            ShippingAddress = order.ShippingAddress,
            BillingName = order.BillingName,
            BillingAddress = order.BillingAddress,
            Status = newStatus,
            BuyerId = order.BuyerId
        };

        // Setting the status to the current status is okay for this command.
        if (newStatus == order.Status)
        {
            return new ManagerResponse<OrderDto>(result);
        }
        
        // For the moment, I am only allowing the users to edit the status of orders.
        order.Status = newStatus;

        if (await context.SaveChangesAsync() == 0)
        {            
            return new ManagerResponse<OrderDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"Failed to cancel order with id {orderId}"]
            };
        }

        

        return new ManagerResponse<OrderDto>(result);
    }

    public async Task<ManagerResponse<OrderItemDto>> UpdateOrderItemStatusAsync(long userId, long orderItemId, OrderItemStatus newStatus)
    {
        if (newStatus == OrderItemStatus.None)
        {
            return new ManagerResponse<OrderItemDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"You may not set the status to 'None'."]
            };
        }

        var order = context.Orders
            .Include(order => order.Buyer)
            .Include(order => order.OrderItems)
            .Where(order => order.OrderItems.Any(orderItem => orderItem.Id == orderItemId))
            .FirstOrDefault();

        if (order == null)
        {
            return new ManagerResponse<OrderItemDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"No Order item found with id {orderItemId}"]
            };
        }

        var orderItem = order.OrderItems.First(orderItem => orderItem.Id == orderItemId);
        orderItem.Status = newStatus;

        if (order.Status != OrderStatus.Cancelled &&
            !order.OrderItems.Any(orderItem => orderItem.Status != OrderItemStatus.Delivered 
                                && orderItem.Status != OrderItemStatus.Cancelled))
        {
            order.Status = OrderStatus.Complete;
        }

        if (await context.SaveChangesAsync() == 0)
        {            
            return new ManagerResponse<OrderItemDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"Failed to update order item with id {orderItemId}"]
            };
        }

        var result = new OrderItemDto()
        {
            Id = orderItem.Id,
            Quantity = orderItem.Quantity,
            PricePerCraft = orderItem.PricePerCraft,
            Discount = orderItem.Discount,
            Status = orderItem.Status,
            OrderId = orderItem.OrderId,
            SellerId = userId,
        };

        return new ManagerResponse<OrderItemDto>(result);
    }

    private async Task<ManagerResponse<OrderItem>> CreateOrderItemAsync(OrderItemDto orderItemDto)
    {
        var craft = await context.Crafts
            .Include(craft => craft.Seller!)
                .ThenInclude(user => user.ProfileImage)
            .Where(craft => craft.Id == orderItemDto.CraftId)
            .FirstOrDefaultAsync();
        ;
        if (craft == null)
        {                
            return new ManagerResponse<OrderItem>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A craft with id {orderItemDto.CraftId} does not exist."]
            };
        }
        if (craft.Price != orderItemDto.PricePerCraft)
        {                             
            return new ManagerResponse<OrderItem>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"The craft {orderItemDto.CraftName} does not cost {orderItemDto.PricePerCraft}. Try refreshing your order."]
            };
        }
        if (craft.SellerId != orderItemDto.SellerId)
        {                                       
            return new ManagerResponse<OrderItem>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"The craft {orderItemDto.CraftName} is not sold by user with id {orderItemDto.SellerId}."]
            };
        }
        // TODO (Nate) Implement discounts here.

        var newOrderItem = new OrderItem()
        {
            Quantity = orderItemDto.Quantity,
            PricePerCraft = orderItemDto.PricePerCraft,
            Discount = 1.0f, // = orderItemDto.Discount when discounts are implemented.
            Status = OrderItemStatus.Pending,                
            CraftId = orderItemDto.CraftId,
            Craft = craft,
            SellerId = orderItemDto.SellerId,
            Seller = craft.Seller
        };

        return new ManagerResponse<OrderItem>(newOrderItem);
    }
}