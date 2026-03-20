namespace API.Entities;

public class MessageConnection
{
    // This will store the HubCallerContext ConnectionId, so it must be a string
    public required string Id { get; set; }
    public required string MessageGroupName { get; set;}
    public required long UserId { get; set; }
}