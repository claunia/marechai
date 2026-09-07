using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers;

/// <summary>
///     Parses the old MobyGames cover art tab HTML (pre-2024 redesign) stored in the source database.
///     Structure: div.coverHeading contains h2 (platform) + table (metadata),
///     followed by div.row with thumbnail cover links.
/// </summary>
public static partial class CoverArtTabParser
{
    public static List<ParsedCoverGroup> Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return Parse(doc);
    }

    public static List<ParsedCoverGroup> Parse(HtmlDocument doc)
    {
        var groups = new List<ParsedCoverGroup>();

        // Old layout: cover groups start with <div class="coverHeading">
        var coverHeadings = doc.DocumentNode.SelectNodes("//div[contains(@class,'coverHeading')]");

        if(coverHeadings is null) return groups;

        foreach(var heading in coverHeadings)
        {
            var group = new ParsedCoverGroup();

            // Platform name from <h2> inside coverHeading
            var h2 = heading.SelectSingleNode(".//h2");

            if(h2 is not null)
                group.Platform = WebUtility.HtmlDecode(h2.InnerText).Trim();

            // Metadata table: <table SUMMARY="Description of Covers">
            var table = heading.SelectSingleNode(".//table");

            if(table is not null)
                ParseMetadataTable(table, group);

            // Extract group ID from the sidebar link that follows:
            // <div class="sideBarLinks">[<a href="...coverGroupId,{id}/...">add covers</a>]</div>
            // and cover thumbnails from <div class="row"> - both are siblings of this heading,
            // somewhere before the *next* coverHeading. Walking siblings (rather than searching
            // heading.ParentNode's whole subtree) keeps this scoped to the current group instead
            // of always matching the first such node on the whole page for every group.
            // Structure: div.row > div.col-* > div.thumbnail > div.thumbnail-image-wrapper > a.thumbnail-cover
            //            + div.thumbnail-cover-caption > p (type label)
            var nextSibling = heading.NextSibling;

            while(nextSibling is not null)
            {
                if(nextSibling.NodeType == HtmlNodeType.Element)
                {
                    // If we hit another coverHeading, this group's section has ended.
                    if(nextSibling.GetAttributeValue("class", "").Contains("coverHeading"))
                        break;

                    // Check if this is the row containing thumbnail covers
                    if(nextSibling.GetAttributeValue("class", "").Contains("row"))
                        ParseCoversFromRow(nextSibling, group);

                    if(string.IsNullOrEmpty(group.GroupId))
                    {
                        var sgLink = nextSibling.SelectSingleNode(".//a[contains(@href,'coverGroupId')]");

                        if(sgLink is not null)
                        {
                            var m = CoverGroupIdRegex().Match(sgLink.GetAttributeValue("href", ""));

                            if(m.Success)
                                group.GroupId = m.Groups[1].Value;
                        }
                    }
                }

                nextSibling = nextSibling.NextSibling;
            }

            if(group.Covers.Count > 0 || !string.IsNullOrWhiteSpace(group.Platform))
                groups.Add(group);
        }

        return groups;
    }

    static void ParseMetadataTable(HtmlNode table, ParsedCoverGroup group)
    {
        var rows = table.SelectNodes(".//tr");

        if(rows is null) return;

        foreach(var row in rows)
        {
            var cells = row.SelectNodes("td");

            if(cells is null || cells.Count < 3) continue;

            // Old format: td[0]=key, td[1]=separator, td[2]=value
            string key   = WebUtility.HtmlDecode(cells[0].InnerText).Trim().Replace("\u00a0", " ");
            string value = WebUtility.HtmlDecode(cells[2].InnerText).Trim();

            switch(key)
            {
                case "Packaging":
                    group.Packaging = value;

                    break;
                case "Video Standard":
                    group.VideoStandard = value;

                    break;
                case "Country":
                    // Old format: country names as plain text before <img> flag icons
                    // e.g., "United States <img...>" or multiple countries separated by ", "
                    // The <span> contains text + img children
                    var spans = cells[2].SelectNodes(".//span");

                    if(spans is not null)
                    {
                        foreach(var span in spans)
                        {
                            // Get text nodes only (before the <img>)
                            string countryText = "";

                            foreach(var child in span.ChildNodes)
                            {
                                if(child.NodeType == HtmlNodeType.Text)
                                    countryText += child.InnerText;
                            }

                            countryText = WebUtility.HtmlDecode(countryText).Trim().TrimEnd(',');

                            if(!string.IsNullOrWhiteSpace(countryText))
                                group.Countries.Add(countryText);
                        }
                    }

                    // Fallback: try img alt attributes for country names
                    if(group.Countries.Count == 0)
                    {
                        var imgs = cells[2].SelectNodes(".//img[@alt]");

                        if(imgs is not null)
                        {
                            foreach(var img in imgs)
                            {
                                string alt = img.GetAttributeValue("alt", "").Trim();

                                if(!string.IsNullOrWhiteSpace(alt))
                                    group.Countries.Add(alt);
                            }
                        }
                    }

                    break;
            }
        }
    }

    static void ParseCoversFromRow(HtmlNode rowNode, ParsedCoverGroup group)
    {
        // Find all thumbnail cover links: <a class="thumbnail-cover" href="...gameCoverId,{id}/...">
        var coverLinks = rowNode.SelectNodes(".//a[contains(@class,'thumbnail-cover')]");

        if(coverLinks is null)
        {
            // Fallback: try any link containing gameCoverId
            coverLinks = rowNode.SelectNodes(".//a[contains(@href,'gameCoverId')]");
        }

        if(coverLinks is null) return;

        foreach(var link in coverLinks)
        {
            string href = link.GetAttributeValue("href", "");

            // Extract cover ID from URL pattern: gameCoverId,{id}/
            var coverMatch = GameCoverIdRegex().Match(href);

            if(!coverMatch.Success) continue;

            string coverId = coverMatch.Groups[1].Value;

            // Find the type label in the caption:
            // Traverse up to div.thumbnail, then find div.thumbnail-cover-caption > p
            string typeLabel = null;
            var    thumbnail = link.Ancestors("div")
                                   .FirstOrDefault(d => d.GetAttributeValue("class", "").Contains("thumbnail"));

            if(thumbnail is not null)
            {
                var caption = thumbnail.SelectSingleNode(
                    ".//div[contains(@class,'thumbnail-cover-caption')]//p");

                if(caption is not null)
                    typeLabel = WebUtility.HtmlDecode(caption.InnerText).Trim();
            }

            // Fallback: try the title attribute of the link
            if(string.IsNullOrWhiteSpace(typeLabel))
            {
                string title = link.GetAttributeValue("title", "");

                if(!string.IsNullOrWhiteSpace(title))
                {
                    // Title is like "Game Name Platform Front Cover" — extract last two words
                    var titleMatch = TitleCoverTypeRegex().Match(title);

                    if(titleMatch.Success)
                        typeLabel = titleMatch.Groups[1].Value;
                }
            }

            if(string.IsNullOrWhiteSpace(typeLabel)) continue;

            // Avoid duplicates
            if(group.Covers.Any(c => c.CoverId == coverId)) continue;

            // Extract thumbnail URL from background-image CSS style
            string thumbnailUrl = null;
            string style        = link.GetAttributeValue("style", "");
            var    bgMatch      = BackgroundImageRegex().Match(style);

            if(bgMatch.Success)
                thumbnailUrl = bgMatch.Groups[1].Value;

            group.Covers.Add(new ParsedCoverImage
            {
                Type          = typeLabel,
                DetailPageUrl = href,
                CoverId       = coverId,
                ThumbnailUrl  = thumbnailUrl
            });
        }
    }

    // Old format: coverGroupId,{id}
    [GeneratedRegex(@"coverGroupId,(\d+)", RegexOptions.Compiled)]
    private static partial Regex CoverGroupIdRegex();

    // Old format: gameCoverId,{id}
    [GeneratedRegex(@"gameCoverId,(\d+)", RegexOptions.Compiled)]
    private static partial Regex GameCoverIdRegex();

    // Extract cover type from title like "Game Name Platform Front Cover"
    [GeneratedRegex(@"((?:Front|Back|Inside|Media|Spine|Manual|Other)\b.+)$", RegexOptions.Compiled)]
    private static partial Regex TitleCoverTypeRegex();

    // Extract URL from background-image:url(...)
    [GeneratedRegex(@"background-image:\s*url\(([^)]+)\)", RegexOptions.Compiled)]
    private static partial Regex BackgroundImageRegex();
}
