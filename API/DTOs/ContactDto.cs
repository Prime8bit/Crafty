using API.Entities;

namespace API.DTOs;

public class ContactDto
{
    public long Id { get; set; } 
    public string DisplayName { get; set; }
    public string ProfileImageUrl { get; set; }

    public string LastMessage { get; set; }
    public DateTime LastMessageDate { get; set; }
    public bool WasLastMessageRead { get; set; }

    public ContactDto(Message message, bool useRecipient)
    {
        Id = useRecipient ? message.RecipientId : message.SenderId;
        DisplayName = useRecipient ? message.Recipient.DisplayName : message.Sender.DisplayName;
        ProfileImageUrl = useRecipient ? message.Recipient.ProfileImage.Url : message.Sender.ProfileImage.Url;
        LastMessage = useRecipient ? "" : message.Content;
        LastMessageDate = message.DateSent;
        WasLastMessageRead = message.DateRead != null;
    }
}