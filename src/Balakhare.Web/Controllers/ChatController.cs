using Balakhare.Core.Entities;
using Balakhare.Core.Enums;
using Balakhare.Infrastructure.Data;
using Balakhare.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Balakhare.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IFileService _fileService;

    public ChatController(AppDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetMessages(int chatId, int? userId = null, string? query = null)
    {
        if (userId.HasValue)
        {
            var unreadMessages = await _context.Messages
                .Where(m => m.ChatId == chatId && !m.IsRead && m.SenderId != userId.Value)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var m in unreadMessages) m.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        var messagesQuery = _context.Messages
            .Where(m => m.ChatId == chatId);

        if (!string.IsNullOrEmpty(query))
        {
            messagesQuery = messagesQuery.Where(m => m.Content != null && m.Content.Contains(query));
        }

        var messages = await messagesQuery
            .OrderBy(m => m.SentAt)
            .Include(m => m.Sender)
            .Include(m => m.Reactions)
            .Select(m => new
            {
                m.Id,
                m.Content,
                m.FilePath,
                m.FileName,
                m.SentAt,
                SenderName = m.Sender != null ? m.Sender.FullName : "سیستم",
                m.SenderId,
                m.ParentMessageId,
                ParentContent = m.ParentMessage != null ? m.ParentMessage.Content : null,
                m.IsPinned,
                Reactions = m.Reactions.GroupBy(r => r.ReactionType).Select(g => new { g.Key, Count = g.Count() }),
                LinkPreview = new { m.LinkPreviewTitle, m.LinkPreviewDescription, m.LinkPreviewImageUrl, m.LinkPreviewUrl }
            })
            .ToListAsync();

        return Ok(messages);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        try
        {
            var path = await _fileService.SaveFileAsync(file);
            return Ok(new { FilePath = path, FileName = file.FileName, FileSize = file.Length });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetChats(int userId)
    {
        var chatMembers = await _context.ChatMembers
            .Where(cm => cm.UserId == userId)
            .Include(cm => cm.Chat)
            .ToListAsync();

        var userChats = new List<object>();

        foreach (var cm in chatMembers)
        {
            var lastMessage = await _context.Messages
                .Where(m => m.ChatId == cm.ChatId)
                .OrderByDescending(m => m.SentAt)
                .Select(m => m.Content)
                .FirstOrDefaultAsync() ?? "بدون پیام";

            var unreadCount = await _context.Messages
                .Where(m => m.ChatId == cm.ChatId && !m.IsRead && m.SenderId != userId)
                .CountAsync();

            userChats.Add(new
            {
                cm.Chat.Id,
                cm.Chat.Title,
                cm.Chat.Type,
                LastMessage = lastMessage,
                UnreadCount = unreadCount
            });
        }

        var publicChatEntity = await _context.Chats
            .FirstOrDefaultAsync(c => c.Type == ChatType.PublicChatroom);

        if (publicChatEntity != null && !chatMembers.Any(cm => cm.ChatId == publicChatEntity.Id))
        {
            var lastMessage = await _context.Messages
                .Where(m => m.ChatId == publicChatEntity.Id)
                .OrderByDescending(m => m.SentAt)
                .Select(m => m.Content)
                .FirstOrDefaultAsync() ?? "بدون پیام";

            userChats.Add(new
            {
                publicChatEntity.Id,
                publicChatEntity.Title,
                publicChatEntity.Type,
                LastMessage = lastMessage,
                UnreadCount = 0
            });
        }

        return Ok(userChats);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateChat([FromBody] ChatRequest request)
    {
        var chat = new Chat
        {
            Title = request.Title,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow
        };

        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        if (request.UserIds != null)
        {
            foreach (var userId in request.UserIds)
            {
                _context.ChatMembers.Add(new ChatMember
                {
                    ChatId = chat.Id,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();
        }

        return Ok(chat);
    }
}

public class ChatRequest
{
    public string? Title { get; set; }
    public ChatType Type { get; set; }
    public List<int>? UserIds { get; set; }
}
