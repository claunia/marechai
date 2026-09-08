using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public partial class CoverStateService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public CoverStateService(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<HashSet<string>> GetProcessedCoverUrlsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var urls = await context.MobyGamesCoverDownloadStates
                                .Where(s => s.Status == MobyGamesCoverDownloadStatus.Downloaded)
                                .Select(s => s.CoverPageUrl)
                                .ToListAsync();

        return [..urls];
    }

    public async Task<List<MobyGamesCoverDownloadState>> GetStatesForGameAsync(string mobyGameId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.MobyGamesCoverDownloadStates
                            .Where(s => s.MobyGameId == mobyGameId)
                            .ToListAsync();
    }

    /// <summary>
    ///     Numeric MobyGames cover id from either URL layout: old site
    ///     <c>.../cover-art/gameCoverId,279493/</c> or new site <c>.../cover/group-N/cover-279493/</c>.
    /// </summary>
    public static string ExtractCoverId(string url)
    {
        if(string.IsNullOrEmpty(url)) return null;

        Match m = CoverIdRegex().Match(url);

        return m.Success ? m.Groups[1].Value : null;
    }

    public static HashSet<string> ExtractCoverIds(IEnumerable<string> urls)
    {
        var ids = new HashSet<string>();

        foreach(string url in urls)
            if(ExtractCoverId(url) is string id)
                ids.Add(id);

        return ids;
    }

    [GeneratedRegex(@"(?:gameCoverId,|/cover-)(\d+)/?", RegexOptions.IgnoreCase)]
    private static partial Regex CoverIdRegex();

    /// <summary>
    ///     Finds a state row for a cover by its numeric id regardless of which URL layout the
    ///     row was created with. Prefers a <c>Downloaded</c> row when several exist.
    /// </summary>
    public async Task<MobyGamesCoverDownloadState> GetStateByCoverIdAsync(string coverId)
    {
        if(string.IsNullOrWhiteSpace(coverId)) return null;

        await using var context = await _contextFactory.CreateDbContextAsync();

        string oldPattern = $"%gameCoverId,{coverId}/%";
        string newPattern = $"%/cover-{coverId}/%";

        return await context.MobyGamesCoverDownloadStates
                            .Where(s => EF.Functions.Like(s.CoverPageUrl, oldPattern) ||
                                        EF.Functions.Like(s.CoverPageUrl, newPattern))
                            .OrderByDescending(s => s.Status == MobyGamesCoverDownloadStatus.Downloaded)
                            .FirstOrDefaultAsync();
    }

    public async Task<MobyGamesCoverDownloadState> GetStateByCoverUrlAsync(string coverPageUrl)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.MobyGamesCoverDownloadStates
                            .FirstOrDefaultAsync(s => s.CoverPageUrl == coverPageUrl);
    }

    public async Task CreateStateAsync(MobyGamesCoverDownloadState state)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.MobyGamesCoverDownloadStates.Add(state);
        await context.SaveChangesAsync();
    }

    public async Task UpdateStateAsync(MobyGamesCoverDownloadState state)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.MobyGamesCoverDownloadStates
                                    .FirstOrDefaultAsync(s => s.Id == state.Id);

        if(existing is null) return;

        existing.Status            = state.Status;
        existing.ErrorMessage      = state.ErrorMessage;
        existing.ProcessedOn       = state.ProcessedOn;
        existing.SoftwareCoverId   = state.SoftwareCoverId;
        existing.SoftwareReleaseId = state.SoftwareReleaseId;
        existing.OriginalUrl       = state.OriginalUrl;

        await context.SaveChangesAsync();
    }

    public async Task PrintCoverStatusAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        int pending    = await context.MobyGamesCoverDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Pending);
        int downloaded = await context.MobyGamesCoverDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Downloaded);
        int failed     = await context.MobyGamesCoverDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Failed);
        int skipped    = await context.MobyGamesCoverDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.Skipped);
        int noRelease  = await context.MobyGamesCoverDownloadStates.CountAsync(s => s.Status == MobyGamesCoverDownloadStatus.NoRelease);
        int total      = pending + downloaded + failed + skipped + noRelease;

        Console.WriteLine($"\n  Cover Download Status:");
        Console.WriteLine($"    Downloaded: {downloaded}");
        Console.WriteLine($"    Failed:     {failed}");
        Console.WriteLine($"    Skipped:    {skipped}");
        Console.WriteLine($"    No Release: {noRelease}");
        Console.WriteLine($"    Pending:    {pending}");
        Console.WriteLine($"    Total:      {total}");
    }
}
