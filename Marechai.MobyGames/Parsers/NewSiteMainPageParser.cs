using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Marechai.MobyGames.Parsers;

public static partial class NewSiteMainPageParser
{
    /// <summary>
    ///     Labels that MobyGames uses for "this page is a child of another game" sidebar
    ///     blocks. The block markup is always
    ///     <c>&lt;div class="border border-1 mb flowroot"&gt;&lt;b&gt;{LABEL}&lt;/b&gt;
    ///     &lt;ul id="related*"&gt;&lt;li&gt;&lt;a href="/game/N/slug/"&gt;...&lt;/a&gt;&lt;/li&gt;...&lt;/ul&gt;&lt;/div&gt;</c>.
    ///     <list type="bullet">
    ///         <item><description>"Base Game" — proper DLC pages (e.g. Diablo IV: Lord of Hatred)</description></item>
    ///         <item><description>"Included in" — standalone-expansion / mod pages that ship inside a parent product (e.g. Darkest Hour: Europe '44-'45 → Red Orchestra: Ostfront 41-45)</description></item>
    ///         <item><description>"Expansion of" / "Standalone Expansion of" — older expansion-pack pages</description></item>
    ///         <item><description>"Mod of" — game-mod pages</description></item>
    ///         <item><description>"Requires" — booster packs / patch-style add-ons</description></item>
    ///     </list>
    ///     Values are ranks (lower wins). "Included in" ranks last because it also points DLCs at
    ///     the Season Pass compilation that bundles them, which is not a base game.
    /// </summary>
    static readonly Dictionary<string, int> BaseGameLabelRanks = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Base Game"]               = 0,
        ["Base Games"]              = 0,
        ["Expansion of"]            = 1,
        ["Standalone Expansion of"] = 1,
        ["Mod of"]                  = 2,
        ["Requires"]                = 3,
        ["Included in"]             = 4
    };

    /// <summary>Rank of the "Included in" label, the only non-strict parent link.</summary>
    const int IncludedInRank = 4;

    /// <summary>
    ///     Extracts the MobyGames numeric ID and slug of the base/parent game from a new-site
    ///     game page's HTML. See <see cref="ParseParentGame" /> for the label ranking.
    /// </summary>
    /// <param name="html">Raw HTML of the new MobyGames game page</param>
    /// <returns>Tuple of (numericId, slug), or (null, null) if not found</returns>
    public static (int? Id, string Slug) ParseBaseGame(string html)
    {
        (int? id, string slug, _) = ParseParentGame(html);

        return (id, slug);
    }

    /// <summary>
    ///     Extracts the parent game of a new-site game page. Walks every <c>&lt;ul id="related*"&gt;</c>
    ///     sidebar block whose preceding <c>&lt;b&gt;</c> label is in <see cref="BaseGameLabelRanks" /> and
    ///     returns the first <c>/game/N/slug/</c> link of the best-ranked block, so a DLC that is both
    ///     "Base Game: X" and "Included in: Season Pass" resolves to X. Falls back to the pre-existing
    ///     "Base Game"-only regex when the DOM parse finds nothing (legacy captures).
    /// </summary>
    /// <param name="html">Raw HTML of the new MobyGames game page</param>
    /// <returns>
    ///     (numericId, slug, isStrict) where <c>isStrict</c> is false when the only parent link is
    ///     "Included in" (the parent may be a compilation such as a Season Pass rather than a base game).
    /// </returns>
    public static (int? Id, string Slug, bool IsStrict) ParseParentGame(string html)
    {
        if(string.IsNullOrWhiteSpace(html)) return (null, null, false);

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var relatedLists = doc.DocumentNode.SelectNodes("//ul[starts-with(@id,'related')]");

            if(relatedLists is not null)
            {
                int    bestRank = int.MaxValue;
                int?   bestId   = null;
                string bestSlug = null;

                foreach(var ul in relatedLists)
                {
                    string label = GetBlockLabel(ul);

                    if(!BaseGameLabelRanks.TryGetValue(label, out int rank) || rank >= bestRank)
                        continue;

                    var anchor = ul.SelectSingleNode(".//a[contains(@href,'/game/')]");
                    string href = anchor?.GetAttributeValue("href", null);

                    if(string.IsNullOrWhiteSpace(href)) continue;

                    Match m = GameIdSlugRegex().Match(href);

                    if(!m.Success || !int.TryParse(m.Groups[1].Value, out int parsedId)) continue;

                    bestRank = rank;
                    bestId   = parsedId;
                    bestSlug = m.Groups[2].Value;
                }

                if(bestId is not null) return (bestId, bestSlug, bestRank < IncludedInRank);
            }
        }
        catch
        {
            // Fall through to regex fallback below.
        }

        // Legacy / fallback path: literal "Base Game" anywhere in the body, then the first
        // /game/N/slug/ link after it. Kept for backwards compatibility with cached chunks
        // whose markup predates the current sidebar shape.
        Match fallback = LegacyBaseGameRegex().Match(html);

        if(!fallback.Success || !int.TryParse(fallback.Groups[1].Value, out int id))
            return (null, null, false);

        string slug = fallback.Groups[2].Success ? fallback.Groups[2].Value : null;

        return (id, slug, true);
    }

    /// <summary>
    ///     Extracts the (numericId, slug) links listed under the "This Compilation Includes" sidebar
    ///     block of a new-site game page. Used to walk from a Season Pass / costume set down to the
    ///     individual DLCs it bundles.
    /// </summary>
    public static List<(int Id, string Slug)> ParseCompilationIncludes(string html)
    {
        var result = new List<(int Id, string Slug)>();

        if(string.IsNullOrWhiteSpace(html)) return result;

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var relatedLists = doc.DocumentNode.SelectNodes("//ul[starts-with(@id,'related')]");

            if(relatedLists is null) return result;

            var seen = new HashSet<int>();

            foreach(var ul in relatedLists)
            {
                string label = GetBlockLabel(ul);

                if(!label.Equals("This Compilation Includes", StringComparison.OrdinalIgnoreCase))
                    continue;

                var anchors = ul.SelectNodes(".//a[contains(@href,'/game/')]");

                if(anchors is null) continue;

                foreach(var anchor in anchors)
                {
                    Match m = GameIdSlugRegex().Match(anchor.GetAttributeValue("href", ""));

                    if(m.Success && int.TryParse(m.Groups[1].Value, out int id) && seen.Add(id))
                        result.Add((id, m.Groups[2].Value));
                }
            }
        }
        catch
        {
            // Malformed HTML: return whatever was collected.
        }

        return result;
    }

    /// <summary>
    ///     Returns the <c>&lt;b&gt;</c> label introducing a <c>related*</c> list. Several blocks can share
    ///     one parent <c>&lt;div&gt;</c> (e.g. "Included in" followed by "This Compilation Includes"), so the
    ///     label is the nearest preceding <c>&lt;b&gt;</c> sibling, not the parent's first one.
    /// </summary>
    static string GetBlockLabel(HtmlNode ul) =>
        ul.SelectSingleNode("preceding-sibling::b[1]")?.InnerText?.Trim().TrimEnd(':').Trim() ?? "";

    /// <summary>Convenience wrapper returning only the numeric ID.</summary>
    public static int? ParseBaseGameId(string html) => ParseBaseGame(html).Id;

    [GeneratedRegex(@"/game/(\d+)/([a-z0-9][a-z0-9_-]*)/?", RegexOptions.IgnoreCase)]
    private static partial Regex GameIdSlugRegex();

    [GeneratedRegex(@"Base\s*Game.*?/game/(\d+)/([^/""<>\s]+)?/?",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex LegacyBaseGameRegex();
}

