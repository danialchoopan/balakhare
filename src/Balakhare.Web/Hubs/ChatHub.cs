using Balakhare.Core.Entities;
using Balakhare.Core.Enums;
using Balakhare.Infrastructure.Data;
using Balakhare.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Balakhare.Web.Hubs;

public class ChatHub : Hub
{
    private readonly AppDbContext _context;
    private readonly ILinkPreviewService _linkPreviewService;

    public ChatHub(AppDbContext context, ILinkPreviewService linkPreviewService)
    {
        _context = context;
        _linkPreviewService = linkPreviewService;
    }

    public async Task SendMessage(int chatId, int senderId, string content, string? filePath = null, string? fileName = null, int? parentMessageId = null, int? forwardedFromUserId = null)
    {
        var isMember = await _context.ChatMembers.AnyAsync(cm => cm.ChatId == chatId && cm.UserId == senderId);
        var chat = await _context.Chats.FindAsync(chatId);

        if (!isMember && chat?.Type != ChatType.PublicChatroom)
        {
            throw new HubException("شما عضو این گفتگو نیستید.");
        }

        var message = new ChatMessage
        {
            ChatId = chatId,
            SenderId = senderId,
            Content = content,
            FilePath = filePath,
            FileName = fileName,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            ParentMessageId = parentMessageId,
            ForwardedFromUserId = forwardedFromUserId
        };

        // Enrich with Link Preview
        await _linkPreviewService.EnrichWithPreviewAsync(message);

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var sender = await _context.Users.FindAsync(senderId);
        var parent = parentMessageId.HasValue ? await _context.Messages.FindAsync(parentMessageId) : null;

        await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", new
        {
            Id = message.Id,
            ChatId = message.ChatId,
            SenderId = senderId,
            SenderName = sender?.FullName ?? "Unknown",
            Content = content,
            FilePath = filePath,
            FileName = fileName,
            SentAt = message.SentAt,
            ParentMessageId = parentMessageId,
            ParentContent = parent?.Content,
            ForwardedFromUserName = forwardedFromUserId.HasValue ? (await _context.Users.FindAsync(forwardedFromUserId))?.FullName : null,
            LinkPreview = new {
                Title = message.LinkPreviewTitle,
                Description = message.LinkPreviewDescription,
                ImageUrl = message.LinkPreviewImageUrl,
                Url = message.LinkPreviewUrl
            }
        });
    }

    public async Task PinMessage(int chatId, int messageId)
    {
        var chat = await _context.Chats.Include(c => c.Messages).FirstOrDefaultAsync(c => c.Id == chatId);
        if (chat == null) return;

        // Reset other pins in this chat (if only one is allowed) or just set this one
        var messages = await _context.Messages.Where(m => m.ChatId == chatId).ToListAsync();
        foreach (var m in messages) m.IsPinned = (m.Id == messageId);

        await _context.SaveChangesAsync();

        var pinnedMsg = messages.First(m => m.Id == messageId);
        await Clients.Group(chatId.ToString()).SendAsync("MessagePinned", new
        {
            MessageId = messageId,
            Content = pinnedMsg.Content?.Length > 50 ? pinnedMsg.Content.Substring(0, 50) + "..." : pinnedMsg.Content
        });
    }

    public async Task AddReaction(int messageId, int userId, string reactionType)
    {
        var existing = await _context.MessageReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);

        if (existing != null)
        {
            if (existing.ReactionType == reactionType)
            {
                _context.MessageReactions.Remove(existing);
            }
            else
            {
                existing.ReactionType = reactionType;
            }
        }
        else
        {
            _context.MessageReactions.Add(new MessageReaction
            {
                MessageId = messageId,
                UserId = userId,
                ReactionType = reactionType,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        var reactions = await _context.MessageReactions
            .Where(r => r.MessageId == messageId)
            .GroupBy(r => r.ReactionType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        var message = await _context.Messages.FindAsync(messageId);
        if (message != null)
        {
            await Clients.Group(message.ChatId.ToString()).SendAsync("UpdateReactions", new
            {
                MessageId = messageId,
                Reactions = reactions
            });
        }
    }

    public async Task JoinChat(int chatId, int userId)
    {
        var chat = await _context.Chats.FindAsync(chatId);
        var isMember = await _context.ChatMembers.AnyAsync(cm => cm.ChatId == chatId && cm.UserId == userId);

        if (isMember || (chat != null && chat.Type == ChatType.PublicChatroom))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());

            // Send pinned message if exists
            var pinned = await _context.Messages.FirstOrDefaultAsync(m => m.ChatId == chatId && m.IsPinned);
            if (pinned != null)
            {
                await Clients.Caller.SendAsync("MessagePinned", new
                {
                    MessageId = pinned.Id,
                    Content = pinned.Content?.Length > 50 ? pinned.Content.Substring(0, 50) + "..." : pinned.Content
                });
            }
        }
        else
        {
            throw new HubException("شما اجازه ورود به این گفتگو را ندارید.");
        }
    }

    public async Task LeaveChat(int chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task TrackTyping(int chatId, string fullName, bool isTyping)
    {
        await Clients.OthersInGroup(chatId.ToString()).SendAsync("UserTyping", new
        {
            ChatId = chatId,
            FullName = fullName,
            IsTyping = isTyping
        });
    }

    public async Task UpdateUserStatus(int userId, bool isOnline)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsOnline = isOnline;
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("UserStatusChanged", new { UserId = userId, IsOnline = isOnline });
        }
    }
}
