using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers;

/// <summary>
///     Parses the new MobyGames screenshots page HTML (post-2024 redesign).
///     Structure:
///       &lt;div id="screenshots-platform-{mobyPlatformId}"&gt;
///         &lt;h2&gt;{PlatformName} screenshots&lt;/h2&gt;
///         &lt;div class="img-holder mb"&gt;
///           &lt;figure&gt;
///             &lt;a href="/game/{id}/{slug}/screenshots/{platformSlug}/{screenshotId}/"&gt;
///               &lt;img src="https://cdn.mobygames.com/{hash}.webp" class="img-thumbnail"&gt;
///             &lt;/a&gt;
///             &lt;figcaption&gt;&lt;small class="text-muted"&gt;{caption}&lt;/small&gt;&lt;/figcaption&gt;
///           &lt;/figure&gt;
///           ...
///         &lt;/div&gt;
///       &lt;/div&gt;
/// </summary>
public static partial class ScreenshotsPageParser
{
    public static List<ParsedScreenshotGroup> Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return Parse(doc);
    }

    public static List<ParsedScreenshotGroup> Parse(HtmlDocument doc)
    {
        var groups = new List<ParsedScreenshotGroup>();

        // Find all platform sections: <div id="screenshots-platform-{id}">
        var platformDivs = doc.DocumentNode.SelectNodes("//div[starts-with(@id, 'screenshots-platform-')]");

        if(platformDivs is null) return groups;

        foreach(var platformDiv in platformDivs)
        {
            string divId       = platformDiv.GetAttributeValue("id", "");
            var    platformMatch = PlatformIdRegex().Match(divId);
            string mobyPlatformId = platformMatch.Success ? platformMatch.Groups[1].Value : null;

            // Extract platform name from <h2> heading, stripping " screenshots" suffix
            var heading = platformDiv.SelectSingleNode(".//h2");

            if(heading is null) continue;

            string platformName = WebUtility.HtmlDecode(heading.InnerText).Trim();

            if(platformName.EndsWith(" screenshots"))
                platformName = platformName[..^" screenshots".Length];

            var group = new ParsedScreenshotGroup
            {
                PlatformName   = platformName,
                MobyPlatformId = mobyPlatformId
            };

            // Find all <figure> elements within the img-holder div
            var figures = platformDiv.SelectNodes(".//div[contains(@class, 'img-holder')]//figure");

            if(figures is null) continue;

            foreach(var figure in figures)
            {
                // Detail page URL from <a> element
                var link = figure.SelectSingleNode(".//a[@href]");

                if(link is null) continue;

                string href = link.GetAttributeValue("href", "");

                // Extract screenshot ID from URL (last numeric segment)
                var    idMatch     = ScreenshotIdRegex().Match(href);
                string screenshotId = idMatch.Success ? idMatch.Groups[1].Value : null;

                if(screenshotId is null) continue;

                // Skip duplicates
                if(group.Images.Any(i => i.ScreenshotId == screenshotId)) continue;

                // Thumbnail URL from <img class="img-thumbnail">
                var    img          = figure.SelectSingleNode(".//img[contains(@class, 'img-thumbnail')]");
                string thumbnailUrl = img?.GetAttributeValue("src", null);

                // Caption from <figcaption><small class="text-muted">
                var    captionNode = figure.SelectSingleNode(".//figcaption//small[contains(@class, 'text-muted')]");
                string caption     = captionNode is not null
                                         ? WebUtility.HtmlDecode(captionNode.InnerText).Trim()
                                         : null;

                group.Images.Add(new ParsedScreenshotImage
                {
                    ScreenshotId  = screenshotId,
                    DetailPageUrl = href,
                    Caption       = caption,
                    ThumbnailUrl  = thumbnailUrl
                });
            }

            if(group.Images.Count > 0)
                groups.Add(group);
        }

        return groups;
    }

    [GeneratedRegex(@"screenshots-platform-(\d+)", RegexOptions.Compiled)]
    private static partial Regex PlatformIdRegex();

    [GeneratedRegex(@"/(\d+)/?$", RegexOptions.Compiled)]
    private static partial Regex ScreenshotIdRegex();
}
