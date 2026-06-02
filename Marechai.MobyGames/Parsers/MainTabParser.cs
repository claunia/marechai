using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;
using ReverseMarkdown;

namespace Marechai.MobyGames.Parsers;

public static partial class MainTabParser
{
    public static void Parse(HtmlDocument doc, ParsedGame game)
    {
        game.HasMainTab = true;

        ParseGameName(doc, game);
        ParseCoreInfo(doc, game);
        ParseGenres(doc, game);
        ParseDescription(doc, game);
        ParseCompilationContents(doc, game);
        ParseGroups(doc, game);
    }

    static void ParseGameName(HtmlDocument doc, ParsedGame game)
    {
        var h1 = doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'niceHeaderTitle')]");

        if(h1 is null) return;

        var a = h1.SelectSingleNode("a");

        game.Name = a != null ? WebUtility.HtmlDecode(a.InnerText).Trim() : WebUtility.HtmlDecode(h1.GetDirectInnerText()).Trim();
    }

    static void ParseCoreInfo(HtmlDocument doc, ParsedGame game)
    {
        var releaseDiv = doc.DocumentNode.SelectSingleNode("//*[@id='coreGameRelease']");

        if(releaseDiv is null) return;

        var boldDivs = releaseDiv.SelectNodes("div[contains(@style,'font-weight: bold')]");

        if(boldDivs is null) return;

        foreach(var boldDiv in boldDivs)
        {
            string label = WebUtility.HtmlDecode(boldDiv.InnerText).Trim();
            var    valueDiv = boldDiv.NextSibling;

            while(valueDiv != null && valueDiv.NodeType != HtmlNodeType.Element)
                valueDiv = valueDiv.NextSibling;

            if(valueDiv is null) continue;

            string value = WebUtility.HtmlDecode(valueDiv.InnerText).Trim();

            switch(label)
            {
                case "Published by":
                    var pubLinks = valueDiv.SelectNodes(".//a");
                    if(pubLinks != null)
                        game.Publishers = pubLinks
                                         .Select(a => WebUtility.HtmlDecode(a.InnerText).Trim())
                                         .Where(s => !string.IsNullOrWhiteSpace(s))
                                         .ToList();
                    else if(!string.IsNullOrWhiteSpace(value))
                        game.Publishers = [value];
                    break;
                case "Developed by":
                    var devLinks = valueDiv.SelectNodes(".//a");
                    if(devLinks != null)
                        game.Developers = devLinks
                                         .Select(a => WebUtility.HtmlDecode(a.InnerText).Trim())
                                         .Where(s => !string.IsNullOrWhiteSpace(s))
                                         .ToList();
                    else if(!string.IsNullOrWhiteSpace(value))
                        game.Developers = [value];
                    break;
                case "Released":
                    game.ReleaseDate = value;
                    break;
                case "Platform":
                case "Platforms":
                    var platformLinks = valueDiv.SelectNodes(".//a");
                    if(platformLinks != null)
                        game.Platforms = platformLinks
                                       .Select(a => WebUtility.HtmlDecode(a.InnerText).Trim())
                                       .Where(s => !string.IsNullOrWhiteSpace(s))
                                       .Distinct()
                                       .ToList();
                    else
                        game.Platforms = [value];

                    break;
            }
        }
    }

    static void ParseGenres(HtmlDocument doc, ParsedGame game)
    {
        var genreDiv = doc.DocumentNode.SelectSingleNode("//*[@id='coreGameGenre']");

        if(genreDiv is null) return;

        var boldDivs = genreDiv.SelectNodes(".//div[contains(@style,'font-weight: bold')]");

        if(boldDivs is null) return;

        foreach(var boldDiv in boldDivs)
        {
            string genreType = WebUtility.HtmlDecode(boldDiv.InnerText).Trim();
            var    valueDiv  = boldDiv.NextSibling;

            while(valueDiv != null && valueDiv.NodeType != HtmlNodeType.Element)
                valueDiv = valueDiv.NextSibling;

            if(valueDiv is null) continue;

            var links = valueDiv.SelectNodes(".//a");

            if(links is null) continue;

            foreach(var link in links)
            {
                string name = WebUtility.HtmlDecode(link.InnerText).Trim();

                if(!string.IsNullOrWhiteSpace(name))
                {
                    game.Genres.Add(new ParsedGenre
                    {
                        Type = genreType,
                        Name = name
                    });
                }
            }
        }
    }

    static void ParseDescription(HtmlDocument doc, ParsedGame game)
    {
        var descH2 = doc.DocumentNode.SelectSingleNode("//h2[text()='Description']");

        if(descH2 is null) return;

        // Collect all HTML nodes between the Description h2 and the next h2 or sideBarLinks div
        var sibling  = descH2.NextSibling;
        var htmlParts = new List<string>();

        while(sibling != null)
        {
            if(sibling.NodeType == HtmlNodeType.Element)
            {
                string tag = sibling.Name.ToLowerInvariant();

                if(tag is "h2") break;

                if(sibling.GetAttributeValue("class", "").Contains("sideBarLinks")) break;
            }

            htmlParts.Add(sibling.OuterHtml);
            sibling = sibling.NextSibling;
        }

        string rawHtml = string.Join("", htmlParts).Trim();

        if(string.IsNullOrWhiteSpace(rawHtml)) return;

        // Parse the collected HTML to strip links and images
        var descDoc = new HtmlDocument();
        descDoc.LoadHtml($"<div>{rawHtml}</div>");

        // Replace <a> tags with their inner content (keep text, remove link)
        var anchors = descDoc.DocumentNode.SelectNodes("//a");

        if(anchors != null)
        {
            foreach(var anchor in anchors)
            {
                var parent = anchor.ParentNode;
                // Insert all children before the anchor, then remove it
                foreach(var child in anchor.ChildNodes.ToList())
                    parent.InsertBefore(child, anchor);

                parent.RemoveChild(anchor);
            }
        }

        // Remove <img> and <picture> elements entirely
        var images = descDoc.DocumentNode.SelectNodes("//img|//picture");

        if(images != null)
        {
            foreach(var img in images)
                img.Remove();
        }

        // Get the cleaned HTML (unwrap the wrapper div)
        string cleanedHtml = descDoc.DocumentNode.FirstChild.InnerHtml.Trim();

        // Strip the trailing " (from Ad Blurbs)" fragment that MobyGames appends to
        // ad-blurb sub-headings such as <h3>PlayStation Store Description (from Ad Blurbs)</h3>.
        // The descriptive label ("PlayStation Store Description", "Steam Store Description",
        // "Back of Box Description", etc.) is kept; only the parenthetical suffix is removed.
        cleanedHtml = AdBlurbsSuffixRegex().Replace(cleanedHtml, string.Empty);

        if(string.IsNullOrWhiteSpace(cleanedHtml)) return;

        game.DescriptionHtml = cleanedHtml;

        // Convert to Markdown for the text column
        var converter = new Converter(new Config
        {
            UnknownTags         = Config.UnknownTagsOption.PassThrough,
            GithubFlavored      = true,
            SmartHrefHandling   = true,
            RemoveComments      = true
        });

        // Defensive second pass on the converted markdown in case ReverseMarkdown
        // reorders text in a way that resurrects the phrase.
        game.Description = AdBlurbsSuffixRegex().Replace(converter.Convert(cleanedHtml), string.Empty).Trim();
    }

    static void ParseGroups(HtmlDocument doc, ParsedGame game)
    {
        var groupsH2 = doc.DocumentNode.SelectSingleNode("//h2[text()='Part of the Following Groups']");

        if(groupsH2 is null) return;

        var ul = groupsH2.NextSibling;

        while(ul != null && ul.Name != "ul")
            ul = ul.NextSibling;

        if(ul is null) return;

        var links = ul.SelectNodes(".//a");

        if(links is null) return;

        foreach(var link in links)
        {
            string name = WebUtility.HtmlDecode(link.InnerText).Trim();

            if(!string.IsNullOrWhiteSpace(name))
                game.Groups.Add(name);
        }
    }

    /// <summary>
    ///     If any genre is "Compilation", extracts the slugs of contained games
    ///     from the description section's list items (li/a).
    /// </summary>
    static void ParseCompilationContents(HtmlDocument doc, ParsedGame game)
    {
        bool isCompilation = game.Genres.Any(g =>
            g.Name.Contains("Compilation", StringComparison.OrdinalIgnoreCase));

        if(!isCompilation) return;

        var descH2 = doc.DocumentNode.SelectSingleNode("//h2[text()='Description']");

        if(descH2 is null) return;

        // Build self-slug set to exclude the compilation's own link
        var selfSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if(!string.IsNullOrWhiteSpace(game.MobyGameId))
        {
            selfSlugs.Add(game.MobyGameId);
            selfSlugs.Add(game.MobyGameId.TrimStart('-'));
        }

        // Collect ALL raw HTML between <h2>Description</h2> and next h2/sideBarLinks
        var sibling   = descH2.NextSibling;
        var htmlParts = new List<string>();

        while(sibling != null)
        {
            if(sibling.NodeType == HtmlNodeType.Element)
            {
                string tag = sibling.Name.ToLowerInvariant();

                if(tag is "h2") break;

                if(sibling.GetAttributeValue("class", "").Contains("sideBarLinks")) break;
            }

            htmlParts.Add(sibling.OuterHtml);
            sibling = sibling.NextSibling;
        }

        string descHtml = string.Join("", htmlParts);

        if(string.IsNullOrWhiteSpace(descHtml)) return;

        // Parse the collected description HTML for game links inside <li> elements
        var descDoc = new HtmlDocument();
        descDoc.LoadHtml($"<div>{descHtml}</div>");

        // Find ALL <a> links inside <li> elements (handles both <ul><li><a> and loose <li><a>)
        var links = descDoc.DocumentNode.SelectNodes("//li//a[@href]");

        if(links != null)
        {
            foreach(var link in links)
            {
                string href = link.GetAttributeValue("href", "");

                // Tab/anchor-only links (#, #anchor) and javascript: URLs are page navigation, not games.
                if(IsIgnorableHref(href)) continue;

                string slug = ExtractGameSlugFromHref(href);

                if(slug != null)
                {
                    if(!selfSlugs.Contains(slug) &&
                       !game.CompilationGameSlugs.Contains(slug))
                        game.CompilationGameSlugs.Add(slug);
                }
                else if(!string.IsNullOrWhiteSpace(href) &&
                        (href.Contains("/search/", StringComparison.OrdinalIgnoreCase) ||
                         !href.Contains("/game/", StringComparison.OrdinalIgnoreCase)))
                {
                    // Link is NOT a /game/ URL — could be /search/quick?game= or some other non-game link.
                    // This game has no proper MobyGames entry — mark as unresolvable but keep the
                    // href so admins can investigate the original anchor.
                    string gameName = WebUtility.HtmlDecode(link.InnerText).Trim();
                    string absHref  = MakeAbsoluteMobyGamesUrl(href);

                    if(!string.IsNullOrWhiteSpace(gameName) &&
                       !game.UnresolvableCompilationGames.Any(u =>
                           string.Equals(u.Name, gameName, StringComparison.OrdinalIgnoreCase) &&
                           string.Equals(u.Href, absHref, StringComparison.OrdinalIgnoreCase)))
                    {
                        game.UnresolvableCompilationGames.Add(new UnresolvableCompilationLink
                        {
                            Name = gameName,
                            Href = absHref
                        });
                    }
                }
            }
        }

        // Fallback: also scan raw HTML with regex for search URLs missed by DOM parsing
        var searchMatches = SearchUrlRegex().Matches(descHtml);

        foreach(Match match in searchMatches)
        {
            string href     = match.Groups[1].Value;

            // /search/quick?company=..., /search/quick?gamegroup=..., and /search/quick?developer=...
            // links never represent a contained game — skip them outright so they don't
            // pollute the admin's compilation report.
            if(IsIgnorableSearchUrl(href)) continue;

            string gameName = WebUtility.HtmlDecode(match.Groups[2].Value).Trim();
            string absHref  = MakeAbsoluteMobyGamesUrl(href);

            if(!string.IsNullOrWhiteSpace(gameName) &&
               !game.UnresolvableCompilationGames.Any(u =>
                   string.Equals(u.Name, gameName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(u.Href, absHref, StringComparison.OrdinalIgnoreCase)))
            {
                game.UnresolvableCompilationGames.Add(new UnresolvableCompilationLink
                {
                    Name = gameName,
                    Href = absHref
                });
            }
        }
    }

    /// <summary>
    ///     MobyGames tab/section slugs that share the <c>/game/...</c> URL prefix but never
    ///     refer to a contained game. These are filtered out so the importer doesn't try to
    ///     resolve them and doesn't pester admins about anchors that are simply page navigation.
    /// </summary>
    static readonly HashSet<string> KnownNonGameSlugs =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "mobyrank",
            "cover-art",
            "release-info",
            "techinfo",
            "adblurbs",
            "buy-trade",

            // Other tab subpages we want rejected if they ever appear as 1-segment paths
            "credits",
            "screenshots",
            "promo",
            "promo-art",
            "trivia",
            "hints",
            "rating-systems",
            "reviews",
            "forums"
        };

    /// <summary>
    ///     Returns <c>true</c> for hrefs that are obviously not game references and that
    ///     therefore should be ignored entirely (neither parsed as a slug nor recorded as
    ///     an unresolvable anchor). Covers tab/anchor-only links (<c>#</c>, <c>#anchor</c>),
    ///     empty hrefs, <c>javascript:</c> URLs, MobyGames game-group URLs
    ///     (<c>/game-group/...</c>) which are series/franchise pages, developer
    ///     profile URLs (<c>/developer/...</c>) which are people pages, and MobyGames
    ///     non-game search URLs (<c>/search/quick?company=...</c> /
    ///     <c>/search/quick?gamegroup=...</c> / <c>/search/quick?developer=...</c>) which
    ///     are publisher / series / developer lookups, not games.
    /// </summary>
    static bool IsIgnorableHref(string href)
    {
        if(string.IsNullOrWhiteSpace(href)) return true;

        string trimmed = href.Trim();

        if(trimmed == "#" || trimmed.StartsWith("#", StringComparison.Ordinal)) return true;

        if(trimmed.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)) return true;

        // /game-group/... links are MobyGames series/franchise pages, not individual games.
        if(trimmed.Contains("/game-group/", StringComparison.OrdinalIgnoreCase)) return true;

        // /developer/... links are MobyGames people profile pages, not games.
        if(trimmed.Contains("/developer/", StringComparison.OrdinalIgnoreCase)) return true;

        // /search/quick?company=..., /search/quick?gamegroup=..., and /search/quick?developer=...
        // are MobyGames company / series / developer lookup search results, never a game.
        // Recording them as unresolvable just pollutes the admin's compilation report.
        if(IsIgnorableSearchUrl(trimmed)) return true;

        return false;
    }

    /// <summary>
    ///     Returns <c>true</c> for MobyGames non-game search URLs in any of their common
    ///     forms (relative <c>/search/quick?company=...</c> /
    ///     <c>/search/quick?gamegroup=...</c> / <c>/search/quick?developer=...</c> or the
    ///     absolute equivalents under <c>https://www.mobygames.com</c>). Such anchors are
    ///     never game entries and must be filtered out of the compilation parser's
    ///     "unresolvable anchors" report.
    /// </summary>
    static bool IsIgnorableSearchUrl(string href)
    {
        if(string.IsNullOrWhiteSpace(href)) return false;

        return href.Contains("/search/quick?company",   StringComparison.OrdinalIgnoreCase) ||
               href.Contains("/search/quick?gamegroup", StringComparison.OrdinalIgnoreCase) ||
               href.Contains("/search/quick?developer", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Makes a MobyGames URL absolute. Leading-slash hrefs are prefixed with the
    ///     mobygames.com origin. Hrefs that already include a scheme are returned
    ///     unchanged. Empty/null input returns null.
    /// </summary>
    static string MakeAbsoluteMobyGamesUrl(string href)
    {
        if(string.IsNullOrWhiteSpace(href)) return null;

        string trimmed = href.Trim();

        if(trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
           trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return trimmed;

        if(trimmed.StartsWith("/", StringComparison.Ordinal))
            return "https://www.mobygames.com" + trimmed;

        return trimmed;
    }

    /// <summary>
    ///     Matches anchor tags with search/quick URLs and captures the href and link text.
    /// </summary>
    [GeneratedRegex(@"<a\s[^>]*href=""([^""]*(?:/search/|search\.php)[^""]*)""[^>]*>([^<]+)</a>",
                    RegexOptions.IgnoreCase)]
    private static partial Regex SearchUrlRegex();

    /// <summary>
    ///     Matches the trailing " (from Ad Blurbs)" fragment that MobyGames appends to
    ///     ad-blurb sub-headings. Allows optional inline whitespace before the open paren
    ///     (but not newlines, so a heading/body separator survives) and flexible spacing
    ///     inside the parens. Case-insensitive.
    /// </summary>
    [GeneratedRegex(@"[ \t]*\(\s*from\s+Ad\s+Blurbs\s*\)", RegexOptions.IgnoreCase)]
    private static partial Regex AdBlurbsSuffixRegex();

    /// <summary>
    ///     Extracts a MobyGames game slug from a single href URL.
    ///     Only matches direct game URLs (1-2 path segments: /game/slug or /game/platform/slug
    ///     or /game/12345/slug/). Rejects subpage URLs (/game/platform/slug/forums) and known
    ///     non-game slugs like /game/mobyrank, /game/cover-art (see <see cref="KnownNonGameSlugs" />).
    ///     Returns null if the href is not a valid game URL.
    /// </summary>
    static string ExtractGameSlugFromHref(string href)
    {
        if(string.IsNullOrWhiteSpace(href)) return null;

        var match = GameSlugRegex().Match(href);

        if(!match.Success) return null;

        string fullPath = match.Groups[1].Value.TrimEnd('/');
        var    segments = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        string slug = segments.Length switch
        {
            // /game/slug — one segment, must not be purely numeric
            1 when !int.TryParse(segments[0], out _) => segments[0],

            // /game/platform/slug or /game/12345/slug — two segments, last is the slug
            2 when !int.TryParse(segments[1], out _) => segments[1],

            // /game/12345/ — numeric only, no slug
            // /game/platform/slug/subpage — too many segments, subpage URL
            _ => null
        };

        // Filter out known non-game slugs (tab subpages that share the /game/ URL space).
        if(slug != null && KnownNonGameSlugs.Contains(slug)) return null;

        return slug;
    }

    /// <summary>
    ///     Extracts MobyGames game slugs from raw HTML by finding anchor tags inside
    ///     list items (li &gt; a) that link to /game/ URLs.
    ///     Handles both proper &lt;ul&gt;&lt;li&gt;&lt;a&gt; structures and loose &lt;li&gt; elements.
    ///     Used by CompilationRelationService for retroactive processing of raw HTML chunks.
    /// </summary>
    public static List<string> ExtractGameSlugsFromHtml(string html, string selfSlug = null)
    {
        if(string.IsNullOrWhiteSpace(html)) return [];

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml($"<div>{html}</div>");

        var slugs     = new List<string>();
        var selfSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if(!string.IsNullOrWhiteSpace(selfSlug))
        {
            selfSlugs.Add(selfSlug);
            selfSlugs.Add(selfSlug.TrimStart('-'));
        }

        // Match links inside <li> elements — whether inside <ul> or loose
        var links = htmlDoc.DocumentNode.SelectNodes("//li/a[@href]");

        if(links is null) return slugs;

        foreach(var link in links)
        {
            string slug = ExtractGameSlugFromHref(link.GetAttributeValue("href", ""));

            if(slug != null && !selfSlugs.Contains(slug) && !slugs.Contains(slug))
                slugs.Add(slug);
        }

        return slugs;
    }

    /// <summary>
    ///     Extracts both the resolved game slugs AND the unresolvable anchors
    ///     (search URLs / non-game links) from a compilation description HTML chunk.
    ///     Used by <c>CompilationRelationService</c> for retroactive processing.
    /// </summary>
    public static (List<string> slugs, List<UnresolvableCompilationLink> unresolvable)
        ExtractCompilationContentsFromHtml(string html, string selfSlug = null)
    {
        var slugs        = new List<string>();
        var unresolvable = new List<UnresolvableCompilationLink>();

        if(string.IsNullOrWhiteSpace(html)) return (slugs, unresolvable);

        // GATE: legacy compilation pages introduce the contents list with a natural-language
        // preamble in the Description section. Without one of these phrases, the chunk's
        // <li><a href="/game/..."> items are almost certainly sidebar / related-games / DLC
        // lists rather than compilation members. Mis-parsing those caused base games like
        // Assassin's Creed IV: Black Flag and The Last of Us: Remastered to be treated as
        // compilations containing 100+ unrelated add-ons. Mirror the new-layout strictness
        // (which requires a literal <b>This Compilation Includes</b> sidebar label).
        if(!HasLegacyCompilationPreamble(html)) return (slugs, unresolvable);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml($"<div>{html}</div>");

        var selfSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if(!string.IsNullOrWhiteSpace(selfSlug))
        {
            selfSlugs.Add(selfSlug);
            selfSlugs.Add(selfSlug.TrimStart('-'));
        }

        var links = htmlDoc.DocumentNode.SelectNodes("//li//a[@href]");

        if(links != null)
        {
            foreach(var link in links)
            {
                string href = link.GetAttributeValue("href", "");

                // Tab/anchor-only links (#, #anchor) and javascript: URLs are page navigation, not games.
                if(IsIgnorableHref(href)) continue;

                string slug = ExtractGameSlugFromHref(href);

                if(slug != null)
                {
                    if(!selfSlugs.Contains(slug) && !slugs.Contains(slug))
                        slugs.Add(slug);
                }
                else if(!string.IsNullOrWhiteSpace(href) &&
                        (href.Contains("/search/", StringComparison.OrdinalIgnoreCase) ||
                         !href.Contains("/game/", StringComparison.OrdinalIgnoreCase)))
                {
                    string gameName = WebUtility.HtmlDecode(link.InnerText).Trim();
                    string absHref  = MakeAbsoluteMobyGamesUrl(href);

                    if(!string.IsNullOrWhiteSpace(gameName) &&
                       !unresolvable.Any(u =>
                           string.Equals(u.Name, gameName, StringComparison.OrdinalIgnoreCase) &&
                           string.Equals(u.Href, absHref, StringComparison.OrdinalIgnoreCase)))
                    {
                        unresolvable.Add(new UnresolvableCompilationLink
                        {
                            Name = gameName,
                            Href = absHref
                        });
                    }
                }
            }
        }

        // Regex fallback for search URLs the DOM walker misses.
        var searchMatches = SearchUrlRegex().Matches(html);

        foreach(Match match in searchMatches)
        {
            string href     = match.Groups[1].Value;

            // /search/quick?company=..., /search/quick?gamegroup=..., and /search/quick?developer=...
            // links never represent a contained game — skip them outright so they don't
            // pollute the admin's compilation report.
            if(IsIgnorableSearchUrl(href)) continue;

            string gameName = WebUtility.HtmlDecode(match.Groups[2].Value).Trim();
            string absHref  = MakeAbsoluteMobyGamesUrl(href);

            if(!string.IsNullOrWhiteSpace(gameName) &&
               !unresolvable.Any(u =>
                   string.Equals(u.Name, gameName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(u.Href, absHref, StringComparison.OrdinalIgnoreCase)))
            {
                unresolvable.Add(new UnresolvableCompilationLink
                {
                    Name = gameName,
                    Href = absHref
                });
            }
        }

        return (slugs, unresolvable);
    }

    /// <summary>
    ///     Matches href attributes pointing to MobyGames game URLs.
    ///     Captures the path portion after /game/.
    /// </summary>
    [GeneratedRegex(@"(?:https?://(?:www\.)?mobygames\.com)?/game/([^""?\s#]+)",
                    RegexOptions.IgnoreCase)]
    private static partial Regex GameSlugRegex();

    /// <summary>
    ///     Detects whether the legacy chunk's Description body actually introduces a
    ///     compilation contents list. Real compilation pages use one of a small set of
    ///     stock phrases ("This compilation includes", "the following games are included",
    ///     "This release includes", "is a compilation that features", etc.). Pages that
    ///     merely happen to carry a "Compilation" genre row but never enumerate their
    ///     members (typically Add-on / bundle / package-edition rows mis-tagged by
    ///     MobyGames editors) do NOT contain any of these phrases. Gating on this avoids
    ///     hoovering up unrelated /game/ links from sidebars or related-games panels.
    /// </summary>
    static bool HasLegacyCompilationPreamble(string html)
    {
        if(string.IsNullOrWhiteSpace(html)) return false;

        // Phrase comparison is case-insensitive and ignores intervening &nbsp; / <br> /
        // <i> / </i> markup. Strip tags + collapse whitespace once, then look for any
        // known preamble.
        string text = StripTagsAndCollapseWhitespace(html).ToLowerInvariant();

        foreach(string phrase in CompilationPreamblePhrases)
            if(text.Contains(phrase, StringComparison.Ordinal))
                return true;

        return false;
    }

    static readonly string[] CompilationPreamblePhrases =
    [
        "this compilation includes",
        "this compilation contains",
        "this compilation features",
        "this compilation comprises",
        "this release includes",
        "this release contains",
        "this release features",
        "this bundle includes",
        "this bundle contains",
        "this collection includes",
        "this collection contains",
        "this collection features",
        "this pack includes",
        "this pack contains",
        "this package includes",
        "this package contains",
        "the following games are included",
        "the following titles are included",
        "is a compilation that features",
        "is a compilation that includes",
        "is a compilation that contains",
        "is a compilation of",
        "is a collection that features",
        "is a collection that includes",
        "is a collection of",
        "is a bundle that includes",
        "is a bundle that contains",
        "is a bundle of"
    ];

    static string StripTagsAndCollapseWhitespace(string html)
    {
        var sb       = new System.Text.StringBuilder(html.Length);
        bool inTag   = false;
        bool lastWs  = false;

        foreach(char c in html)
        {
            if(c == '<') { inTag = true; continue; }
            if(c == '>') { inTag = false; continue; }
            if(inTag) continue;

            if(char.IsWhiteSpace(c) || c == '\u00A0')
            {
                if(!lastWs && sb.Length > 0)
                {
                    sb.Append(' ');
                    lastWs = true;
                }

                continue;
            }

            sb.Append(c);
            lastWs = false;
        }

        return sb.ToString();
    }
}
