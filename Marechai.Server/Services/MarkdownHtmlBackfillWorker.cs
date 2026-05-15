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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     Background worker that renders the markdown <c>Text</c> column to <c>Html</c> for any
///     description row where <c>Html</c> is still null. Previously this work was a synchronous
///     loop in <c>Program.Main</c> that ran inside the migration scope BEFORE <c>app.Run()</c>,
///     blocking the listening port for as long as the backfill took. Moving it to a hosted
///     service lets the API start serving requests immediately; the brief window where
///     <c>Description.Html</c> is still null is acceptable because the markdown <c>Text</c> is
///     also returned in the same DTO and the Blazor renderer falls back to it.
/// </summary>
/// <remarks>
///     <para>One backfill pass per startup. Exits when all 5 description tables have no remaining
///     null-<c>Html</c> rows. Operates on a fresh <see cref="MarechaiContext" /> per table so
///     change-tracker pressure stays bounded.</para>
///     <para>Mirrors the trivial path used by the various <c>*Controller.CreateOrUpdateDescription</c>
///     endpoints which use Markdig's "advanced extensions" pipeline.</para>
/// </remarks>
public sealed class MarkdownHtmlBackfillWorker(IDbContextFactory<MarechaiContext>    dbFactory,
                                               ILogger<MarkdownHtmlBackfillWorker>   logger)
    : BackgroundService
{
    /// <summary>How many rows to load + save per inner batch. Bounds the change tracker.</summary>
    const int _batchSize = 200;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Defer one tick so the host has fully started and IS NOT racing with EF migrations
        // (which run synchronously in Program.Main BEFORE the host's StartAsync completes).
        await Task.Yield();

        if(stoppingToken.IsCancellationRequested) return;

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        try
        {
            await BackfillCompanyDescriptionsAsync(pipeline, stoppingToken).ConfigureAwait(false);
            await BackfillMachineDescriptionsAsync(pipeline, stoppingToken).ConfigureAwait(false);
            await BackfillSoundSynthDescriptionsAsync(pipeline, stoppingToken).ConfigureAwait(false);
            await BackfillProcessorDescriptionsAsync(pipeline, stoppingToken).ConfigureAwait(false);
            await BackfillGpuDescriptionsAsync(pipeline, stoppingToken).ConfigureAwait(false);
        }
        catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
        {
            // Host shutting down — clean exit.
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Markdown HTML backfill failed");
        }
    }

    async Task BackfillCompanyDescriptionsAsync(MarkdownPipeline pipeline, CancellationToken ct)
    {
        int total = 0;

        while(!ct.IsCancellationRequested)
        {
            await using MarechaiContext context = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var batch = await context.CompanyDescriptions
                                     .Where(cd => cd.Html == null)
                                     .Take(_batchSize)
                                     .ToListAsync(ct)
                                     .ConfigureAwait(false);

            if(batch.Count == 0) break;

            foreach(var row in batch)
                row.Html = Markdown.ToHtml(row.Text ?? string.Empty, pipeline);

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            total += batch.Count;
        }

        if(total > 0) logger.LogInformation("Backfilled HTML for {Count} company descriptions", total);
    }

    async Task BackfillMachineDescriptionsAsync(MarkdownPipeline pipeline, CancellationToken ct)
    {
        int total = 0;

        while(!ct.IsCancellationRequested)
        {
            await using MarechaiContext context = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var batch = await context.MachineDescriptions
                                     .Where(md => md.Html == null)
                                     .Take(_batchSize)
                                     .ToListAsync(ct)
                                     .ConfigureAwait(false);

            if(batch.Count == 0) break;

            foreach(var row in batch)
                row.Html = Markdown.ToHtml(row.Text ?? string.Empty, pipeline);

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            total += batch.Count;
        }

        if(total > 0) logger.LogInformation("Backfilled HTML for {Count} machine descriptions", total);
    }

    async Task BackfillSoundSynthDescriptionsAsync(MarkdownPipeline pipeline, CancellationToken ct)
    {
        int total = 0;

        while(!ct.IsCancellationRequested)
        {
            await using MarechaiContext context = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var batch = await context.SoundSynthDescriptions
                                     .Where(sd => sd.Html == null)
                                     .Take(_batchSize)
                                     .ToListAsync(ct)
                                     .ConfigureAwait(false);

            if(batch.Count == 0) break;

            foreach(var row in batch)
                row.Html = Markdown.ToHtml(row.Text ?? string.Empty, pipeline);

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            total += batch.Count;
        }

        if(total > 0) logger.LogInformation("Backfilled HTML for {Count} sound synth descriptions", total);
    }

    async Task BackfillProcessorDescriptionsAsync(MarkdownPipeline pipeline, CancellationToken ct)
    {
        int total = 0;

        while(!ct.IsCancellationRequested)
        {
            await using MarechaiContext context = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var batch = await context.ProcessorDescriptions
                                     .Where(pd => pd.Html == null)
                                     .Take(_batchSize)
                                     .ToListAsync(ct)
                                     .ConfigureAwait(false);

            if(batch.Count == 0) break;

            foreach(var row in batch)
                row.Html = Markdown.ToHtml(row.Text ?? string.Empty, pipeline);

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            total += batch.Count;
        }

        if(total > 0) logger.LogInformation("Backfilled HTML for {Count} processor descriptions", total);
    }

    async Task BackfillGpuDescriptionsAsync(MarkdownPipeline pipeline, CancellationToken ct)
    {
        int total = 0;

        while(!ct.IsCancellationRequested)
        {
            await using MarechaiContext context = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var batch = await context.GpuDescriptions
                                     .Where(gd => gd.Html == null)
                                     .Take(_batchSize)
                                     .ToListAsync(ct)
                                     .ConfigureAwait(false);

            if(batch.Count == 0) break;

            foreach(var row in batch)
                row.Html = Markdown.ToHtml(row.Text ?? string.Empty, pipeline);

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            total += batch.Count;
        }

        if(total > 0) logger.LogInformation("Backfilled HTML for {Count} GPU descriptions", total);
    }
}
