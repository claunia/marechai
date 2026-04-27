using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers;

public static partial class ReleasesTabParser
{
    public static void Parse(HtmlDocument doc, ParsedGame game)
    {
        game.HasReleasesTab = true;

        var contentDiv = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'col-md-8')]");

        if(contentDiv is null) return;

        string currentPlatform = null;
        string currentPublisher   = null;
        string currentDeveloper   = null;
        string currentDistributor = null;
        string currentLocalizer   = null;
        var    currentExtraRoles  = new Dictionary<string, string>();

        foreach(var child in contentDiv.ChildNodes)
        {
            if(child.NodeType != HtmlNodeType.Element) continue;

            // Platform header
            if(child.Name == "h2")
            {
                currentPlatform    = WebUtility.HtmlDecode(child.InnerText).Trim();
                currentPublisher   = null;
                currentDeveloper   = null;
                currentDistributor = null;
                currentLocalizer   = null;
                currentExtraRoles.Clear();

                continue;
            }

            // Company role line: <div class="floatholder"><div class="fl">Published by</div><a>Company</a></div>
            if(child.Name == "div" &&
               child.GetAttributeValue("class", "").Contains("floatholder") &&
               !child.GetAttributeValue("class", "").Contains("relInfo"))
            {
                var roleDiv = child.SelectSingleNode("div[contains(@class,'fl')]");

                if(roleDiv != null)
                {
                    string roleLabel = WebUtility.HtmlDecode(roleDiv.InnerText).Trim();
                    var    companyLink = child.SelectSingleNode(".//a");
                    string companyName = companyLink != null
                                             ? WebUtility.HtmlDecode(companyLink.InnerText).Trim()
                                             : null;

                    switch(roleLabel)
                    {
                        case "Published by":
                            currentPublisher = companyName;
                            break;
                        case "Developed by":
                            currentDeveloper = companyName;
                            break;
                        case "Distributed by":
                            currentDistributor = companyName;
                            break;
                        case "Localized by":
                            currentLocalizer = companyName;
                            break;
                        default:
                            if(companyName != null)
                                currentExtraRoles[roleLabel] = companyName;
                            break;
                    }

                    continue;
                }
            }

            // Release block: <div style="margin: 0.25em ..."> containing relInfo divs
            if(child.Name == "div" && child.GetAttributeValue("style", "").Contains("margin:"))
            {
                var relInfoDivs = child.SelectNodes(".//div[contains(@class,'relInfo')]");

                if(relInfoDivs is null) continue;

                var release = new ParsedRelease
                {
                    Platform    = currentPlatform,
                    Publisher   = currentPublisher,
                    Developer   = currentDeveloper,
                    Distributor = currentDistributor,
                    Localizer   = currentLocalizer,
                    CompanyRoles = new Dictionary<string, string>(currentExtraRoles)
                };

                foreach(var relInfo in relInfoDivs)
                {
                    var titleDiv = relInfo.SelectSingleNode("div[contains(@class,'relInfoTitle')]");
                    var detailsDiv = relInfo.SelectSingleNode("div[contains(@class,'relInfoDetails')]");

                    if(titleDiv is null || detailsDiv is null) continue;

                    string title   = WebUtility.HtmlDecode(titleDiv.InnerText).Trim();
                    string details = WebUtility.HtmlDecode(detailsDiv.InnerText).Trim();

                    switch(title)
                    {
                        case "Country":
                        case "Countries":
                            // Extract country names from spans (strip flag imgs)
                            var spans = detailsDiv.SelectNodes(".//span");

                            if(spans != null)
                            {
                                foreach(var span in spans)
                                {
                                    // Get text, remove img alt text influence
                                    string countryText = WebUtility.HtmlDecode(span.InnerText).Trim()
                                                                   .TrimEnd(',').Trim();

                                    if(!string.IsNullOrWhiteSpace(countryText))
                                        release.Countries.Add(countryText);
                                }
                            }
                            else
                            {
                                release.Countries.Add(details);
                            }

                            break;

                        case "Release Date":
                            release.ReleaseDate = details;
                            break;

                        case "Comments":
                            release.Comments = details;
                            break;

                        case "UPC-A":
                            release.Barcodes.Add(new ParsedBarcode { Type = "UPC-A", Code = CleanBarcode(details) });
                            break;

                        case "EAN-13":
                            release.Barcodes.Add(new ParsedBarcode { Type = "EAN-13", Code = CleanBarcode(details) });
                            break;

                        case "Sony PN":
                            release.ProductCodes.Add(new ParsedProductCode { Type = "Sony PN", Code = details });
                            break;

                        case "PSN/SEN Code":
                            release.ProductCodes.Add(new ParsedProductCode { Type = "PSN/SEN Code", Code = details });
                            break;

                        default:
                            // Other product codes (Nintendo PN, Microsoft PN, etc.)
                            if(title.EndsWith("PN") || title.Contains("Code"))
                                release.ProductCodes.Add(new ParsedProductCode { Type = title, Code = details });

                            break;
                    }
                }

                game.Releases.Add(release);
            }
        }
    }

    static string CleanBarcode(string barcode) =>
        WhitespaceRegex().Replace(barcode, "").Replace("\u00a0", "");

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
