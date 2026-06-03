using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace Marechai.WinWorld.Parsers;

/// <summary>
///     Single entry on a library listing page (<c>/library/applications</c>, <c>/library/dev</c>,
///     <c>/library/sys</c>).
/// </summary>
public sealed class LibraryListingEntry
{
    public string       Slug              { get; set; }
    public string       SourceUrl         { get; set; }
    public string       Name              { get; set; }
    public List<string> Categories        { get; set; } = new();
    public List<string> Platforms         { get; set; } = new();
    public string       ShortDescription  { get; set; }
}

public sealed class LibraryListingPage
{
    public int                        MaxPage { get; set; } = 1;
    public List<LibraryListingEntry>  Entries { get; set; } = new();
}

public static class LibraryListingParser
{
    /// <summary>
    ///     Parses one paginated listing page. The relevant markup is
    ///     <code>
    ///     &lt;dl id="libraryList"&gt;
    ///       &lt;dt&gt;
    ///         &lt;a href="/product/{slug}"&gt;{name}&lt;/a&gt;
    ///         &lt;a class="badge badge-secondary" href="/search?...&amp;tags={cat}"&gt;{cat}&lt;/a&gt;*
    ///         &lt;a class="badge badge-info" href="/search?...&amp;platforms={plat}"&gt;{plat}&lt;/a&gt;*
    ///       &lt;/dt&gt;
    ///       &lt;dd&gt;&lt;p&gt;{description}&lt;/p&gt;&lt;/dd&gt;
    ///     &lt;/dl&gt;
    ///     </code>
    /// </summary>
    public static LibraryListingPage Parse(string html)
    {
        var result = new LibraryListingPage();
        if(string.IsNullOrEmpty(html)) return result;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Highest page number among pagination links (<a href="?page=N">). The listing also
        // serves "?page=1" as the canonical first page, so 1 is always a valid lower bound.
        HtmlNodeCollection pageLinks = doc.DocumentNode.SelectNodes("//a[contains(@href, '?page=')]");
        if(pageLinks != null)
        {
            foreach(HtmlNode a in pageLinks)
            {
                string href = a.GetAttributeValue("href", "");
                int    qIdx = href.IndexOf("?page=", StringComparison.Ordinal);
                if(qIdx < 0) continue;
                string tail = href[(qIdx + 6)..];
                int    amp  = tail.IndexOf('&');
                if(amp >= 0) tail = tail[..amp];
                if(int.TryParse(tail, out int n) && n > result.MaxPage) result.MaxPage = n;
            }
        }

        HtmlNode list = doc.GetElementbyId("libraryList");
        if(list == null) return result;

        HtmlNode currentDt = null;
        foreach(HtmlNode child in list.ChildNodes)
        {
            if(child.NodeType != HtmlNodeType.Element) continue;
            if(child.Name.Equals("dt", StringComparison.OrdinalIgnoreCase))
            {
                currentDt = child;
            }
            else if(child.Name.Equals("dd", StringComparison.OrdinalIgnoreCase) && currentDt != null)
            {
                LibraryListingEntry entry = BuildEntry(currentDt, child);
                if(entry != null) result.Entries.Add(entry);
                currentDt = null;
            }
        }

        return result;
    }

    static LibraryListingEntry BuildEntry(HtmlNode dt, HtmlNode dd)
    {
        // Primary anchor is the first <a> with /product/ href (no badge class).
        HtmlNode nameAnchor = dt.SelectSingleNode(".//a[starts-with(@href, '/product/') and not(contains(@class, 'badge'))]");
        if(nameAnchor == null) return null;

        string href = nameAnchor.GetAttributeValue("href", "").Trim();
        string slug = href.TrimEnd('/').Substring(href.LastIndexOf('/') + 1);
        if(string.IsNullOrEmpty(slug)) return null;

        var entry = new LibraryListingEntry
        {
            Slug      = slug,
            SourceUrl = href,
            Name      = WebUtility.HtmlDecode(nameAnchor.InnerText).Trim()
        };

        foreach(HtmlNode badge in dt.SelectNodes(".//a[contains(@class, 'badge-secondary')]") ?? Enumerable.Empty<HtmlNode>())
        {
            string label = WebUtility.HtmlDecode(badge.InnerText).Trim();
            if(!string.IsNullOrEmpty(label) && !entry.Categories.Contains(label, StringComparer.OrdinalIgnoreCase))
                entry.Categories.Add(label);
        }

        foreach(HtmlNode badge in dt.SelectNodes(".//a[contains(@class, 'badge-info')]") ?? Enumerable.Empty<HtmlNode>())
        {
            string label = WebUtility.HtmlDecode(badge.InnerText).Trim();
            if(!string.IsNullOrEmpty(label) && !entry.Platforms.Contains(label, StringComparer.OrdinalIgnoreCase))
                entry.Platforms.Add(label);
        }

        // Description is typically the first <p> inside the <dd>.
        HtmlNode p = dd.SelectSingleNode(".//p");
        if(p != null)
            entry.ShortDescription = WebUtility.HtmlDecode(p.InnerText).Trim();

        return entry;
    }
}
