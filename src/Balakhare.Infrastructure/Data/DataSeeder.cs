using Balakhare.Core.Entities;
using Balakhare.Core.Enums;
using Balakhare.Infrastructure.Data;

namespace Balakhare.Infrastructure.Data;

public static class DataSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any()) return;

        // Seed Users
        var admin = new User { Username = "admin", FullName = "مدیر سیستم", IsAdmin = true, CreatedAt = DateTime.UtcNow };
        var user1 = new User { Username = "reza", FullName = "رضا محمدی", CreatedAt = DateTime.UtcNow };
        var user2 = new User { Username = "sara", FullName = "سارا احمدی", CreatedAt = DateTime.UtcNow };

        context.Users.AddRange(admin, user1, user2);
        context.SaveChanges();

        // Seed Public Chatroom
        var publicChat = context.Chats.FirstOrDefault(c => c.Type == ChatType.PublicChatroom);
        if (publicChat == null)
        {
            publicChat = new Chat { Title = "اتاق گفتگو عمومی", Type = ChatType.PublicChatroom, CreatedAt = DateTime.UtcNow };
            context.Chats.Add(publicChat);
            context.SaveChanges();
        }

        // Seed a Group
        var teamGroup = new Chat { Title = "تیم توسعه بلاخره", Type = ChatType.Group, CreatedAt = DateTime.UtcNow };
        context.Chats.Add(teamGroup);
        context.SaveChanges();

        context.ChatMembers.AddRange(
            new ChatMember { ChatId = teamGroup.Id, UserId = admin.Id, IsAdmin = true, JoinedAt = DateTime.UtcNow },
            new ChatMember { ChatId = teamGroup.Id, UserId = user1.Id, JoinedAt = DateTime.UtcNow },
            new ChatMember { ChatId = teamGroup.Id, UserId = user2.Id, JoinedAt = DateTime.UtcNow }
        );

        // Seed a Channel
        var newsChannel = new Chat { Title = "اخبار تکنولوژی", Type = ChatType.Channel, CreatedAt = DateTime.UtcNow };
        context.Chats.Add(newsChannel);
        context.SaveChanges();

        context.ChatMembers.Add(new ChatMember { ChatId = newsChannel.Id, UserId = admin.Id, IsAdmin = true, JoinedAt = DateTime.UtcNow });

        // Seed Messages
        context.Messages.AddRange(
            new ChatMessage { ChatId = publicChat.Id, SenderId = user1.Id, Content = "سلام به همه!", SentAt = DateTime.UtcNow.AddMinutes(-10) },
            new ChatMessage { ChatId = publicChat.Id, SenderId = user2.Id, Content = "سلام رضا، خوش آمدی", SentAt = DateTime.UtcNow.AddMinutes(-9) },
            new ChatMessage { ChatId = teamGroup.Id, SenderId = admin.Id, Content = "جلسه فردا ساعت ۱۰ صبح برگزار می‌شود.", SentAt = DateTime.UtcNow.AddMinutes(-5) },
            new ChatMessage { ChatId = newsChannel.Id, SenderId = admin.Id, Content = "نسخه جدید بلاخره منتشر شد!", SentAt = DateTime.UtcNow.AddMinutes(-2) }
        );

        context.SaveChanges();
    }
}
