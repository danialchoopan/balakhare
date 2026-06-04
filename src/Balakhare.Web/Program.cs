using Balakhare.Core.Entities;
using Balakhare.Core.Enums;
using Balakhare.Infrastructure.Data;
using Balakhare.Infrastructure.Services;
using Balakhare.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SQLite";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    switch (dbProvider.ToLower())
    {
        case "sqlserver":
            options.UseSqlServer(connectionString);
            break;
        case "postgresql":
            options.UseNpgsql(connectionString);
            break;
        case "sqlite":
        default:
            options.UseSqlite(connectionString ?? "Data Source=balakhare.db");
            break;
    }
});

// Services
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ILinkPreviewService, LinkPreviewService>();
builder.Services.AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

// Seed data and Ensure DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DataSeeder.Seed(db);
}

app.Run();
