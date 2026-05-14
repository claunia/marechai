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
using Marechai.Data;
using Marechai.Helpers;
using Marechai.Pages.Suggestions;
using Marechai.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Marechai.Pages.SoundSynths;

public partial class View
{
    List<MachineDto> _computers = [];
    List<MachineDto> _consoles  = [];
    List<MachineDto> _smartphones = [];
    List<MachineDto> _tablets = [];
    List<MachineDto> _pdas = [];
    string          _description;
    string          _descriptionLanguageServed;
    bool            _descriptionFellBack;
    bool            _hasAnyDescription;
    HashSet<string> _existingDescriptionLangs = new(StringComparer.Ordinal);
    HashSet<string> _pendingDescriptionLangs  = new(StringComparer.Ordinal);
    string           _displayName;
    int              _lastId;
    bool             _loaded;
    List<Guid>       _photos = [];
    PhotoLightbox    _lightbox;
    SoundSynthDto    _synth;
    List<SoundSynthVideoDto> _videos = [];

    // Tab state — backs the responsive sticky MudTabs in View.razor.
    // _tabNames is rebuilt after data loads to include "machines" only when
    // at least one of the per-machine-type lists is non-empty.
    string[] _tabNames  = ["specifications", "media"];
    string   _activeTab = "specifications";

    int _activeTabIndex => Math.Max(0, Array.IndexOf(_tabNames, _activeTab));

    // Sentinel rows (currently only DB_SOFTWARE, id = -2 per Marechai.Database
    // Operations.DbSoftware) represent abstract/virtual sound synthesizers and
    // have no specifications, machines, photos, or videos. Detect by name so
    // the rename pattern matches the existing _displayName line below.
    bool _isSentinel => _synth?.Name == "DB_SOFTWARE";

    [CascadingParameter]
    Task<AuthenticationState> AuthState { get; set; }

    [Parameter]
    public int Id { get; set; }

    [SupplyParameterFromQuery(Name = "tab")]
    public string TabParam { get; set; }

    [Inject]
    NavigationManager NavManager { get; set; }

    protected override void OnParametersSet()
    {
        // Validate against the static superset; the index resolver collapses
        // missing tabs (e.g. "machines" on a synth with none) to 0.
        string tab = (TabParam ?? "specifications").ToLowerInvariant();
        _activeTab = tab is "specifications" or "machines" or "media" ? tab : "specifications";

        if(Id == _lastId) return;

        _lastId = Id;
        _loaded = false;
    }

