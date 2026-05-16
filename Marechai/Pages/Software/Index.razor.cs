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
using Marechai.ApiClient.Models;
using Marechai.Pages.Suggestions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Software;

public partial class Index
{
    // Stable key under which the prerender side ships the landing data to the
    // hydration side via <persist-component-state/>. Bumping this invalidates
    // any in-flight prerendered HTML the browser may still be holding.
    const string PersistKey = "software-landing-v1";

    int                                        _count;
    Dictionary<string, List<SoftwareGenreDto>> _genresByType;
    // Heavy chip sections start collapsed; the body inside each MudExpansionPanel
    // is `@if`-gated on these flags so the chips are not in the render tree (or
    // the SignalR diff) until the user opens the panel.
    bool                                       _genresPanelExpanded;
    bool                                       _loaded;
    int                                        _maxYear;
    int                                        _minYear;
    PersistingComponentStateSubscription       _persistSub;
    List<SoftwarePlatformDto>                  _platforms  = [];
    List<SoftwareSpecKeyDto>                   _specsByKey = [];
    bool                                       _specsPanelExpanded;
    // Per-spec-key expansion state, keyed by the canonical (untranslated) spec key.
    readonly Dictionary<string, bool>          _specKeyExpanded = new();

    [Inject] PersistentComponentState State { get; set; } = default!;

    /// <summary>
    /// Loads the landing-page data. Runs twice per page view: once during prerender
    /// (Phase 2 — fan-out API calls) and once during the interactive SignalR
    /// hydration that follows (Phase 1 — picks up the state the prerender side
    /// shipped via <see cref="PersistentComponentState"/>). Skipping the fetch on
    /// hydration is what eliminates the visible "chips → spinner → chips" flash:
    /// the hydrated component starts with <c>_loaded=true</c> and a fully
    /// populated model, so Blazor's first interactive diff against the
    /// prerendered DOM is empty.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        if(_loaded) return;

        // Phase 1 — interactive hydration. Try to recover the state the
        // prerender side shipped inline in the HTML. On success, skip every
        // API round-trip and let the existing prerendered DOM stand.
        if(State.TryTakeFromJson<PersistedLanding>(PersistKey, out PersistedLanding saved) && saved is not null)
        {
            ApplyPersisted(saved);
            _loaded = true;

            return;
        }

        // Phase 2 — prerender (or interactive without persisted state, e.g. when
        // <persist-component-state/> is missing or the JSON failed to deserialize).
        // Fan out: every call is independent, so kick them off in parallel
        // instead of awaiting each one sequentially. Was 6 round-trips
        // (~6 × DB RTT) on the same critical path.
        Task<int>                       countTask     = Service.GetSoftwareCountAsync();
        Task<int>                       minYearTask   = Service.GetMinimumYearAsync();
        Task<int>                       maxYearTask   = Service.GetMaximumYearAsync();
        Task<List<SoftwarePlatformDto>> platformsTask = Service.GetPlatformsAsync();
        Task<List<SoftwareGenreDto>>    genresTask    = Service.GetAllGenresAsync();
        Task<List<SoftwareSpecKeyDto>>  specsTask     = Service.GetSpecificationsAsync();

        await Task.WhenAll(countTask, minYearTask, maxYearTask, platformsTask, genresTask, specsTask);

        _count     = countTask.Result;
        _minYear   = minYearTask.Result;
        _maxYear   = maxYearTask.Result;
        _platforms = platformsTask.Result;

        _genresByType = genresTask.Result
                                  .GroupBy(g => g.TypeName ?? "Genre")
                                  .ToDictionary(g => g.Key, g => g.ToList());

        _specsByKey = specsTask.Result;

        _loaded = true;

