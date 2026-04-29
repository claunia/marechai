using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Marechai.MobyGames.Parsers;

public static partial class CoverDetailPageParser
{
    /// <summary>
    ///     Parse a cover detail page to extract the original-resolution download URL.
    ///     When logged in with MobyPlus, the page contains a direct link to the original image.
    ///     When not logged in, the page shows "Upgrade to MobyPlus to unlock this feature!"
    /// </summary>
    /// <returns>Tuple of (originalUrl, dimensions) or (null, null) if not found</returns>
    public static (string originalUrl, string dimensions) Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        string originalUrl = null;
        string dimensions  = null;

        // Extract dimensions from "Download Original Cover: WxH" text
        var dimensionsMatch = DimensionsRegex().Match(html);

        if(dimensionsMatch.Success)
            dimensions = dimensionsMatch.Groups[1].Value;

        // Strategy 1: Look for a direct download link for MobyPlus users
        // When authenticated, the "Download Original Cover" section contains a direct <a> link
        var downloadLinks = doc.DocumentNode.SelectNodes("//a[contains(@href,'cdn.mobygames.com')]");

        if(downloadLinks is not null)
        {
            foreach(var link in downloadLinks)
            {
                string href = link.GetAttributeValue("href", "");

                if(href.Contains("/covers/") && !href.Contains("/s/"))
                {
                    originalUrl = href;

                    break;
                }
            }
        }

        // Strategy 2: Look for the main cover image tag — the large display image
        // The CDN URL in the <img> tag is the medium/display version
        // For original, MobyPlus may provide a different URL format
        if(originalUrl is null)
        {
            var mainImg = doc.DocumentNode.SelectSingleNode(
                "//img[contains(@src,'cdn.mobygames.com/covers/')]");

            if(mainImg is not null)
            {
                originalUrl = mainImg.GetAttributeValue("src", null);
            }
        }

        // Strategy 3: Search for any CDN cover URL in the page source
        if(originalUrl is null)
        {
            var cdnMatch = CdnCoverUrlRegex().Match(html);

            if(cdnMatch.Success)
                originalUrl = cdnMatch.Value;
        }

        return (originalUrl, dimensions);
    }

    [GeneratedRegex(@"Download Original Cover:\s*(\d+x\d+)", RegexOptions.Compiled)]
    private static partial Regex DimensionsRegex();

    [GeneratedRegex(@"https?://cdn\.mobygames\.com/covers/[^\s""'<>]+", RegexOptions.Compiled)]
    private static partial Regex CdnCoverUrlRegex();
}
