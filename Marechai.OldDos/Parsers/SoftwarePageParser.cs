using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Marechai.Data;

namespace Marechai.OldDos.Parsers;

public sealed class SoftwarePageData
{
    public int    SourceId            { get; set; }
    public string Name                { get; set; }
    public string RussianDescription  { get; set; }
    public string OsName              { get; set; }
    public string DeveloperName       { get; set; }
    public string PublisherName       { get; set; }
    public string BreadcrumbPath      { get; set; }
    public int?   CategoryId          { get; set; }
    public List<SoftwareVersionEntry> Versions { get; set; } = new();
}

public sealed class SoftwareVersionEntry
{
    public string    VersionString        { get; set; }
    public DateTime? ReleaseDate          { get; set; }
    public DatePrecision ReleaseDatePrecision { get; set; }
    public string    DownloadUrl          { get; set; }
    public string    FileName             { get; set; }
    public string    Notes                { get; set; }
}

public static class SoftwarePageParser
{
    static readonly Regex ListCatRegex   = new(@"do=list&(?:amp;)?cat=(\d+)", RegexOptions.Compiled);
    static readonly Regex DateFullRegex  = new(@"(\d{4})[-./](\d{1,2})[-./](\d{1,2})", RegexOptions.Compiled);
    static readonly Regex DateMonthRegex = new(@"(\d{1,2})[/.](\d{4})", RegexOptions.Compiled);
    static readonly Regex YearOnlyRegex  = new(@"\b(19\d{2}|20\d{2})\b", RegexOptions.Compiled);

