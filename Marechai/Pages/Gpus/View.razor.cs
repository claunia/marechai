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

namespace Marechai.Pages.Gpus;

public partial class View
{
    List<MachineDto>    _computers = [];
    List<MachineDto>    _consoles  = [];
    List<MachineDto>    _smartphones = [];
    List<MachineDto>    _tablets = [];
    string              _description;
    string              _descriptionLanguageServed;
    bool                _descriptionFellBack;
    bool                _hasAnyDescription;
    HashSet<string>     _existingDescriptionLangs = new(StringComparer.Ordinal);
    HashSet<string>     _pendingDescriptionLangs  = new(StringComparer.Ordinal);
    string              _displayName;
    GpuDto              _gpu;
    int                 _lastId;
    PhotoLightbox       _lightbox;
    bool                _loaded;
    List<Guid>          _photos      = [];
    List<ResolutionDto> _resolutions = [];
    List<GpuVideoDto>   _videos      = [];

    // Tab state — backs the responsive sticky MudTabs in View.razor.
    // _tabNames is rebuilt after data loads to include "machines" only when
    // at least one of the per-machine-type lists is non-empty.
    string[] _tabNames = ["specifications", "media"];
    string   _activeTab = "specifications";

    int _activeTabIndex => Math.Max(0, Array.IndexOf(_tabNames, _activeTab));

    // Sentinel rows (DB_NONE id=-1, DB_FRAMEBUFFER id=-2 per Marechai.Database
    // Operations.DbNone / DbSoftware) represent abstract "no GPU" / "software
    // framebuffer" placeholders and have no specifications, machines, photos,
    // resolutions or videos. Detect by name so the rename pattern matches the
    // existing _displayName switch below.
    bool _isSentinel => _gpu?.Name is "DB_FRAMEBUFFER" or "DB_SOFTWARE" or "DB_NONE";

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
        // missing tabs (e.g. "machines" on a GPU with none) to 0.
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

        // Single backend round-trip: the consolidated /gpus/{id}/full endpoint
        // returns head + company logo + description + resolutions + machines +
        // photos + videos in one response. Replaces six sequential REST calls
        // (and a per-resolution N+1 fallback that was caused by the Kiota
        // composed-type-wrapper trap on ResolutionByGpuDto.Resolution before
        // the [Required] fix in Marechai.Data/Dtos/ResolutionByGpuDto.cs).
        GpuFullDto full = await Service.GetGpuFullAsync(Id, UiLanguage.GetIso639_3());

        if(full?.Gpu is null)
        {
            _loaded = true;
            StateHasChanged();

            return;
        }

        _gpu = full.Gpu;

        _displayName = _gpu.Name switch
        {
            "DB_FRAMEBUFFER" => L["Framebuffer"],
            "DB_SOFTWARE"    => L["Software"],
            "DB_NONE"        => L["None"],
            _                => _gpu.Name
        };

        _resolutions = full.Resolutions ?? [];

        List<MachineDto> machines = full.Machines ?? [];
        _computers   = machines.Where(m => m.Type == (int)MachineType.Computer).ToList();
        _consoles    = machines.Where(m => m.Type == (int)MachineType.Console).ToList();
        _smartphones = machines.Where(m => m.Type == (int)MachineType.Smartphone).ToList();
        _tablets     = machines.Where(m => m.Type == (int)MachineType.Tablet).ToList();

        _description = full.DescriptionHtml ?? full.DescriptionText;
        _descriptionLanguageServed = full.DescriptionLanguageCode;

        string requested = UiLanguage.GetIso639_3();
        _hasAnyDescription   = !string.IsNullOrWhiteSpace(_description);
        _descriptionFellBack = _hasAnyDescription
                            && _descriptionLanguageServed is not null
                            && !string.Equals(_descriptionLanguageServed, requested, StringComparison.Ordinal);

        // Lightweight metadata fetch so the picker can show "Has description" / "Empty" chips.
        List<GpuDescriptionDto> all = await Service.GetDescriptionsAsync(Id);
        _existingDescriptionLangs = new HashSet<string>(StringComparer.Ordinal);
        foreach(GpuDescriptionDto d in all ?? new List<GpuDescriptionDto>())
        {
            if(!string.IsNullOrEmpty(d.LanguageCode))
                _existingDescriptionLangs.Add(d.LanguageCode);
        }

        // Kiota emits photos as List<Guid?>? from the OpenAPI primitive collection.
        // Materialize to List<Guid> by dropping the nullability (server projects
        // GpuPhoto.Id which is non-nullable on disk).
        _photos = full.Photos?.Where(g => g.HasValue).Select(g => g!.Value).ToList() ?? [];
        _videos = full.Videos ?? [];

