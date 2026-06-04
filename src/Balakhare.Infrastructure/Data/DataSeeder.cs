using Balakhare.Core.Entities;
using Balakhare.Core.Enums;
using Balakhare.Infrastructure.Data;

namespace Balakhare.Infrastructure.Data;

public static class DataSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any()) return;

        // Users
        var users = new List<User>
        {
            new User { Username = "admin", FullName = "مدیر سیستم", Bio = "مدیریت کل پلتفرم بلاخره", IsAdmin = true, CreatedAt = DateTime.UtcNow },
            new User { Username = "reza", FullName = "رضا محمدی", Bio = "عاشق برنامه‌نویسی و تکنولوژی", CreatedAt = DateTime.UtcNow },
            new User { Username = "sara", FullName = "سارا احمدی", Bio = "طراح رابط کاربری", CreatedAt = DateTime.UtcNow },
            new User { Username = "ali", FullName = "علی رضایی", Bio = "برنامه‌نویس بک‌اند", CreatedAt = DateTime.UtcNow },
            new User { Username = "maryam", FullName = "مریم پاکزاد", Bio = "متخصص شبکه", CreatedAt = DateTime.UtcNow },
            new User { Username = "hosein", FullName = "حسین علوی", Bio = "تحلیلگر داده", CreatedAt = DateTime.UtcNow },
            new User { Username = "negar", FullName = "نگار جواهری", Bio = "نویسنده محتوا", CreatedAt = DateTime.UtcNow },
            new User { Username = "omid", FullName = "امید ناصری", Bio = "توسعه‌دهنده موبایل", CreatedAt = DateTime.UtcNow },
            new User { Username = "zohre", FullName = "زهره کریمی", Bio = "تستر نرم‌افزار", CreatedAt = DateTime.UtcNow },
            new User { Username = "amin", FullName = "امین شریفی", Bio = "مدیر پروژه", CreatedAt = DateTime.UtcNow }
        };
        context.Users.AddRange(users);
        context.SaveChanges();

        var admin = users[0];
        var reza = users[1];
        var sara = users[2];

        // Public Chatroom
        var publicChat = new Chat { Title = "اتاق گفتگو عمومی", Type = ChatType.PublicChatroom, CreatedAt = DateTime.UtcNow };
        context.Chats.Add(publicChat);
        context.SaveChanges();

        // Groups
        var teamGroup = new Chat { Title = "تیم توسعه بلاخره", Type = ChatType.Group, CreatedAt = DateTime.UtcNow };
        var designGroup = new Chat { Title = "کارگروه طراحی", Type = ChatType.Group, CreatedAt = DateTime.UtcNow };
        context.Chats.AddRange(teamGroup, designGroup);
        context.SaveChanges();

        foreach(var u in users) context.ChatMembers.Add(new ChatMember { ChatId = teamGroup.Id, UserId = u.Id, JoinedAt = DateTime.UtcNow });
        context.ChatMembers.Add(new ChatMember { ChatId = designGroup.Id, UserId = sara.Id, IsAdmin = true, JoinedAt = DateTime.UtcNow });
        context.ChatMembers.Add(new ChatMember { ChatId = designGroup.Id, UserId = reza.Id, JoinedAt = DateTime.UtcNow });

        // Channels
        var newsChannel = new Chat { Title = "اخبار تکنولوژی", Type = ChatType.Channel, CreatedAt = DateTime.UtcNow };
        var sportsChannel = new Chat { Title = "دنیای ورزش", Type = ChatType.Channel, CreatedAt = DateTime.UtcNow };
        context.Chats.AddRange(newsChannel, sportsChannel);
        context.SaveChanges();

        context.ChatMembers.Add(new ChatMember { ChatId = newsChannel.Id, UserId = admin.Id, IsAdmin = true, JoinedAt = DateTime.UtcNow });
        context.ChatMembers.Add(new ChatMember { ChatId = sportsChannel.Id, UserId = admin.Id, IsAdmin = true, JoinedAt = DateTime.UtcNow });

        // Messages
        context.Messages.AddRange(
            new ChatMessage { ChatId = publicChat.Id, SenderId = reza.Id, Content = "سلام، کسی اینجا هست؟", SentAt = DateTime.UtcNow.AddHours(-5) },
            new ChatMessage { ChatId = publicChat.Id, SenderId = sara.Id, Content = "سلام رضا جان، بله من هستم.", SentAt = DateTime.UtcNow.AddHours(-4) },
            new ChatMessage { ChatId = teamGroup.Id, SenderId = admin.Id, Content = "دوستان لطفاً گزارش‌های هفتگی را ارسال کنید.", IsPinned = true, SentAt = DateTime.UtcNow.AddHours(-3) },
            new ChatMessage { ChatId = teamGroup.Id, SenderId = users[3].Id, Content = "بله حتماً تا پایان امروز آماده می‌شود.", SentAt = DateTime.UtcNow.AddHours(-2.5) },
            new ChatMessage { ChatId = newsChannel.Id, SenderId = admin.Id, Content = "هوش مصنوعی جایگزین برنامه‌نویسان نخواهد شد!", SentAt = DateTime.UtcNow.AddHours(-2) },
            new ChatMessage { ChatId = designGroup.Id, SenderId = sara.Id, Content = "نظرتون در مورد پالت رنگی جدید چیه؟", SentAt = DateTime.UtcNow.AddHours(-1.5) },
            new ChatMessage { ChatId = publicChat.Id, SenderId = users[5].Id, Content = "بلاخره واقعاً پیام‌رسان سریع و خوبیه.", SentAt = DateTime.UtcNow.AddHours(-1) }
        );

        // Reactions
        context.SaveChanges();
        var firstMsg = context.Messages.First();
        context.MessageReactions.Add(new MessageReaction { MessageId = firstMsg.Id, UserId = sara.Id, ReactionType = "Like", CreatedAt = DateTime.UtcNow });

        context.SaveChanges();
    }
}
