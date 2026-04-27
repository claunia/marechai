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

                string key   = WebUtility.HtmlDecode(cells[0].InnerText).Trim();
                string value = WebUtility.HtmlDecode(cells[1].InnerText).Trim();

                if(string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;

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