        // Phase 3 — register the persister so that when the prerender component
        // tree is torn down (right after the HTML is sent to the browser), the
        // loaded data is serialized into the HTML for the hydration side to
        // pick up via Phase 1. No-op on the hydration side because Phase 1 has
        // already returned before we got here.
        _persistSub = State.RegisterOnPersisting(PersistState);
    }

    Task PersistState()
    {
        // Wire-trimmed shape: only the fields the Razor actually consumes.
        // Avoids serializing Kiota's IDictionary<string, object> AdditionalData
        // which trips up System.Text.Json's deserializer on the receiving end.
        State.PersistAsJson(PersistKey, new PersistedLanding
        {
            Count   = _count,
            MinYear = _minYear,
            MaxYear = _maxYear,
            Platforms = _platforms.Select(p => new PersistedPlatform
                                          {
                                              Id   = p.Id ?? 0,
                                              Name = p.Name ?? string.Empty
                                          })
                                  .ToList(),
            GenreGroups = _genresByType?.Select(g => new PersistedGenreGroup
                                                    {
                                                        TypeName = g.Key,
                                                        Genres = g.Value.Select(x => new PersistedGenre
                                                                                     {
                                                                                         Id   = x.Id ?? 0,
                                                                                         Name = x.Name ?? string.Empty
                                                                                     })
                                                                  .ToList()
                                                    })
                                       .ToList() ?? [],
            Specs = _specsByKey.Select(s => new PersistedSpec
                                            {
                                                Key           = s.Key           ?? string.Empty,
                                                DisplayKey    = s.DisplayKey    ?? string.Empty,
                                                Values        = s.Values        ?? [],
                                                DisplayValues = s.DisplayValues ?? []
                                            })
                               .ToList()
        });

        return Task.CompletedTask;
    }

    void ApplyPersisted(PersistedLanding s)
    {
        _count   = s.Count;
        _minYear = s.MinYear;
        _maxYear = s.MaxYear;

        _platforms = s.Platforms.Select(p => new SoftwarePlatformDto
                                             {
                                                 Id   = p.Id,
                                                 Name = p.Name
                                             })
                      .ToList();

        _genresByType = s.GenreGroups.ToDictionary(g => g.TypeName,
                                                   g => g.Genres.Select(x => new SoftwareGenreDto
                                                                              {
                                                                                  Id   = x.Id,
                                                                                  Name = x.Name
                                                                              })
                                                         .ToList());

        _specsByKey = s.Specs.Select(x => new SoftwareSpecKeyDto
                                          {
                                              Key           = x.Key,
                                              DisplayKey    = x.DisplayKey,
                                              Values        = x.Values,
                                              DisplayValues = x.DisplayValues
                                          })
                      .ToList();
    }

    /// <summary>
    /// `OwningComponentBase` already implements <see cref="IDisposable"/> to drop
    /// the per-component DI scope. We hook the protected <c>Dispose(bool)</c>
    /// to additionally unsubscribe our persist callback so it doesn't fire on
    /// an already-disposed component.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if(disposing) _persistSub.Dispose();

        base.Dispose(disposing);
    }

    /// <summary>
    ///     Opens the SoftwareSuggestionDialog in CREATION mode for a brand-new Software.
    ///     Software has no user-facing surface without a release, so the dialog requires
    ///     the user to fill BOTH the Software fields AND the first-release fields in the
    ///     same submission. Server-side, the applier orchestrates both inserts inside an
    ///     EF transaction (atomic — either both succeed or neither does). The dialog UI
    ///     is the same one used for editing existing software (in-place reuse via the
    ///     <c>IsCreation</c> parameter).
    /// </summary>
    async Task OpenSuggestNewSoftwareDialog()
    {
        var parameters = new DialogParameters<SoftwareSuggestionDialog>
        {
            { x => x.EntityId,   0L },
            { x => x.CurrentDto, (SoftwareDto)null },
            { x => x.IsCreation, true }
        };

        var options = new DialogOptions
        {
            MaxWidth         = MaxWidth.Large,
            FullWidth        = true,
            CloseOnEscapeKey = true
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareSuggestionDialog>(
            L["Suggest new software"], parameters, options);

        DialogResult result = await dialog.Result;
        if(result is null || result.Canceled) return;

        // No reload needed — pending suggestions only take effect after admin review.
    }

    // ── Persisted-state DTOs ──
    //
    // Trimmed shape used by `PersistentComponentState` to ship the landing-page
    // data from the prerender render to the hydrated interactive render.
    // Kept intentionally minimal (only the fields the Razor reads), with public
    // get/set properties and a parameterless ctor so `System.Text.Json` can
    // round-trip them without custom converters. Reconstituted into the
    // matching Kiota DTOs in `ApplyPersisted` so the Razor's foreach loops
    // keep operating on the original `SoftwarePlatformDto`/`SoftwareGenreDto`
    // /`SoftwareSpecKeyDto` types and no XAML/.razor changes are required.

    sealed class PersistedLanding
    {
        public int                       Count       { get; set; }
        public int                       MinYear     { get; set; }
        public int                       MaxYear     { get; set; }
        public List<PersistedPlatform>   Platforms   { get; set; } = [];
        public List<PersistedGenreGroup> GenreGroups { get; set; } = [];
        public List<PersistedSpec>       Specs       { get; set; } = [];
    }

    sealed class PersistedPlatform
    {
        public int    Id   { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    sealed class PersistedGenreGroup
    {
        public string               TypeName { get; set; } = string.Empty;
        public List<PersistedGenre> Genres   { get; set; } = [];
    }

    sealed class PersistedGenre
    {
        public int    Id   { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    sealed class PersistedSpec
    {
        public string       Key           { get; set; } = string.Empty;
        public string       DisplayKey    { get; set; } = string.Empty;
        public List<string> Values        { get; set; } = [];
        public List<string> DisplayValues { get; set; } = [];
    }
}
