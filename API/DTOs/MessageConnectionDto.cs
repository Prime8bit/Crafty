using System.Diagnostics.CodeAnalysis;
using API.Entities;

namespace API.DTOs;

public class MessageConnectionDto
{    
    // This will store the HubCallerContext ConnectionId, so it must be a string
    public required string Id { get; set; }
    public required string MessageGroupName { get; set;}
    public required long UserId { get; set; }

    [SetsRequiredMembers]
    public MessageConnectionDto(MessageConnection messageConnection)
    {
        Id = messageConnection.Id;
        MessageGroupName = messageConnection.MessageGroupName;
        UserId = messageConnection.UserId;
    }
}