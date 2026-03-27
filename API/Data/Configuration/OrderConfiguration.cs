using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);

        // Don't forget to use .HasForeignKey with the explicit type when .WithOne() has no back reference.
        builder.HasOne(order => order.Buyer)
            .WithMany(user => user.OrdersAsBuyer)
            .HasForeignKey(order => order.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}