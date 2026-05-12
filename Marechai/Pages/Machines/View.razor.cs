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
    List<Guid>       _photos;
    List<SoftwareDto> _software;
    List<MachineVideoDto> _videos;
    bool             _togglingCollection;

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
            Task<MachineDto>           machineTask     = Service.GetMachine(Id);
            Task<List<Guid>>           photosTask      = MachinePhotosService.GetGuidsByMachineAsync(Id);
            Task<List<SoftwareDto>>    softwareTask    = Service.GetSoftwareByMachineAsync(Id);
            Task<List<MachineVideoDto>> videosTask     = Service.GetVideosByMachineAsync(Id);

            await Task.WhenAll(machineTask, photosTask, softwareTask, videosTask);

            _machine     = machineTask.Result;
            _photos      = photosTask.Result;
            _software    = softwareTask.Result;
            _videos      = videosTask.Result;

            // Load the description state separately because it issues two API
            // calls (full DTO + descriptions list) that need to chain.
            await LoadDescriptionStateAsync();

            AuthenticationState authState = await AuthState;

            if(authState.User.Identity?.IsAuthenticated == true)
                _isCollected = await CollectionSvc.IsMachineCollectedAsync(Id);

            // Insert the Software tab between Specifications and Media when the
            // machine has any software, so _activeTabIndex resolves "software"
            // correctly on first paint after a deep link.
            _tabNames = _software is { Count: > 0 }
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
}