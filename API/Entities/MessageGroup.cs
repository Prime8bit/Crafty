using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class MessageGroup()
{
    [Key]
    public required string Name { get; set; }
    public ICollection<MessageConnection> Connections { get; set; } = [];

}