        // Insert the Machines tab between Specifications and Media when the
        // GPU has any attached computers/consoles/smartphones, so
        // _activeTabIndex resolves "machines" correctly on first paint after a
        // deep link. Sentinel rows hide the entire tab block at the markup
        // level, so leave _tabNames at its default for them.
        if(!_isSentinel)
        {
            bool hasMachines = _computers.Count > 0 || _consoles.Count > 0 || _smartphones.Count > 0 || _tablets.Count > 0;
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
        if(_gpu is null) return;

        // Use the route parameter Id (always valid — the page wouldn't have rendered otherwise)
        // rather than _gpu.Id.Value. This sidesteps the OwningComponentBase trap where the
        // inner-scope DTO load could in theory leave Id unpopulated.
        long gpuId = Id;

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
        GpuDescriptionDto existing = await Service.GetDescriptionAsync(Id, langCode);
        bool isEdit       = existing is not null && string.Equals(existing.LanguageCode, langCode, StringComparison.Ordinal);
        string initialMd  = isEdit ? (existing?.Markdown ?? string.Empty) : string.Empty;

        var editorParams = new DialogParameters
        {
            ["EntityType"]          = SuggestionEntityType.GpuDescription,
            ["EntityId"]            = gpuId,
            ["Subkey"]              = langCode,
            ["EntityDisplayName"]   = _displayName ?? _gpu.Name ?? $"#{gpuId}",
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
            if(s.EntityType == (int?)SuggestionEntityType.GpuDescription
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
    ///     Open the dedicated GPU edit-suggestion dialog. Captures the route parameter Id
    ///     to a local long (not _gpu.Id.Value) to sidestep async race conditions where the
    ///     full DTO load could leave Id unpopulated. Sentinel rows (DB_NONE/DB_FRAMEBUFFER/
    ///     DB_SOFTWARE) are placeholders with no editable details and are guarded out of
    ///     the View by _isSentinel; the pencil button is also wrapped in AuthorizeView so
    ///     anonymous users never see it.
    /// </summary>
    async Task OpenGpuSuggestionDialogAsync()
    {
        if(_gpu is null) return;

        long gpuId = Id;

        var dialogParams = new DialogParameters
        {
            ["EntityId"]   = gpuId,
            ["CurrentDto"] = _gpu
        };
        var dialogOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Large
        };

        var dialogRef = await DialogService.ShowAsync<GpuSuggestionDialog>(
            L["Suggest changes"],
            dialogParams, dialogOptions);

        // Don't need to inspect the result here — Snackbar surfaces success/failure inside
        // the dialog itself. Pulling the result via await keeps the dialog Task alive
        // until the user closes it.
        await dialogRef.Result;
    }

    /// <summary>
    ///     Open the collaborative GPU-photos batch upload dialog. The user can stage 1-15
    ///     pending photos, set a suggestion-level license + source URL, optionally annotate
    ///     each photo with a comment, then submit ONE Suggestion row that an admin reviews
    ///     per-photo.
    /// </summary>
    async Task OpenSuggestPhotosDialog()
    {
        int gpuId = Id;
        if(gpuId <= 0) return;

        var dialogParams = new DialogParameters
        {
            ["GpuId"]   = gpuId,
            ["GpuName"] = _gpu?.Name ?? string.Empty
        };
        var dialogOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Large
        };

        var dialogRef = await DialogService.ShowAsync<GpuPhotosSuggestionDialog>(
            L["Suggest GPU photos"], dialogParams, dialogOptions);

        DialogResult result = await dialogRef.Result;

        // If the suggestion was submitted (DialogResult.Ok), reload the photos list so the
        // page stays in sync once an admin accepts. We don't need to refresh anything else.
        if(result is { Canceled: false })
        {
            // Photos won't appear until an admin accepts them; nothing to refresh now.
            // Method left as a hook in case future revisions want to surface a hint.
        }
    }

    /// <summary>
    ///     Opens the GpuVideoSuggestionDialog so an authenticated user can suggest a single
    ///     brand-new YouTube video link for this GPU. The server fetches the canonical title
    ///     from YouTube oEmbed at submission time. Acceptance is gated on admin review; the
    ///     local _videos list is not refreshed because acceptance is async.
    /// </summary>
    async Task OpenSuggestVideoDialog()
    {
        int gpuId = Id;
        if(gpuId <= 0) return;

        var dialogParams = new DialogParameters
        {
            ["EntityId"] = (long)gpuId,
            ["GpuName"]  = _gpu?.Name ?? string.Empty
        };
        var dialogOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Medium
        };

        var dialogRef = await DialogService.ShowAsync<GpuVideoSuggestionDialog>(
            L["Suggest a YouTube video"], dialogParams, dialogOptions);

        await dialogRef.Result;
    }
}
