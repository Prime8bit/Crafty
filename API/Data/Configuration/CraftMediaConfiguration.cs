using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configuration;

public class CraftMediaConfiguration : IEntityTypeConfiguration<CraftMedia>
{
    public void Configure(EntityTypeBuilder<CraftMedia> builder)
    {
        builder.HasKey(craftMedia => craftMedia.Id);
        
        builder.HasOne(craftMedia => craftMedia.Craft)
             .WithMany(craft => craft.Medias)
             .HasForeignKey(craftMedia => craftMedia.CraftId)
             .OnDelete(DeleteBehavior.Cascade);
    }
}