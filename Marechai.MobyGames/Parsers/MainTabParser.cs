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

        game.Description = converter.Convert(cleanedHtml).Trim();
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
                    // This game has no proper MobyGames entry — mark as unresolvable.
                    string gameName = WebUtility.HtmlDecode(link.InnerText).Trim();

                    if(!string.IsNullOrWhiteSpace(gameName) &&
                       !game.UnresolvableCompilationGames.Contains(gameName))
                        game.UnresolvableCompilationGames.Add(gameName);
                }
            }
        }

        // Fallback: also scan raw HTML with regex for search URLs missed by DOM parsing
        var searchMatches = SearchUrlRegex().Matches(descHtml);

        foreach(Match match in searchMatches)
        {
            string gameName = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();

            if(!string.IsNullOrWhiteSpace(gameName) &&
               !game.UnresolvableCompilationGames.Contains(gameName))
                game.UnresolvableCompilationGames.Add(gameName);
        }
    }

    /// <summary>
    ///     Matches anchor tags with search/quick URLs and captures the link text.
    /// </summary>
    [GeneratedRegex(@"<a\s[^>]*href=""[^""]*(?:/search/|search\.php)[^""]*""[^>]*>([^<]+)</a>",
                    RegexOptions.IgnoreCase)]
    private static partial Regex SearchUrlRegex();

    /// <summary>
    ///     Extracts a MobyGames game slug from a single href URL.
    ///     Only matches direct game URLs (1-2 path segments: /game/slug or /game/platform/slug
    ///     or /game/12345/slug/). Rejects subpage URLs (/game/platform/slug/forums).
    ///     Returns null if the href is not a valid game URL.
    /// </summary>
    static string ExtractGameSlugFromHref(string href)
    {
        if(string.IsNullOrWhiteSpace(href)) return null;

        var match = GameSlugRegex().Match(href);

        if(!match.Success) return null;

        string fullPath = match.Groups[1].Value.TrimEnd('/');
        var    segments = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        return segments.Length switch
        {
            // /game/slug — one segment, must not be purely numeric
            1 when !int.TryParse(segments[0], out _) => segments[0],

            // /game/platform/slug or /game/12345/slug — two segments, last is the slug
            2 when !int.TryParse(segments[1], out _) => segments[1],

            // /game/12345/ — numeric only, no slug
            // /game/platform/slug/subpage — too many segments, subpage URL
            _ => null
        };
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
    ///     Matches href attributes pointing to MobyGames game URLs.
    ///     Captures the path portion after /game/.
    /// </summary>
    [GeneratedRegex(@"(?:https?://(?:www\.)?mobygames\.com)?/game/([^""?\s#]+)",
                    RegexOptions.IgnoreCase)]
    private static partial Regex GameSlugRegex();
}
