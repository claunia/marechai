using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers.NewSite;

/// <summary>
///     Parses the per-platform Releases sub-page on the new MobyGames layout.
///     Each platform is introduced by an <c>&lt;h4&gt;</c> followed by one or
///     more <c>table.releaseTable</c>s. Inside a table, rows split into:
///     a <c>tr.bg-dark</c> header carrying the release date, then label/value
///     <c>&lt;td&gt;</c> pairs for Published by / Developed by / Distributed
///     by / Localized by / Countries / Comments / product codes.
/// </summary>
public static partial class ReleasesTabParser
{
    public static void Parse(HtmlDocument doc, ParsedGame game)
    {
        game.HasReleasesTab = true;

        // Walk the document looking for h4 headings paired with releaseTable siblings.
        var h4s = doc.DocumentNode.SelectNodes("//h4");

        if(h4s is null) return;

        foreach(var h4 in h4s)
        {
            // The "+" suffix on the h4 is a contribute-link decoration; strip it.
            string platform = WebUtility.HtmlDecode(h4.GetDirectInnerText()).Trim().TrimEnd('+').Trim();

            if(string.IsNullOrEmpty(platform)) continue;

            // Find ALL releaseTable siblings up to the next h4 — each table is a release.
            var sib = h4.NextSibling;

            while(sib != null)
            {
                if(sib.NodeType == HtmlNodeType.Element)
                {
                    if(sib.Name == "h4") break;

                    if(sib.Name == "table" && sib.GetAttributeValue("class", "").Contains("releaseTable"))
                    {
                        ParseReleaseTable(sib, platform, game);
                    }
                }

                sib = sib.NextSibling;
            }
        }
    }

    static void ParseReleaseTable(HtmlNode table, string platform, ParsedGame game)
    {
        var release = new ParsedRelease { Platform = platform };

        var rows = table.SelectNodes("./tr");

        if(rows is null) return;

        foreach(var tr in rows)
        {
            // Header row: <tr class="bg-dark"><td colspan="2"><b>Mar 3, 2023 Release</b></td></tr>
            if(tr.GetAttributeValue("class", "").Contains("bg-dark"))
            {
                string headerText = WebUtility.HtmlDecode(tr.InnerText).Trim();
                // Drop trailing " Release" word so the date string matches the old shape.
                if(headerText.EndsWith(" Release")) headerText = headerText[..^" Release".Length];

                release.ReleaseDate = headerText;

                continue;
            }

            var cells = tr.SelectNodes("./td");

            if(cells is null || cells.Count < 2) continue;

            string label = WebUtility.HtmlDecode(cells[0].InnerText).Trim().TrimEnd(':').Trim();
            var    val   = cells[1];
            string valueText = WebUtility.HtmlDecode(val.InnerText).Trim();

            switch(label)
            {
                case "Published by":
                    release.Publisher = FirstAnchorText(val) ?? valueText;

                    break;
                case "Developed by":
                    release.Developer = FirstAnchorText(val) ?? valueText;

                    break;
                case "Distributed by":
                    release.Distributor = FirstAnchorText(val) ?? valueText;

                    break;
                case "Localized by":
                    release.Localizer = FirstAnchorText(val) ?? valueText;

                    break;
                case "Country":
                case "Countries":
                    // Strip the flag image — its alt text would otherwise duplicate.
                    foreach(var img in val.SelectNodes(".//img") ?? Enumerable.Empty<HtmlNode>())
                        img.Remove();

                    string countriesText = WebUtility.HtmlDecode(val.InnerText).Trim();

                    foreach(string c in countriesText.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)))
                        release.Countries.Add(c);

                    break;
                case "Comments":
                    release.Comments = valueText;

                    break;
                case "UPC-A":
                    release.Barcodes.Add(new ParsedBarcode { Type = "UPC-A", Code = CleanBarcode(valueText) });

                    break;
                case "EAN-13":
                    release.Barcodes.Add(new ParsedBarcode { Type = "EAN-13", Code = CleanBarcode(valueText) });

                    break;
                case "Sony PN":
                    release.ProductCodes.Add(new ParsedProductCode { Type = "Sony PN", Code = valueText });

                    break;
                case "PSN/SEN Code":
                    release.ProductCodes.Add(new ParsedProductCode { Type = "PSN/SEN Code", Code = valueText });

                    break;
                default:
                    if(label.EndsWith("PN") || label.Contains("Code"))
                        release.ProductCodes.Add(new ParsedProductCode { Type = label, Code = valueText });
                    else if(!string.IsNullOrWhiteSpace(label) && !string.IsNullOrWhiteSpace(valueText))
                    {
                        // Generic company-role row (e.g. "Recorded by", "Mastered by") not
                        // covered by the well-known labels above.
                        string company = FirstAnchorText(val);

                        if(!string.IsNullOrWhiteSpace(company) && label.EndsWith(" by"))
                            release.CompanyRoles[label] = company;
                    }

                    break;
            }
        }

        // Only record the release if we got something useful out of it (date OR at least one
        // non-platform field). An empty all-null release row just clutters the import.
        if(!string.IsNullOrEmpty(release.ReleaseDate) || release.Publisher != null ||
           release.Developer    != null              || release.Countries.Count > 0)
            game.Releases.Add(release);
    }

    static string FirstAnchorText(HtmlNode cell)
    {
        var a = cell.SelectSingleNode(".//a");

        if(a is null) return null;

        string txt = WebUtility.HtmlDecode(a.InnerText).Trim();

        return string.IsNullOrWhiteSpace(txt) ? null : txt;
    }

    static string CleanBarcode(string barcode) =>
        WhitespaceRegex().Replace(barcode, "").Replace("\u00a0", "");

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
