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
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.Pages.SoundSynths;

public partial class Index
{
    List<SoundSynthListItem> _filteredSynths;
    bool                     _loaded;
    string                   _searchText;
    List<SoundSynthListItem> _synths;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        List<SoundSynthDto> allSynths = await Service.GetAllAsync();

        var specialSynths = new List<SoundSynthListItem>();
        var regularSynths = new List<SoundSynthListItem>();

        foreach(SoundSynthDto synth in allSynths)
        {
            string displayName = synth.Name ?? string.Empty;
            bool   isSpecial   = false;

            if(displayName == "DB_SOFTWARE")
            {
                displayName = L["Software"];
                isSpecial   = true;
            }

            var item = new SoundSynthListItem
            {
                Id          = synth.Id ?? 0,
                DisplayName = displayName,
                Company     = synth.Company ?? string.Empty,
                IsSpecial   = isSpecial
            };

            if(isSpecial)
                specialSynths.Add(item);
            else
                regularSynths.Add(item);
        }

        regularSynths.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

        _synths         = [..specialSynths, ..regularSynths];
        _filteredSynths = _synths;
        _loaded         = true;
        StateHasChanged();
    }

    void OnSearchChanged()
    {
        _filteredSynths = string.IsNullOrWhiteSpace(_searchText)
                              ? _synths
                              : _synths
                               .Where(s => s.DisplayName.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                                           s.Company.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                               .ToList();
    }

    sealed class SoundSynthListItem
    {
        public int    Id          { get; init; }
        public string DisplayName { get; init; }
        public string Company     { get; init; }
        public bool   IsSpecial   { get; init; }
    }
}
