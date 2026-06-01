using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;
using ReverseMarkdown;

namespace Marechai.MobyGames.Parsers.NewSite;

/// <summary>
///     Parses the Main page of a game on the redesigned (Vue-based) MobyGames
///     layout. The new layout uses `info-release`/`info-genres` definition
///     lists instead of the old `coreGameRelease`/`coreGameGenre` divs, and
///     wraps the description in `&lt;section id="gameOfficialDescription"&gt;`
///     instead of using an `&lt;h2&gt;Description&lt;/h2&gt;` marker. The
///     fields produced here are the same as the legacy
///     <see cref="MainTabParser" /> so the rest of the importer keeps working
///     unchanged.
/// </summary>
public static partial class MainTabParser
{
    public static void Parse(HtmlDocument doc, ParsedGame game)
    {
        game.HasMainTab   = true;
        game.IsNewLayout  = true;

        ParseNumericIdAndSlug(doc, game);
        ParseGameName(doc, game);
        ParseInfoRelease(doc, game);
        ParseGenres(doc, game);
        ParseDescription(doc, game);
        ParseGroups(doc, game);
        ParseMediaFlags(doc, game);
        ParseCompilationContents(doc, game);
    }

    // ----------------------------------------------------------------------
    // 1) Pull the numeric id + slug from any internal anchor of the form
    //    /game/{numericId}/{slug}/...  — these are abundant on the new site
    //    (every sub-page anchor uses this form). We use the most common pair
    //    to avoid being fooled by stray cross-references.
    // ----------------------------------------------------------------------
    static void ParseNumericIdAndSlug(HtmlDocument doc, ParsedGame game)
    {
        var anchors = doc.DocumentNode.SelectNodes("//a[@href]");

        if(anchors is null) return;

        var counts = new Dictionary<(int Id, string Slug), int>();

        foreach(var a in anchors)
        {
            string href = a.GetAttributeValue("href", "");

            if(string.IsNullOrWhiteSpace(href)) continue;

            Match m = GameIdSlugRegex().Match(href);

            if(!m.Success || !int.TryParse(m.Groups[1].Value, out int id)) continue;

            string slug = m.Groups[2].Value;
            var    key  = (id, slug);
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }

        if(counts.Count == 0) return;

        var best = counts.OrderByDescending(kv => kv.Value).First();
        game.NumericId = best.Key.Id;
        game.Slug      = best.Key.Slug;
    }

    static void ParseGameName(HtmlDocument doc, ParsedGame game)
    {
        var h1 = doc.DocumentNode.SelectSingleNode("//h1[contains(concat(' ',@class,' '),' mb-0 ')]");

        if(h1 is null) return;

        game.Name = WebUtility.HtmlDecode(h1.InnerText).Trim();
    }