    void OnTabChanged(int index)
    {
        if(index < 0 || index >= _tabNames.Length) return;

        string newTab = _tabNames[index];

        if(newTab == _activeTab) return;

        _activeTab = newTab;

        // Default tab (specifications) drops the query param to keep URLs clean.
        string newUri = NavManager.GetUriWithQueryParameter("tab", newTab == "specifications" ? null : newTab);
        NavManager.NavigateTo(newUri, false, true);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        // ── Phase 1+3 retrofit ────────────────────────────────────────────────
        // One consolidated HTTP round-trip via /sound-synths/{Id}/full (head
        // projection + company logo via inline subquery + description with
        // single-query language fallback + machines + photos + videos), all
        // executed in parallel server-side on independent DbContext instances.
        // Replaces the original 5 sequential GetByIdAsync /
        // GetMachinesBySoundSynthAsync / GetDescriptionTextAsync /
        // GetGuidsBySoundSynthAsync / GetVideosBySoundSynthAsync calls.
        SoundSynthFullDto full = await Service.GetFullAsync(Id, UiLanguage.GetIso639_3());

        if(full?.SoundSynth is null)
        {
            _synth  = null;
            _loaded = true;
            StateHasChanged();

            return;
        }

        _synth       = full.SoundSynth;
        _displayName = _synth.Name == "DB_SOFTWARE" ? L["Software"] : _synth.Name;

        // Photos arrive as List<Guid?>? (Kiota emits nullable element type even
        // for non-nullable server collections). Materialise into the existing
        // List<Guid> field shape that the .razor view expects.
        _photos = full.Photos?.Where(g => g.HasValue).Select(g => g!.Value).ToList() ?? [];

        _videos = full.Videos ?? [];

        _description = full.DescriptionHtml ?? full.DescriptionText;
        _descriptionLanguageServed = full.DescriptionLanguageCode;

        string requested = UiLanguage.GetIso639_3();
        _hasAnyDescription   = !string.IsNullOrWhiteSpace(_description);
        _descriptionFellBack = _hasAnyDescription
                            && _descriptionLanguageServed is not null
                            && !string.Equals(_descriptionLanguageServed, requested, StringComparison.Ordinal);

        // Lightweight metadata fetch so the picker can show "Has description" / "Empty" chips.
        List<SoundSynthDescriptionDto> all = await Service.GetDescriptionsAsync(Id);
        _existingDescriptionLangs = new HashSet<string>(StringComparer.Ordinal);
        foreach(SoundSynthDescriptionDto d in all ?? new List<SoundSynthDescriptionDto>())
        {
            if(!string.IsNullOrEmpty(d.LanguageCode))
                _existingDescriptionLangs.Add(d.LanguageCode);
        }

        List<MachineDto> machines = full.Machines ?? [];
        _computers   = machines.Where(m => m.Type == (int)MachineType.Computer).ToList();
        _consoles    = machines.Where(m => m.Type == (int)MachineType.Console).ToList();
        _smartphones = machines.Where(m => m.Type == (int)MachineType.Smartphone).ToList();
        _tablets = machines.Where(m => m.Type == (int)MachineType.Tablet).ToList();
        _pdas = machines.Where(m => m.Type == (int)MachineType.Pda).ToList();

        // Insert the Machines tab between Specifications and Media when the
        // synth has any attached computers/consoles/smartphones, so
        // _activeTabIndex resolves "machines" correctly on first paint after a
        // deep link. Sentinel rows hide the entire tab block at the markup
        // level, so leave _tabNames at its default for them.
        if(!_isSentinel)
        {
            bool hasMachines = _computers.Count > 0 || _consoles.Count > 0 || _smartphones.Count > 0 || _tablets.Count > 0 || _pdas.Count > 0;
            _tabNames = hasMachines
                            ? ["specifications", "machines", "media"]
                            : ["specifications", "media"];
        }

        _loaded = true;
        StateHasChanged();
    }

    /// <summary>
    ///     Open the language picker. On selection, opens the markdown editor pre-loaded with
    ///     the existing description for that language (or empty for a new translation).
    ///     Refreshes the description card on success so the user immediately sees their pending
    ///     suggestion's status (the description itself only updates when the admin accepts).
    /// </summary>
    async Task OpenDescriptionPickerAsync()
    {
        if(_synth is null) return;

        // Use the route parameter Id (always valid — the page wouldn't have rendered otherwise)
        // rather than _synth.Id.Value. This sidesteps the OwningComponentBase trap where the
        // inner-scope DTO load could in theory leave Id unpopulated.
        long soundSynthId = Id;

        // Refresh per-user pending list lazily so anonymous users never hit /auth/me/suggestions.
        await RefreshPendingDescriptionLangsAsync();

        var pickerParams = new DialogParameters
        {
            ["ExistingLanguages"] = _existingDescriptionLangs,
            ["PendingLanguages"]  = _pendingDescriptionLangs,
            ["DefaultLanguage"]   = UiLanguage.GetIso639_3()
        };
        var pickerOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Small
        };

        var pickerRef = await DialogService.ShowAsync<LanguagePickerDialog>(
            L["Choose language for description"], pickerParams, pickerOptions);
        var pickerResult = await pickerRef.Result;

        if(pickerResult.Canceled || pickerResult.Data is not string langCode) return;

        // Pre-fetch the existing markdown for the chosen language (may be null/empty).
        SoundSynthDescriptionDto existing = await Service.GetDescriptionAsync(Id, langCode);
        bool isEdit       = existing is not null && string.Equals(existing.LanguageCode, langCode, StringComparison.Ordinal);
        string initialMd  = isEdit ? (existing?.Markdown ?? string.Empty) : string.Empty;

