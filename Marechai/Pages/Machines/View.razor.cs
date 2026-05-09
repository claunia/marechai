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
using Marechai.Helpers;
using Marechai.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Marechai.Pages.Machines;

public partial class View
{
    int              _lastId;
    string           _description;
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
            // Fetch the five independent endpoints concurrently. The Blazor
            // service wrappers each issue a single HTTP request so they're
            // safe to run in parallel; this collapses what used to be five
            // sequential ~200 ms round-trips into one ~370 ms parallel batch
            // (bounded by the slowest call, /full).
            Task<MachineDto>           machineTask     = Service.GetMachine(Id);
            Task<List<Guid>>           photosTask      = MachinePhotosService.GetGuidsByMachineAsync(Id);
            Task<List<SoftwareDto>>    softwareTask    = Service.GetSoftwareByMachineAsync(Id);
            Task<string>               descriptionTask = Service.GetDescriptionTextAsync(Id, UiLanguage.GetIso639_3());
            Task<List<MachineVideoDto>> videosTask     = Service.GetVideosByMachineAsync(Id);

            await Task.WhenAll(machineTask, photosTask, softwareTask, descriptionTask, videosTask);

            _machine     = machineTask.Result;
            _photos      = photosTask.Result;
            _software    = softwareTask.Result;
            _description = descriptionTask.Result;
            _videos      = videosTask.Result;

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