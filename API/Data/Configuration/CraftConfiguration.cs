using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configuration;

public class CraftConfiguration : IEntityTypeConfiguration<Craft>
{
    public void Configure(EntityTypeBuilder<Craft> builder)
    {
        builder.HasKey(craft => craft.Id);

        builder.HasOne(craft => craft.Seller)
            .WithMany(user => user.Products)
            .HasForeignKey(craft => craft.SellerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Don't forget to use .HasForeignKey with the explicit type when .WithOne() has no back reference.
        builder.HasOne(craft => craft.SearchImage)
            .WithOne()
            .HasForeignKey<Craft>(craft => craft.SearchImageId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}