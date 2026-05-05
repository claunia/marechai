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
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Magazines;

public partial class View
{
    List<CompanyByMagazineDto>       _companies;
    long                             _lastId;
    bool                             _loaded;
    List<MagazineByMachineFamilyDto> _machineFamilies;
    List<MagazineByMachineDto>       _machines;
    MagazineDto                      _magazine;
    List<PersonByMagazineDto>        _people;
    DocumentSynopsisDto              _synopsis;

    [Parameter]
    public long Id { get; set; }

    protected override void OnParametersSet()
    {
        if(Id == _lastId) return;

        _lastId = Id;
        _loaded = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        if(Id <= 0)
        {
            _loaded = true;

            return;
        }

        _magazine = await Service.GetMagazineAsync(Id);

        if(_magazine is null)
        {
            _loaded = true;
            StateHasChanged();

            return;
        }

        _synopsis        = await Service.GetMagazineSynopsisAsync(Id);
        _people          = await Service.GetPeopleByMagazineAsync(Id);
        _companies       = await Service.GetCompaniesByMagazineAsync(Id);
        _machines        = await Service.GetMachinesByMagazineAsync(Id);
        _machineFamilies = await Service.GetMachineFamiliesByMagazineAsync(Id);

        _loaded = true;
        StateHasChanged();
    }
}
