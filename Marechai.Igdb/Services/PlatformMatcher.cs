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

        var unresolved = new List<IgdbPlatform>();

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
            {
                needsReview++;
                unresolved.Add(igdbPlatform);
            }
        }

        if(!dryRun)
            await context.SaveChangesAsync();

        if(!dryRun && unresolved.Count > 0)
        {
            if(Console.IsInputRedirected)
                Console.WriteLine(
                    $"\n{unresolved.Count} platform(s) could not be automatically matched. Run interactively (not redirected) to link them manually.");
            else
                (matched, needsReview) = await ResolveInteractivelyAsync(context, platforms, unresolved, matched,
                                                                            needsReview);
        }

        return (matched, needsReview);
    }

    /// <summary>Walks the platforms that automatic matching could not resolve, one at a time, letting the
    /// operator search the existing local catalog, create a brand-new <see cref="SoftwarePlatform" />, skip the
    /// current one, or quit (leaving any remaining platforms as <see cref="IgdbMatchStatus.NeedsReview" /> for a
    /// later run).</summary>
    async Task<(int matched, int needsReview)> ResolveInteractivelyAsync(MarechaiContext context,
        List<SoftwarePlatform> platforms, List<IgdbPlatform> unresolved, int matched, int needsReview)
    {
        Console.WriteLine(
            $"\n{unresolved.Count} platform(s) could not be automatically matched. Resolve them now, or press 'q' at any prompt to stop and leave the rest unresolved.");

        foreach(IgdbPlatform igdbPlatform in unresolved)
        {
            (SoftwarePlatform resolved, bool quit) = PromptForPlatform(igdbPlatform, platforms);

            if(quit)
            {
                Console.WriteLine("  Quitting; remaining platforms left as \"needs review\".");

                break;
            }

            if(resolved == null)
                continue;

            if(resolved.Id == 0)
            {
                context.SoftwarePlatforms.Add(resolved);
                await context.SaveChangesAsync();
                platforms.Add(resolved);
                Console.WriteLine($"  Created new platform \"{resolved.Name}\" (id {resolved.Id}).");
            }

            igdbPlatform.SoftwarePlatformId = resolved.Id;
            igdbPlatform.MatchStatus        = IgdbMatchStatus.Matched;
            igdbPlatform.MatchType          = "manual";
            igdbPlatform.MatchedOn          = DateTime.UtcNow;

            matched++;
            needsReview--;

            await context.SaveChangesAsync();
            Console.WriteLine($"  Linked \"{igdbPlatform.Name}\" to \"{resolved.Name}\" (id {resolved.Id}).");
        }

        return (matched, needsReview);
    }

    /// <summary>Prompts for a single platform. Returns <c>(null, true)</c> on quit, <c>(null, false)</c> on
    /// skip, or the chosen/newly-built <see cref="SoftwarePlatform" /> (unsaved if newly built, signaled by
    /// <see cref="SoftwarePlatform.Id" /> being <c>0</c>) otherwise.</summary>
    static (SoftwarePlatform platform, bool quit) PromptForPlatform(IgdbPlatform igdbPlatform,
        List<SoftwarePlatform> platforms)
    {
        while(true)
        {
            Console.WriteLine();
            Console.WriteLine($"Unmatched IGDB platform: \"{igdbPlatform.Name}\" (igdb id {igdbPlatform.Id}).");
            Console.Write(
                "  Enter part of the local platform name to search, 'n' to create a new platform with this name, 's' to skip, or 'q' to quit: ");

            string input = Console.ReadLine()?.Trim();

            if(input == null)
                return (null, true);

            if(input.Length == 0)
                continue;

            if(input.Equals("q", StringComparison.OrdinalIgnoreCase))
                return (null, true);

            if(input.Equals("s", StringComparison.OrdinalIgnoreCase))
                return (null, false);

            if(input.Equals("n", StringComparison.OrdinalIgnoreCase))
            {
                Console.Write($"  Create new platform named \"{igdbPlatform.Name}\"? [Y/n/q]: ");
                string confirm = Console.ReadLine()?.Trim();

                if(confirm != null && confirm.Equals("q", StringComparison.OrdinalIgnoreCase))
                    return (null, true);

                if(confirm == null || confirm.Length == 0 || confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
                    return (new SoftwarePlatform { Name = igdbPlatform.Name }, false);

                continue;
            }

            List<SoftwarePlatform> candidates = platforms
                                                .Where(p => p.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
                                                .OrderBy(p => p.Name).ToList();

            if(candidates.Count == 0)
            {
                Console.WriteLine("  No local platforms match that text. Try again.");

                continue;
            }

            for(int i = 0; i < candidates.Count; i++)
                Console.WriteLine($"  [{i + 1}] {candidates[i].Name}");

            Console.Write($"  Pick a number (1-{candidates.Count}), 'q' to quit, or anything else to search again: ");
            string choice = Console.ReadLine()?.Trim();

            if(choice == null)
                return (null, true);

            if(choice.Equals("q", StringComparison.OrdinalIgnoreCase))
                return (null, true);

            if(int.TryParse(choice, out int idx) && idx >= 1 && idx <= candidates.Count)
                return (candidates[idx - 1], false);
        }
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
