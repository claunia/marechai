using System.Text.RegularExpressions;

namespace Marechai.MobyGames.Parsers;

public static partial class NewSiteMainPageParser
{
    /// <summary>
    ///     Extracts the MobyGames numeric ID and slug of the base game from a new-site DLC page's HTML.
    ///     Looks for the "Base Game" section which contains a link like /game/{numericId}/{slug}/.
    /// </summary>
    /// <param name="html">Raw HTML of the new MobyGames game page</param>
    /// <returns>Tuple of (numericId, slug), or (null, null) if not found</returns>
    public static (int? Id, string Slug) ParseBaseGame(string html)
    {
        if(string.IsNullOrWhiteSpace(html)) return (null, null);

        Match match = BaseGameRegex().Match(html);

        if(!match.Success || !int.TryParse(match.Groups[1].Value, out int id))
            return (null, null);

        string slug = match.Groups[2].Success ? match.Groups[2].Value : null;

        return (id, slug);
    }

    /// <summary>Convenience wrapper returning only the numeric ID.</summary>
    public static int? ParseBaseGameId(string html) => ParseBaseGame(html).Id;

    [GeneratedRegex(@"Base\s*Game.*?/game/(\d+)/([^/""<>\s]+)?/?", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex BaseGameRegex();
}
