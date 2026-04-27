using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers;

public static class MainTabParser
{
    public static void Parse(HtmlDocument doc, ParsedGame game)
    {
        game.HasMainTab = true;

        ParseGameName(doc, game);
        ParseCoreInfo(doc, game);
        ParseGenres(doc, game);
        ParseDescription(doc, game);
        ParseGroups(doc, game);
    }

    static void ParseGameName(HtmlDocument doc, ParsedGame game)
    {
        var h1 = doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'niceHeaderTitle')]");

        if(h1 is null) return;

        var a = h1.SelectSingleNode("a");

        game.Name = a != null ? WebUtility.HtmlDecode(a.InnerText).Trim() : WebUtility.HtmlDecode(h1.GetDirectInnerText()).Trim();
    }

    static void ParseCoreInfo(HtmlDocument doc, ParsedGame game)
    {
        var releaseDiv = doc.DocumentNode.SelectSingleNode("//*[@id='coreGameRelease']");

        if(releaseDiv is null) return;

        var boldDivs = releaseDiv.SelectNodes("div[contains(@style,'font-weight: bold')]");

        if(boldDivs is null) return;

        foreach(var boldDiv in boldDivs)
        {
            string label = WebUtility.HtmlDecode(boldDiv.InnerText).Trim();
            var    valueDiv = boldDiv.NextSibling;

            while(valueDiv != null && valueDiv.NodeType != HtmlNodeType.Element)
                valueDiv = valueDiv.NextSibling;

            if(valueDiv is null) continue;

            string value = WebUtility.HtmlDecode(valueDiv.InnerText).Trim();

            switch(label)
            {
                case "Published by":
                    var pubLinks = valueDiv.SelectNodes(".//a");
                    if(pubLinks != null)
                        game.Publishers = pubLinks
                                         .Select(a => WebUtility.HtmlDecode(a.InnerText).Trim())
                                         .Where(s => !string.IsNullOrWhiteSpace(s))
                                         .ToList();
                    else if(!string.IsNullOrWhiteSpace(value))
                        game.Publishers = [value];
                    break;
                case "Developed by":
                    var devLinks = valueDiv.SelectNodes(".//a");
                    if(devLinks != null)
                        game.Developers = devLinks
                                         .Select(a => WebUtility.HtmlDecode(a.InnerText).Trim())
                                         .Where(s => !string.IsNullOrWhiteSpace(s))
                                         .ToList();
                    else if(!string.IsNullOrWhiteSpace(value))
                        game.Developers = [value];
                    break;
                case "Released":
                    game.ReleaseDate = value;
                    break;
                case "Platform":
                case "Platforms":
                    var platformLinks = valueDiv.SelectNodes(".//a");
                    if(platformLinks != null)
                        game.Platforms = platformLinks
                                       .Select(a => WebUtility.HtmlDecode(a.InnerText).Trim())
                                       .Where(s => !string.IsNullOrWhiteSpace(s))
                                       .Distinct()
                                       .ToList();
                    else
                        game.Platforms = [value];

                    break;
            }
        }
    }

    static void ParseGenres(HtmlDocument doc, ParsedGame game)
    {
        var genreDiv = doc.DocumentNode.SelectSingleNode("//*[@id='coreGameGenre']");

        if(genreDiv is null) return;

        var boldDivs = genreDiv.SelectNodes(".//div[contains(@style,'font-weight: bold')]");

        if(boldDivs is null) return;

        foreach(var boldDiv in boldDivs)
        {
            string genreType = WebUtility.HtmlDecode(boldDiv.InnerText).Trim();
            var    valueDiv  = boldDiv.NextSibling;

            while(valueDiv != null && valueDiv.NodeType != HtmlNodeType.Element)
                valueDiv = valueDiv.NextSibling;

            if(valueDiv is null) continue;

            var links = valueDiv.SelectNodes(".//a");

            if(links is null) continue;

            foreach(var link in links)
            {
                string name = WebUtility.HtmlDecode(link.InnerText).Trim();

                if(!string.IsNullOrWhiteSpace(name))
                {
                    game.Genres.Add(new ParsedGenre
                    {
                        Type = genreType,
                        Name = name
                    });
                }
            }
        }
    }

    static void ParseDescription(HtmlDocument doc, ParsedGame game)
    {
        var descH2 = doc.DocumentNode.SelectSingleNode("//h2[text()='Description']");

        if(descH2 is null) return;

        // Collect text nodes and br tags between Description h2 and the next h2 or sideBarLinks div
        var    sibling     = descH2.NextSibling;
        var    parts       = new List<string>();

        while(sibling != null)
        {
            // Stop at next structural element
            if(sibling.NodeType == HtmlNodeType.Element)
            {
                string tag = sibling.Name.ToLowerInvariant();

                if(tag is "h2") break;

                if(sibling.GetAttributeValue("class", "").Contains("sideBarLinks")) break;

                if(tag == "br")
                {
                    parts.Add("\n");
                }
                else if(tag == "i" || tag == "b" || tag == "em" || tag == "strong" || tag == "a")
                {
                    parts.Add(WebUtility.HtmlDecode(sibling.InnerText));
                }
                else
                {
                    parts.Add(WebUtility.HtmlDecode(sibling.InnerText));
                }
            }
            else if(sibling.NodeType == HtmlNodeType.Text)
            {
                parts.Add(WebUtility.HtmlDecode(sibling.InnerText));
            }

            sibling = sibling.NextSibling;
        }

        game.Description = string.Join("", parts).Trim();
    }

    static void ParseGroups(HtmlDocument doc, ParsedGame game)
    {
        var groupsH2 = doc.DocumentNode.SelectSingleNode("//h2[text()='Part of the Following Groups']");

        if(groupsH2 is null) return;

        var ul = groupsH2.NextSibling;

        while(ul != null && ul.Name != "ul")
            ul = ul.NextSibling;

        if(ul is null) return;

        var links = ul.SelectNodes(".//a");

        if(links is null) return;

        foreach(var link in links)
        {
            string name = WebUtility.HtmlDecode(link.InnerText).Trim();

            if(!string.IsNullOrWhiteSpace(name))
                game.Groups.Add(name);
        }
    }
}
