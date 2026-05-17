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
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     EF wrapper around the <see cref="MobyGamesDiscoveredGame" /> table. Handles idempotent upserts
///     during the sitemap-driven discovery phase and the cursor queries used by the per-game raw
///     fetcher (<c>NewGameRawFetcher</c>).
/// </summary>
public class DiscoveryStateService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public DiscoveryStateService(IDbContextFactory<MarechaiContext> contextFactory) =>
        _contextFactory = contextFactory;

    /// <summary>
    ///     Insert a new discovery row or refresh the <see cref="MobyGamesDiscoveredGame.LastSeenAt" />
    ///     of an existing one. Returns <c>true</c> if a new row was inserted; <c>false</c> if an existing
    ///     row was touched.
    /// </summary>
    public async Task<bool> UpsertAsync(string slug, int numericId, DateTime? lastMod)
    {
        if(string.IsNullOrWhiteSpace(slug)) return false;

        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesDiscoveredGames
                                    .FirstOrDefaultAsync(g => g.Slug == slug);

        DateTime now = DateTime.UtcNow;
        int      yr  = lastMod?.Year ?? 0;

        if(existing != null)
        {
            existing.NumericId  = numericId;
            existing.LastSeenAt = lastMod ?? now;
            if(yr != 0 && existing.ReleaseYear != yr) existing.ReleaseYear = yr;
            await context.SaveChangesAsync();

            return false;
        }

        context.MobyGamesDiscoveredGames.Add(new MobyGamesDiscoveredGame
        {
            Slug              = slug,
            NumericId         = numericId,
            ReleaseYear       = yr,
            FirstDiscoveredAt = now,
            LastSeenAt        = lastMod ?? now
        });

        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    ///     Bulk variant of <see cref="UpsertAsync" /> — used by legacy callers that have only a slug,
    ///     numeric id and an optional last-modified timestamp. Returns <c>(inserted, updated)</c> counts.
    /// </summary>
    public async Task<(int Inserted, int Updated)> UpsertBatchAsync(
        IReadOnlyList<(string Slug, int NumericId, DateTime? LastMod)> batch)
    {
        if(batch is null || batch.Count == 0) return (0, 0);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Pull only the rows whose slug appears in the incoming batch, into a hashmap.
        string[] slugs = batch.Select(b => b.Slug).Distinct().ToArray();

        Dictionary<string, MobyGamesDiscoveredGame> existing =
            await context.MobyGamesDiscoveredGames
                         .Where(g => slugs.Contains(g.Slug))
                         .ToDictionaryAsync(g => g.Slug, g => g);

        DateTime now      = DateTime.UtcNow;
        int      inserted = 0;
        int      updated  = 0;

        foreach((string slug, int numericId, DateTime? lastMod) in batch)
        {
            if(string.IsNullOrWhiteSpace(slug)) continue;

            int yr = lastMod?.Year ?? 0;

            if(existing.TryGetValue(slug, out MobyGamesDiscoveredGame row) && row is not null)
            {
                row.NumericId  = numericId;
                row.LastSeenAt = lastMod ?? now;
                if(yr != 0 && row.ReleaseYear != yr) row.ReleaseYear = yr;
                updated++;
            }
            else
            {
                var newRow = new MobyGamesDiscoveredGame
                {
                    Slug              = slug,
                    NumericId         = numericId,
                    ReleaseYear       = yr,
                    FirstDiscoveredAt = now,
                    LastSeenAt        = lastMod ?? now
                };

                context.MobyGamesDiscoveredGames.Add(newRow);

                // Track the freshly-added entity so a duplicate slug LATER in the same batch hits
                // the update path (last-write-wins). The MobyGames PK is the numeric id, but the
                // legacy `mobygames_raw` table keys by slug as PK — so two different MobyGames
                // games sharing a slug (e.g. multiple `only-you` titles) MUST collapse into one
                // discovered row, matching the storage constraint. Storing `null` here (the old
                // marker) defeated the dedup, leading to either NRE (no null guard) or a SQL
                // duplicate-key error (with null guard) on the unique IX_…_Slug index.
                existing[slug] = newRow;
                inserted++;
            }
        }

        await context.SaveChangesAsync();

        return (inserted, updated);
    }

    /// <summary>
    ///     Search-results discovery variant of <see cref="UpsertBatchAsync(IReadOnlyList{ValueTuple{string,int,DateTime?}})"/>
    ///     that also writes the <see cref="MobyGamesDiscoveredGame.Title"/>, <see cref="MobyGamesDiscoveredGame.Developer"/>
    ///     and <see cref="MobyGamesDiscoveredGame.ReleaseYear"/> columns from the search payload. For
    ///     existing rows, all four fields are refreshed along with <see cref="MobyGamesDiscoveredGame.LastSeenAt"/>;
    ///     <see cref="MobyGamesDiscoveredGame.RawFetchedAt"/> and <see cref="MobyGamesDiscoveredGame.FirstDiscoveredAt"/>
    ///     are preserved.
    /// </summary>
    public async Task<(int Inserted, int Updated)> UpsertBatchAsync(
        IReadOnlyList<(string Slug, int NumericId, string Title, string Developer, int? Year)> batch)
    {
        if(batch is null || batch.Count == 0) return (0, 0);

        await using var context = await _contextFactory.CreateDbContextAsync();

        string[] slugs = batch.Select(b => b.Slug).Distinct().ToArray();

        Dictionary<string, MobyGamesDiscoveredGame> existing =
            await context.MobyGamesDiscoveredGames
                         .Where(g => slugs.Contains(g.Slug))
                         .ToDictionaryAsync(g => g.Slug, g => g);

        DateTime now      = DateTime.UtcNow;
        int      inserted = 0;
        int      updated  = 0;

        foreach((string slug, int numericId, string title, string developer, int? year) in batch)
        {
            if(string.IsNullOrWhiteSpace(slug)) continue;

            string truncatedTitle     = Truncate(title,     512);
            string truncatedDeveloper = Truncate(developer, 512);

            if(existing.TryGetValue(slug, out MobyGamesDiscoveredGame row) && row is not null)
            {
                row.NumericId  = numericId;
                row.LastSeenAt = now;

                if(year.HasValue) row.ReleaseYear = year.Value;
                if(!string.IsNullOrWhiteSpace(truncatedTitle))     row.Title     = truncatedTitle;
                if(!string.IsNullOrWhiteSpace(truncatedDeveloper)) row.Developer = truncatedDeveloper;
                updated++;
            }
            else
            {
                var newRow = new MobyGamesDiscoveredGame
                {
                    Slug              = slug,
                    NumericId         = numericId,
                    Title             = truncatedTitle,
                    Developer         = truncatedDeveloper,
                    ReleaseYear       = year ?? 0,
                    FirstDiscoveredAt = now,
                    LastSeenAt        = now
                };

                context.MobyGamesDiscoveredGames.Add(newRow);

                // Track the freshly-added entity (NOT null) so a duplicate slug LATER in the
                // same batch hits the update path. See the matching comment in the (Slug,
                // NumericId, DateTime?) overload for the full rationale.
                existing[slug] = newRow;
                inserted++;
            }
        }

        await context.SaveChangesAsync();

        return (inserted, updated);
    }

    static string Truncate(string s, int max)
    {
        if(string.IsNullOrEmpty(s)) return s;

        return s.Length <= max ? s : s[..max];
    }

    /// <summary>
    ///     Page through rows that still need their raw HTML fetched. Ordered by slug so successive calls
    ///     across processes remain deterministic.
    /// </summary>
    public async Task<List<MobyGamesDiscoveredGame>> GetPendingFetchAsync(int batchSize, int maxErrorCount = 5)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.MobyGamesDiscoveredGames
                            .Where(g => g.RawFetchedAt == null && g.ErrorCount < maxErrorCount)
                            .OrderBy(g => g.Slug)
                            .Take(batchSize)
                            .ToListAsync();
    }

    public async Task MarkFetchedAsync(string slug)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var row = await context.MobyGamesDiscoveredGames.FirstOrDefaultAsync(g => g.Slug == slug);
        if(row is null) return;

        row.RawFetchedAt = DateTime.UtcNow;
        row.FetchError   = null;

        await context.SaveChangesAsync();
    }

    public async Task MarkErrorAsync(string slug, string error)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var row = await context.MobyGamesDiscoveredGames.FirstOrDefaultAsync(g => g.Slug == slug);
        if(row is null) return;

        row.FetchError = error?[..Math.Min(error.Length, 1024)];
        row.ErrorCount += 1;

        await context.SaveChangesAsync();
    }

    public async Task MarkSkippedAsync(string slug, string reason)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var row = await context.MobyGamesDiscoveredGames.FirstOrDefaultAsync(g => g.Slug == slug);
        if(row is null) return;

        row.RawFetchedAt  = DateTime.UtcNow;
        row.SkippedReason = reason?[..Math.Min(reason.Length, 64)];

        await context.SaveChangesAsync();
    }

    public async Task PrintStatusAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        int total    = await context.MobyGamesDiscoveredGames.CountAsync();
        int pending  = await context.MobyGamesDiscoveredGames.CountAsync(g => g.RawFetchedAt == null && g.ErrorCount < 5);
        int fetched  = await context.MobyGamesDiscoveredGames.CountAsync(g => g.RawFetchedAt != null && g.SkippedReason == null);
        int skipped  = await context.MobyGamesDiscoveredGames.CountAsync(g => g.SkippedReason != null);
        int errored  = await context.MobyGamesDiscoveredGames.CountAsync(g => g.RawFetchedAt == null && g.ErrorCount >= 5);

        Console.WriteLine($"\n  Discovery Status:");
        Console.WriteLine($"    Total discovered: {total}");
        Console.WriteLine($"    Pending fetch:    {pending}");
        Console.WriteLine($"    Fetched:          {fetched}");
        Console.WriteLine($"    Skipped:          {skipped}");
        Console.WriteLine($"    Permanently errored (>= 5 attempts): {errored}");
    }
}
