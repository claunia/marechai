using System.Net;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers;

public static class SpecsTabParser
{
    public static void Parse(HtmlDocument doc, ParsedGame game)
    {
        game.HasSpecsTab = true;

        var tables = doc.DocumentNode.SelectNodes("//table[contains(@class,'techInfo')]");

        if(tables is null) return;

        foreach(var table in tables)
        {
            // Platform name from thead
            var thead = table.SelectSingleNode(".//thead//td");
            string platform = thead != null ? WebUtility.HtmlDecode(thead.InnerText).Trim() : "Unknown";

            var rows = table.SelectNodes(".//tr[@valign='middle' or @valign='top']");

            if(rows is null) continue;

            foreach(var row in rows)
            {
                // Skip thead rows
                if(row.SelectSingleNode("th") != null) continue;

                var cells = row.SelectNodes("td");

                if(cells is null || cells.Count < 2) continue;

                string key = WebUtility.HtmlDecode(cells[0].InnerText).Trim();

                if(string.IsNullOrWhiteSpace(key)) continue;

                // MobyGames renders multi-value spec cells as a series of <a> links
                // separated by commas, e.g. CPU = "<a>Intel 386</a>, <a>Intel 486</a>".
                // Concatenating cells[1].InnerText would bake the commas into the value
                // and break exact-match search via /software/by-spec. Split on the
                // anchor element structure (the source of truth — values themselves can
                // legitimately contain commas, e.g. notes), emitting one ParsedSpec per
                // anchor.
                var anchors = cells[1].SelectNodes(".//a");

                if(anchors is { Count: >= 2 })
                {
                    foreach(var a in anchors)
                    {
                        string anchorValue = WebUtility.HtmlDecode(a.InnerText).Trim();

                        if(string.IsNullOrWhiteSpace(anchorValue)) continue;

                        game.Specs.Add(new ParsedSpec
                        {
                            Platform = platform,
                            Key      = key,
                            Value    = anchorValue
                        });
                    }
                }
                else
                {
                    string value = WebUtility.HtmlDecode(cells[1].InnerText).Trim();

                    if(string.IsNullOrWhiteSpace(value)) continue;

                    game.Specs.Add(new ParsedSpec
                    {
                        Platform = platform,
                        Key      = key,
                        Value    = value
                    });
                }
            }
        }
    }
}
