using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace Marechai.WinWorld.Parsers;

public sealed class ScreenshotIndexEntry
{
    public string SourceUrl { get; set; }   // /screenshot/{releaseUuid}/{shotUuid}
    public string ImageUrl  { get; set; }   // /res/img/screenshots/{hash}.png
    public string Caption   { get; set; }
}

public static class ScreenshotIndexParser
{
    /// <summary>
    ///     Parses <c>/screenshot/{releaseUuid}</c>. Each row inside
    ///     <c>&lt;div id="screenshotGallery"&gt;</c> contains anchors wrapping a thumbnail image
    ///     whose <c>alt</c> / <c>title</c> attribute is the caption.
    /// </summary>
    public static List<ScreenshotIndexEntry> Parse(string html)
    {
        var result = new List<ScreenshotIndexEntry>();
        if(string.IsNullOrEmpty(html)) return result;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        HtmlNode gallery = doc.GetElementbyId("screenshotGallery");
        if(gallery == null) return result;

        foreach(HtmlNode a in gallery.SelectNodes(".//a[starts-with(@href, '/screenshot/')]")
                          ?? Enumerable.Empty<HtmlNode>())
        {
            HtmlNode img = a.SelectSingleNode(".//img");
            if(img == null) continue;

            string href     = a.GetAttributeValue("href", "").Trim();
            string src      = img.GetAttributeValue("src", "").Trim();
            if(string.IsNullOrEmpty(src))
                src = img.GetAttributeValue("data-src", "").Trim();
            string caption  = img.GetAttributeValue("title", "");
            if(string.IsNullOrEmpty(caption))
                caption = img.GetAttributeValue("alt", "");

            if(string.IsNullOrEmpty(href)) continue;
            result.Add(new ScreenshotIndexEntry
            {
                SourceUrl = href,
                ImageUrl  = src,
                Caption   = WebUtility.HtmlDecode(caption ?? "").Trim()
            });
        }

        return result;
    }
}
