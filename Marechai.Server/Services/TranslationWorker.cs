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
using System.Threading;
using System.Threading.Tasks;
using Marechai.Translation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     Hosted service that drives every registered <see cref="ITranslationProvider" /> on a periodic
///     tick to fill missing translation rows by calling <see cref="TranslationService" /> (OpenAI
///     preferred, NLLB fallback). Per-entity logic lives in the providers; the worker is purely a
///     scheduler.
/// </summary>
/// <remarks>
///     <para>Lifecycle:</para>
///     <list type="number">
///         <item>On start, exits permanently if <see cref="TranslationService.IsAvailable" /> is false.</item>
///         <item>Loop: for each registered provider, (a) refresh the in-memory cache for newly added
///         items via <see cref="ITranslationProvider.DiscoverNewItemsAsync" />, (b) for each
///         non-<c>eng</c> language call <see cref="ITranslationProvider.TranslateMissingAsync" />.
///         Sleep 1 hour between full sweeps.</item>
///     </list>
///     <para>Providers are iterated SERIALLY and per-language batches are run SERIALLY inside each
///     provider — keeps OpenAI rate-limit handling trivial. Do NOT introduce parallelism here.</para>
/// </remarks>
public sealed class TranslationWorker(TranslationService                translationService,
                                      IEnumerable<ITranslationProvider> providers,
                                      ILogger<TranslationWorker>        logger) : BackgroundService
{
    static readonly TimeSpan _interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if(!translationService.IsAvailable)
        {
            logger.LogInformation(
                "Translation providers (OpenAI / NLLB) are not configured; TranslationWorker exiting.");

            return;
        }

        // Materialise once — DI returns a fresh enumerator each time IEnumerable<T> is iterated.
        var providerList = new List<ITranslationProvider>(providers);

        if(providerList.Count == 0)
        {
            logger.LogInformation("No ITranslationProvider implementations are registered; TranslationWorker exiting.");

            return;
        }

        // Make sure every provider's cache is populated before the first sweep — even if the
        // eager warm-up in Program.cs failed, this still gives the worker a consistent view.
        foreach(ITranslationProvider provider in providerList)
        {
            try
            {
                await provider.EnsureCacheLoadedAsync(stoppingToken);
            }
            catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch(Exception ex)
            {
                logger.LogError(ex,
                                "TranslationWorker initial cache load failed for provider {Provider}; will retry next tick.",
                                provider.Name);
            }
        }

        while(!stoppingToken.IsCancellationRequested)
        {
            foreach(ITranslationProvider provider in providerList)
            {
                try
                {
                    int discovered = await provider.DiscoverNewItemsAsync(stoppingToken);

                    if(discovered > 0)
                        logger.LogInformation("TranslationWorker: provider {Provider} discovered {Count} new items.",
                                              provider.Name, discovered);

                    foreach(string lang in TranslationService.SupportedLanguageCodes)
                    {
                        if(string.Equals(lang, "eng", StringComparison.Ordinal)) continue;

                        stoppingToken.ThrowIfCancellationRequested();

                        int inserted = await provider.TranslateMissingAsync(lang, stoppingToken);

                        if(inserted > 0)
                            logger.LogInformation(
                                "TranslationWorker: provider {Provider} inserted {Count} translations into {Lang}.",
                                provider.Name, inserted, lang);
                    }
                }
                catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "TranslationWorker tick failed for provider {Provider}; continuing.",
                                    provider.Name);
                }
            }

            try { await Task.Delay(_interval, stoppingToken); }
            catch(OperationCanceledException) { break; }
        }
    }
}
