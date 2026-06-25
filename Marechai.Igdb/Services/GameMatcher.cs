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
                = MatchOne(igdbGame, softwareList, platformsBySoftwareId, softwarePlatformByIgdbPlatformId,
                           gameTypeById, resolvedSoftwareIdByIgdbId);

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
        MatchOne(IgdbGame igdbGame, List<Software> softwareList, Dictionary<ulong, HashSet<ulong>> platformsBySoftwareId,
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

        List<Software> exactNameMatches = softwareList
                                          .Where(s => string.Equals(s.Name, igdbGame.Name,
                                                                     StringComparison.OrdinalIgnoreCase))
                                          .ToList();

        if(exactNameMatches.Count == 1)
            return (exactNameMatches[0].Id, "exact", null, null);

        if(exactNameMatches.Count > 1)
        {
            List<(Software software, double overlap)> withOverlap = exactNameMatches
               .Select(s => (software: s, overlap: PlatformOverlap(s.Id)))
               .Where(s => s.overlap > 0)
               .ToList();

            if(withOverlap.Count == 1)
                return (withOverlap[0].software.Id, "exact-platform-disambiguated", null, null);

            return (null, null, null,
                    exactNameMatches.Select(s => (item: s, score: 1.0)).ToList());
        }

        List<(Software item, double score)> sweep =
            JaroWinkler.FindMatches(igdbGame.Name, softwareList, s => s.Name, FullSweepReviewThreshold);

        if(sweep.Count == 1)
        {
            bool hasExistingReleases = platformsBySoftwareId.TryGetValue(sweep[0].item.Id, out HashSet<ulong> releases) &&
                                        releases.Count > 0;

            double overlap = PlatformOverlap(sweep[0].item.Id);

            if(sweep[0].score >= FullSweepAcceptThreshold && (!hasExistingReleases || overlap > 0))
                return (sweep[0].item.Id, "jw-platform-corroborated", sweep[0].score, null);

            return (null, null, null, sweep);
        }

        if(sweep.Count > 1)
            return (null, null, null, sweep);

        return (null, null, null, null);
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
