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
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.People;

public partial class View
{
    List<PersonByBookDto>     _books;
    List<PersonByCompanyDto>  _companies;
    string                    _description;
    List<PersonByDocumentDto> _documents;
    int                       _lastId;
    bool                      _loaded;
    List<PersonByMagazineDto> _magazines;
    PersonDto                 _person;
    List<PersonBySoftwareDto> _softwareCredits;

    [Parameter]
    public int Id { get; set; }

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

        // Single consolidated /people/{id}/full call replaces six sequential
        // round-trips. The legacy individual endpoints are still available for
        // admin / internal callers; this page is the only consumer of /full.
        // Description is fetched in parallel because /full is intentionally
        // language-agnostic (cached per id) — same pattern as Machine/GPU views.
        try
        {
            Task<PersonFullDto> fullTask        = Service.GetPersonFullAsync(Id);
            Task<string>        descriptionTask = Service.GetDescriptionTextAsync(Id);

            await Task.WhenAll(fullTask, descriptionTask);

            PersonFullDto full = fullTask.Result;

            if(full?.Person is null)
            {
                _loaded = true;
                StateHasChanged();

                return;
            }

            _person          = full.Person;
            _companies       = full.Companies       ?? [];
            _books           = full.Books           ?? [];
            _documents       = full.Documents       ?? [];
            _magazines       = full.Magazines       ?? [];
            _softwareCredits = full.SoftwareCredits ?? [];
            _description     = descriptionTask.Result;
        }
        catch(ObjectDisposedException)
        {
            // Component was disposed mid-load (navigation away). Nothing to do.
            return;
        }

        _loaded = true;
        StateHasChanged();
    }
}
