using System;

namespace API.Entities;

public class OrderItem
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

    // Navigation Properties    
    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public long CraftId { get; set; }
    // This needs to be set to the null forgiving operator for entity framework to
    // set the SQL property to not null.
    public Craft Craft { get; set; } = null!;
}