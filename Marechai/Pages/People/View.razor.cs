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
        // round-trips. The /full endpoint also resolves the language-aware
        // biography server-side (with English fallback collapsed into one
        // ordered query), mirroring the GPU/Processor/SoundSynth pattern.
        try
        {
            PersonFullDto full = await Service.GetPersonFullAsync(Id, UiLanguage.GetIso639_3());

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
            _description     = full.DescriptionHtml ?? full.DescriptionText;
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
