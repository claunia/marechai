using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Marechai.Data;

namespace Marechai.OldDos.Parsers;

/// <summary>
///     Output of <see cref="CategoryListPageParser" /> for a single software listing row.
/// </summary>
public sealed class CategorySoftwareEntry
{
    public int    SourceId           { get; set; }
    public string SourceUrl          { get; set; }
    public string Name               { get; set; }
    public string ShortRussianDesc   { get; set; }
    public string OsName             { get; set; }
    public string Requirements       { get; set; }
    public string DeveloperName      { get; set; }
    public string PublisherName      { get; set; }
}

/// <summary>
///     Output of <see cref="CategoryListPageParser.ParseHeader" /> describing the page itself.
/// </summary>
public sealed class CategoryPageHeader
{
    public string                BreadcrumbPath { get; set; }
    public List<(int Id, string Name)> Breadcrumb { get; set; } = new();
    public int    CurrentCategoryId { get; set; }
    public int    TotalSoftware     { get; set; }
    public List<int> SubcategoryIds { get; set; } = new();
    public List<int> NextPageIds    { get; set; } = new();
}

public static class CategoryListPageParser
{
    static readonly Regex ShowIdRegex     = new(@"do=show&(?:amp;)?id=(\d+)", RegexOptions.Compiled);
    static readonly Regex ListCatRegex    = new(@"do=list&(?:amp;)?cat=(\d+)(?:&(?:amp;)?id=(\d+))?", RegexOptions.Compiled);
    static readonly Regex TotalFilesRegex = new(@"Всего файлов\s+(\d+)", RegexOptions.Compiled);

