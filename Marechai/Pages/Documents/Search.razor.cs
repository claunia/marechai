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

public partial class Search
{
    char?              _character;
    List<DocumentDto>  _documents;
    bool               _loaded;
    string             _startingCharacter;
    int?               _year;

    [Parameter]
    public int? Year
    {
        get => _year;
        set
        {
            if(_year == value) return;

            _year   = value;
            _loaded = false;
        }
    }

    [Parameter]
    public string StartingCharacter
    {
        get => _startingCharacter;
        set
        {
            if(_startingCharacter == value) return;

            _startingCharacter = value;
            _loaded            = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        _character = null;

        if(!string.IsNullOrWhiteSpace(StartingCharacter) && StartingCharacter.Length == 1)
        {
            _character = StartingCharacter[0];

            // ToUpper()
            if(_character >= 'a' && _character <= 'z') _character -= (char)32;

            // Check if not letter or number
            if(_character < '0' || _character > '9' && _character < 'A' || _character > 'Z') _character = null;
        }

        if(_character.HasValue) _documents = await Service.GetDocumentsByLetterAsync(_character.Value);

        if(Year.HasValue && _documents is null) _documents = await Service.GetDocumentsByYearAsync(Year.Value);

        _documents ??= await Service.GetDocumentsAsync();
        _loaded    =   true;
        StateHasChanged();
    }
}
