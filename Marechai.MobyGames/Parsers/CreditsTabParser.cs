using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers;

public static class CreditsTabParser
{
    public static void Parse(HtmlDocument doc, ParsedGame game)
    {
        game.HasCreditsTab = true;

        // Credits are in a table with rows having class "crln"
        var creditRows = doc.DocumentNode.SelectNodes("//table[@summary='List of Credits']//tr[@class='crln']");

        if(creditRows is null) return;

        foreach(var row in creditRows)
        {
            var cells = row.SelectNodes("td");

            if(cells is null || cells.Count < 2) continue;

            string role = WebUtility.HtmlDecode(cells[0].InnerText).Trim();

            // Skip copyright notices
            if(role.StartsWith("(C)") || role.StartsWith("©")) continue;

            // Second cell may contain multiple people separated by commas
            var personLinks = cells[1].SelectNodes(".//a[contains(@href,'/developer/')]");

            if(personLinks != null)
            {
                foreach(var link in personLinks)
                {
                    string personName = WebUtility.HtmlDecode(link.InnerText).Trim();

                    if(!string.IsNullOrWhiteSpace(personName))
                    {
                        game.Credits.Add(new ParsedCredit
                        {
                            Role       = role,
                            PersonName = personName
                        });
                    }
                }
            }
            else
            {
                // No links — try plain text (rare but possible)
                string text = WebUtility.HtmlDecode(cells[1].InnerText).Trim();

                if(!string.IsNullOrWhiteSpace(text) && text != "All rights reserved")
                {
                    // Split by comma for multiple names
                    foreach(string name in text.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        game.Credits.Add(new ParsedCredit
                        {
                            Role       = role,
                            PersonName = name
                        });
                    }
                }
            }
        }
    }
}
