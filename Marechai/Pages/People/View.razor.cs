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

namespace Marechai.Pages.People;

public partial class View
{
    List<PersonByBookDto>     _books;
    List<PersonByCompanyDto>  _companies;
    List<PersonByDocumentDto> _documents;
    int                       _id;
    bool                      _loaded;
    List<PersonByMagazineDto> _magazines;
    PersonDto                 _person;

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

        _person = await Service.GetPersonAsync(Id);

        if(_person is null)
        {
            _loaded = true;
            StateHasChanged();

            return;
        }

        _companies = await Service.GetCompaniesByPersonAsync(Id);
        _books     = await Service.GetBooksByPersonAsync(Id);
        _documents = await Service.GetDocumentsByPersonAsync(Id);
        _magazines = await Service.GetMagazinesByPersonAsync(Id);

        _loaded = true;
        StateHasChanged();
    }
}
