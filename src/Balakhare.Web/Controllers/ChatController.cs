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
    public async Task<IActionResult> GetMessages(int chatId, string? query = null)
    {
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
        var userChats = await _context.ChatMembers
            .Where(cm => cm.UserId == userId)
            .Include(cm => cm.Chat)
            .Select(cm => new
            {
                cm.Chat.Id,
                cm.Chat.Title,
                cm.Chat.Type,
                LastMessage = _context.Messages
                    .Where(m => m.ChatId == cm.ChatId)
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Content)
                    .FirstOrDefault() ?? "بدون پیام"
            })
            .ToListAsync();

        var publicChat = await _context.Chats
            .Where(c => c.Type == ChatType.PublicChatroom)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Type,
                LastMessage = _context.Messages
                    .Where(m => m.ChatId == c.Id)
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Content)
                    .FirstOrDefault() ?? "بدون پیام"
            })
            .FirstOrDefaultAsync();

        if (publicChat != null && !userChats.Any(c => c.Id == publicChat.Id))
        {
            userChats.Add(publicChat);
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
