using System;
using System.Collections.Generic;
using System.Linq;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Detects software descriptions polluted with MobyGames page chrome (sidebar links,
///     screenshots grid, review tables, custom &lt;moby&gt; tags) leaked by the legacy
///     description parser's runaway sibling walk. The markers below cannot appear in a
///     correctly parsed description, which stops at the section boundary and strips
///     anchors/images.
/// </summary>
public static class DescriptionCorruptionDetector
{
    public static readonly string[] Markers =
    [
        "sideBarLinks",
        "<moby",
        "[edit description]",
        "reviewList",
        "<h2>Screenshots",
        "## Screenshots",
        "<h2>User Reviews",
        "## User Reviews",
        "add promo images",
        "[add screenshots]",
        "[add trivia]",
        "[review game]",
        "otherGameAttribution"
    ];

    public static bool IsCorrupted(string text, string html) => MatchingMarkers(text, html).Any();

    /// <summary>Returns the markers found in either column, for per-marker audit counts.</summary>
    public static IEnumerable<string> MatchingMarkers(string text, string html) =>
        Markers.Where(m => text?.Contains(m, StringComparison.Ordinal) == true ||
                           html?.Contains(m, StringComparison.Ordinal) == true);
}
