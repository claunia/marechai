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

namespace Marechai.Pages.Companies;

public partial class View
{
    CompanyDto           _company;
    List<MachineDto>     _computers;
    List<MachineDto>     _consoles;
    string               _description;
    List<GpuDto>         _gpus;
    List<SoundSynthDto>  _soundSynths;
    List<ProcessorDto>   _processors;
    List<MachineFamilyDto> _machineFamilies;
    List<BookDto>        _books;
    List<DocumentDto>    _documents;
    List<MagazineDto>    _magazines;
    List<SoftwareDto>    _software;
    List<PersonByCompanyDto> _people;
    int                  _id;
    bool                 _loaded;
    List<CompanyLogoDto> _logos;
    int                  _selectedIndex;
    CompanyDto           _soldTo;

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

        _company = await Service.GetAsync(Id);
        List<MachineDto> machines = await Service.GetMachinesAsync(Id);

        _computers       = machines.Where(m => m.Type == (int)MachineType.Computer).ToList();
        _consoles        = machines.Where(m => m.Type == (int)MachineType.Console).ToList();
        _gpus            = await Service.GetGpusAsync(Id);
        _soundSynths     = await Service.GetSoundSynthsAsync(Id);
        _processors      = await Service.GetProcessorsAsync(Id);
        _machineFamilies = await Service.GetMachineFamiliesAsync(Id);
        _books           = await Service.GetBooksAsync(Id);
        _documents       = await Service.GetDocumentsAsync(Id);
        _magazines       = await Service.GetMagazinesAsync(Id);
        _software        = await Service.GetSoftwareAsync(Id);
        _people          = await Service.GetPeopleAsync(Id);

        _description = await Service.GetDescriptionTextAsync(Id);
        _soldTo      = await Service.GetSoldToAsync(_company.SoldToId);
        _logos       = await CompanyLogosService.GetByCompany(Id);

        _loaded = true;
        StateHasChanged();
    }

}