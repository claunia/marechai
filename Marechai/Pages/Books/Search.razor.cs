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

namespace Marechai.Pages.Books;

public partial class Search
{
    List<BookDto> _books;
    char?         _character;
    bool          _loaded;
    string        _lastStartingCharacter;
    int?          _lastYear;

    [Parameter]
    public int? Year { get; set; }

    [Parameter]
    public string StartingCharacter { get; set; }

    protected override void OnParametersSet()
    {
        if(Year == _lastYear && StartingCharacter == _lastStartingCharacter) return;

        _lastYear              = Year;
        _lastStartingCharacter = StartingCharacter;
        _loaded                = false;
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

        if(_character.HasValue) _books = await Service.GetBooksByLetterAsync(_character.Value);

        if(Year.HasValue && _books is null) _books = await Service.GetBooksByYearAsync(Year.Value);

        _books  ??= await Service.GetBooksAsync();
        _loaded =   true;
        StateHasChanged();
    }
}
