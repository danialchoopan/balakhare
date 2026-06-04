namespace Balakhare.Core.Entities;

public class MessageReaction
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public ChatMessage Message { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string ReactionType { get; set; } = string.Empty; // e.g., "Like", "Heart"
    public DateTime CreatedAt { get; set; }
}
