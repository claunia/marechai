using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Helpers;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Igdb.Services;

public class PlatformMatcher
{
    const double AcceptThreshold = 0.90;

    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public PlatformMatcher(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    const int MinSubstringTokenLength = 4;

    public async Task<(int matched, int needsReview)> RunAsync(bool dryRun)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        List<SoftwarePlatform> platforms = await context.SoftwarePlatforms.ToListAsync();
        List<IgdbPlatform> pending = await context.IgdbPlatforms
                                                   .Where(p => p.MatchStatus == IgdbMatchStatus.Pending)
                                                   .ToListAsync();

        // Marechai groups platforms the way MobyGames does: short colloquial names ("Jaguar" rather than
        // "Atari Jaguar") and sometimes several platforms combined into one row ("DOS, Windows and Windows
        // 3.x"). IGDB instead has one row per platform with a full canonical name. Expand each local row into
        // its individual name variants so a single IGDB platform can match any of them.
        var variantsByPlatform = platforms.ToDictionary(p => p, Variants);

        int matched     = 0;
        int needsReview = 0;

        foreach(IgdbPlatform igdbPlatform in pending)
        {
            string normalizedInput = Normalize(igdbPlatform.Name);

            SoftwarePlatform match = null;
            string matchType = null;

            SoftwarePlatform exact = platforms.FirstOrDefault(p =>
                variantsByPlatform[p].Any(v => string.Equals(v, igdbPlatform.Name, StringComparison.OrdinalIgnoreCase)));

            if(exact != null)
            {
                match     = exact;
                matchType = "exact";
            }
            else
            {
                SoftwarePlatform normalizedMatch = platforms.FirstOrDefault(p =>
                    variantsByPlatform[p].Any(v => Normalize(v) == normalizedInput));

                if(normalizedMatch != null)
                {
                    match     = normalizedMatch;
                    matchType = "normalized";
                }
                else
                {
                    List<SoftwarePlatform> substringCandidates = platforms.Where(p =>
                        variantsByPlatform[p].Any(v => IsSubstringMatch(normalizedInput, Normalize(v)))).ToList();

                    if(substringCandidates.Count == 1)
                    {
                        match     = substringCandidates[0];
                        matchType = "substring";
                    }
                    else
                    {
                        List<(SoftwarePlatform item, double score)> candidates =
                            JaroWinkler.FindMatches(igdbPlatform.Name, platforms, p => p.Name, AcceptThreshold);

                        if(candidates.Count == 1)
                        {
                            match     = candidates[0].item;
                            matchType = "jaro-winkler";
                        }
                    }
                }
            }

            if(!dryRun)
            {
                if(match != null)
                {
                    igdbPlatform.SoftwarePlatformId = match.Id;
                    igdbPlatform.MatchStatus        = IgdbMatchStatus.Matched;
                    igdbPlatform.MatchType          = matchType;
                    igdbPlatform.MatchedOn          = DateTime.UtcNow;
                }
                else
                {
                    igdbPlatform.MatchStatus = IgdbMatchStatus.NeedsReview;
                }
            }

            if(match != null)
                matched++;
            else
                needsReview++;
        }

        if(!dryRun)
            await context.SaveChangesAsync();

        return (matched, needsReview);
    }

    static List<string> Variants(SoftwarePlatform platform)
    {
        var variants = new List<string> { platform.Name };

        variants.AddRange(Regex.Split(platform.Name, @"\s*,\s*|\s+and\s+", RegexOptions.IgnoreCase)
                                .Select(v => v.Trim())
                                .Where(v => v.Length > 0));

        return variants.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    static bool IsSubstringMatch(string normalizedA, string normalizedB)
    {
        if(normalizedA.Length < MinSubstringTokenLength || normalizedB.Length < MinSubstringTokenLength)
            return false;

        (string shorter, string longer) = normalizedA.Length <= normalizedB.Length
                                               ? (normalizedA, normalizedB)
                                               : (normalizedB, normalizedA);

        return Regex.IsMatch(longer, $@"(?<![a-z0-9]){Regex.Escape(shorter)}(?![a-z0-9])");
    }

    static string Normalize(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            return string.Empty;

        string result = Regex.Replace(name, "[^a-zA-Z0-9]+", " ").Trim().ToLowerInvariant();

        return Regex.Replace(result, "\\s+", " ");
    }
}
