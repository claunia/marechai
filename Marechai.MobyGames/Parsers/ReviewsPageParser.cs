using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers;

public static partial class ReviewsPageParser
{
    /// <summary>
    ///     Parse critic reviews from the old MobyGames cached HTML (Reviews tab).
    ///     Each review is a <![CDATA[<div class="floatholder mobyrank scoresource">]]> block.
    /// </summary>
    public static List<ParsedCriticReview> Parse(string html)
    {
        var reviews = new List<ParsedCriticReview>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Find all critic review blocks
        var reviewNodes = doc.DocumentNode.SelectNodes(
            "//div[contains(@class,'mobyrank') and contains(@class,'scoresource')]");

        if(reviewNodes is null)
            return reviews;

        foreach(var node in reviewNodes)
        {
            var review = new ParsedCriticReview();

            // Score: <div class="fl scoreBoxMed scoreHi">96</div>
            var scoreNode = node.SelectSingleNode(".//div[contains(@class,'scoreBoxMed') or contains(@class,'scoreBoxBig')]");

            if(scoreNode != null)
            {
                string scoreText = WebUtility.HtmlDecode(scoreNode.InnerText).Trim();

                if(int.TryParse(scoreText, out int score))
                    review.NormalizedScore = score;

                // else: unscored (contains &nbsp; or empty)
            }

            // Source div: <div class="source ..."><span class="fr">DOS</span><a href="...">Name</a> (Jan, 1990)</div>
            var sourceDiv = node.SelectSingleNode(".//div[contains(@class,'source')]");

            if(sourceDiv != null)
            {
                // Platform: <span class="fr">DOS</span>
                var platformSpan = sourceDiv.SelectSingleNode(".//span[contains(@class,'fr')]");

                if(platformSpan != null)
                    review.PlatformName = WebUtility.HtmlDecode(platformSpan.InnerText).Trim();

                // Publication name: <a href="...sourceId,...">Name</a>
                var publicationLink = sourceDiv.SelectSingleNode(".//a[contains(@href,'mobyrank/source') or contains(@href,'sourceId')]");

                if(publicationLink != null)
                    review.PublicationName = WebUtility.HtmlDecode(publicationLink.InnerText).Trim();

                // Date: text after the </a> in parentheses, e.g. " (Jan, 1990)" or " (1990)"
                string sourceText = WebUtility.HtmlDecode(sourceDiv.InnerText).Trim();
                var dateMatch = DateRegex().Match(sourceText);

                if(dateMatch.Success)
                    review.ReviewDate = dateMatch.Groups[1].Value.Trim();
            }

            // Review text: <div class="citation">...</div>
            var citationNode = node.SelectSingleNode(".//div[contains(@class,'citation')]");

            if(citationNode != null)
            {
                string text = citationNode.InnerHtml.Trim();

                // Convert <br> to newlines and strip remaining HTML
                text = text.Replace("<br>",  "\n").Replace("<BR>",  "\n")
                           .Replace("<br/>", "\n").Replace("<br />", "\n");

                text = WebUtility.HtmlDecode(HtmlEntity.DeEntitize(
                    Regex.Replace(text, "<[^>]+>", "")));

                review.ReviewText = text.Trim();
            }

            // URL (optional): <div class="url"><a target="_blank" href="...">read review</a></div>
            var urlDiv = node.SelectSingleNode(".//div[contains(@class,'url')]");

            if(urlDiv != null)
            {
                var urlLink = urlDiv.SelectSingleNode(".//a[@href]");

                if(urlLink != null)
                    review.ReviewUrl = urlLink.GetAttributeValue("href", null);
            }

            // Only add if we have a publication name
            if(!string.IsNullOrWhiteSpace(review.PublicationName))
                reviews.Add(review);
        }

        return reviews;
    }

    /// <summary>Matches date in parentheses: (Jan, 1990) or (1990) or (Apr, 1994)</summary>
    [GeneratedRegex(@"\(([^)]+)\)\s*$", RegexOptions.Compiled)]
    private static partial Regex DateRegex();
}
