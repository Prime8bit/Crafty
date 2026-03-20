using API.Entities;

namespace API.DTOs;

public class MessageDto
{
    public long Id { get; set; }
    public string Content { get; set; }
    public DateTime? DateRead { get; set; }
    public DateTime DateSent { get; set; } = DateTime.UtcNow;

    // Nav Properties
    public long SenderId { get; set; }
    public string SenderDisplayName { get; set; }
    public string? SenderProfileImageUrl { get; set; }
    public long RecipientId { get; set; }
    public string RecipientDisplayName { get; set; }

    public MessageDto()
    {
        Content = "";
        SenderId = 0;
        SenderDisplayName = "";
        RecipientId = 0;
        RecipientDisplayName = "";
    }

    public MessageDto(Message message)
    {
        Id = message.Id;
        Content = message.Content;
        DateRead = message.DateRead;
        DateSent = message.DateSent;
        SenderId = message.SenderId;
        SenderDisplayName = message.Sender?.DisplayName ?? "";
        SenderProfileImageUrl = message.Sender?.ProfileImage?.Url;
        RecipientId = message.RecipientId;
        RecipientDisplayName = message.Recipient?.DisplayName ?? "";
    }
}