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

namespace Marechai.Pages.Documents;

public partial class View
{
    List<CompanyByDocumentDto>       _companies;
    DocumentDto                      _document;
    long                             _id;
    bool                             _loaded;
    List<DocumentByMachineFamilyDto> _machineFamilies;
    List<DocumentByMachineDto>       _machines;
    List<PersonByDocumentDto>        _people;
    DocumentSynopsisDto              _synopsis;

    [Parameter]
    public long Id
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

        _document = await Service.GetDocumentAsync(Id);

        if(_document is null)
        {
            _loaded = true;
            StateHasChanged();

            return;
        }

        _synopsis        = await Service.GetDocumentSynopsisAsync(Id);
        _people          = await Service.GetPeopleByDocumentAsync(Id);
        _companies       = await Service.GetCompaniesByDocumentAsync(Id);
        _machines        = await Service.GetMachinesByDocumentAsync(Id);
        _machineFamilies = await Service.GetMachineFamiliesByDocumentAsync(Id);

        _loaded = true;
        StateHasChanged();
    }
}
