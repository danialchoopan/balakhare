using Balakhare.Core.Entities;
using Balakhare.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Balakhare.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            user = new User
            {
                Username = username,
                FullName = username, // Default to username
                CreatedAt = DateTime.UtcNow, // Note: Added this if needed or just use default
                LastSeen = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return Ok(user);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.FullName,
                u.Bio,
                u.ProfilePicturePath,
                u.IsOnline
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] User updatedUser)
    {
        var user = await _context.Users.FindAsync(updatedUser.Id);
        if (user == null) return NotFound();

        user.FullName = updatedUser.FullName;
        user.Bio = updatedUser.Bio;
        user.ProfilePicturePath = updatedUser.ProfilePicturePath;

        await _context.SaveChangesAsync();
        return Ok(user);
    }
}
