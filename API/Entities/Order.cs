using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;


public enum OrderStatus
{
    // I add explicit numeric values so even if I remove or add items in this enum,
    // The database will still continue to work as expected.
    None = 0,
    Pending = 1,
    PaymentReceived = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}

[Table("Orders")]
public class Order
{
    public long Id { get; set; }
    public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public float TotalPrice { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    // Navigation Properties

    // This needs to be set to the null forgiving operator for entity framework to
    // set the SQL property to not null.
    public required long SellerId { get; set;}
    // This needs to be set to the null forgiving operator for entity framework to
    // set the SQL property to not null.
    public User Seller { get; set; } = null!;
    // Entity framework requires a separate property for the ID
    public required long BuyerId { get; set;}
    // Entity framework requires a separate property for the ID
    public User Buyer { get; set; } = null!;
    
    public ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();
}