using API.Entities;

namespace API.DTOs;

public class OrderItemDto
{    
    public long Id { get; set; }
    
    // Entity framework requires a separate property for the ID
    public int Quantity { get; set; }
    // This is the price per craft at the time of the order, which may be different from the current price
    // This includes any discounts that may have existed at the time.
    public float PricePerCraft { get; set; }
    // This is the discount per craft at the time of the order, which may be different from the current discount
    // This is not displayed to the buyer, but is used for internal calculations
    public float Discount { get; set; }
    public long CraftId { get; set; }
    public string CraftName { get; set; } = null!;
    public CraftMediaDto? CraftMediaItem { get; set; }
    public OrderItemDto() {}

    public OrderItemDto(OrderItem orderItem)
    {
        Id = orderItem.Id;
        Quantity = orderItem.Quantity;
        PricePerCraft = orderItem.PricePerCraft;
        Discount = orderItem.Discount;
        CraftId = orderItem.Craft.Id;
        CraftName = orderItem.Craft.Name;
        CraftMediaItem = orderItem.Craft.SearchImage == null ? null : new CraftMediaDto(orderItem.Craft.SearchImage);
    }
}