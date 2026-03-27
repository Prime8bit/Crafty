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
    public long ProfileImageId { get; set; }
    public UserMedia? ProfileImage { get; set; } = null!;
    public ICollection<OrderItem> OrderItemsAsSeller { get; set; } = new List<OrderItem>();
    public ICollection<Order> OrdersAsBuyer { get; set; } = new List<Order>();
    public ICollection<Craft> Products { get; set; } = new List<Craft>();
    public ICollection<Craft> Wishlist { get; set; } = new List<Craft>();
    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<Message> MessagesSent { get; set; } = new List<Message>();
    public ICollection<Message> MessagesReceived { get; set; } = new List<Message>();

}
