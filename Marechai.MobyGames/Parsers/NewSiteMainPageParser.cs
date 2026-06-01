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
    /// </summary>
    static readonly HashSet<string> BaseGameLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        "Base Game", "Base Games",
        "Included in",
        "Expansion of", "Standalone Expansion of",
        "Mod of",
        "Requires"
    };

    /// <summary>
    ///     Extracts the MobyGames numeric ID and slug of the base/parent game from a new-site
    ///     game page's HTML. Walks every <c>&lt;ul id="related*"&gt;</c> sidebar block, picks the
    ///     first one whose preceding <c>&lt;b&gt;</c> label matches <see cref="BaseGameLabels"/>,
    ///     and returns the first <c>/game/N/slug/</c> link inside that block. Falls back to the
    ///     pre-existing "Base Game"-only regex when the DOM parse finds nothing (handles legacy
    ///     captures or edge cases where the markup differs).
    /// </summary>
    /// <param name="html">Raw HTML of the new MobyGames game page</param>
    /// <returns>Tuple of (numericId, slug), or (null, null) if not found</returns>
    public static (int? Id, string Slug) ParseBaseGame(string html)
    {
        if(string.IsNullOrWhiteSpace(html)) return (null, null);

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var relatedLists = doc.DocumentNode.SelectNodes("//ul[starts-with(@id,'related')]");

            if(relatedLists is not null)
            {
                foreach(var ul in relatedLists)
                {
                    string label = ul.ParentNode?.SelectSingleNode("./b")?.InnerText?.Trim() ?? "";

                    if(!BaseGameLabels.Contains(label.TrimEnd(':').Trim())) continue;

                    var anchor = ul.SelectSingleNode(".//a[contains(@href,'/game/')]");
                    string href = anchor?.GetAttributeValue("href", null);

                    if(string.IsNullOrWhiteSpace(href)) continue;

                    Match m = GameIdSlugRegex().Match(href);

                    if(m.Success && int.TryParse(m.Groups[1].Value, out int parsedId))
                        return (parsedId, m.Groups[2].Value);
                }
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
            return (null, null);

        string slug = fallback.Groups[2].Success ? fallback.Groups[2].Value : null;

        return (id, slug);
    }

    /// <summary>Convenience wrapper returning only the numeric ID.</summary>
    public static int? ParseBaseGameId(string html) => ParseBaseGame(html).Id;

    [GeneratedRegex(@"/game/(\d+)/([a-z0-9][a-z0-9_-]*)/?", RegexOptions.IgnoreCase)]
    private static partial Regex GameIdSlugRegex();

    [GeneratedRegex(@"Base\s*Game.*?/game/(\d+)/([^/""<>\s]+)?/?",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex LegacyBaseGameRegex();
}

