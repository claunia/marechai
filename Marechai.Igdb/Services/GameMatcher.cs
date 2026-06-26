using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Helpers;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Igdb.Services;

public class GameMatcher
{
    const double FullSweepAcceptThreshold = 0.90;
    const double FullSweepReviewThreshold = 0.85;

    static readonly HashSet<string> StandaloneGameTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "main_game", "dlc_addon", "expansion", "standalone_expansion", "mod", "episode", "season"
    };

    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public GameMatcher(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<(int matched, int needsReview, int noMatch)> RunAsync(bool dryRun)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        List<Software> softwareList = await context.Softwares.Select(s => new Software
        {
            Id   = s.Id,
            Name = s.Name
        }).ToListAsync();

        // Local alternative titles, grouped by Software, so each side of the match can be
        // widened to "canonical name + every alternative title" instead of just the canonical
        // name. Mirrors IgdbAlternativeNames below.
        Dictionary<ulong, List<string>> alternativeTitlesBySoftwareId =
            (await context.SoftwareAlternativeTitles.Select(t => new { t.SoftwareId, t.Title }).ToListAsync())
           .GroupBy(t => t.SoftwareId)
           .ToDictionary(g => g.Key, g => g.Select(t => t.Title).ToList());

        // IGDB's own alternative names (mirrored from /alternative_names), grouped by the
        // referenced game's IGDB id.
        Dictionary<long, List<string>> igdbAlternativeNamesByGameId =
            (await context.IgdbAlternativeNames.Select(a => new { a.GameIgdbId, a.Name }).ToListAsync())
           .GroupBy(a => a.GameIgdbId)
           .ToDictionary(g => g.Key, g => g.Select(a => a.Name).ToList());

        // Precompute the full name-variant set (canonical + alternatives, deduplicated) for
        // every local Software row once, instead of recomputing it per IGDB game.
        Dictionary<ulong, List<string>> softwareNameVariantsById = softwareList.ToDictionary(s => s.Id,
            s => BuildNameVariants(s.Name,
                alternativeTitlesBySoftwareId.TryGetValue(s.Id, out List<string> alts) ? alts : null));

        Dictionary<ulong, HashSet<ulong>> platformsBySoftwareId =
            (await context.SoftwareReleases.Where(r => r.SoftwareId != null && r.PlatformId != null)
                           .Select(r => new { SoftwareId = r.SoftwareId.Value, PlatformId = r.PlatformId.Value })
                           .ToListAsync())
           .GroupBy(r => r.SoftwareId)
           .ToDictionary(g => g.Key, g => g.Select(r => r.PlatformId).ToHashSet());

        Dictionary<int, ulong> softwarePlatformByIgdbPlatformId =
            await context.IgdbPlatforms.Where(p => p.SoftwarePlatformId != null)
                          .ToDictionaryAsync(p => p.Id, p => p.SoftwarePlatformId.Value);

        Dictionary<int, string> gameTypeById = await context.IgdbGameTypes.ToDictionaryAsync(t => t.Id, t => t.Type);

        List<IgdbGame> allGames = await context.IgdbGames.ToListAsync();

        Dictionary<long, ulong> resolvedSoftwareIdByIgdbId = allGames
                                                             .Where(g => g.MatchStatus == IgdbMatchStatus.Matched &&
                                                                         g.SoftwareId.HasValue)
                                                             .ToDictionary(g => g.IgdbId, g => g.SoftwareId.Value);

        List<IgdbGame> pending = allGames.Where(g => g.MatchStatus == IgdbMatchStatus.Pending).ToList();

        // Process parentless / standalone games first so dependents can inherit within the same run.
        List<IgdbGame> ordered = pending
                                 .OrderBy(g => g.ParentGameId.HasValue || g.VersionParentId.HasValue ? 1 : 0)
                                 .ToList();

        int matched     = 0;
        int needsReview = 0;
        int noMatch     = 0;

        foreach(IgdbGame igdbGame in ordered)
        {
            (ulong? softwareId, string matchType, double? score, List<(Software item, double score)> candidates) result
                = MatchOne(igdbGame, softwareList, softwareNameVariantsById, igdbAlternativeNamesByGameId,
                           platformsBySoftwareId, softwarePlatformByIgdbPlatformId, gameTypeById,
                           resolvedSoftwareIdByIgdbId);

            if(result.softwareId.HasValue)
                resolvedSoftwareIdByIgdbId[igdbGame.IgdbId] = result.softwareId.Value;

            if(!dryRun)
            {
                if(result.softwareId.HasValue)
                {
                    igdbGame.SoftwareId  = result.softwareId.Value;
                    igdbGame.MatchStatus = IgdbMatchStatus.Matched;
                    igdbGame.MatchType   = result.matchType;
                    igdbGame.MatchScore  = result.score;
                    igdbGame.MatchedOn   = DateTime.UtcNow;
                }
                else if(result.candidates is { Count: > 0 })
                {
                    igdbGame.MatchStatus    = IgdbMatchStatus.NeedsReview;
                    igdbGame.CandidatesJson =
                        JsonSerializer.Serialize(result.candidates.Select(c => new { c.item.Id, c.item.Name, c.score }));
                }
                else
                {
                    igdbGame.MatchStatus = IgdbMatchStatus.NoMatch;
                }
            }

            if(result.softwareId.HasValue)
                matched++;
            else if(result.candidates is { Count: > 0 })
                needsReview++;
            else
                noMatch++;
        }

        if(!dryRun)
            await context.SaveChangesAsync();

        return (matched, needsReview, noMatch);
    }

    static (ulong? softwareId, string matchType, double? score, List<(Software item, double score)> candidates)
        MatchOne(IgdbGame igdbGame, List<Software> softwareList, Dictionary<ulong, List<string>> softwareNameVariantsById,
                 Dictionary<long, List<string>> igdbAlternativeNamesByGameId,
                 Dictionary<ulong, HashSet<ulong>> platformsBySoftwareId,
                 Dictionary<int, ulong> softwarePlatformByIgdbPlatformId, Dictionary<int, string> gameTypeById,
                 Dictionary<long, ulong> resolvedSoftwareIdByIgdbId)
    {
        // Step 1: edition/bundle inheritance.
        string gameType = igdbGame.GameTypeId.HasValue && gameTypeById.TryGetValue(igdbGame.GameTypeId.Value, out string t)
                               ? t
                               : null;

        bool isStandaloneType = gameType == null || StandaloneGameTypes.Contains(gameType);
        long? parentIgdbId    = igdbGame.ParentGameId ?? igdbGame.VersionParentId;

        if(!isStandaloneType && parentIgdbId.HasValue)
        {
            if(resolvedSoftwareIdByIgdbId.TryGetValue(parentIgdbId.Value, out ulong parentSoftwareId))
                return (parentSoftwareId, "inherited-from-parent", null, null);

            // Parent not resolved yet (possibly in a later mirror batch) — leave Pending for a later run.
            return (null, null, null, null);
        }

        // Steps 2-4: name + platform corroboration.
        HashSet<int> igdbPlatformIds = ParsePlatformIds(igdbGame.PlatformIdsJson);

        HashSet<ulong> targetSoftwarePlatformIds = igdbPlatformIds
                                                    .Where(softwarePlatformByIgdbPlatformId.ContainsKey)
                                                    .Select(id => softwarePlatformByIgdbPlatformId[id])
                                                    .ToHashSet();

        double PlatformOverlap(ulong softwareId)
        {
            if(targetSoftwarePlatformIds.Count == 0)
                return 0;

            if(!platformsBySoftwareId.TryGetValue(softwareId, out HashSet<ulong> releasePlatforms))
                return 0;

            int overlap = targetSoftwarePlatformIds.Count(releasePlatforms.Contains);

            return (double)overlap / targetSoftwarePlatformIds.Count;
        }

        // Name candidates on the IGDB side: the canonical name plus every mirrored
        // IGDB alternative_names row for this game (e.g. Japanese title, working title).
        List<string> igdbNames = BuildNameVariants(igdbGame.Name,
            igdbAlternativeNamesByGameId.TryGetValue(igdbGame.IgdbId, out List<string> igdbAlts) ? igdbAlts : null);

        // Exact match: any IGDB name variant against any local name variant (canonical name +
        // SoftwareAlternativeTitle rows) — so an alternate can match an alternate.
        List<Software> exactNameMatches = softwareList
                                          .Where(s => NameSetsOverlap(igdbNames, softwareNameVariantsById[s.Id]))
                                          .ToList();

        if(exactNameMatches.Count == 1)
        {
            string matchType = string.Equals(exactNameMatches[0].Name, igdbGame.Name, StringComparison.OrdinalIgnoreCase)
                                    ? "exact"
                                    : "exact-alt";

            return (exactNameMatches[0].Id, matchType, null, null);
        }

        if(exactNameMatches.Count > 1)
        {
            List<(Software software, double overlap)> withOverlap = exactNameMatches
               .Select(s => (software: s, overlap: PlatformOverlap(s.Id)))
               .Where(s => s.overlap > 0)
               .ToList();

            if(withOverlap.Count == 1)
            {
                string matchType =
                    string.Equals(withOverlap[0].software.Name, igdbGame.Name, StringComparison.OrdinalIgnoreCase)
                        ? "exact-platform-disambiguated"
                        : "exact-platform-disambiguated-alt";

                return (withOverlap[0].software.Id, matchType, null, null);
            }

            return (null, null, null,
                    exactNameMatches.Select(s => (item: s, score: 1.0)).ToList());
        }

        // Fuzzy sweep: best Jaro-Winkler score across the full cross-product of IGDB name
        // variants and local name variants, so e.g. an IGDB alternative name can fuzzy-match a
        // local alternative title even when neither side's canonical name is close.
        List<(Software item, double score, bool viaCanonicalPair)> sweep = softwareList
           .Select(s =>
            {
                (double score, bool canonical) = BestNameScore(igdbGame.Name, igdbNames, s.Name,
                    softwareNameVariantsById[s.Id]);

                return (item: s, score, viaCanonicalPair: canonical);
            })
           .Where(t => t.score >= FullSweepReviewThreshold)
           .OrderByDescending(t => t.score)
           .ToList();

        if(sweep.Count == 1)
        {
            bool hasExistingReleases = platformsBySoftwareId.TryGetValue(sweep[0].item.Id, out HashSet<ulong> releases) &&
                                        releases.Count > 0;

            double overlap = PlatformOverlap(sweep[0].item.Id);

            if(sweep[0].score >= FullSweepAcceptThreshold && (!hasExistingReleases || overlap > 0))
            {
                string matchType = sweep[0].viaCanonicalPair
                                        ? "jw-platform-corroborated"
                                        : "jw-platform-corroborated-alt";

                return (sweep[0].item.Id, matchType, sweep[0].score, null);
            }

            return (null, null, null, sweep.Select(s => (s.item, s.score)).ToList());
        }

        if(sweep.Count > 1)
            return (null, null, null, sweep.Select(s => (s.item, s.score)).ToList());

        return (null, null, null, null);
    }

    /// <summary>
    ///     Builds the deduplicated (case-insensitive) list of name variants for one side of a
    ///     match: the canonical name first, followed by any alternative titles/names, skipping
    ///     blanks and duplicates of the canonical name.
    /// </summary>
    static List<string> BuildNameVariants(string canonicalName, List<string> alternatives)
    {
        var variants = new List<string>();

        if(!string.IsNullOrWhiteSpace(canonicalName))
            variants.Add(canonicalName);

        if(alternatives is not null)
            foreach(string alt in alternatives)
            {
                if(string.IsNullOrWhiteSpace(alt))
                    continue;

                if(variants.Any(v => string.Equals(v, alt, StringComparison.OrdinalIgnoreCase)))
                    continue;

                variants.Add(alt);
            }

        return variants;
    }

    /// <summary>Whether any name in <paramref name="a" /> case-insensitively equals any name in
    /// <paramref name="b" />.</summary>
    static bool NameSetsOverlap(List<string> a, List<string> b) =>
        a.Any(x => b.Any(y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    ///     Best Jaro-Winkler score across the cross-product of <paramref name="aNames" /> and
    ///     <paramref name="bNames" />. The returned <c>canonicalPair</c> flag tells the caller
    ///     whether the winning pair was canonical-vs-canonical (<paramref name="aCanonical" /> vs
    ///     <paramref name="bCanonical" />) or involved at least one alternative name/title, purely
    ///     for audit-trail labelling (match type suffix).
    /// </summary>
    static (double score, bool canonicalPair) BestNameScore(string aCanonical, List<string> aNames,
        string bCanonical, List<string> bNames)
    {
        double best          = 0.0;
        bool   bestCanonical = false;

        foreach(string a in aNames)
        foreach(string b in bNames)
        {
            double score = JaroWinkler.Similarity(a, b);

            if(score <= best)
                continue;

            best          = score;
            bestCanonical = string.Equals(a, aCanonical, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(b, bCanonical, StringComparison.OrdinalIgnoreCase);
        }

        return (best, bestCanonical);
    }

    static HashSet<int> ParsePlatformIds(string json)
    {
        if(string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<int>>(json)?.ToHashSet() ?? [];
        }
        catch(JsonException)
        {
            return [];
        }
    }
}
