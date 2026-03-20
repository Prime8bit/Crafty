using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Configuration;

public class DataContext(DbContextOptions options) : 
    IdentityDbContext<User, Role, long>(options)
{
    // The names of these sets define the names of the database tables.
    // That is why they are all singular.
    public DbSet<Craft> Crafts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<WishlistItem> CraftWishlistItems { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageGroup> MessageGroups { get; set; }
    public DbSet<MessageConnection> MessageConnections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);        
        modelBuilder.ApplyConfiguration(new CraftConfiguration());
        modelBuilder.ApplyConfiguration(new CraftMediaConfiguration());
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserMediaConfiguration());
        modelBuilder.ApplyConfiguration(new MessageGroupConfiguration());
    }
}
