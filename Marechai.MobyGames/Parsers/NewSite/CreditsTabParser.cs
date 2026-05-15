using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers.NewSite;

/// <summary>
///     Parses the per-platform Credits sub-page on the new MobyGames layout.
///     Each platform's credits live in a `&lt;section id="credits-platform-N"&gt;`
///     wrapping a single `table.table-credits` with two-column rows
///     (role label / comma-separated person anchors). Header rows split the
///     table into department groupings (Development, Sound, Voice Cast…)
///     which we don't preserve because the existing
///     <see cref="ParsedCredit" /> model only carries role + person.
/// </summary>
public static class CreditsTabParser
{
    public static void Parse(HtmlDocument doc, ParsedGame game)
    {
        game.HasCreditsTab = true;

        var tables = doc.DocumentNode.SelectNodes("//table[contains(@class,'table-credits')]");

        if(tables is null) return;

        foreach(var table in tables)
        {
            var rows = table.SelectNodes(".//tr");

            if(rows is null) continue;

            foreach(var row in rows)
            {
                // Skip section-header rows: they have <th colspan="2"><h4>…</h4></th>.
                if(row.SelectSingleNode("./th") != null) continue;

                var cells = row.SelectNodes("./td");

                if(cells is null || cells.Count < 2) continue;

                string role = WebUtility.HtmlDecode(cells[0].InnerText).Trim();

                if(string.IsNullOrEmpty(role)) continue;
                if(role.StartsWith("(C)") || role.StartsWith("©")) continue;

                // The new site uses /person/{id}/{slug}/ rather than the legacy /developer/.
                var personLinks = cells[1].SelectNodes(".//a[contains(@href,'/person/')]");

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
                    string text = WebUtility.HtmlDecode(cells[1].InnerText).Trim();

                    if(!string.IsNullOrWhiteSpace(text) && text != "All rights reserved")
                    {
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
}
