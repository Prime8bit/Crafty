using System;
using API.Entities;

namespace API.DTOs;

public class UserDto
{
    // If a public property is named "Id" it is implicitly chosen as the primary key
    // If you want to change the name of this property then uncomment the line below
    // [Key]
    public long Id { get; set; }
    
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? DisplayName { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastActive { get; set; }
    public string? Address { get; set; }

    public UserMediaDto? ProfileImage { get; set; }
    
    //public List<Order> OrdersAsSeller { get; set; } = new List<Order>();
    //public List<Order> OrdersAsBuyer { get; set; } = new List<Order>();
    public List<CraftDto> Products { get; set; } = new List<CraftDto>();

    // An explicit parameterless constructor is needed for deserialization
    public UserDto() {}

    public UserDto(User user)
    {
        Id = user.Id;
        UserName = user.UserName;
        Email = user.Email;
        FullName = user.FullName;
        DisplayName = user.DisplayName;
        Created = user.Created;
        LastActive = user.LastActive;
        Address = user.Address;
        if (user.ProfileImage != null)
        {
            ProfileImage = new UserMediaDto(user.ProfileImage)
            {
                UserUserName = user.UserName
            };
        }
        foreach (var craft in user.Products)
        {
            Products.Add(new CraftDto(craft));
        }
    }
}
