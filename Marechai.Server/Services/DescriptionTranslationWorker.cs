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
using Marechai.Server.Services.DescriptionTranslation;
using Marechai.Translation;
using Markdig;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marechai.Server.Services;

/// <summary>
///     Companion to <see cref="TranslationWorker" />: while the primary worker is in its
///     inter-sweep slumber (signalled via <see cref="TranslationPhaseCoordinator" />) this worker
///     fills missing per-language rows for every description / synopsis table by translating the
///     English source in bounded waves. Each row is saved immediately so progress is durable; when
///     the primary worker wakes, the in-progress translations are allowed to finish + save and
///     this worker yields back gracefully.
/// </summary>
/// <remarks>
///     <para>The translated markdown gets a hand-translated automated-translation disclaimer
///     prepended in the target language so the model never sees / mangles it. Description tables
///     also get a Markdig-rendered HTML column populated, mirroring the
///     <c>*Controller.CreateOrUpdateDescriptionAsync</c> + <c>*SuggestionApplier</c> path.</para>
///     <para>Wave size is controlled by the server's live-reloaded
///     <see cref="TranslationOptions.MaxParallelTranslations" /> setting. <c>0</c> pauses this
///     worker until the setting is raised again.</para>
/// </remarks>
public sealed class DescriptionTranslationWorker(TranslationService                          translationService,
                                                 TranslationPhaseCoordinator                 coordinator,
                                                 IEnumerable<IDescriptionTranslationSource>  sources,
                                                 IOptionsMonitor<TranslationOptions>         options,
                                                 ILogger<DescriptionTranslationWorker>       logger)
    : BackgroundService
{
    /// <summary>
    ///     Hand-translated disclaimer per non-<c>eng</c> language. Prepended AFTER the model
    ///     translation so the model never sees the disclaimer (avoids bad disclaimer translations
    ///     and keeps the input window slim). Italic single-paragraph markdown so themes can style
    ///     the rendered <c>&lt;em&gt;</c> as a note above the body.
    /// </summary>
    static readonly IReadOnlyDictionary<string, string> _disclaimers = new Dictionary<string, string>
    {
        ["spa"] =
            "*(esta es una traducción automática; puedes ayudarnos detectando errores o colaborando con una traducción mejor)*",
        ["deu"] =
            "*(dies ist eine automatische Übersetzung; du kannst uns helfen, indem du Fehler findest oder eine bessere Übersetzung beisteuerst)*",
        ["fra"] =
            "*(ceci est une traduction automatique\u00a0; vous pouvez nous aider en signalant des erreurs ou en proposant une meilleure traduction)*",
        ["ita"] =
            "*(questa è una traduzione automatica; puoi aiutarci segnalando errori o contribuendo con una traduzione migliore)*",
        ["lat"] =
            "*(haec versio automatica est; nos adiuvare potes errores reperiendo aut meliorem versionem conferendo)*",
        ["por"] =
            "*(esta é uma tradução automática; pode ajudar-nos encontrando erros ou colaborando com uma tradução melhor)*"
    };

    /// <summary>
    ///     Domain hint passed to <see cref="TranslationService.TranslateAsync" /> so the model
    ///     translates encyclopedic prose in the right register and preserves brand names / model
    ///     numbers / version strings unchanged.
    /// </summary>
    const string _domainContext =
        "The text is an encyclopedic description of a piece of computing history (a hardware product, software, " +
        "company, person, magazine, book or document). Preserve all markdown formatting (headings, lists, links, " +
        "code spans, fenced code blocks, tables, emphasis, blockquotes) exactly. Keep brand names, model numbers, " +
        "version strings, copyright notices, and quoted source titles unchanged. Do not add commentary.";

    /// <summary>Same column max as the description / synopsis tables (<c>[MaxLength(262144)]</c>).</summary>
    const int _maxColumnLength = 262_144;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if(!translationService.IsAvailable)
        {
            logger.LogInformation(
                "Translation providers (OpenAI / NLLB) are not configured; DescriptionTranslationWorker exiting.");

            return;
        }

        var sourceList = new List<IDescriptionTranslationSource>(sources);

        if(sourceList.Count == 0)
        {
            logger.LogInformation(
                "No IDescriptionTranslationSource implementations are registered; DescriptionTranslationWorker exiting.");

            return;
        }

        // Render markdown→HTML for the description tables. Same pipeline used by the controllers
        // and the suggestion appliers — keep them aligned.
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        while(!stoppingToken.IsCancellationRequested)
        {
            CancellationToken slumberCt;

            try
            {
                slumberCt = await coordinator.WaitForSlumberAsync(stoppingToken);
            }
            catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await RunSlumberPassAsync(sourceList, pipeline, slumberCt, stoppingToken);
            }
            catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
            {
                coordinator.NotifyDescriptionStopped();

                return;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "DescriptionTranslationWorker slumber pass failed; yielding to translation worker.");
            }
            finally
            {
                coordinator.NotifyDescriptionStopped();
            }
        }
    }

    /// <summary>
    ///     One slumber pass: round-robin over (source × non-<c>eng</c> language). Returns when
    ///     either the slumber CT fires, the host stops, or a full round of every (source, language)
    ///     pair finds zero missing rows — at which point we sleep on the slumber CT until wake.
    /// </summary>
    async Task RunSlumberPassAsync(List<IDescriptionTranslationSource> sourceList, MarkdownPipeline pipeline,
                                   CancellationToken                   slumberCt, CancellationToken stoppingToken)
    {
        logger.LogInformation("DescriptionTranslationWorker: slumber pass starting.");
        using var schedulingCts = CancellationTokenSource.CreateLinkedTokenSource(slumberCt, stoppingToken);
        CancellationToken schedulingToken = schedulingCts.Token;

        // Snapshot the non-eng target languages once per pass.
        var targetLanguages = new List<string>();
        foreach(string lang in TranslationService.SupportedLanguageCodes)
        {
            if(string.Equals(lang, "eng", StringComparison.Ordinal)) continue;
            targetLanguages.Add(lang);
        }

        while(true)
        {
            if(slumberCt.IsCancellationRequested || stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("DescriptionTranslationWorker: stop requested, yielding.");

                return;
            }

            bool madeProgressThisRound = false;

            for(var sourceIndex = 0; sourceIndex < sourceList.Count;)
            {
                int maxParallelTranslations =
                    await BackgroundTranslationSettings.WaitForAvailableParallelismAsync(options, schedulingToken);
                var wave = new List<Task<bool>>(maxParallelTranslations);

                while(wave.Count < maxParallelTranslations && sourceIndex < sourceList.Count)
                {
                    IDescriptionTranslationSource source = sourceList[sourceIndex];

                    foreach(string lang in targetLanguages)
                    {
                        if(wave.Count >= maxParallelTranslations) break;

                        if(slumberCt.IsCancellationRequested || stoppingToken.IsCancellationRequested)
                        {
                            logger.LogInformation(
                                "DescriptionTranslationWorker: stop requested mid-round, yielding.");

                            return;
                        }

                        wave.Add(TryTranslateOneAsync(source, lang, pipeline, stoppingToken));
                    }

                    sourceIndex++;
                }

                if(wave.Count == 0) break;

                bool[] waveResults = await Task.WhenAll(wave);

                foreach(bool didWork in waveResults)
                {
                    if(didWork) madeProgressThisRound = true;
                }
            }

            if(madeProgressThisRound) continue;

            // Nothing left to translate right now. Wait for the slumber CT (or host stop) so we
            // don't busy-loop, but do NOT exit — the next slumber phase will start a new pass.
            logger.LogInformation(
                "DescriptionTranslationWorker: nothing missing for any (source, language); idling until next slumber.");

            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, slumberCt);
            }
            catch(OperationCanceledException)
            {
                // slumberCt fired — translation worker is waking. Yield gracefully.
            }

            return;
        }
    }

    /// <summary>
    ///     Fetch one missing row for <paramref name="source" /> / <paramref name="lang" />,
    ///     translate it, prepend the language-specific disclaimer, render HTML if the source needs
    ///     it, and save. Returns <c>true</c> if a row was processed (regardless of save outcome),
    ///     <c>false</c> when nothing was missing.
    /// </summary>
    /// <remarks>
    ///     The translation call itself is NOT cancelled by <c>slumberCt</c> — once we've committed
    ///     OpenAI tokens we want the row to land in the DB before we yield, per the user spec
    ///     ("stop as soon as the current in-progress translation is finished and saved").
    /// </remarks>
    async Task<bool> TryTranslateOneAsync(IDescriptionTranslationSource source, string lang,
                                          MarkdownPipeline              pipeline, CancellationToken stoppingToken)
    {
        DescriptionTranslationItem item;

        try
        {
            item = await source.FetchNextMissingAsync(lang, stoppingToken);
        }
        catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested) { throw; }
        catch(Exception ex)
        {
            logger.LogError(ex,
                            "DescriptionTranslationWorker: fetch failed for {Source}/{Lang}; skipping this slot.",
                            source.Name, lang);

            return false;
        }

        if(item is null) return false;

        if(string.IsNullOrWhiteSpace(item.SourceMarkdown))
        {
            logger.LogDebug(
                "DescriptionTranslationWorker: {Source} entity {Id} has empty English source; skipping {Lang}.",
                source.Name, item.EntityId, lang);

            return false;
        }

        // Defensive cap. Translation can grow text by a factor (especially DE / FR), so leave
        // headroom for the disclaimer prefix (~150 chars) plus growth.
        string sourceMarkdown = item.SourceMarkdown;
        const int maxSourceLength = _maxColumnLength - 4_096;
        if(sourceMarkdown.Length > maxSourceLength) sourceMarkdown = sourceMarkdown[..maxSourceLength];

        (string translated, string error) = await translationService.TranslateAsync(
            sourceMarkdown, lang, progress: null, plainText: false, domainContext: _domainContext);

        if(string.IsNullOrWhiteSpace(translated))
        {
            logger.LogWarning(
                "DescriptionTranslationWorker: translation failed for {Source} entity {Id} into {Lang}: {Error}",
                source.Name, item.EntityId, lang, error ?? "no result");

            return true;
        }

        // Prepend the hand-translated disclaimer paragraph + blank line so it renders as a
        // standalone italic note above the body. Falls back to no prefix for unknown languages
        // (shouldn't happen — TranslationService guards against unsupported codes — but cheap).
        string finalMarkdown = _disclaimers.TryGetValue(lang, out string disclaimer)
                                   ? disclaimer + "\n\n" + translated
                                   : translated;

        if(finalMarkdown.Length > _maxColumnLength) finalMarkdown = finalMarkdown[.._maxColumnLength];

        string html = string.Empty;

        if(source.RendersHtml)
        {
            html = Markdown.ToHtml(finalMarkdown, pipeline) ?? string.Empty;

            // Mirror the suggestion-applier guard: if the rendered HTML somehow exceeds the
            // column cap, drop it and let the public view fall back to rendering markdown on
            // demand rather than losing the description entirely.
            if(html.Length > _maxColumnLength) html = string.Empty;
        }

        try
        {
            await source.SaveAsync(item, finalMarkdown, html, stoppingToken);

            logger.LogInformation(
                "DescriptionTranslationWorker: saved {Source} entity {Id} translation into {Lang} ({Chars} chars).",
                source.Name, item.EntityId, lang, finalMarkdown.Length);
        }
        catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested) { throw; }
        catch(Exception ex)
        {
            logger.LogError(ex,
                            "DescriptionTranslationWorker: save failed for {Source} entity {Id} into {Lang}.",
                            source.Name, item.EntityId, lang);
        }

        return true;
    }
}
