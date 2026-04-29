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
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Magazines;

public partial class Companies
{
    char?            _character;
    List<CompanyDto> _companies;
    List<CompanyDto> _filteredCompanies;
    bool             _loaded;
    string           _searchText;
    string           _startingCharacter;

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

        if(_character.HasValue)
            _companies = await Service.GetCompaniesByLetterAsync(_character.Value);

        _companies     ??= await Service.GetCompaniesAsync();
        _filteredCompanies = _companies;
        _loaded            = true;
        StateHasChanged();
    }

    void OnSearchChanged()
    {
        _filteredCompanies = string.IsNullOrWhiteSpace(_searchText)
                                 ? _companies
                                 : _companies
                                  .Where(c => c.Name != null &&
                                              c.Name.Contains(_searchText, System.StringComparison.OrdinalIgnoreCase))
                                  .ToList();
    }
}
