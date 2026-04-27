using System.Collections.Generic;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;
using Marechai.MobyGames.Parsers;

namespace Marechai.MobyGames.Services;

public static class GameAssembler
{
    public static ParsedGame Assemble(string gameId, List<MobyGamesRawRow> rows)
    {
        var game = new ParsedGame { MobyGameId = gameId };

        foreach(var row in rows)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(row.Body);

            MobyTab tab = TabDetector.Detect(row.Body);

            switch(tab)
            {
                case MobyTab.Main:
                    MainTabParser.Parse(doc, game);
                    break;
                case MobyTab.Credits:
                    CreditsTabParser.Parse(doc, game);
                    break;
                case MobyTab.Releases:
                    ReleasesTabParser.Parse(doc, game);
                    break;
                case MobyTab.Specs:
                    SpecsTabParser.Parse(doc, game);
                    break;
                case MobyTab.RatingSystems:
                    RatingsTabParser.Parse(doc, game);
                    break;
                // Skip tabs we don't import: Screenshots, CoverArt, PromoArt, Reviews, Trivia, AdBlurb, BuyTrade
            }
        }

        // If game name not set from Main tab, try extracting from any tab's h1
        if(string.IsNullOrWhiteSpace(game.Name) && rows.Count > 0)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(rows[0].Body);

            var h1 = doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'niceHeaderTitle')]//a");

            if(h1 != null)
                game.Name = System.Net.WebUtility.HtmlDecode(h1.InnerText).Trim();
        }

        return game;
    }
}
