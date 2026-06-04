using Balakhare.Core.Enums;

namespace Balakhare.Core.Entities;

public class Chat
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public ChatType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
