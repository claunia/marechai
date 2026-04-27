using System.Net;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers;

public static class RatingsTabParser
{
    public static void Parse(HtmlDocument doc, ParsedGame game)
    {
        game.HasRatingsTab = true;

        var contentDiv = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'col-md-8')]");

        if(contentDiv is null) return;

        string currentPlatform = null;

        foreach(var child in contentDiv.ChildNodes)
        {
            if(child.NodeType != HtmlNodeType.Element) continue;

            if(child.Name == "h2")
            {
                currentPlatform = WebUtility.HtmlDecode(child.InnerText).Trim();

                continue;
            }

            if(child.Name == "table" &&
               child.GetAttributeValue("summary", "").Contains("Rating"))
            {
                var rows = child.SelectNodes("tr");

                if(rows is null) continue;

                foreach(var row in rows)
                {
                    var cells = row.SelectNodes("td");

                    if(cells is null || cells.Count < 3) continue;

                    string system = WebUtility.HtmlDecode(cells[0].InnerText).Trim();
                    string ratingCell = cells[2].InnerText.Trim();

                    // Skip unknown ratings
                    if(ratingCell == "unknown") continue;

                    // Extract rating value — could be in link text
                    var ratingLink = cells[2].SelectSingleNode(".//a");
                    string rating = ratingLink != null
                                        ? WebUtility.HtmlDecode(ratingLink.InnerText).Trim()
                                        : WebUtility.HtmlDecode(ratingCell).Trim();

                    // Extract descriptors if present (text after parentheses)
                    string descriptors = null;
                    string fullText    = WebUtility.HtmlDecode(cells[2].InnerText);
                    int    parenStart  = fullText.IndexOf('(');

                    if(parenStart >= 0)
                    {
                        int parenEnd = fullText.LastIndexOf(')');

                        if(parenEnd > parenStart)
                        {
                            string inside = fullText.Substring(parenStart + 1, parenEnd - parenStart - 1);

                            // Remove label prefixes like "Descriptors: " or "Content Indicator: "
                            int colonPos = inside.IndexOf(':');

                            descriptors = colonPos >= 0
                                              ? inside[(colonPos + 1)..].Trim()
                                              : inside.Trim();
                        }
                    }

                    game.Ratings.Add(new ParsedRating
                    {
                        Platform    = currentPlatform,
                        System      = system,
                        Rating      = rating,
                        Descriptors = descriptors
                    });
                }
            }
        }
    }
}
