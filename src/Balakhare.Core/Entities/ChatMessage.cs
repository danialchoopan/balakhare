namespace Balakhare.Core.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public Chat Chat { get; set; } = null!;
    public int? SenderId { get; set; }
    public User? Sender { get; set; }
    public string? Content { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }

    // Pin Feature
    public bool IsPinned { get; set; }

    // Reply Feature
    public int? ParentMessageId { get; set; }
    public ChatMessage? ParentMessage { get; set; }

    // Forward Feature
    public int? ForwardedFromUserId { get; set; }
    public User? ForwardedFromUser { get; set; }

    // Link Preview Feature
    public string? LinkPreviewTitle { get; set; }
    public string? LinkPreviewDescription { get; set; }
    public string? LinkPreviewImageUrl { get; set; }
    public string? LinkPreviewUrl { get; set; }

    // Reactions
    public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
}
