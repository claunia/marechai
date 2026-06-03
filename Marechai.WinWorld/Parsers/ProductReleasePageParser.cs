using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Marechai.WinWorld.Parsers;

public sealed class ProductReleaseLink
{
    public string Label { get; set; }   // "1.x", "Windows 95"
    public string Url   { get; set; }   // "/product/adobe-illustrator/1x"
    public bool   IsActive { get; set; }
}

public sealed class ProductDownload
{
    public string VersionString { get; set; }    // "1.0 for Windows"
    public string Language      { get; set; }    // "English"
    public string Architecture  { get; set; }    // "x86"
    public string MediaKind     { get; set; }    // "3½ Floppy"
    public string SizeText      { get; set; }    // "3MB"
    public string DownloadUrl   { get; set; }    // "/download/{uuid}"
}

public sealed class ProductReleaseScreenshot
{
    public string SourceUrl { get; set; }    // "/screenshot/{releaseUuid}/{shotUuid}"
    public string ImageUrl  { get; set; }    // "/res/img/screenshots/{hash}.png"
    public string Caption   { get; set; }
}

public sealed class ProductReleasePageData
{
    /// <summary>Display name of the product (e.g. "Adobe Illustrator"); the version suffix in &lt;small&gt; is stripped.</summary>
    public string                          ProductName        { get; set; }
    /// <summary>Major release label as shown next to the title (e.g. "1.x", "Windows 95").</summary>
    public string                          CurrentReleaseLabel { get; set; }
    /// <summary>Sanitized plain-text description scraped from the descriptionColumn.</summary>
    public string                          Description        { get; set; }
    /// <summary>Right-hand "Product type" anchor text ("Application" / "Development tool" / "System tool" / "Game" / "Operating System").</summary>
    public string                          ProductTypeText    { get; set; }
    public List<string>                    Categories         { get; set; } = new();
    public string                          VendorName         { get; set; }
    public string                          VendorUrl          { get; set; }
    public string                          ReleaseDateText    { get; set; }
    public string                          UserInterface      { get; set; }
    public List<string>                    Platforms          { get; set; } = new();
    public List<ProductReleaseLink>        Releases           { get; set; } = new();
    public string                          ScreenshotIndexUrl { get; set; }
    public List<ProductDownload>           Downloads          { get; set; } = new();
}

