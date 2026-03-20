using API.Entities;

namespace API.DTOs;

public class OrderDto
{
    public long Id { get; set; }
    public string? OrderDate { get; set; } = "";
    public float TotalPrice { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.None;
    public long SellerId { get; set; }
    public string SellerUserName { get; set; } = "";
    public long BuyerId { get; set; }
    public string BuyerName { get; set; } = "";
    public string BuyerAddress { get; set; } = "";
    public ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

    public OrderDto() {}

    public OrderDto(Order order)
    {
        Id = order.Id;
        OrderDate = order.OrderDate.ToString("o");
        TotalPrice = order.TotalPrice;
        Status = order.Status;
        SellerId = order.Seller.Id;
        SellerUserName = order.Seller.UserName ?? "";
        BuyerId = order.Buyer.Id;
        BuyerName = order.Buyer.FullName;
        BuyerAddress = order.Buyer.Address;
        OrderItems = order.OrderItems.Select(orderItem => new OrderItemDto(orderItem)).ToList();
    }
}