        var editorParams = new DialogParameters
        {
            ["EntityType"]          = SuggestionEntityType.SoundSynthDescription,
            ["EntityId"]            = soundSynthId,
            ["Subkey"]              = langCode,
            ["EntityDisplayName"]   = _synth.Name ?? $"#{soundSynthId}",
            ["LanguageDisplayName"] = LanguageDisplayName(langCode),
            ["InitialMarkdown"]     = initialMd,
            ["IsEdit"]              = isEdit
        };
        var editorOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Large
        };

        var editorRef = await DialogService.ShowAsync<MarkdownSuggestionDialog>(
            L["Suggest description"], editorParams, editorOptions);
        var editorResult = await editorRef.Result;

        if(!editorResult.Canceled && editorResult.Data is not null)
        {
            // Add the just-submitted language to the pending set so a follow-up click on the
            // same language disables the button without a round-trip.
            _pendingDescriptionLangs.Add(langCode);
            StateHasChanged();
        }
    }

    async Task RefreshPendingDescriptionLangsAsync()
    {
        AuthenticationState auth = await AuthState;
        if(!(auth.User?.Identity?.IsAuthenticated ?? false))
        {
            _pendingDescriptionLangs.Clear();
            return;
        }

        List<SuggestionDto> mine = await Suggestions.GetMyAsync();
        var fresh = new HashSet<string>(StringComparer.Ordinal);

        foreach(SuggestionDto s in mine ?? new List<SuggestionDto>())
        {
            if(s.EntityType == (int?)SuggestionEntityType.SoundSynthDescription
            && s.EntityId == (long?)Id
            && s.Status == (int?)SuggestionStatus.Pending
            && !string.IsNullOrEmpty(s.Subkey))
                fresh.Add(s.Subkey);
        }

        _pendingDescriptionLangs = fresh;
    }

    static string LanguageDisplayName(string iso639_3) => iso639_3 switch
    {
        "eng" => "English",
        "spa" => "Spanish",
        "deu" => "German",
        "fra" => "French",
        "ita" => "Italian",
        "lat" => "Latin",
        "por" => "Portuguese",
        _     => iso639_3
    };

    /// <summary>
    ///     Open the entity-edit suggestion dialog for the SoundSynth scalar fields. The
    ///     user can modify any subset of the 11 scalar fields and submit; an admin can
    ///     later accept or reject each field independently in the diff panel.
    /// </summary>
    async Task OpenSoundSynthSuggestionDialogAsync()
    {
        if(_synth is null) return;

        // Use the route parameter Id (always valid — the page wouldn't have rendered
        // otherwise) rather than _synth.Id.Value to sidestep async race conditions where
        // the cast could silently produce 0 → server treats as addition request.
        long soundSynthId = Id;

        var parameters = new DialogParameters
        {
            ["EntityId"]   = soundSynthId,
            ["CurrentDto"] = _synth
        };
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Large
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoundSynthSuggestionDialog>(
            L["Suggest changes"], parameters, options);
        await dialog.Result;
    }

    /// <summary>
    ///     Open the collaborative sound-synth-photo upload dialog. Lets the signed-in user stage 1-15
    ///     pending photos, set a suggestion-level license + source URL, optionally annotate
    ///     each photo with a comment, then submit ONE Suggestion row that an admin reviews
    ///     per-photo.
    /// </summary>
    async Task OpenSuggestPhotosDialog()
    {
        int soundSynthId = Id;
        if(soundSynthId <= 0) return;

        var dialogParams = new DialogParameters
        {
            ["SoundSynthId"]   = soundSynthId,
            ["SoundSynthName"] = _synth?.Name ?? string.Empty
        };
        var dialogOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Large
        };

        var dialogRef = await DialogService.ShowAsync<SoundSynthPhotosSuggestionDialog>(
            L["Suggest sound synth photos"], dialogParams, dialogOptions);

        DialogResult result = await dialogRef.Result;

        if(result is { Canceled: false })
        {
            // Photos won't appear until an admin accepts them; nothing to refresh now.
            // Method left as a hook in case future revisions want to surface a hint.
        }
    }

    /// <summary>
    ///     Opens the SoundSynthVideoSuggestionDialog so an authenticated user can suggest a
    ///     single brand-new YouTube video link for this sound synth. The server fetches the
    ///     canonical title from YouTube oEmbed at submission time. Acceptance is gated on
    ///     admin review; the local _videos list is not refreshed because acceptance is async.
    /// </summary>
    async Task OpenSuggestVideoDialog()
    {
        int synthId = Id;
        if(synthId <= 0) return;

        var dialogParams = new DialogParameters
        {
            ["EntityId"]       = (long)synthId,
            ["SoundSynthName"] = _synth?.Name ?? string.Empty
        };
        var dialogOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Medium
        };

        var dialogRef = await DialogService.ShowAsync<SoundSynthVideoSuggestionDialog>(
            L["Suggest a YouTube video"], dialogParams, dialogOptions);

        await dialogRef.Result;
    }
}
