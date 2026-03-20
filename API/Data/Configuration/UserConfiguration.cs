using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);

        builder.HasMany(user => user.Wishlist)
             .WithMany(craft => craft.WishListingUsers)
             .UsingEntity<WishlistItem>();

        builder.HasMany(user => user.Roles)
            .WithMany(role => role.Users)
            .UsingEntity<IdentityUserRole<long>>();
    }
}