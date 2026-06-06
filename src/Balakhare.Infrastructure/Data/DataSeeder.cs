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
            new User { Username = "admin", FullName = "مدیر سیستم", Bio = "مدیریت کل پلتفرم بلاخره", IsAdmin = true, CreatedAt = DateTime.UtcNow.AddMonths(-1) },
            new User { Username = "reza", FullName = "رضا محمدی", Bio = "عاشق برنامه‌نویسی و تکنولوژی", CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new User { Username = "sara", FullName = "سارا احمدی", Bio = "طراح رابط کاربری", CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new User { Username = "ali", FullName = "علی رضایی", Bio = "برنامه‌نویس بک‌اند", CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new User { Username = "maryam", FullName = "مریم پاکزاد", Bio = "متخصص شبکه", CreatedAt = DateTime.UtcNow.AddDays(-8) },
            new User { Username = "hosein", FullName = "حسین علوی", Bio = "تحلیلگر داده", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new User { Username = "negar", FullName = "نگار جواهری", Bio = "نویسنده محتوا", CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new User { Username = "omid", FullName = "امید ناصری", Bio = "توسعه‌دهنده موبایل", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new User { Username = "zohre", FullName = "زهره کریمی", Bio = "تستر نرم‌افزار", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new User { Username = "amin", FullName = "امین شریفی", Bio = "مدیر پروژه", CreatedAt = DateTime.UtcNow.AddMinutes(-30) }
        };
        context.Users.AddRange(users);
        context.SaveChanges();

        var admin = users[0];
        var reza = users[1];
        var sara = users[2];
        var ali = users[3];

        // Public Chatroom
        var publicChat = new Chat { Title = "اتاق گفتگو عمومی", Type = ChatType.PublicChatroom, CreatedAt = DateTime.UtcNow.AddMonths(-1) };
        context.Chats.Add(publicChat);
        context.SaveChanges();

        // Groups
        var teamGroup = new Chat { Title = "تیم توسعه بلاخره", Type = ChatType.Group, CreatedAt = DateTime.UtcNow.AddDays(-20) };
        var designGroup = new Chat { Title = "کارگروه طراحی", Type = ChatType.Group, CreatedAt = DateTime.UtcNow.AddDays(-15) };
        context.Chats.AddRange(teamGroup, designGroup);
        context.SaveChanges();

        foreach(var u in users) context.ChatMembers.Add(new ChatMember { ChatId = teamGroup.Id, UserId = u.Id, JoinedAt = DateTime.UtcNow.AddDays(-20) });
        context.ChatMembers.Add(new ChatMember { ChatId = designGroup.Id, UserId = sara.Id, IsAdmin = true, JoinedAt = DateTime.UtcNow.AddDays(-15) });
        context.ChatMembers.Add(new ChatMember { ChatId = designGroup.Id, UserId = reza.Id, JoinedAt = DateTime.UtcNow.AddDays(-14) });

        // Channels
        var newsChannel = new Chat { Title = "اخبار تکنولوژی", Type = ChatType.Channel, CreatedAt = DateTime.UtcNow.AddDays(-10) };
        var sportsChannel = new Chat { Title = "دنیای ورزش", Type = ChatType.Channel, CreatedAt = DateTime.UtcNow.AddDays(-5) };
        context.Chats.AddRange(newsChannel, sportsChannel);
        context.SaveChanges();

        context.ChatMembers.Add(new ChatMember { ChatId = newsChannel.Id, UserId = admin.Id, IsAdmin = true, JoinedAt = DateTime.UtcNow.AddDays(-10) });
        context.ChatMembers.Add(new ChatMember { ChatId = sportsChannel.Id, UserId = admin.Id, IsAdmin = true, JoinedAt = DateTime.UtcNow.AddDays(-5) });

        // Private Chat
        var pvChat = new Chat { Title = "سارا احمدی", Type = ChatType.PV, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        context.Chats.Add(pvChat);
        context.SaveChanges();
        context.ChatMembers.Add(new ChatMember { ChatId = pvChat.Id, UserId = admin.Id, JoinedAt = DateTime.UtcNow.AddDays(-1) });
        context.ChatMembers.Add(new ChatMember { ChatId = pvChat.Id, UserId = sara.Id, JoinedAt = DateTime.UtcNow.AddDays(-1) });

        // Messages
        context.Messages.AddRange(
            new ChatMessage { ChatId = publicChat.Id, SenderId = reza.Id, Content = "سلام، کسی اینجا هست؟", SentAt = DateTime.UtcNow.AddHours(-10), IsRead = true },
            new ChatMessage { ChatId = publicChat.Id, SenderId = sara.Id, Content = "سلام رضا جان، بله من هستم.", SentAt = DateTime.UtcNow.AddHours(-9), IsRead = true },
            new ChatMessage { ChatId = teamGroup.Id, SenderId = admin.Id, Content = "دوستان لطفاً گزارش‌های هفتگی را ارسال کنید.", IsPinned = true, SentAt = DateTime.UtcNow.AddHours(-8), IsRead = true },
            new ChatMessage { ChatId = teamGroup.Id, SenderId = ali.Id, Content = "بله حتماً تا پایان امروز آماده می‌شود.", SentAt = DateTime.UtcNow.AddHours(-7), IsRead = true },
            new ChatMessage { ChatId = newsChannel.Id, SenderId = admin.Id, Content = "هوش مصنوعی جایگزین برنامه‌نویسان نخواهد شد! این یک واقعیت است که ابزارها فقط به ما کمک می‌کنند.", SentAt = DateTime.UtcNow.AddHours(-6), IsRead = true },
            new ChatMessage { ChatId = newsChannel.Id, SenderId = admin.Id, Content = "لینک خبر: https://tech-news.com/ai-and-coding", LinkPreviewTitle = "AI will not replace coders", LinkPreviewDescription = "Experts believe AI will be a tool, not a replacement for human logic.", LinkPreviewUrl = "https://tech-news.com/ai-and-coding", SentAt = DateTime.UtcNow.AddHours(-5), IsRead = true },
            new ChatMessage { ChatId = designGroup.Id, SenderId = sara.Id, Content = "نظرتون در مورد پالت رنگی جدید چیه؟", SentAt = DateTime.UtcNow.AddHours(-4), IsRead = true },
            new ChatMessage { ChatId = publicChat.Id, SenderId = users[5].Id, Content = "بلاخره واقعاً پیام‌رسان سریع و خوبیه.", SentAt = DateTime.UtcNow.AddHours(-3), IsRead = true },
            new ChatMessage { ChatId = pvChat.Id, SenderId = sara.Id, Content = "سلام ادمین عزیز، طرح‌های جدید آماده است.", SentAt = DateTime.UtcNow.AddHours(-2), IsRead = false },
            new ChatMessage { ChatId = teamGroup.Id, SenderId = reza.Id, Content = "من کدها را پوش کردم.", SentAt = DateTime.UtcNow.AddHours(-1), IsRead = false }
        );

        context.SaveChanges();

        // Reactions
        var firstMsg = context.Messages.First();
        context.MessageReactions.Add(new MessageReaction { MessageId = firstMsg.Id, UserId = sara.Id, ReactionType = "Like", CreatedAt = DateTime.UtcNow.AddHours(-9) });
        context.MessageReactions.Add(new MessageReaction { MessageId = firstMsg.Id, UserId = ali.Id, ReactionType = "Like", CreatedAt = DateTime.UtcNow.AddHours(-8) });

        context.SaveChanges();
    }
}
