using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers.NewSite;

/// <summary>
///     Parses the combined Specs + Ratings page on the new MobyGames layout.
///     The new site collapses the legacy `Specs` and `RatingSystems` tabs into
///     a single page, with two `&lt;h2&gt;` sections — "Tech Specs/Attributes"
///     and "Ratings" — each followed by a `&lt;table class="table"&gt;`.
///     This parser fills both <see cref="ParsedGame.Specs" /> and
///     <see cref="ParsedGame.Ratings" /> in one pass and sets both
///     <see cref="ParsedGame.HasSpecsTab" /> and
///     <see cref="ParsedGame.HasRatingsTab" />.
/// </summary>
public static partial class SpecsTabParser
{
    public static void Parse(HtmlDocument doc, ParsedGame game)
    {
        game.HasSpecsTab   = true;
        game.HasRatingsTab = true;

        // Locate the two section headings.
        var h2s = doc.DocumentNode.SelectNodes("//h2");

        if(h2s is null) return;

        foreach(var h2 in h2s)
        {
            string headingText = WebUtility.HtmlDecode(h2.InnerText).Trim().TrimEnd('+').Trim();
            bool   isSpecs     = headingText.StartsWith("Tech Specs", StringComparison.OrdinalIgnoreCase);
            bool   isRatings   = headingText.StartsWith("Ratings",     StringComparison.OrdinalIgnoreCase);

            if(!isSpecs && !isRatings) continue;

            // Walk forward to the next <table> sibling.
            var table = h2.NextSibling;

            while(table != null && (table.NodeType != HtmlNodeType.Element || table.Name != "table"))
                table = table.NextSibling;

            if(table is null) continue;

            ParseTable(table, game, isSpecs);
        }
    }

    static void ParseTable(HtmlNode table, ParsedGame game, bool isSpecs)
    {
        string currentPlatform = null;

        var rows = table.SelectNodes("./tr");

        if(rows is null) return;

        foreach(var tr in rows)
        {
            // Platform-header row: contains an <h4> nested under <th> or <td colspan="2">.
            var h4 = tr.SelectSingleNode(".//h4");

            if(h4 != null)
            {
                // Strip the "+" contribute-link suffix.
                string ptext = WebUtility.HtmlDecode(h4.GetDirectInnerText()).Trim().TrimEnd('+').Trim();

                if(!string.IsNullOrEmpty(ptext)) currentPlatform = ptext;

                continue;
            }

            var cells = tr.SelectNodes("./td");

            if(cells is null || cells.Count < 2) continue;

            string label = WebUtility.HtmlDecode(cells[0].InnerText).Trim().TrimEnd(':').Trim();
            var    valueCell = cells[1];

            if(string.IsNullOrEmpty(label)) continue;

            if(isSpecs)
                EmitSpecRow(label, valueCell, currentPlatform, game);
            else
                EmitRatingRow(label, valueCell, currentPlatform, game);
        }
    }

    // ----------------------------------------------------------------------
    // Spec values are usually a ul.commaList of <li><a>Value</a></li>, with
    // optional <img> flags before the anchor. Each value becomes its own
    // ParsedSpec row (matching the legacy parser's "split by anchor" rule).
    // ----------------------------------------------------------------------
    static void EmitSpecRow(string label, HtmlNode cell, string platform, ParsedGame game)
    {
        var anchors = cell.SelectNodes(".//a");

        if(anchors is { Count: >= 1 })
        {
            foreach(var a in anchors)
            {
                string v = WebUtility.HtmlDecode(a.InnerText).Trim();

                if(!string.IsNullOrEmpty(v))
                {
                    game.Specs.Add(new ParsedSpec
                    {
                        Platform = platform ?? "Unknown",
                        Key      = label,
                        Value    = v
                    });
                }
            }

            return;
        }

        string text = WebUtility.HtmlDecode(cell.InnerText).Trim();

        if(string.IsNullOrEmpty(text)) return;

        game.Specs.Add(new ParsedSpec
        {
            Platform = platform ?? "Unknown",
            Key      = label,
            Value    = text
        });
    }

    // ----------------------------------------------------------------------
    // Rating rows look like:
    //   <ul class="commaList"><li><img/><a>Mature</a></li></ul>
    //   ( <a>Blood and Gore</a>, <a>In-game Purchases</a> ... )
    // The first ul is the rating; the parenthesised tail is the descriptor list.
    // ----------------------------------------------------------------------
    static void EmitRatingRow(string label, HtmlNode cell, string platform, ParsedGame game)
    {
        // Drop the " Rating" suffix from the label so "ESRB Rating" → "ESRB", to match
        // the shape stored by the legacy parser.
        string system = label;

        if(system.EndsWith(" Rating", StringComparison.OrdinalIgnoreCase))
            system = system[..^" Rating".Length].Trim();

        if(string.IsNullOrEmpty(system)) return;

        // Rating value — first anchor inside the first commaList ul.
        var firstUlAnchor = cell.SelectSingleNode(".//ul[contains(@class,'commaList')]//a");
        string rating = firstUlAnchor != null
                            ? WebUtility.HtmlDecode(firstUlAnchor.InnerText).Trim()
                            : null;

        if(string.IsNullOrEmpty(rating))
        {
            // Fall back to plain text, ignoring images.
            string text = WebUtility.HtmlDecode(cell.InnerText).Trim();

            if(string.IsNullOrEmpty(text) || string.Equals(text, "unknown", StringComparison.OrdinalIgnoreCase))
                return;

            rating = text;
        }

        // Descriptors — the contents of the trailing parenthesised block. We work
        // off the cell text and prefer anchor text (so flag-image alt text doesn't
        // leak into the value), falling back to the raw inside-parens text.
        string descriptors = null;

        string fullText = WebUtility.HtmlDecode(cell.InnerText);
        int    open     = fullText.IndexOf('(');
        int    close    = fullText.LastIndexOf(')');

        if(open >= 0 && close > open)
        {
            // Pull anchor texts from anchors that sit AFTER the first commaList ul.
            string inside = fullText[(open + 1)..close];

            // If there are any anchors at all in the cell, attempt to enumerate descriptor anchors:
            var allAnchors = cell.SelectNodes(".//a");
            var firstUl    = cell.SelectSingleNode(".//ul[contains(@class,'commaList')]");

            if(allAnchors != null)
            {
                var descAnchors = allAnchors.SkipWhile(a => firstUl != null && a.Ancestors().Contains(firstUl));
                var names = descAnchors
                    .Select(a => WebUtility.HtmlDecode(a.InnerText).Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                if(names.Count > 0)
                {
                    descriptors = string.Join(", ", names);
                }
            }

            if(string.IsNullOrEmpty(descriptors))
                descriptors = WhitespaceRegex().Replace(inside.Trim(), " ");
        }

        game.Ratings.Add(new ParsedRating
        {
            Platform    = platform ?? "Unknown",
            System      = system,
            Rating      = rating,
            Descriptors = descriptors
        });
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
