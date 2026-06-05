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
using Marechai.ApiClient.Models;
using Marechai.Data;
using Marechai.Helpers;
using Marechai.Pages.Suggestions;
using Marechai.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Marechai.Pages.Machines;

public partial class View
{
    int              _lastId;
    string           _description;
    string           _descriptionLanguageServed;
    bool             _descriptionFellBack;
    bool             _hasAnyDescription;
    HashSet<string>  _existingDescriptionLangs = new(StringComparer.Ordinal);
    HashSet<string>  _pendingDescriptionLangs  = new(StringComparer.Ordinal);
    bool             _isCollected;
    PhotoLightbox    _lightbox;
    bool             _loaded;
    MachineDto _machine;
    List<Guid>            _photos;
    List<MachineVideoDto> _videos;
    bool                  _togglingCollection;

    // Software tab — server-side pagination
    List<SoftwareDto> _softwareItems  = [];
    int               _softwareTotal;
    int               _softwarePage     = 1;
    const int         _softwarePageSize = 25;
    string            _softwareSearch;
    bool              _softwareLoading;

    // Tab state — backs the responsive sticky MudTabs in View.razor.
    // _tabNames is rebuilt after data loads to include "software" only when
    // the machine actually has software entries (the list can be very long,
    // so we hide the panel entirely when empty).
    string[] _tabNames = ["specifications", "media"];
    string   _activeTab = "specifications";

    int _activeTabIndex => Math.Max(0, Array.IndexOf(_tabNames, _activeTab));

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
        // missing tabs (e.g. "software" on a machine with none) to 0.
        string tab = (TabParam ?? "specifications").ToLowerInvariant();
        _activeTab = tab is "specifications" or "software" or "media" ? tab : "specifications";

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