    // ----------------------------------------------------------------------
    // 2) The "info-release" definition list carries Released / Publishers /
    //    Developers / Platforms (the "Releases by Date" dt is also present
    //    but we ignore it for the Main parser — releases.html has the full
    //    per-platform detail).
    // ----------------------------------------------------------------------
    static void ParseInfoRelease(HtmlDocument doc, ParsedGame game)
    {
        var dl = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'info-release')]//dl[contains(@class,'metadata')]");

        if(dl is null) return;

        var dts = dl.SelectNodes("./dt");

        if(dts is null) return;

        foreach(var dt in dts)
        {
            // dt text is the label — strip a leading "Releases" link if any.
            string label = WebUtility.HtmlDecode(dt.GetDirectInnerText()).Trim();

            if(string.IsNullOrEmpty(label))
            {
                // The "Releases by Date" dt wraps "Releases" in an <a>; fall back to InnerText.
                label = WebUtility.HtmlDecode(dt.InnerText).Trim();
            }

            // Find the matching <dd> sibling.
            var dd = dt.NextSibling;

            while(dd != null && (dd.NodeType != HtmlNodeType.Element || dd.Name != "dd"))
                dd = dd.NextSibling;

            if(dd is null) continue;

            switch(label)
            {
                case "Released":
                    // dd shape: <a>March 3, 2023</a> on <a>Windows</a>
                    var dateAnchor = dd.SelectSingleNode("./a[1]");

                    if(dateAnchor != null)
                        game.ReleaseDate = WebUtility.HtmlDecode(dateAnchor.InnerText).Trim();

                    var firstPlatform = dd.SelectSingleNode("./a[2]");

                    if(firstPlatform != null)
                    {
                        string p = WebUtility.HtmlDecode(firstPlatform.InnerText).Trim();

                        if(!string.IsNullOrWhiteSpace(p) && !game.Platforms.Contains(p))
                            game.Platforms.Add(p);
                    }
                    break;
                case "Publishers":
                case "Publisher":
                    game.Publishers = ExtractLinkTexts(dd);
                    break;
                case "Developers":
                case "Developer":
                    game.Developers = ExtractLinkTexts(dd);
                    break;
                default:
                    // The "Releases by Date" dt is special — its dd contains a list of
                    // platform anchors inside <li> wrappers. Pull every <a href="/platform/.../">.
                    if(label.StartsWith("Releases", StringComparison.OrdinalIgnoreCase))
                    {
                        var platforms = dd.SelectNodes(".//a[contains(@href,'/platform/')]");

                        if(platforms != null)
                        {
                            foreach(var p in platforms)
                            {
                                string name = WebUtility.HtmlDecode(p.InnerText).Trim();

                                if(!string.IsNullOrWhiteSpace(name) && !game.Platforms.Contains(name))
                                    game.Platforms.Add(name);
                            }
                        }
                    }
                    break;
            }
        }
    }

    static List<string> ExtractLinkTexts(HtmlNode dd)
    {
        var result = new List<string>();
        var links  = dd.SelectNodes(".//a");

        if(links != null)
        {
            foreach(var a in links)
            {
                string txt = WebUtility.HtmlDecode(a.InnerText).Trim();

                if(!string.IsNullOrWhiteSpace(txt) && !result.Contains(txt))
                    result.Add(txt);
            }
        }
        else
        {
            string txt = WebUtility.HtmlDecode(dd.InnerText).Trim();

            if(!string.IsNullOrWhiteSpace(txt)) result.Add(txt);
        }

        return result;
    }

    // ----------------------------------------------------------------------
    // 3) Genres / Perspective / Gameplay / Interface / Setting live in the
    //    "info-genres" definition list; each <dt> is the category and the
    //    matching <dd> contains the value anchors.
    // ----------------------------------------------------------------------
    static void ParseGenres(HtmlDocument doc, ParsedGame game)
    {
        var dl = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'info-genres')]//dl[contains(@class,'metadata')]");

        if(dl is null) return;

        var dts = dl.SelectNodes("./dt");

        if(dts is null) return;

        foreach(var dt in dts)
        {
            string genreType = WebUtility.HtmlDecode(dt.InnerText).Trim();
            var    dd        = dt.NextSibling;

            while(dd != null && (dd.NodeType != HtmlNodeType.Element || dd.Name != "dd"))
                dd = dd.NextSibling;

            if(dd is null) continue;

            var links = dd.SelectNodes(".//a");

            if(links is null) continue;

            foreach(var link in links)
            {
                string name = WebUtility.HtmlDecode(link.InnerText).Trim();

                if(!string.IsNullOrWhiteSpace(name))
                    game.Genres.Add(new ParsedGenre { Type = genreType, Name = name });
            }
        }
    }

    // ----------------------------------------------------------------------
    // 4) The description body lives inside <section id="gameOfficialDescription">.
    //    The text may be wrapped in <details>/<sensitive-blur-wrapper>/<blurred-content>
    //    (adult-content gate) — we strip those layers but keep the inner HTML so
    //    paragraphs and h3 sub-headings survive the markdown conversion. Links and
    //    images are stripped to match the legacy parser's output.
    // ----------------------------------------------------------------------
    static void ParseDescription(HtmlDocument doc, ParsedGame game)
    {
        var section = doc.DocumentNode.SelectSingleNode("//section[@id='gameOfficialDescription']");

        if(section is null) return;

        // Peel off the optional wrappers. Try the deepest wrapper first.
        var inner = section.SelectSingleNode(".//div[contains(@class,'blurred-content')]") ?? section;

        // Collect children excluding the cover/picture/image (the first <div> typically
        // holds the cover image when the description is adult-blurred).
        var sb = new System.Text.StringBuilder();

        foreach(var child in inner.ChildNodes)
        {
            if(child.NodeType != HtmlNodeType.Element)
            {
                sb.Append(child.OuterHtml);

                continue;
            }

            string tag = child.Name.ToLowerInvariant();

            // Skip cover-image div, cover-swap-links Vue component, and the
            // "This cover may contain mature content." warning paragraph.
            if(tag == "cover-swap-links") continue;

            if(tag == "div")
            {
                if(child.SelectSingleNode(".//*[@id='cover']") != null) continue;
                if(child.SelectSingleNode(".//cover-swap-links") != null) continue;
            }

            sb.Append(child.OuterHtml);
        }

        string rawHtml = sb.ToString().Trim();

        if(string.IsNullOrWhiteSpace(rawHtml)) return;

        var descDoc = new HtmlDocument();
        descDoc.LoadHtml($"<div>{rawHtml}</div>");

        // Strip the adult-content warning paragraph if it survived.
        var warn = descDoc.DocumentNode.SelectSingleNode("//p[contains(@class,'text-muted') and contains(.,'mature content')]");
        warn?.Remove();

        // Replace <a> with inner content.
        var anchors = descDoc.DocumentNode.SelectNodes("//a");

        if(anchors != null)
        {
            foreach(var anchor in anchors)
            {
                var parent = anchor.ParentNode;

                foreach(var c in anchor.ChildNodes.ToList())
                    parent.InsertBefore(c, anchor);

                parent.RemoveChild(anchor);
            }
        }

        // Drop images and pictures.
        var imgs = descDoc.DocumentNode.SelectNodes("//img|//picture");

        if(imgs != null)
        {
            foreach(var i in imgs)
                i.Remove();
        }

        string cleanedHtml = descDoc.DocumentNode.FirstChild.InnerHtml.Trim();

        // Strip the trailing " (from Ad Blurbs)" fragment if present (the new
        // site uses it too on store-blurb sub-headings).
        cleanedHtml = AdBlurbsSuffixRegex().Replace(cleanedHtml, string.Empty);

        if(string.IsNullOrWhiteSpace(cleanedHtml)) return;

        game.DescriptionHtml = cleanedHtml;

        var converter = new Converter(new Config
        {
            UnknownTags       = Config.UnknownTagsOption.PassThrough,
            GithubFlavored    = true,
            SmartHrefHandling = true,
            RemoveComments    = true
        });

        game.Description = AdBlurbsSuffixRegex().Replace(converter.Convert(cleanedHtml), string.Empty).Trim();
    }

    // ----------------------------------------------------------------------
    // 5) Groups live in <section id="gameGroups">/<ul>/<li>/<a>.
    // ----------------------------------------------------------------------
    static void ParseGroups(HtmlDocument doc, ParsedGame game)
    {
        var section = doc.DocumentNode.SelectSingleNode("//section[@id='gameGroups']");

        if(section is null) return;

        var links = section.SelectNodes(".//ul//li//a");

        if(links is null) return;

        foreach(var link in links)
        {
            string name = WebUtility.HtmlDecode(link.InnerText).Trim();

            if(!string.IsNullOrWhiteSpace(name) && !game.Groups.Contains(name))
                game.Groups.Add(name);
        }
    }

    // ----------------------------------------------------------------------
    // 6) Media-availability flags. On the new site the Main page links to
    //    each sub-page directly (/screenshots/, /promo/, /covers/, /media/,
    //    /reviews/) using the canonical /game/{id}/{slug}/<sub>/ form. The
    //    presence of any such anchor is treated as "this sub-page exists".
    // ----------------------------------------------------------------------
    static void ParseMediaFlags(HtmlDocument doc, ParsedGame game)
    {
        if(game.NumericId is null || string.IsNullOrEmpty(game.Slug)) return;

        string baseAbs  = $"https://www.mobygames.com/game/{game.NumericId.Value}/{game.Slug}/";
        string baseRel  = $"/game/{game.NumericId.Value}/{game.Slug}/";

        bool HasSubPage(string sub)
        {
            string suffix = sub + "/";
            string abs    = baseAbs + suffix;
            string rel    = baseRel + suffix;

            var anchors = doc.DocumentNode.SelectNodes("//a[@href]");

            if(anchors is null) return false;

            foreach(var a in anchors)
            {
                string href = a.GetAttributeValue("href", "");

                // Accept exact match OR href that prefixes the sub-page path
                // (e.g. promo/group-XXX/image-YYY/ implies /promo/ exists too).
                if(href == abs || href == rel) return true;

                if(href.StartsWith(abs + "group-",   StringComparison.OrdinalIgnoreCase) ||
                   href.StartsWith(rel + "group-",   StringComparison.OrdinalIgnoreCase))
                    return true;

                if(href.StartsWith(abs + "cover-",   StringComparison.OrdinalIgnoreCase) ||
                   href.StartsWith(rel + "cover-",   StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        game.HasScreenshots = HasSubPage("screenshots");
        game.HasPromoArt    = HasSubPage("promo");
        game.HasCoverArt    = HasSubPage("covers") || HasSubPage("cover");
        game.HasMedia       = HasSubPage("media")  || HasSubPage("videos");
        game.HasReviews     = HasSubPage("reviews");
    }

    // ----------------------------------------------------------------------
    // 7) Compilation contents are extracted from the description's <li><a>
    //    items, identical to the legacy parser. The genre check uses the
    //    Genres list we just populated.
    // ----------------------------------------------------------------------
    static void ParseCompilationContents(HtmlDocument doc, ParsedGame game)
    {
        bool isCompilation = game.Genres.Any(g => g.Name.Contains("Compilation", StringComparison.OrdinalIgnoreCase));

        if(!isCompilation) return;

        var selfSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if(!string.IsNullOrWhiteSpace(game.Slug))       selfSlugs.Add(game.Slug);
        if(!string.IsNullOrWhiteSpace(game.MobyGameId)) selfSlugs.Add(game.MobyGameId);

        (List<string> slugs, List<UnresolvableCompilationLink> unresolvable) =
            ExtractCompilationContentsFromHtml(doc, selfSlugs);

        foreach(string s in slugs)
            if(!game.CompilationGameSlugs.Contains(s))
                game.CompilationGameSlugs.Add(s);

        foreach(var u in unresolvable)
            if(!game.UnresolvableCompilationGames.Any(x =>
                   string.Equals(x.Name, u.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(x.Href, u.Href, StringComparison.OrdinalIgnoreCase)))
                game.UnresolvableCompilationGames.Add(u);
    }

    /// <summary>
    ///     New-site equivalent of <see cref="Parsers.MainTabParser.ExtractCompilationContentsFromHtml"/>.
    ///     Scopes the link walk to <c>&lt;section id="gameOfficialDescription"&gt;</c> so sidebar /
    ///     related-games / compare-credits anchors elsewhere on the page are NOT picked up as
    ///     compilation members. Used by <c>CompilationRelationService</c> when the cached chunk
    ///     is post-2023 layout HTML.
    /// </summary>
    public static (List<string> slugs, List<UnresolvableCompilationLink> unresolvable)
        ExtractCompilationContentsFromHtml(string html, string selfSlug = null)
    {
        if(string.IsNullOrWhiteSpace(html))
            return ([], []);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var selfSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if(!string.IsNullOrWhiteSpace(selfSlug))
        {
            selfSlugs.Add(selfSlug);
            selfSlugs.Add(selfSlug.TrimStart('-'));
        }

        return ExtractCompilationContentsFromHtml(doc, selfSlugs);
    }

    static (List<string> slugs, List<UnresolvableCompilationLink> unresolvable)
        ExtractCompilationContentsFromHtml(HtmlDocument doc, HashSet<string> selfSlugs)
    {
        var slugs        = new List<string>();
        var unresolvable = new List<UnresolvableCompilationLink>();

        // Source 1 (primary on new layout): sidebar / aside block of the shape
        //   <div class="border border-1 mb flowroot">
        //     <b>{LABEL}</b>     <!-- "Original", "Standard", "This Compilation Includes", ... -->
        //     <ul id="related1" class="list-group toggle-long-text toggle-max-3 mb-0">
        //       <li><a href=".../game/N/slug/"><img .../></a> <a href="...">Title</a> <small>(YYYY)</small></li>
        //       ...
        //     </ul>
        //   </div>
        // The same shape is reused by DLC pages for the "Base Game" link and by every game for
        // optional "Series" / "Groups" lists, so we skip well-known non-compilation labels.
        var relatedLists = doc.DocumentNode.SelectNodes("//ul[starts-with(@id,'related')]");

        if(relatedLists is not null)
        {
            foreach(var ul in relatedLists)
            {
                string label = ul.ParentNode?.SelectSingleNode("./b")?.InnerText?.Trim() ?? "";

                if(IsNonCompilationRelatedLabel(label)) continue;

                CollectGameLinksFromLis(ul, selfSlugs, slugs, unresolvable);
            }
        }

        // Source 2 (sometimes present alongside, sometimes the only source on old layouts):
        // the description body inside <section id="gameOfficialDescription"> with an inline
        // bullet list of titles (and possibly role suffixes like "(base game)").
        var section = doc.DocumentNode.SelectSingleNode("//section[@id='gameOfficialDescription']");

        if(section is not null)
            CollectGameLinksFromLis(section, selfSlugs, slugs, unresolvable);

        return (slugs, unresolvable);
    }

    /// <summary>
    ///     Sidebar "related*" lists appear on EVERY new-layout game page, not just compilations.
    ///     Skip labels we know never enumerate compilation contents so series / franchise /
    ///     base-game links don't leak into the contained-games list.
    /// </summary>
    static bool IsNonCompilationRelatedLabel(string label)
    {
        if(string.IsNullOrWhiteSpace(label)) return false;

        string l = label.Trim().TrimEnd(':').Trim();

        return l.Equals("Base Game",       StringComparison.OrdinalIgnoreCase) ||
               l.Equals("Base Games",      StringComparison.OrdinalIgnoreCase) ||
               l.Equals("Series",          StringComparison.OrdinalIgnoreCase) ||
               l.Equals("Groups",          StringComparison.OrdinalIgnoreCase) ||
               l.Equals("Group",           StringComparison.OrdinalIgnoreCase) ||
               l.Equals("DLC",             StringComparison.OrdinalIgnoreCase) ||
               l.Equals("DLC / Add-Ons",   StringComparison.OrdinalIgnoreCase) ||
               l.Equals("Add-Ons",         StringComparison.OrdinalIgnoreCase) ||
               l.Equals("Expansions",      StringComparison.OrdinalIgnoreCase) ||
               l.Equals("Related Games",   StringComparison.OrdinalIgnoreCase);
    }

    static void CollectGameLinksFromLis(HtmlNode scope, HashSet<string> selfSlugs,
                                        List<string> slugs, List<UnresolvableCompilationLink> unresolvable)
    {
        var links = scope.SelectNodes(".//li//a[@href]");

        if(links is null) return;

        foreach(var link in links)
        {
            string href = link.GetAttributeValue("href", "");

            if(string.IsNullOrWhiteSpace(href)) continue;
            if(href.StartsWith("#") || href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)) continue;

            Match m = GameIdSlugRegex().Match(href);

            if(m.Success)
            {
                string slug = m.Groups[2].Value;

                if(!selfSlugs.Contains(slug) && !slugs.Contains(slug))
                    slugs.Add(slug);
            }
            else if(href.Contains("/search/", StringComparison.OrdinalIgnoreCase) ||
                    !href.Contains("/game/",  StringComparison.OrdinalIgnoreCase))
            {
                string name = WebUtility.HtmlDecode(link.InnerText).Trim();

                if(string.IsNullOrWhiteSpace(name)) continue;

                string abs = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                 ? href
                                 : href.StartsWith("/")
                                       ? "https://www.mobygames.com" + href
                                       : "https://www.mobygames.com/" + href;

                if(!unresolvable.Any(u =>
                       string.Equals(u.Name, name, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(u.Href, abs,  StringComparison.OrdinalIgnoreCase)))
                    unresolvable.Add(new UnresolvableCompilationLink { Name = name, Href = abs });
            }
        }
    }

    [GeneratedRegex(@"/game/(\d+)/([a-z0-9][a-z0-9_-]*)/", RegexOptions.IgnoreCase)]
    private static partial Regex GameIdSlugRegex();

    [GeneratedRegex(@"\s*\(from Ad Blurbs\)\s*", RegexOptions.IgnoreCase)]
    private static partial Regex AdBlurbsSuffixRegex();
}