public static class ProductReleasePageParser
{
    public static ProductReleasePageData Parse(string html)
    {
        var data = new ProductReleasePageData();
        if(string.IsNullOrEmpty(html)) return data;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // === Title ===
        HtmlNode h1 = doc.DocumentNode.SelectSingleNode("//div[@id='descriptionColumn']//h1");
        if(h1 != null)
        {
            HtmlNode small = h1.SelectSingleNode(".//small");
            if(small != null)
            {
                data.CurrentReleaseLabel = WebUtility.HtmlDecode(small.InnerText).Trim();
                small.Remove();
            }
            data.ProductName = WebUtility.HtmlDecode(h1.InnerText).Trim();
        }

        // === Description ===
        HtmlNode descColumn = doc.GetElementbyId("descriptionColumn");
        if(descColumn != null)
        {
            var sb = new StringBuilder();
            foreach(HtmlNode child in descColumn.ChildNodes)
            {
                if(child.NodeType != HtmlNodeType.Element) continue;
                // Description is the leading <p> blocks before the releaseInformation card / ads / screenshot panel.
                if(child.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                {
                    string text = WebUtility.HtmlDecode(child.InnerText).Trim();
                    if(text.Length > 0)
                    {
                        if(sb.Length > 0) sb.Append("\n\n");
                        sb.Append(text);
                    }
                }
                else if(child.Name.Equals("div", StringComparison.OrdinalIgnoreCase) &&
                        child.GetAttributeValue("id", "") == "releaseInformation")
                    break;
                else if(child.Name.Equals("h3", StringComparison.OrdinalIgnoreCase))
                    break;
            }
            data.Description = sb.ToString().Trim();
        }

        // === Available releases ===
        HtmlNode releasesList = doc.GetElementbyId("releasesList");
        if(releasesList != null)
        {
            foreach(HtmlNode a in releasesList.SelectNodes(".//a[@href]") ?? Enumerable.Empty<HtmlNode>())
            {
                string label = WebUtility.HtmlDecode(a.InnerText).Trim();
                // Strip the "(current)" sr-only suffix.
                label = Regex.Replace(label, @"\s*\(current\)\s*$", "").Trim();
                string href     = a.GetAttributeValue("href", "").Trim();
                bool   isActive = (a.GetAttributeValue("class", "") ?? "").Contains("active");
                if(!string.IsNullOrEmpty(href) && !string.IsNullOrEmpty(label))
                    data.Releases.Add(new ProductReleaseLink { Label = label, Url = href, IsActive = isActive });
            }
        }

        // === Information card ===
        HtmlNode infoSheet = doc.GetElementbyId("infoSheetBody");
        if(infoSheet != null)
        {
            HtmlNode cur = infoSheet.FirstChild;
            while(cur != null)
            {
                if(cur.NodeType == HtmlNodeType.Element && cur.Name.Equals("dt", StringComparison.OrdinalIgnoreCase))
                {
                    string label = WebUtility.HtmlDecode(cur.InnerText).Trim();
                    HtmlNode dd  = NextElement(cur, "dd");
                    if(dd != null) ApplyInfoField(data, label, dd);
                }
                cur = cur.NextSibling;
            }
        }

        // === Screenshot index URL ===
        HtmlNode shotHeader =
            doc.DocumentNode.SelectSingleNode("//h3/a[starts-with(@href, '/screenshot/')]");
        if(shotHeader != null)
            data.ScreenshotIndexUrl = shotHeader.GetAttributeValue("href", "").Trim();

        // === Downloads ===
        HtmlNode downloadsTable = doc.GetElementbyId("downloadsTable");
        if(downloadsTable != null)
        {
            foreach(HtmlNode tr in downloadsTable.SelectNodes(".//tbody/tr") ?? Enumerable.Empty<HtmlNode>())
            {
                List<HtmlNode> tds = tr.SelectNodes("./td")?.ToList();
                if(tds == null || tds.Count < 4) continue;

                var dl = new ProductDownload();
                // Cell 0: name + media icon. The <img title="..."> carries the media kind.
                HtmlNode anchor = tds[0].SelectSingleNode(".//a[@href]");
                if(anchor != null) dl.DownloadUrl = anchor.GetAttributeValue("href", "").Trim();
                HtmlNode mediaImg = tds[0].SelectSingleNode(".//img[@title]");
                if(mediaImg != null) dl.MediaKind = mediaImg.GetAttributeValue("title", "").Trim();

                // Cell 1: version label (text content; badges are span siblings without text).
                dl.VersionString = ExtractTextOnly(tds[1]);
                // Cell 2: language
                if(tds.Count > 2) dl.Language = WebUtility.HtmlDecode(tds[2].InnerText).Trim();
                // Cell 3: architecture (image title)
                if(tds.Count > 3)
                {
                    HtmlNode archImg = tds[3].SelectSingleNode(".//img[@title]");
                    dl.Architecture = archImg?.GetAttributeValue("title", "").Trim()
                                   ?? WebUtility.HtmlDecode(tds[3].InnerText).Trim();
                }
                // Cell 4: file size
                if(tds.Count > 4) dl.SizeText = WebUtility.HtmlDecode(tds[4].InnerText).Trim();

                if(!string.IsNullOrEmpty(dl.VersionString) || !string.IsNullOrEmpty(dl.DownloadUrl))
                    data.Downloads.Add(dl);
            }
        }

        return data;
    }

    static void ApplyInfoField(ProductReleasePageData data, string label, HtmlNode dd)
    {
        string l = label.TrimEnd(':').Trim();

        switch(l.ToLowerInvariant())
        {
            case "product type":
                // First badge = product type ("Application"); the rest are category chips.
                List<HtmlNode> badges = dd.SelectNodes(".//a[contains(@class, 'badge')]")?.ToList()
                                     ?? new List<HtmlNode>();
                if(badges.Count > 0)
                {
                    data.ProductTypeText = WebUtility.HtmlDecode(badges[0].InnerText).Trim();
                    for(int i = 1; i < badges.Count; i++)
                    {
                        string c = WebUtility.HtmlDecode(badges[i].InnerText).Trim();
                        if(!string.IsNullOrEmpty(c) &&
                           !data.Categories.Contains(c, StringComparer.OrdinalIgnoreCase))
                            data.Categories.Add(c);
                    }
                }
                break;

            case "vendor":
                HtmlNode v = dd.SelectSingleNode(".//a[@href]");
                if(v != null)
                {
                    data.VendorName = WebUtility.HtmlDecode(v.InnerText).Trim();
                    data.VendorUrl  = v.GetAttributeValue("href", "").Trim();
                }
                else
                    data.VendorName = WebUtility.HtmlDecode(dd.InnerText).Trim();
                break;

            case "release date":
                data.ReleaseDateText = WebUtility.HtmlDecode(dd.InnerText).Trim();
                break;

            case "user interface":
                data.UserInterface = WebUtility.HtmlDecode(dd.InnerText).Trim();
                break;

            case "platform":
            case "platforms":
                foreach(HtmlNode b in dd.SelectNodes(".//a[contains(@class, 'badge')]")
                                   ?? Enumerable.Empty<HtmlNode>())
                {
                    string p = WebUtility.HtmlDecode(b.InnerText).Trim();
                    if(!string.IsNullOrEmpty(p) &&
                       !data.Platforms.Contains(p, StringComparer.OrdinalIgnoreCase))
                        data.Platforms.Add(p);
                }
                break;
        }
    }

    static HtmlNode NextElement(HtmlNode after, string name)
    {
        HtmlNode n = after.NextSibling;
        while(n != null && (n.NodeType != HtmlNodeType.Element ||
                            !n.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            n = n.NextSibling;
        return n;
    }

    static string ExtractTextOnly(HtmlNode node)
    {
        var sb = new StringBuilder();
        foreach(HtmlNode child in node.ChildNodes)
        {
            if(child.NodeType == HtmlNodeType.Text)
                sb.Append(child.InnerText);
            else if(child.NodeType == HtmlNodeType.Element &&
                    !child.Name.Equals("span", StringComparison.OrdinalIgnoreCase))
                sb.Append(child.InnerText);
        }
        return Regex.Replace(WebUtility.HtmlDecode(sb.ToString()).Trim(), @"\s+", " ");
    }
}
