using System.Text.RegularExpressions;
using Balakhare.Core.Entities;

namespace Balakhare.Infrastructure.Services;

public interface ILinkPreviewService
{
    Task<bool> EnrichWithPreviewAsync(ChatMessage message);
}

public class LinkPreviewService : ILinkPreviewService
{
    private static readonly Regex UrlRegex = new Regex(@"(https?://[^\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public async Task<bool> EnrichWithPreviewAsync(ChatMessage message)
    {
        if (string.IsNullOrEmpty(message.Content)) return false;

        var match = UrlRegex.Match(message.Content);
        if (!match.Success) return false;

        var url = match.Value;
        message.LinkPreviewUrl = url;

        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var html = await client.GetStringAsync(url);

            // Simple Meta Scraping
            message.LinkPreviewTitle = GetMetaContent(html, "og:title") ?? GetTitle(html) ?? "پیش‌نمایش سایت";
            message.LinkPreviewDescription = GetMetaContent(html, "og:description") ?? GetMetaContent(html, "description");
            message.LinkPreviewImageUrl = GetMetaContent(html, "og:image");

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string? GetMetaContent(string html, string property)
    {
        var pattern = $"<meta [^>]*property=[\"']{property}[\"'] [^>]*content=[\"']([^\"']+)[\"']";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            pattern = $"<meta [^>]*name=[\"']{property}[\"'] [^>]*content=[\"']([^\"']+)[\"']";
            match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        }
        return match.Success ? System.Net.WebUtility.HtmlDecode(match.Groups[1].Value) : null;
    }

    private string? GetTitle(string html)
    {
        var match = Regex.Match(html, "<title>(.*?)</title>", RegexOptions.IgnoreCase);
        return match.Success ? System.Net.WebUtility.HtmlDecode(match.Groups[1].Value) : null;
    }
}
