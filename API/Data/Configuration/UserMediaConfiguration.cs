using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configuration;

public class UserMediaConfiguration : IEntityTypeConfiguration<UserMedia>
{
    public void Configure(EntityTypeBuilder<UserMedia> builder)
    {
        builder.HasKey(userMedia => userMedia.Id);
        
        builder.HasOne(userMedia => userMedia.User)
             .WithOne(user => user.ProfileImage)
             .HasForeignKey<UserMedia>(userMedia => userMedia.UserId)
             .OnDelete(DeleteBehavior.Cascade);
    }
}