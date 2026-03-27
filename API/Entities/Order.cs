using CraftyCommon.DTOs;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

public class Order
{
    public long Id { get; set; }
    public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public float TotalPrice { get; set; } = 0f;
    public required string ShippingName { get; set; }
    public required string ShippingAddress { get; set; }
    public required string BillingName { get; set; }
    public required string BillingAddress { get; set; }
    public required OrderStatus Status { get; set; }
    
    // Navigation Properties
    // Entity framework requires a separate property for the ID
    public long BuyerId { get; set;}
    public User? Buyer { get; set; }
    
    public ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();
}