        try
        {
            // Fetch the four independent endpoints concurrently. The Blazor
            // service wrappers each issue a single HTTP request so they're
            // safe to run in parallel; this collapses what used to be sequential
            // round-trips into one parallel batch (bounded by the slowest call).
            Task<MachineDto>            machineTask  = Service.GetMachine(Id);
            Task<List<Guid>>            photosTask   = MachinePhotosService.GetGuidsByMachineAsync(Id);
            Task<int>                   swCountTask  = Service.GetSoftwareByMachineCountAsync(Id);
            Task<List<MachineVideoDto>> videosTask   = Service.GetVideosByMachineAsync(Id);

            await Task.WhenAll(machineTask, photosTask, swCountTask, videosTask);

            _machine       = machineTask.Result;
            _photos        = photosTask.Result;
            _softwareTotal = swCountTask.Result;
            _videos        = videosTask.Result;

            // Load first page of software if any exist.
            if(_softwareTotal > 0)
                await LoadSoftwarePageAsync();

            // Load the description state separately because it issues two API
            // calls (full DTO + descriptions list) that need to chain.
            await LoadDescriptionStateAsync();

            AuthenticationState authState = await AuthState;

            if(authState.User.Identity?.IsAuthenticated == true)
                _isCollected = await CollectionSvc.IsMachineCollectedAsync(Id);

            // Insert the Software tab between Specifications and Media when the
            // machine has any software, so _activeTabIndex resolves "software"
            // correctly on first paint after a deep link.
            _tabNames = _softwareTotal > 0
                            ? ["specifications", "software", "media"]
                            : ["specifications", "media"];

            _loaded = true;
            StateHasChanged();
        }
        catch(ObjectDisposedException)
        {
            // Component was disposed during async loading — ignore
        }
    }

    async Task LoadSoftwarePageAsync(CancellationToken cancellationToken = default)
    {
        _softwareLoading = true;
        int skip = (_softwarePage - 1) * _softwarePageSize;
        _softwareItems = await Service.GetSoftwareByMachinePagedAsync(
            Id, skip, _softwarePageSize, _softwareSearch, cancellationToken: cancellationToken);
        _softwareLoading = false;
    }

    async Task OnSoftwareSearch(string text)
    {
        _softwareSearch = text;
        _softwarePage   = 1;
        _softwareTotal  = await Service.GetSoftwareByMachineCountAsync(Id, _softwareSearch);
        await LoadSoftwarePageAsync();
        StateHasChanged();
    }

    async Task OnSoftwarePageChanged(int page)
    {
        _softwarePage = page;
        await LoadSoftwarePageAsync();
        StateHasChanged();
    }

    /// <summary>
    ///     Load the description for the user's UI language, falling back to whatever the server
    ///     returns (typically English). Sets <see cref="_descriptionFellBack" /> when the served
    ///     language differs from the requested one. Also pulls the lightweight metadata list so
    ///     the language picker can render "Has description" / "Empty" chips.
    /// </summary>
    async Task LoadDescriptionStateAsync()
    {
        string requested = UiLanguage.GetIso639_3();
        MachineDescriptionDto served = await Service.GetDescriptionAsync(Id, requested);

        _description               = served?.Html ?? served?.Markdown ?? string.Empty;
        _description               = HtmlFragmentFixer.FixFragmentLinks(_description, $"/machine/{Id}");
        _descriptionLanguageServed = served?.LanguageCode;
        _hasAnyDescription         = !string.IsNullOrWhiteSpace(_description);
        _descriptionFellBack       = _hasAnyDescription
                                  && _descriptionLanguageServed is not null
                                  && !string.Equals(_descriptionLanguageServed, requested, StringComparison.Ordinal);

        // Lightweight metadata fetch so the picker can show "Has description" / "Empty" chips.
        List<MachineDescriptionDto> all = await Service.GetDescriptionsAsync(Id);
        _existingDescriptionLangs = new HashSet<string>(StringComparer.Ordinal);
        foreach(MachineDescriptionDto d in all ?? new List<MachineDescriptionDto>())
        {
            if(!string.IsNullOrEmpty(d.LanguageCode))
                _existingDescriptionLangs.Add(d.LanguageCode);
        }
    }

    /// <summary>
    ///     Open the language picker. On selection, opens the markdown editor pre-loaded with
    ///     the existing description for that language (or empty for a new translation).
    ///     Refreshes the description card on success so the user immediately sees their pending
    ///     suggestion's status (the description itself only updates when the admin accepts).
    /// </summary>
    async Task OpenDescriptionPickerAsync()
    {
        if(_machine is null) return;

        // Use the route parameter Id (always valid — the page wouldn't have rendered otherwise)
        // rather than _machine.Id.Value. This sidesteps the OwningComponentBase trap where the
        // inner-scope DTO load could in theory leave Id unpopulated.
        long machineId = Id;

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
        MachineDescriptionDto existing = await Service.GetDescriptionAsync(Id, langCode);
        bool isEdit       = existing is not null && string.Equals(existing.LanguageCode, langCode, StringComparison.Ordinal);
        string initialMd  = isEdit ? (existing?.Markdown ?? string.Empty) : string.Empty;

        var editorParams = new DialogParameters
        {
            ["EntityType"]          = SuggestionEntityType.MachineDescription,
            ["EntityId"]            = machineId,
            ["Subkey"]              = langCode,
            ["EntityDisplayName"]   = _machine.Name ?? $"#{machineId}",
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
            if(s.EntityType == (int?)SuggestionEntityType.MachineDescription
            && s.EntityId == (long?)Id
            && s.Status == (int?)SuggestionStatus.Pending
            && !string.IsNullOrEmpty(s.Subkey))
                fresh.Add(s.Subkey);
        }

        _pendingDescriptionLangs = fresh;
    }

    /// <summary>
    ///     Open the dedicated Machine suggestion dialog (scalar fields + 7 junctions). Mirrors
    ///     the canonical pencil-icon → suggestion-dialog pattern from Companies/View. Uses the
    ///     route parameter <c>Id</c> directly, matching <c>OpenDescriptionPickerAsync</c>'s
    ///     defensive convention.
    /// </summary>
    async Task OpenMachineSuggestionDialog()
    {
        if(_machine is null) return;

        var parameters = new DialogParameters
        {
            ["EntityId"]   = (long)Id,
            ["CurrentDto"] = _machine
        };
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Large
        };

        await DialogService.ShowAsync<MachineSuggestionDialog>(L["Suggest changes"], parameters, options);
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

    async Task ToggleCollectionAsync()
    {
        _togglingCollection = true;

        if(_isCollected)
        {
            (bool success, _) = await CollectionSvc.RemoveMachineFromCollectionAsync(Id);

            if(success) _isCollected = false;
        }
        else
        {
            (bool success, _) = await CollectionSvc.AddMachineToCollectionAsync(Id);

            if(success) _isCollected = true;
        }

        _togglingCollection = false;
    }

    /// <summary>
    ///     Resolve the machine kind for user-facing labels. Falls back to <c>Unknown</c>
    ///     when <see cref="_machine"/> hasn't loaded or carries an out-of-range value.
    /// </summary>
    MachineType DeviceKind => _machine?.Type is int t && Enum.IsDefined(typeof(MachineType), t)
                                  ? (MachineType)t
                                  : MachineType.Unknown;

    /// <summary>"Suggest computer/console/smartphone photos" — device-specific button label.</summary>
    string SuggestPhotosLabel => DeviceKind switch
    {
        MachineType.Computer   => L["Suggest computer photos"],
        MachineType.Console    => L["Suggest console photos"],
        MachineType.Smartphone => L["Suggest smartphone photos"],
        MachineType.Pda        => L["Suggest PDA photos"],
        MachineType.Tablet     => L["Suggest tablet photos"],
        _                      => L["Suggest photos"]
    };

    /// <summary>"Suggest more computer/console/smartphone photos" — pencil-icon tooltip on populated cards.</summary>
    string SuggestMorePhotosLabel => DeviceKind switch
    {
        MachineType.Computer   => L["Suggest more computer photos"],
        MachineType.Console    => L["Suggest more console photos"],
        MachineType.Smartphone => L["Suggest more smartphone photos"],
        MachineType.Pda        => L["Suggest more PDA photos"],
        MachineType.Tablet     => L["Suggest more tablet photos"],
        _                      => L["Suggest more photos"]
    };

    /// <summary>Empty-state hint shown when the machine has no photos yet.</summary>
    string NoPhotosHint => DeviceKind switch
    {
        MachineType.Computer   => L["No photos for this computer yet — help us by uploading some."],
        MachineType.Console    => L["No photos for this console yet — help us by uploading some."],
        MachineType.Smartphone => L["No photos for this smartphone yet — help us by uploading some."],
        MachineType.Pda        => L["No photos for this PDA yet — help us by uploading some."],
        MachineType.Tablet     => L["No photos for this tablet yet — help us by uploading some."],
        _                      => L["No photos for this device yet — help us by uploading some."]
    };

    /// <summary>
    ///     Open the collaborative machine-photo upload dialog. Lets the signed-in user stage
    ///     1-30 pending photos, set a suggestion-level license + source URL, optionally
    ///     annotate each photo with a comment, then submit ONE Suggestion row that an admin
    ///     reviews per-photo. The dialog title and entry-point button are device-specific
    ///     (computer / console / smartphone) but the underlying suggestion enum value is
    ///     <see cref="SuggestionEntityType.MachinePhoto"/> for all three.
    /// </summary>
    async Task OpenSuggestPhotosDialog()
    {
        int machineId = Id;
        if(machineId <= 0) return;

        var dialogParams = new DialogParameters
        {
            ["MachineId"]   = machineId,
            ["MachineName"] = _machine?.Name ?? string.Empty
        };
        var dialogOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Large
        };

        var dialogRef = await DialogService.ShowAsync<MachinePhotosSuggestionDialog>(
            SuggestPhotosLabel, dialogParams, dialogOptions);

        DialogResult result = await dialogRef.Result;

        if(result is { Canceled: false })
        {
            // Photos won't appear until an admin accepts them; nothing to refresh now.
            // Method left as a hook in case future revisions want to surface a hint.
        }
    }

    /// <summary>"We have no videos available about this computer/console/smartphone." —
    /// device-specific empty-state hint shown above the Suggest-video button.</summary>
    string NoVideosHint => DeviceKind switch
    {
        MachineType.Computer   => L["We have no videos available about this computer."],
        MachineType.Console    => L["We have no videos available about this console."],
        MachineType.Smartphone => L["We have no videos available about this smartphone."],
        MachineType.Pda        => L["We have no videos available about this PDA."],
        MachineType.Tablet     => L["We have no videos available about this tablet."],
        _                      => L["We have no videos available about this machine."]
    };

    /// <summary>
    ///     Open the collaborative machine-video suggestion dialog. The signed-in user pastes
    ///     a YouTube URL or 11-character video ID; the canonical title is fetched server-side
    ///     from oEmbed at submission time. Whole suggestion accepted/rejected as a unit (no
    ///     per-item dynamic accept-keys). Add-only — collaborators cannot edit/remove
    ///     existing videos. ONE enum value (<see cref="SuggestionEntityType.MachineVideo"/>)
    ///     covers Computer/Console/Smartphone via the shared Machine table.
    /// </summary>
    async Task OpenSuggestVideoDialog()
    {
        int machineId = Id;
        if(machineId <= 0) return;

        var dialogParams = new DialogParameters
        {
            ["EntityId"]    = (long)machineId,
            ["MachineName"] = _machine?.Name ?? string.Empty
        };
        var dialogOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Medium
        };

        var dialogRef = await DialogService.ShowAsync<MachineVideoSuggestionDialog>(
            L["Suggest a YouTube video"], dialogParams, dialogOptions);

        DialogResult result = await dialogRef.Result;

        if(result is { Canceled: false })
        {
            // Video won't appear until an admin accepts it; nothing to refresh now.
        }
    }
}