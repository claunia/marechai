using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers.NewSite;

/// <summary>
///     Detects which media sub-pages (screenshots, promo art, covers, videos,
///     reviews) are linked from a new-site MobyGames main page.
/// </summary>
/// <remarks>
///     The new layout no longer exposes the legacy `&lt;ul class="nav-tabs"&gt;`
///     navigation. Sub-page availability is inferred from anchors of the form
///     `&lt;a href="/game/{id}/{slug}/{sub}/"&gt;` scattered through the page
///     (the "Game info" / "Reviews" / etc. links in the right-rail and inline
///     in the body). This helper does cheap regex scans rather than a full
///     <see cref="MainTabParser" /> pass so the media scrapers can decide
///     whether to enqueue a fetch in O(1) per row.
/// </remarks>
public static partial class MediaPresenceDetector
{
    [GeneratedRegex(@"href=""/game/(\d+)/([^/""]+)/screenshots/""", RegexOptions.IgnoreCase)]
    private static partial Regex ScreenshotsRegex();

    [GeneratedRegex(@"href=""/game/(\d+)/([^/""]+)/promo/""", RegexOptions.IgnoreCase)]
    private static partial Regex PromoRegex();

    [GeneratedRegex(@"href=""/game/(\d+)/([^/""]+)/covers/""", RegexOptions.IgnoreCase)]
    private static partial Regex CoversRegex();

    // The current site links the tab as /media/; older captures used /video/ or /videos/.
    [GeneratedRegex(@"href=""/game/(\d+)/([^/""]+)/(?:media|videos?)/""", RegexOptions.IgnoreCase)]
    private static partial Regex MediaRegex();

    [GeneratedRegex(@"href=""/game/(\d+)/([^/""]+)/reviews/""", RegexOptions.IgnoreCase)]
    private static partial Regex ReviewsRegex();

    [GeneratedRegex(@"/game/(\d+)/([^/""]+)/", RegexOptions.IgnoreCase)]
    private static partial Regex AnyGameAnchorRegex();

    public static bool HasScreenshots(string html) => html != null && ScreenshotsRegex().IsMatch(html);
    public static bool HasPromoArt(string html)    => html != null && PromoRegex().IsMatch(html);
    public static bool HasCoverArt(string html)    => html != null && CoversRegex().IsMatch(html);
    public static bool HasMedia(string html)       => html != null && MediaRegex().IsMatch(html);
    public static bool HasReviews(string html)     => html != null && ReviewsRegex().IsMatch(html);

    /// <summary>
    ///     Returns the most-referenced <c>(numericId, slug)</c> pair in the page,
    ///     which corresponds to the game itself rather than any linked sibling
    ///     (compilation contents, related games, etc.). Returns <c>null</c> if no
    ///     <c>/game/{id}/{slug}/</c> reference is found.
    /// </summary>
    public static (int Id, string Slug)? ExtractGameIdAndSlug(string html)
    {
        if(string.IsNullOrEmpty(html)) return null;

        var counts = new Dictionary<(int, string), int>();

        foreach(Match m in AnyGameAnchorRegex().Matches(html))
        {
            if(!int.TryParse(m.Groups[1].Value, out int id)) continue;

            string slug = m.Groups[2].Value;
            var    key  = (id, slug);

            counts.TryGetValue(key, out int current);
            counts[key] = current + 1;
        }

        if(counts.Count == 0) return null;

        (int Id, string Slug)? best       = null;
        int                    bestCount  = -1;

        foreach(var kv in counts)
        {
            if(kv.Value > bestCount)
            {
                bestCount = kv.Value;
                best      = kv.Key;
            }
        }

        return best;
    }
}
