using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configuration;

public class MessageGroupConfiguration : IEntityTypeConfiguration<MessageGroup>
{
    public void Configure(EntityTypeBuilder<MessageGroup> builder)
    {
        builder.HasKey(messageGroup => messageGroup.Name);

        builder.HasMany(messageGroup => messageGroup.Connections)
            .WithOne()
            .HasForeignKey(messageConnection => messageConnection.MessageGroupName)
            .OnDelete(DeleteBehavior.Cascade);
    }
}