    public static CategoryPageHeader ParseHeader(int currentCategoryId, string html)
    {
        var header = new CategoryPageHeader { CurrentCategoryId = currentCategoryId };
        if(string.IsNullOrEmpty(html)) return header;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        Match totalMatch = TotalFilesRegex.Match(doc.DocumentNode.InnerText);
        if(totalMatch.Success) header.TotalSoftware = int.Parse(totalMatch.Groups[1].Value);

        var crumb = new List<(int Id, string Name)>();
        // Breadcrumb is "Cat >> SubCat >> Current" — typically first <h1> or top of page.
        HtmlNode firstHeading = doc.DocumentNode.SelectSingleNode("//h1") ??
                                doc.DocumentNode.SelectSingleNode("//*[contains(text(),'>>')]");
        if(firstHeading != null)
        {
            foreach(HtmlNode a in firstHeading.SelectNodes(".//a") ?? Enumerable.Empty<HtmlNode>())
            {
                Match mc = ListCatRegex.Match(HttpUtility.HtmlDecode(a.GetAttributeValue("href", "")));
                if(mc.Success)
                    crumb.Add((int.Parse(mc.Groups[1].Value), SoftwarePageParser.DecodeAll(a.InnerText).Trim()));
            }
            // The last crumb (current cat) is the trailing text after ">>"
            string raw = SoftwarePageParser.DecodeAll(firstHeading.InnerText);
            string[] parts = raw.Split(">>");
            if(parts.Length > 0)
            {
                string last = parts[^1].Trim();
                // Strip the pagination suffix old-dos.ru appends on pages 2..N
                // (e.g. "MS-DOS - Страница 2"). Without this we end up persisting fake
                // categories like "MS-DOS - Страница 2" alongside the real "MS-DOS".
                last = System.Text.RegularExpressions.Regex.Replace(last,
                    @"\s*[-\u2013\u2014]\s*Страница\s*\d+\s*$", "",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
                if(last.Length > 0)
                    crumb.Add((currentCategoryId, last));
            }
        }

        header.Breadcrumb     = crumb;
        header.BreadcrumbPath = string.Join(" >> ", crumb.Select(c => c.Name));

        // Pagination links: ?...&do=list&cat=N&id=M
        var nextPages = new HashSet<int>();
        foreach(HtmlNode a in doc.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>())
        {
            string href = HttpUtility.HtmlDecode(a.GetAttributeValue("href", ""));
            Match mc = ListCatRegex.Match(href);
            if(!mc.Success) continue;
            int cat = int.Parse(mc.Groups[1].Value);
            if(cat == currentCategoryId && mc.Groups[2].Success)
                nextPages.Add(int.Parse(mc.Groups[2].Value));
            else if(cat != currentCategoryId && !header.SubcategoryIds.Contains(cat))
                header.SubcategoryIds.Add(cat);
        }

        header.NextPageIds = nextPages.OrderBy(x => x).ToList();
        return header;
    }

    /// <summary>
    ///     Extract one entry per software row. Old-dos lays each entry out as a small table where
    ///     the left column is a Russian description and the right column carries label-style fields
    ///     ("Операционная система:", "Разработчик:", …). We walk every table and use the presence
    ///     of a "Скачать" (Download) anchor that links to a "do=show&id=" URL to identify rows.
    /// </summary>
    public static List<CategorySoftwareEntry> ParseEntries(string html)
    {
        var results = new List<CategorySoftwareEntry>();
        if(string.IsNullOrEmpty(html)) return results;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        foreach(HtmlNode table in doc.DocumentNode.SelectNodes("//table") ?? Enumerable.Empty<HtmlNode>())
        {
            // Find a "do=show&id=N" anchor inside this table — that's the canonical detail link.
            HtmlNode showAnchor = table.SelectSingleNode(".//a[contains(@href,'do=show')]");
            if(showAnchor == null) continue;

            Match mc = ShowIdRegex.Match(HttpUtility.HtmlDecode(showAnchor.GetAttributeValue("href", "")));
            if(!mc.Success) continue;

            int id = int.Parse(mc.Groups[1].Value);
            // Name appears in the table caption/header. Use the first non-label, non-empty heading row.
            string name = null;
            foreach(HtmlNode row in table.SelectNodes(".//tr") ?? Enumerable.Empty<HtmlNode>())
            {
                string text = SoftwarePageParser.DecodeAll(row.InnerText).Trim();
                if(string.IsNullOrEmpty(text)) continue;
                if(text.Equals("Описание", StringComparison.OrdinalIgnoreCase)) continue;
                if(text.Contains("Информация", StringComparison.OrdinalIgnoreCase) &&
                   text.Contains("Описание", StringComparison.OrdinalIgnoreCase)) continue;
                name = text;
                break;
            }

            // Strip <script>/<noscript>/<style>/<iframe>/<img> nodes before reading text — old-dos.ru
            // embeds a Yandex sharing widget whose JavaScript body otherwise lands in InnerText and
            // pollutes the OS / Developer / Publisher fields (often overflowing the DB columns).
            SoftwarePageParser.StripNoise(table);
            string fullText = SoftwarePageParser.DecodeAll(table.InnerText);
            string shortDesc = ExtractField(fullText, "Описание", new[] { "Операционная система", "Информация" });
            string os        = ExtractField(fullText, "Операционная система", new[] { "Требования", "Разработчик", "Издатель", "Скачать" });
            string reqs      = ExtractField(fullText, "Требования",          new[] { "Разработчик", "Издатель", "Скачать" });
            string dev       = ExtractField(fullText, "Разработчик",         new[] { "Издатель", "Скачать" });
            string pub       = ExtractField(fullText, "Издатель",            new[] { "Скачать" });

            results.Add(new CategorySoftwareEntry
            {
                SourceId         = id,
                SourceUrl        = $"/index.php?page=files&mode=files&do=show&id={id}",
                Name             = SoftwarePageParser.Cap(name?.Trim(), 512),
                ShortRussianDesc = Clean(shortDesc),
                OsName           = SoftwarePageParser.Cap(Clean(os),  256),
                Requirements     = SoftwarePageParser.Cap(Clean(reqs), 2048),
                DeveloperName    = SoftwarePageParser.Cap(Clean(dev) is "-" ? null : Clean(dev), 512),
                PublisherName    = SoftwarePageParser.Cap(Clean(pub) is "-" ? null : Clean(pub), 512)
            });
        }

        return results
              .GroupBy(e => e.SourceId)
              .Select(g => g.First())
              .ToList();
    }

    static string ExtractField(string fullText, string label, string[] stops)
    {
        int idx = fullText.IndexOf(label + ":", StringComparison.OrdinalIgnoreCase);
        if(idx < 0) return null;
        int start = idx + label.Length + 1;
        int end = fullText.Length;
        foreach(string stop in stops)
        {
            int s = fullText.IndexOf(stop, start, StringComparison.OrdinalIgnoreCase);
            if(s > 0 && s < end) end = s;
        }
        return fullText.Substring(start, end - start);
    }

    static string Clean(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : Regex.Replace(s, @"\s+", " ").Trim().TrimEnd('|').Trim();
}
