using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers;

/// <summary>
///     Parses the MobyGames media tab page to extract embedded video information.
///     Extracts YouTube video IDs from lazyframe divs, stripping all referrer/affiliate parameters.
/// </summary>
public static partial class MediaPageParser
{
    public static List<ParsedVideo> Parse(string html)
    {
        var results = new List<ParsedVideo>();
        var doc     = new HtmlDocument();
        doc.LoadHtml(html);

        // MobyGames embeds videos as <div class="lazyframe" data-vendor="youtube" data-src="..." data-title="...">
        var lazyframes = doc.DocumentNode.SelectNodes("//div[contains(@class,'lazyframe')]");

        if(lazyframes is null) return results;

        foreach(var node in lazyframes)
        {
            string vendor  = node.GetAttributeValue("data-vendor", "");
            string dataSrc = node.GetAttributeValue("data-src", "");
            string title   = node.GetAttributeValue("data-title", "");

            if(string.IsNullOrWhiteSpace(dataSrc)) continue;

            string provider = null;
            string videoId  = null;

            if(vendor.Equals("youtube", StringComparison.OrdinalIgnoreCase) ||
               dataSrc.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
               dataSrc.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
            {
                provider = "YouTube";
                videoId  = ExtractYouTubeVideoId(dataSrc);
            }

            if(provider is null || videoId is null) continue;

            results.Add(new ParsedVideo
            {
                Provider = provider,
                VideoId  = videoId,
                Title    = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
                EmbedUrl = dataSrc
            });
        }

        return results;
    }

    static string ExtractYouTubeVideoId(string url)
    {
        // Strip query parameters and fragments first
        int queryIndex = url.IndexOf('?');
        if(queryIndex >= 0) url = url[..queryIndex];

        int fragmentIndex = url.IndexOf('#');
        if(fragmentIndex >= 0) url = url[..fragmentIndex];

        url = url.TrimEnd('/');

        // Match /embed/{id} or /v/{id} or /watch/{id} patterns
        var match = YoutubeEmbedRegex().Match(url);

        if(match.Success) return match.Groups[1].Value;

        // Match youtu.be/{id}
        match = YoutubeShortRegex().Match(url);

        if(match.Success) return match.Groups[1].Value;

        return null;
    }

    [GeneratedRegex(@"youtube\.com/(?:embed|v|watch)/([a-zA-Z0-9_-]{11})", RegexOptions.IgnoreCase)]
    private static partial Regex YoutubeEmbedRegex();

    [GeneratedRegex(@"youtu\.be/([a-zA-Z0-9_-]{11})", RegexOptions.IgnoreCase)]
    private static partial Regex YoutubeShortRegex();
}
