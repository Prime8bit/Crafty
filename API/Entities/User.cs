using System;
using API.Controllers;
using API.DTOs;
using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class User : IdentityUser<long> 
{
    public required string FullName { get; set; }
    public string DisplayName { get; set; } = "";
    public string Address { get; set; } = "";
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    // Navigation properties    
    public UserMedia ProfileImage { get; set; } = null!;
    public List<Order> OrdersAsSeller { get; set; } = new List<Order>();
    public List<Order> OrdersAsBuyer { get; set; } = new List<Order>();
    public List<Craft> Products { get; set; } = new List<Craft>();
    public List<Craft> Wishlist { get; set; } = new List<Craft>();
    public ICollection<Role> Roles { get; set; } = [];
    public List<Message> MessagesSent { get; set; } = new List<Message>();
    public List<Message> MessagesReceived { get; set; } = new List<Message>();

}
