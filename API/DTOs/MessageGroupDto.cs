using System.Diagnostics.CodeAnalysis;
using API.Entities;

namespace API.DTOs;

public class MessageGroupDto
{
    public required string Name { get; set; }
    public IEnumerable<MessageConnectionDto> Connections { get; set; } = [];

    [SetsRequiredMembers]
    public MessageGroupDto(MessageGroup messageGroup)
    {
        Name = messageGroup.Name;
        Connections = messageGroup.Connections.Select(messageConnection => new MessageConnectionDto(messageConnection));
    }
}