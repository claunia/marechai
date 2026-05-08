using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Marechai.Data;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Parsers;

/// <summary>
///     Parses a MobyGames critic/publication page (https://www.mobygames.com/critic/{id}/{slug}/) for the
///     metadata block above the reviews table: URL, First Year, Country, Language.
/// </summary>
public static partial class CriticPageParser
{
    public static ParsedCritic Parse(string html)
    {
        if(string.IsNullOrWhiteSpace(html))
            return null;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // The metadata is in a <p> directly inside <main id="main">, the first <p> after the <h1>.
        var paragraphs = doc.DocumentNode.SelectNodes("//main//p");

        if(paragraphs is null)
            return null;

        var result = new ParsedCritic();

        foreach(var p in paragraphs)
        {
            string text = WebUtility.HtmlDecode(p.InnerText);

            if(string.IsNullOrWhiteSpace(text))
                continue;

            // Only consider the metadata paragraph (it always contains "Country:" and/or "First Year:")
            if(!text.Contains("Country:")    &&
               !text.Contains("First Year:") &&
               !text.Contains("Language:"))
                continue;

            // First Year: "1999", "1999-04", "Apr 1999", "Apr, 1999", "Apr 5, 1999"
            var yearMatch = FirstYearRegex().Match(text);

            if(yearMatch.Success)
            {
                string raw = yearMatch.Groups[1].Value.Trim().TrimEnd(',', '.');
                var (date, precision) = ParseFirstPublication(raw);

                if(date.HasValue)
                {
                    result.FirstPublication          = date;
                    result.FirstPublicationPrecision = precision;
                }
            }

            // Country: United States <img ... alt="us flag">
            // Take everything between "Country:" and the next newline or "Language:"
            var countryMatch = CountryRegex().Match(text);

            if(countryMatch.Success)
            {
                string country = countryMatch.Groups[1].Value
                                              .Replace("\u00a0", " ")
                                              .Trim()
                                              .TrimEnd(',');

                if(!string.IsNullOrWhiteSpace(country))
                    result.CountryName = country;
            }

            break;
        }

        return result;
    }

    static (DateTime? date, DatePrecision precision) ParseFirstPublication(string raw)
    {
        if(string.IsNullOrWhiteSpace(raw))
            return (null, DatePrecision.YearOnly);

        string normalized = raw.Replace("\u00a0", " ").Trim();

        // Bare 4-digit year, e.g. "1999"
        if(YearOnlyRegex().IsMatch(normalized) &&
           int.TryParse(normalized, out int yearOnly) &&
           yearOnly is >= 1900 and <= 2100)
            return (new DateTime(yearOnly, 1, 1), DatePrecision.YearOnly);

        // ISO-style YYYY-MM (or YYYY/MM)
        var isoMonth = IsoYearMonthRegex().Match(normalized);

        if(isoMonth.Success                                                  &&
           int.TryParse(isoMonth.Groups[1].Value, out int iy)               &&
           int.TryParse(isoMonth.Groups[2].Value, out int im)               &&
           iy is >= 1900 and <= 2100                                         &&
           im is >= 1 and <= 12)
            return (new DateTime(iy, im, 1), DatePrecision.MonthYear);

        // ISO-style YYYY-MM-DD (full date)
        var isoFull = IsoYearMonthDayRegex().Match(normalized);

        if(isoFull.Success &&
           DateTime.TryParseExact(isoFull.Value, "yyyy-MM-dd",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None,
                                  out var fullDate))
            return (fullDate, DatePrecision.Full);

        // Textual: "Apr 1999" / "Apr, 1999" / "April 1999"
        string[] monthYearFormats =
        [
            "MMM yyyy", "MMM, yyyy", "MMMM yyyy", "MMMM, yyyy"
        ];

        if(DateTime.TryParseExact(normalized, monthYearFormats,
                                  CultureInfo.InvariantCulture, DateTimeStyles.None,
                                  out var monthYear))
            return (new DateTime(monthYear.Year, monthYear.Month, 1), DatePrecision.MonthYear);

        // Textual full: "Apr 5, 1999" / "April 5, 1999" / "5 Apr 1999"
        string[] fullFormats =
        [
            "MMM d, yyyy", "MMMM d, yyyy", "d MMM yyyy", "d MMMM yyyy"
        ];

        if(DateTime.TryParseExact(normalized, fullFormats,
                                  CultureInfo.InvariantCulture, DateTimeStyles.None,
                                  out var full))
            return (full, DatePrecision.Full);

        // Last-resort: pull a 4-digit year from the string
        var yearAnywhere = AnyYearRegex().Match(normalized);

        if(yearAnywhere.Success                                       &&
           int.TryParse(yearAnywhere.Groups[1].Value, out int anyYear) &&
           anyYear is >= 1900 and <= 2100)
            return (new DateTime(anyYear, 1, 1), DatePrecision.YearOnly);

        return (null, DatePrecision.YearOnly);
    }

    [GeneratedRegex(@"First Year:\s*([^\r\n<]+?)(?=\s{2,}|\r|\n|<|$)", RegexOptions.Compiled)]
    private static partial Regex FirstYearRegex();

    [GeneratedRegex(@"Country:\s*([^\r\n]+?)(?:\s{2,}|\r|\n|Language:|$)", RegexOptions.Compiled)]
    private static partial Regex CountryRegex();

    [GeneratedRegex(@"^\d{4}$", RegexOptions.Compiled)]
    private static partial Regex YearOnlyRegex();

    [GeneratedRegex(@"^(\d{4})[-/](\d{1,2})$", RegexOptions.Compiled)]
    private static partial Regex IsoYearMonthRegex();

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.Compiled)]
    private static partial Regex IsoYearMonthDayRegex();

    [GeneratedRegex(@"\b(\d{4})\b", RegexOptions.Compiled)]
    private static partial Regex AnyYearRegex();
}
