# مستندات فنی پلتفرم بلاخره

این سند شامل جزئیات فنی مربوط به ساختار پایگاه داده و متدهای هاب ارتباطی سیستم پیام‌رسان بلاخره است.

## ساختار پایگاه داده (Database Schema)

سیستم از جداول زیر برای مدیریت داده‌ها استفاده می‌کند:

### ۱. جدول کاربران (Users)
- Id: شناسه منحصر‌به‌فرد
- Username: نام کاربری یکتا
- FullName: نام کامل نمایش داده شده
- Bio: بیوگرافی کوتاه کاربر
- ProfilePicturePath: مسیر ذخیره‌سازی تصویر پروفایل
- IsOnline: وضعیت آنلاین بودن
- CreatedAt: زمان ثبت‌نام
- LastSeen: زمان آخرین بازدید

### ۲. جدول گفتگوها (Chats)
- Id: شناسه گفتگو
- Title: عنوان (برای گروه‌ها و کانال‌ها)
- Type: نوع گفتگو (PV, Group, Channel, PublicChatroom)
- CreatedAt: زمان ایجاد گفتگو

### ۳. جدول اعضای گفتگو (ChatMembers)
- ChatId: شناسه گفتگو
- UserId: شناسه کاربر
- IsAdmin: وضعیت مدیریت
- JoinedAt: زمان عضویت

### ۴. جدول پیام‌ها (Messages)
- Id: شناسه پیام
- ChatId: شناسه گفتگوی مربوطه
- SenderId: شناسه فرستنده
- Content: متن پیام
- FilePath: مسیر فایل پیوست
- FileName: نام فایل
- SentAt: زمان ارسال
- IsRead: وضعیت خوانده شدن
- IsPinned: آیا پیام سنجاق شده است؟
- ParentMessageId: شناسه پیام اصلی (برای قابلیت پاسخ)
- ForwardedFromUserId: شناسه فرستنده اصلی (برای فوروارد)
- LinkPreviewTitle/Description/ImageUrl/Url: اطلاعات پیش‌نمایش لینک

### ۵. جدول واکنش‌ها (MessageReactions)
- Id: شناسه واکنش
- MessageId: شناسه پیام مربوطه
- UserId: شناسه کاربر واکنش‌دهنده
- ReactionType: نوع واکنش (مثلاً Like)
- CreatedAt: زمان ثبت واکنش

---

## هاب سیگنال‌آر (SignalR Hub)

### متدهای جدید و پیشرفته

- SendMessage(... parentMessageId, forwardedFromUserId):
  ارسال پیام با قابلیت پاسخ به پیام دیگر یا فوروارد کردن.

- PinMessage(chatId, messageId):
  سنجاق کردن یک پیام خاص در گفتگو.

- AddReaction(messageId, userId, reactionType):
  ثبت یا حذف واکنش (لایک) روی یک پیام.

- JoinChat(chatId, userId):
  ورود امن به گفتگو و دریافت اطلاعات اولیه مانند پیام سنجاق شده.

### رویدادهای جدید کلاینت

- MessagePinned: دریافت اطلاعات پیام سنجاق شده جدید.
- UpdateReactions: دریافت لیست به‌روزرسانی شده واکنش‌های یک پیام.
- ReceiveMessage: دریافت پیام شامل اطلاعات پاسخ، فوروارد و پیش‌نمایش لینک.
