using Balakhare.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Balakhare.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = new
        {
            UserCount = await _context.Users.CountAsync(),
            MessageCount = await _context.Messages.CountAsync(),
            ChatCount = await _context.Chats.CountAsync(),
            ReactionCount = await _context.MessageReactions.CountAsync()
        };
        return Ok(stats);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new { u.Id, u.Username, u.FullName, u.IsAdmin, u.IsBlocked, u.CreatedAt })
            .ToListAsync();
        return Ok(users);
    }

    [HttpPost("users/{id}/block")]
    public async Task<IActionResult> ToggleBlock(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.IsBlocked = !user.IsBlocked;
        await _context.SaveChangesAsync();
        return Ok(user);
    }

    [HttpDelete("chats/{id}")]
    public async Task<IActionResult> DeleteChat(int id)
    {
        var chat = await _context.Chats.FindAsync(id);
        if (chat == null) return NotFound();
        _context.Chats.Remove(chat);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
