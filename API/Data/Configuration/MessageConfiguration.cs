using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configuration;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(message => message.Id);

        builder.HasOne(message => message.Sender)
            .WithMany(user => user.MessagesSent)
            .HasForeignKey(message => message.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(message => message.Recipient)
            .WithMany(user => user.MessagesReceived)
            .HasForeignKey(message => message.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}