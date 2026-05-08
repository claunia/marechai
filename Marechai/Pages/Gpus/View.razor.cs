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
using Marechai.Shared;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Gpus;

public partial class View
{
    List<MachineDto>    _computers = [];
    List<MachineDto>    _consoles  = [];
    List<MachineDto>    _smartphones = [];
    string              _description;
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

        if(Id <= 0)
        {
            _loaded = true;

            return;
        }

        _gpu = await Service.GetByIdAsync(Id);

        if(_gpu is null)
        {
            _loaded = true;
            StateHasChanged();

            return;
        }

        _displayName = _gpu.Name switch
        {
            "DB_FRAMEBUFFER" => L["Framebuffer"],
            "DB_SOFTWARE"    => L["Software"],
            "DB_NONE"        => L["None"],
            _                => _gpu.Name
        };

        // Load resolutions
        List<ResolutionByGpuDto> resByGpu = await Service.GetResolutionsByGpuAsync(Id);

        _resolutions = [];

        foreach(ResolutionByGpuDto rbg in resByGpu)
        {
            // Try to get the nested ResolutionDto from the composed type
            ResolutionDto res = rbg.Resolution?.ResolutionDto;

            if(res is null && rbg.ResolutionId.HasValue)
                res = await Service.GetResolutionByIdAsync(rbg.ResolutionId.Value);

            if(res != null)
                _resolutions.Add(res);
        }

        List<MachineDto> machines = await Service.GetMachinesByGpuAsync(Id);
        _computers = machines.Where(m => m.Type == (int)MachineType.Computer).ToList();
        _consoles  = machines.Where(m => m.Type == (int)MachineType.Console).ToList();
        _smartphones = machines.Where(m => m.Type == (int)MachineType.Smartphone).ToList();

        _description = await Service.GetDescriptionTextAsync(Id);

        _photos = await GpuPhotosService.GetGuidsByGpuAsync(Id);

        _videos = await Service.GetVideosByGpuAsync(Id);

        // Insert the Machines tab between Specifications and Media when the
        // GPU has any attached computers/consoles/smartphones, so
        // _activeTabIndex resolves "machines" correctly on first paint after a
        // deep link.
        bool hasMachines = _computers.Count > 0 || _consoles.Count > 0 || _smartphones.Count > 0;
        _tabNames = hasMachines
                        ? ["specifications", "machines", "media"]
                        : ["specifications", "media"];

        _loaded = true;
        StateHasChanged();
    }
}
