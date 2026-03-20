namespace API.DTOs;

public class CreateMessageDto
{
    public required long RecipientId { get; set; }
    public required string Content { get; set; }
    public bool IsRead { get; set; } = false;
}