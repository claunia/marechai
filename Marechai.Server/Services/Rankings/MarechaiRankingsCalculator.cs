/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services.Rankings;

/// <summary>
///     Full-catalog Marechai rankings calculator. Walks every <see cref="Software" />,
///     filters to the eligible set (≥ <see cref="MARECHAI_RANKING_MIN_CRITIC_COUNT" /> critic
///     reviews OR ≥ <see cref="MARECHAI_RANKING_MIN_USER_COUNT" /> user ratings), applies the
///     per-side Bayesian shrinkage in <see cref="ComputeMarechaiScore" />, then emits:
///     <list type="number">
///         <item>One <see cref="SoftwareScore" /> row per eligible software (uncapped — drives the
///         per-software score banner even for titles ranked outside any top-250 list).</item>
///         <item>One <see cref="RankingDefinition" /> per non-empty dimension (Overall + per
///         individual genre with <see cref="SoftwareGenreType" /> ∈ {<see cref="SoftwareGenreType.Genre" />,
///         <see cref="SoftwareGenreType.Gameplay" />, <see cref="SoftwareGenreType.Setting" />,
///         <see cref="SoftwareGenreType.Category" />} + per individual platform).</item>
///         <item>Up to 250 <see cref="RankingEntry" /> rows per definition, ordered by Marechai
///         score desc, then evidence desc, then software id asc.</item>
///     </list>
///     The full snapshot is swapped atomically inside a single transaction: the previous
///     three tables are truncated then re-inserted in chunks. On exception the transaction
///     is rolled back and the previous snapshot remains visible.
/// </summary>
public sealed class MarechaiRankingsCalculator(IDbContextFactory<MarechaiContext>     dbFactory,
                                               RankingsComputationState               state,
                                               ILogger<MarechaiRankingsCalculator>    logger)
{
    /// <summary>Eligibility threshold on the critic-review side.</summary>
    public const int MARECHAI_RANKING_MIN_CRITIC_COUNT = 3;

    /// <summary>Eligibility threshold on the user-rating side.</summary>
    public const int MARECHAI_RANKING_MIN_USER_COUNT = 1;

    /// <summary>Bayesian prior strength applied to the critic side of the score.</summary>
    public const int MARECHAI_RANKING_CRITIC_PRIOR = 5;

    /// <summary>Bayesian prior strength applied to the user side of the score.</summary>
    public const int MARECHAI_RANKING_USER_PRIOR = 10;

    /// <summary>Hard cap on entries per <see cref="RankingDefinition" />.</summary>
    public const int MARECHAI_RANKING_TOP_N = 250;

    /// <summary>Bulk-insert chunk size — keeps the EF change tracker bounded.</summary>
    const int INSERT_CHUNK = 1000;

    /// <summary>
    ///     Recomputes every <see cref="SoftwareScore" />, <see cref="RankingDefinition" /> and
    ///     <see cref="RankingEntry" /> row in a single atomic sweep. Must be called under the
    ///     <see cref="RankingsComputationState.Gate" /> semaphore — callers (the worker and the
    ///     optional manual-recompute endpoint) are responsible for acquiring it.
    /// </summary>
    /// <returns><see langword="true" /> on success, <see langword="false" /> on failure (logged).</returns>
    public async Task<bool> ComputeAllAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        state.MarkComputing();

        try
        {
            await using MarechaiContext ctx = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            // -------- 1. Load every software's raw signal --------
            // Same shape as the legacy SoftwareController.GetMarechaiRankingAsync. Compilations
            // are implicitly excluded — they have no aggregated rating signal.
            var rawSignal = await ctx.Softwares
                                     .AsNoTracking()
                                     .Select(s => new
                                      {
                                          s.Id,
                                          CriticAvg = s.CriticReviews
                                                       .Where(r => r.NormalizedScore != null)
                                                       .Select(r => (double?)r.NormalizedScore)
                                                       .Average(),
                                          UserAvg = s.UserRatings.Select(r => (double?)r.Rating).Average(),
                                          CriticCount = s.CriticReviews.Count(r => r.NormalizedScore != null),
                                          UserCount   = s.UserRatings.Count
                                      })
                                     .Where(s => s.CriticCount >= MARECHAI_RANKING_MIN_CRITIC_COUNT ||
                                                 s.UserCount   >= MARECHAI_RANKING_MIN_USER_COUNT)
                                     .ToListAsync(ct)
                                     .ConfigureAwait(false);

            if(rawSignal.Count == 0)
            {
                logger.LogInformation("MarechaiRankingsCalculator: no eligible software found, leaving tables empty.");

                await using IDbContextTransaction emptyTx =
                    await ctx.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

                await ctx.Database.ExecuteSqlRawAsync("DELETE FROM RankingEntries;", ct).ConfigureAwait(false);
                await ctx.Database.ExecuteSqlRawAsync("DELETE FROM RankingDefinitions;", ct).ConfigureAwait(false);
                await ctx.Database.ExecuteSqlRawAsync("DELETE FROM SoftwareScores;", ct).ConfigureAwait(false);

                await emptyTx.CommitAsync(ct).ConfigureAwait(false);

                DateTime emptyNow = DateTime.UtcNow;
                state.MarkCompleted(emptyNow);

                logger.LogInformation("MarechaiRankingsCalculator: wiped (no eligible software) in {Elapsed}.", sw.Elapsed);
                return true;
            }

            // -------- 2. Catalog-wide priors (Bayesian shrinkage) --------
            var criticItems = rawSignal.Where(s => s.CriticAvg.HasValue).ToList();
            var userItems   = rawSignal.Where(s => s.UserAvg.HasValue).ToList();

            double catalogCriticMean10 =
                criticItems.Count > 0 ? criticItems.Average(s => s.CriticAvg!.Value) / 10.0 : 7.0;

            double catalogUserMean10 =
                userItems.Count > 0 ? userItems.Average(s => s.UserAvg!.Value) * 2.0 : 7.0;

            // -------- 3. Score everything, build the global ordering --------
            var scored = rawSignal.Select(s => new ScoredSoftware
                                   {
                                       SoftwareId        = s.Id,
                                       Score             = ComputeMarechaiScore(s.CriticAvg,
                                                                                s.CriticCount,
                                                                                s.UserAvg,
                                                                                s.UserCount,
                                                                                catalogCriticMean10,
                                                                                catalogUserMean10),
                                       CriticAvg100      = s.CriticAvg,
                                       UserAvg5          = s.UserAvg,
                                       CriticReviewCount = s.CriticCount,
                                       UserRatingCount   = s.UserCount
                                   })
                                  .Where(s => s.Score.HasValue)
                                  .OrderByDescending(s => s.Score!.Value)
                                  .ThenByDescending(s => s.CriticReviewCount + s.UserRatingCount)
                                  .ThenBy(s => s.SoftwareId)
                                  .ToList();

            // 1-based global rank.
            for(int i = 0; i < scored.Count; i++) scored[i].GlobalRank = i + 1;

            // -------- 4. Load dimension membership (small, hash-friendly projections) --------
            // (a) Genre membership for ALL eligible software, but only for genres whose Type is
            //     rankable (Perspective is the Marechai catalog's "viewpoint" classifier — Top-N
            //     by viewpoint is not meaningful so it's intentionally excluded).
            HashSet<ulong> eligibleIds = scored.Select(s => s.SoftwareId).ToHashSet();

            List<int> rankableGenreIds = await ctx.SoftwareGenres
                                                  .AsNoTracking()
                                                  .Where(g => g.Type == SoftwareGenreType.Genre    ||
                                                              g.Type == SoftwareGenreType.Gameplay ||
                                                              g.Type == SoftwareGenreType.Setting  ||
                                                              g.Type == SoftwareGenreType.Category)
                                                  .Select(g => g.Id)
                                                  .ToListAsync(ct)
                                                  .ConfigureAwait(false);

            HashSet<int> rankableGenreSet = rankableGenreIds.ToHashSet();

            var genreMembers = new Dictionary<int, List<ulong>>();

            await foreach(var pair in ctx.GenresBySoftware
                                         .AsNoTracking()
                                         .Where(gbs => rankableGenreSet.Contains(gbs.GenreId))
                                         .Select(gbs => new { gbs.GenreId, gbs.SoftwareId })
                                         .AsAsyncEnumerable()
                                         .WithCancellation(ct))
            {
                if(!eligibleIds.Contains(pair.SoftwareId)) continue;

                if(!genreMembers.TryGetValue(pair.GenreId, out List<ulong> members))
                {
                    members                      = [];
                    genreMembers[pair.GenreId] = members;
                }

                members.Add(pair.SoftwareId);
            }

            // (b) Platform membership: SoftwareRelease links to Software either directly
            //     (SoftwareId) or via a SoftwareVersion (SoftwareVersionId). Run two
            //     projections and merge into one (PlatformId → set<SoftwareId>) map.
            var platformMembers = new Dictionary<ulong, HashSet<ulong>>();

            await foreach(var pair in ctx.SoftwareReleases
                                         .AsNoTracking()
                                         .Where(r => r.PlatformId.HasValue && r.SoftwareId.HasValue)
                                         .Select(r => new
                                          {
                                              SoftwareId = r.SoftwareId!.Value,
                                              PlatformId = r.PlatformId!.Value
                                          })
                                         .AsAsyncEnumerable()
                                         .WithCancellation(ct))
            {
                if(!eligibleIds.Contains(pair.SoftwareId)) continue;

                if(!platformMembers.TryGetValue(pair.PlatformId, out HashSet<ulong> members))
                {
                    members                            = [];
                    platformMembers[pair.PlatformId] = members;
                }

                members.Add(pair.SoftwareId);
            }

            await foreach(var pair in ctx.SoftwareReleases
                                         .AsNoTracking()
                                         .Where(r => r.PlatformId.HasValue && r.SoftwareVersionId.HasValue)
                                         .Select(r => new
                                          {
                                              SoftwareId = r.SoftwareVersion.SoftwareId,
                                              PlatformId = r.PlatformId!.Value
                                          })
                                         .AsAsyncEnumerable()
                                         .WithCancellation(ct))
            {
                if(!eligibleIds.Contains(pair.SoftwareId)) continue;

                if(!platformMembers.TryGetValue(pair.PlatformId, out HashSet<ulong> members))
                {
                    members                            = [];
                    platformMembers[pair.PlatformId] = members;
                }

                members.Add(pair.SoftwareId);
            }

            // -------- 5. Build the ranking blueprint (in memory, then commit) --------
            // Score lookup for the per-dimension materialization. `scored` is already in
            // global-rank order, so iterating it and filtering by membership preserves the
            // correct intra-dimension ordering for free.
            DateTime now = DateTime.UtcNow;

            var blueprint = new List<DefinitionBlueprint>(1 + genreMembers.Count + platformMembers.Count);

            // 5a. All — top 250 globally.
            blueprint.Add(new DefinitionBlueprint
            {
                Dimension    = RankingDimension.All,
                DimensionId  = null,
                Entries      = scored.Take(MARECHAI_RANKING_TOP_N).ToList()
            });

            // 5b. Per genre. The eligibility filter above guarantees `members` is non-empty
            //     for every key present in `genreMembers`; still, skip if filtering by score
            //     drops the count to zero (defence in depth).
            foreach((int genreId, List<ulong> members) in genreMembers)
            {
                HashSet<ulong> memberSet = members.ToHashSet();

                List<ScoredSoftware> top = scored.Where(s => memberSet.Contains(s.SoftwareId))
                                                 .Take(MARECHAI_RANKING_TOP_N)
                                                 .ToList();

                if(top.Count == 0) continue;

                blueprint.Add(new DefinitionBlueprint
                {
                    Dimension   = RankingDimension.Genre,
                    DimensionId = genreId,
                    Entries     = top
                });
            }

            // 5c. Per platform.
            foreach((ulong platformId, HashSet<ulong> memberSet) in platformMembers)
            {
                List<ScoredSoftware> top = scored.Where(s => memberSet.Contains(s.SoftwareId))
                                                 .Take(MARECHAI_RANKING_TOP_N)
                                                 .ToList();

                if(top.Count == 0) continue;

                blueprint.Add(new DefinitionBlueprint
                {
                    Dimension   = RankingDimension.Platform,
                    DimensionId = (long)platformId,
                    Entries     = top
                });
            }

            // -------- 6. Atomic swap inside one transaction --------
            await using IDbContextTransaction tx =
                await ctx.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

            // Children first (FK order). RankingEntries → RankingDefinitions; SoftwareScores
            // is independent of both. ExecuteSqlRaw bypasses the change tracker — perfect
            // for "wipe everything".
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM RankingEntries;", ct).ConfigureAwait(false);
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM RankingDefinitions;", ct).ConfigureAwait(false);
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM SoftwareScores;", ct).ConfigureAwait(false);

            // 6a. SoftwareScores — full eligible set, chunked.
            foreach(var chunk in scored.Chunk(INSERT_CHUNK))
            {
                ctx.SoftwareScores.AddRange(chunk.Select(s => new SoftwareScore
                {
                    SoftwareId        = s.SoftwareId,
                    Score             = Math.Round(s.Score!.Value, 4),
                    GlobalRank        = s.GlobalRank,
                    CriticAverage     = s.CriticAvg100,
                    UserStarAverage   = s.UserAvg5,
                    CriticReviewCount = s.CriticReviewCount,
                    UserRatingCount   = s.UserRatingCount,
                    ComputedAt        = now
                }));

                await ctx.SaveChangesAsync(ct).ConfigureAwait(false);
                ctx.ChangeTracker.Clear();
            }

            // 6b. RankingDefinitions — one row per blueprint. Save first so EF backfills the
            //     generated Id we need for RankingEntry.RankingDefinitionId.
            foreach(DefinitionBlueprint b in blueprint)
            {
                b.Definition = new RankingDefinition
                {
                    Dimension   = b.Dimension,
                    DimensionId = b.DimensionId,
                    EntryCount  = b.Entries.Count,
                    ComputedAt  = now
                };

                ctx.RankingDefinitions.Add(b.Definition);
            }

            await ctx.SaveChangesAsync(ct).ConfigureAwait(false);

            // 6c. RankingEntries — chunked across all blueprints.
            int                entryBatch = 0;
            List<RankingEntry> entryBuf   = new(INSERT_CHUNK);

            foreach(DefinitionBlueprint b in blueprint)
            {
                int rank = 1;

                foreach(ScoredSoftware s in b.Entries)
                {
                    entryBuf.Add(new RankingEntry
                    {
                        RankingDefinitionId = b.Definition!.Id,
                        SoftwareId          = s.SoftwareId,
                        Rank                = rank++,
                        Score               = Math.Round(s.Score!.Value, 4)
                    });

                    if(entryBuf.Count < INSERT_CHUNK) continue;

                    ctx.RankingEntries.AddRange(entryBuf);
                    await ctx.SaveChangesAsync(ct).ConfigureAwait(false);
                    ctx.ChangeTracker.Clear();

                    entryBatch += entryBuf.Count;
                    entryBuf.Clear();
                }
            }

            if(entryBuf.Count > 0)
            {
                ctx.RankingEntries.AddRange(entryBuf);
                await ctx.SaveChangesAsync(ct).ConfigureAwait(false);
                entryBatch += entryBuf.Count;
            }

            await tx.CommitAsync(ct).ConfigureAwait(false);

            state.MarkCompleted(now);

            logger.LogInformation(
                "MarechaiRankingsCalculator: computed {ScoreCount} scores + {DefCount} rankings ({EntryCount} entries) in {Elapsed}.",
                scored.Count, blueprint.Count, entryBatch, sw.Elapsed);

            return true;
        }
        catch(OperationCanceledException) when(ct.IsCancellationRequested)
        {
            state.MarkFailed();
            logger.LogInformation("MarechaiRankingsCalculator: cancelled after {Elapsed}.", sw.Elapsed);
            return false;
        }
        catch(Exception ex)
        {
            state.MarkFailed();
            logger.LogError(ex, "MarechaiRankingsCalculator: failed after {Elapsed}. Previous snapshot retained.", sw.Elapsed);
            return false;
        }
    }

    /// <summary>
    ///     Per-side Bayesian shrinkage producing the 0–10 Marechai score. Lifted verbatim
    ///     from the original implementation in <c>SoftwareController</c> so the score number
    ///     remains comparable across the old and new rankings. Returns null when the item
    ///     fails the minimum-evidence threshold on BOTH sides. Inputs use native scales
    ///     (critic 0–100, user 0–5) plus catalog-wide means already normalised to 0–10.
    /// </summary>
    public static double? ComputeMarechaiScore(double? criticAvg100,
                                               int     criticCount,
                                               double? userAvg5,
                                               int     userCount,
                                               double  catalogCriticMean10,
                                               double  catalogUserMean10)
    {
        bool hasCritic = criticAvg100.HasValue && criticCount >= MARECHAI_RANKING_MIN_CRITIC_COUNT;
        bool hasUser   = userAvg5.HasValue && userCount >= MARECHAI_RANKING_MIN_USER_COUNT;

        if(!hasCritic && !hasUser) return null;

        double? shrunkCritic = null;
        double? shrunkUser   = null;

        if(hasCritic)
        {
            double r10 = criticAvg100!.Value / 10.0;

            shrunkCritic = (criticCount * r10 + MARECHAI_RANKING_CRITIC_PRIOR * catalogCriticMean10) /
                           (criticCount + MARECHAI_RANKING_CRITIC_PRIOR);
        }

        if(hasUser)
        {
            double r10 = userAvg5!.Value * 2.0;

            shrunkUser = (userCount * r10 + MARECHAI_RANKING_USER_PRIOR * catalogUserMean10) /
                         (userCount + MARECHAI_RANKING_USER_PRIOR);
        }

        if(shrunkCritic.HasValue && shrunkUser.HasValue)
            return (shrunkCritic.Value + shrunkUser.Value) / 2.0;

        return shrunkCritic ?? shrunkUser!.Value;
    }

    /// <summary>In-memory carrier for one scored software (one element of the global ordering).</summary>
    sealed class ScoredSoftware
    {
        public ulong   SoftwareId        { get; init; }
        public double? Score             { get; init; }
        public double? CriticAvg100      { get; init; }
        public double? UserAvg5          { get; init; }
        public int     CriticReviewCount { get; init; }
        public int     UserRatingCount   { get; init; }
        public int     GlobalRank        { get; set; }
    }

    /// <summary>In-memory carrier for one ranking + its ordered top-N entries.</summary>
    sealed class DefinitionBlueprint
    {
        public RankingDimension     Dimension   { get; init; }
        public long?                DimensionId { get; init; }
        public List<ScoredSoftware> Entries     { get; init; } = [];
        public RankingDefinition    Definition  { get; set; }
    }
}
