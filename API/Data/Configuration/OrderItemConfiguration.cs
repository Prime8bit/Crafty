using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configuration;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(orderItem => orderItem.Id);        

        builder.HasOne(orderItem => orderItem.Order)
            .WithMany(order => order.OrderItems)
            .HasForeignKey(orderItem => orderItem.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(orderItem => orderItem.Craft)
             .WithMany(craft => craft.OrderItems)
             .HasForeignKey(orderItem => orderItem.CraftId)
             .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(orderItem => orderItem.Seller)
            .WithMany(user => user.OrderItemsAsSeller)
            .HasForeignKey(orderItem => orderItem.SellerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}