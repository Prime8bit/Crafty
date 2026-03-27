namespace API.Entities;

public class Message
{
    public long Id { get; set; }
    public required string Content { get; set; }
    public DateTime? DateRead { get; set; }
    public required DateTime DateSent { get; set; }
    public bool SenderDeleted { get; set; } = false;
    public bool RecipientDeleted { get; set; } = false;

    // Nav Properties
    public long SenderId { get; set; }
    public User? Sender { get; set; }
    public long RecipientId { get; set; }
    public User? Recipient { get; set; }
}