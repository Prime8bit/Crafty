namespace API.Entities;

public class Message
{
    public long Id { get; set; }
    public required string Content { get; set; }
    public DateTime? DateRead { get; set; }
    public DateTime DateSent { get; set; } = DateTime.UtcNow;
    public bool SenderDeleted { get; set; }
    public bool RecipientDeleted { get; set; }

    // Nav Properties
    public required long SenderId { get; set; }
    public User Sender { get; set; } = null!;
    public required long RecipientId { get; set; }
    public User Recipient { get; set; } = null!;
}