    public static SoftwarePageData Parse(int sourceId, string html)
    {
        var data = new SoftwarePageData { SourceId = sourceId };
        if(string.IsNullOrEmpty(html)) return data;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Name: typically the first H1, or the table's title row.
        HtmlNode h1 = doc.DocumentNode.SelectSingleNode("//h1");
        if(h1 != null) data.Name = DecodeAll(h1.InnerText).Trim();

        // Breadcrumb: top-of-page heading containing ">>"
        HtmlNode crumb = doc.DocumentNode.SelectSingleNode("//*[contains(text(),'>>')]");
        if(crumb != null)
        {
            var parts = new List<string>();
            int? lastCat = null;
            foreach(HtmlNode a in crumb.SelectNodes(".//a") ?? Enumerable.Empty<HtmlNode>())
            {
                Match mc = ListCatRegex.Match(HttpUtility.HtmlDecode(a.GetAttributeValue("href", "")));
                if(mc.Success)
                {
                    lastCat = int.Parse(mc.Groups[1].Value);
                    parts.Add(DecodeAll(a.InnerText).Trim());
                }
            }
            data.BreadcrumbPath = string.Join(" >> ", parts);
            data.CategoryId     = lastCat;
        }

        // Description + metadata sit in a 2-column table whose header row is
        // <td>Описание</td><td>Информация</td> (NO colons — they're header cells, not labels).
        // Find that header row, then read the next sibling's two TDs: left = description prose,
        // right = labeled metadata where the colon-suffixed labels (Операционная система:,
        // Разработчик:, Издатель:, Требования:) DO appear. The old text-only ExtractField search
        // for "Описание:" never matched on detail pages and silently left RussianDescription null,
        // which made the translate enricher no-op every row.
        HtmlNode descHeader = doc.DocumentNode.SelectSingleNode(
            "//tr[td[normalize-space()='Описание'] and td[normalize-space()='Информация']]");
        if(descHeader?.SelectSingleNode("following-sibling::tr[1]") is { } dataRow)
        {
            List<HtmlNode> tds = dataRow.SelectNodes("./td")?.ToList() ?? new List<HtmlNode>();
            if(tds.Count >= 1)
            {
                string leftHtml = tds[0].InnerHtml;
                // Convert <br> to newlines so the description preserves the line breaks the user wrote.
                leftHtml = Regex.Replace(leftHtml, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
                HtmlDocument leftDoc = new();
                leftDoc.LoadHtml(leftHtml);
                StripNoise(leftDoc);
                string leftText = DecodeAll(leftDoc.DocumentNode.InnerText);
                data.RussianDescription = Regex.Replace(leftText, @"[ \t]+", " ").Trim();
            }

            if(tds.Count >= 2)
            {
                // Right cell holds labels with colons; replace <br> with a separator so adjacent
                // labels stay searchable as "Label: value Label2:".
                string rightHtml = Regex.Replace(tds[1].InnerHtml, @"<br\s*/?>", " | ",
                                                 RegexOptions.IgnoreCase);
                HtmlDocument rightDoc = new();
                rightDoc.LoadHtml(rightHtml);
                // old-dos.ru pages include a Yandex sharing widget (<script>...</script>) right
                // after the metadata block. HtmlAgilityPack's InnerText concatenates that script
                // body as text, which made ExtractField walk well past the real OS value into
                // hundreds of bytes of JavaScript (the OsName column then overflowed at 256).
                StripNoise(rightDoc);
                string rightText = Regex.Replace(DecodeAll(rightDoc.DocumentNode.InnerText),
                                                 @"\s+", " ").Trim();

                data.OsName        = ExtractField(rightText, "Операционная система",
                                                   new[] { "Требования", "Разработчик", "Автор", "Издатель", "Раздел", "Добавил", "|" });
                data.DeveloperName = ExtractField(rightText, "Разработчик",
                                                   new[] { "Издатель", "Раздел", "Добавил", "|" });
                if(string.IsNullOrWhiteSpace(data.DeveloperName))
                    data.DeveloperName = ExtractField(rightText, "Автор",
                                                       new[] { "Издатель", "Раздел", "Добавил", "|" });
                data.PublisherName = ExtractField(rightText, "Издатель",
                                                   new[] { "Раздел", "Добавил", "|" });
                if(data.DeveloperName == "-") data.DeveloperName = null;
                if(data.PublisherName == "-") data.PublisherName = null;
                if(data.OsName        == "-") data.OsName        = null;

                // Hard cap to the column widths so a future parser bug truncates this row
                // instead of failing the entire category save (see OldDosSoftware.cs).
                data.OsName             = Cap(data.OsName,             256);
                data.DeveloperName      = Cap(data.DeveloperName,      512);
                data.PublisherName      = Cap(data.PublisherName,      512);
            }
        }

        data.Name           = Cap(data.Name,           512);
        data.BreadcrumbPath = Cap(data.BreadcrumbPath, 2048);

        // Versions: each file is a top-level <tr id="fileN"> row. The cell layout (after the
        // inner table that holds the download link) is: size, year (or full date), version,
        // language, quality, uploader. We anchor parsing on the row id and on the dl.php link
        // so columns are unambiguous (old-dos.ru does NOT use a do=download URL pattern; the
        // earlier text-only heuristic missed everything as a result).
        foreach(HtmlNode row in doc.DocumentNode.SelectNodes("//tr[starts-with(@id,'file')]") ??
                                Enumerable.Empty<HtmlNode>())
        {
            string rowId = row.GetAttributeValue("id", "");
            if(!Regex.IsMatch(rowId, "^file\\d+$")) continue;

            HtmlNode dl = row.SelectSingleNode(".//a[contains(@href,'dl.php')]");
            if(dl is null) continue;

            string href = HttpUtility.HtmlDecode(dl.GetAttributeValue("href", ""));

            // File name: the first plain-text <td> in the inner table (right of the link icon).
            string fileName = null;
            HtmlNode nameCell = row.SelectSingleNode(".//tr[1]/td[2]");
            if(nameCell != null)
                fileName = Regex.Replace(DecodeAll(nameCell.InnerText), @"\s+", " ").Trim();

            // Outer-row columns: the row's direct <td> children. The first wraps the inner table
            // (filename + download); subsequent siblings are size / year / version / language /
            // quality / uploader. We index by position relative to that.
            List<HtmlNode> outerTds = row.SelectNodes("./td")?.ToList() ?? new List<HtmlNode>();
            string sizeText    = outerTds.Count > 1 ? Clean(outerTds[1].InnerText) : null;
            string dateText    = outerTds.Count > 2 ? Clean(outerTds[2].InnerText) : null;
            string versionText = outerTds.Count > 3 ? Clean(outerTds[3].InnerText) : null;
            string langText    = outerTds.Count > 4 ? Clean(outerTds[4].InnerText) : null;

            var v = new SoftwareVersionEntry
            {
                DownloadUrl   = href,
                FileName      = fileName,
                VersionString = string.IsNullOrWhiteSpace(versionText) || versionText == "-"
                                    ? "(unspecified)"
                                    : versionText,
                Notes = BuildNotes(sizeText, langText)
            };

            (DateTime? dt, DatePrecision precision) = ExtractDate(dateText);
            v.ReleaseDate          = dt;
            v.ReleaseDatePrecision = precision;
            data.Versions.Add(v);
        }

        return data;
    }

    static string Clean(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : Regex.Replace(DecodeAll(s), @"\s+", " ").Trim();

    /// <summary>Cap a string to a maximum byte/char length; null in, null out.</summary>
    internal static string Cap(string s, int max) =>
        string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max);

    /// <summary>
    ///     Remove nodes whose text content would otherwise pollute <see cref="HtmlNode.InnerText" />.
    ///     old-dos.ru embeds a Yandex sharing widget (<c>&lt;script&gt;</c> + <c>&lt;noscript&gt;</c>) inside the
    ///     info cell that gets serialised verbatim into the text stream; without stripping it,
    ///     <see cref="ExtractField" /> can walk hundreds of bytes of JavaScript past the real
    ///     value (e.g. into the <c>OsName</c> column, which overflows at 256 chars).
    /// </summary>
    internal static void StripNoise(HtmlNode root)
    {
        foreach(HtmlNode n in root.SelectNodes(
                                  ".//script|.//style|.//noscript|.//iframe|.//img") ??
                              Enumerable.Empty<HtmlNode>())
            n.Remove();
    }

    internal static void StripNoise(HtmlDocument doc) => StripNoise(doc.DocumentNode);

    /// <summary>
    ///     HTML-decode repeatedly until the result is stable. old-dos.ru pages contain
    ///     double-encoded character entities for Latin-extended characters (e.g.
    ///     <c>J&amp;amp;#246;rg</c> in the source, which decodes once to the entity literal
    ///     <c>J&amp;#246;rg</c> and twice to <c>Jörg</c>). Cap at 4 passes so a hostile or
    ///     pathological input can never loop.
    /// </summary>
    public static string DecodeAll(string s)
    {
        if(string.IsNullOrEmpty(s)) return s;
        for(int i = 0; i < 4; i++)
        {
            string next = HttpUtility.HtmlDecode(s);
            if(string.Equals(next, s, StringComparison.Ordinal)) return s;
            s = next;
        }
        return s;
    }

    static string BuildNotes(string size, string lang)
    {
        var parts = new List<string>();
        if(!string.IsNullOrWhiteSpace(size)) parts.Add(size);
        if(!string.IsNullOrWhiteSpace(lang)) parts.Add(lang);
        return parts.Count == 0 ? null : string.Join(" • ", parts);
    }

    static string ExtractField(string text, string label, string[] stops)
    {
        int idx = text.IndexOf(label + ":", StringComparison.OrdinalIgnoreCase);
        if(idx < 0) return null;
        int start = idx + label.Length + 1;
        int end = text.Length;
        foreach(string s in stops)
        {
            int si = text.IndexOf(s, start, StringComparison.OrdinalIgnoreCase);
            if(si > 0 && si < end) end = si;
        }
        return Regex.Replace(text.Substring(start, end - start), @"\s+", " ").Trim().TrimEnd('|').Trim();
    }

    static (DateTime?, DatePrecision) ExtractDate(string rowText)
    {
        if(string.IsNullOrWhiteSpace(rowText) || rowText == "-" || rowText == "—")
            return (null, DatePrecision.Full);
        Match m = DateFullRegex.Match(rowText);
        if(m.Success)
        {
            if(DateTime.TryParseExact($"{m.Groups[1]}-{m.Groups[2].Value.PadLeft(2, '0')}-{m.Groups[3].Value.PadLeft(2, '0')}",
                                      "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                return (dt, DatePrecision.Full);
        }
        m = DateMonthRegex.Match(rowText);
        if(m.Success && int.TryParse(m.Groups[1].Value, out int mm) && int.TryParse(m.Groups[2].Value, out int yy) && mm is >= 1 and <= 12)
            return (new DateTime(yy, mm, 1), DatePrecision.MonthYear);
        m = YearOnlyRegex.Match(rowText);
        if(m.Success && int.TryParse(m.Groups[1].Value, out int year))
            return (new DateTime(year, 1, 1), DatePrecision.YearOnly);
        return (null, DatePrecision.Full);
    }
}
