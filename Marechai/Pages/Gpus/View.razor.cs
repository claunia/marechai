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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Gpus;

public partial class View
{
    List<MachineDto>    _computers = [];
    List<MachineDto>    _consoles  = [];
    List<MachineDto>    _smartphones = [];
    string              _displayName;
    GpuDto              _gpu;
    int                 _id;
    bool                _loaded;
    List<ResolutionDto> _resolutions = [];

    [Parameter]
    public int Id
    {
        get => _id;
        set
        {
            if(_id == value) return;

            _id     = value;
            _loaded = false;
        }
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

        _loaded = true;
        StateHasChanged();
    }
}
