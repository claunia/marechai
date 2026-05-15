using System.Collections.Generic;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;
using Marechai.MobyGames.Parsers;
using NewSite = Marechai.MobyGames.Parsers.NewSite;

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

            var (tab, layout) = TabDetector.DetectWithLayout(row.Body);

            if(layout == MobyLayout.New)
            {
                switch(tab)
                {
                    case MobyTab.Main:
                        NewSite.MainTabParser.Parse(doc, game);
                        break;
                    case MobyTab.Credits:
                        NewSite.CreditsTabParser.Parse(doc, game);
                        break;
                    case MobyTab.Releases:
                        NewSite.ReleasesTabParser.Parse(doc, game);
                        break;
                    case MobyTab.Specs:
                        // New-site Specs page also carries ratings; the parser fills both.
                        NewSite.SpecsTabParser.Parse(doc, game);
                        break;
                    // Skip media-only sub-pages here: Screenshots, CoverArt, PromoArt,
                    // Media, Reviews, Trivia — they're consumed by the dedicated
                    // scrapers in Marechai.MobyGames/Services/.
                }

                continue;
            }

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

        // If game name not set from Main tab, try extracting from any tab's h1.
        // Cover both layouts.
        if(string.IsNullOrWhiteSpace(game.Name) && rows.Count > 0)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(rows[0].Body);

            var oldH1 = doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'niceHeaderTitle')]//a");

            if(oldH1 != null)
                game.Name = System.Net.WebUtility.HtmlDecode(oldH1.InnerText).Trim();
            else
            {
                var newH1 = doc.DocumentNode.SelectSingleNode("//h1[contains(concat(' ',@class,' '),' mb-0 ')]");

                if(newH1 != null)
                    game.Name = System.Net.WebUtility.HtmlDecode(newH1.InnerText).Trim();
            }
        }

        return game;
    }
}
