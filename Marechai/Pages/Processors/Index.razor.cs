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

namespace Marechai.Pages.Processors;

public partial class Index
{
    List<ProcessorListItem> _filteredProcessors;
    bool                    _loaded;
    List<ProcessorListItem> _processors;
    string                  _searchText;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        List<ProcessorDto> allProcessors = await Service.GetAllAsync();

        _processors = allProcessors
                     .Select(p =>
                      {
                          string displayName = p.Name ?? string.Empty;

                          if(p.Speed > 0)
                          {
                              displayName = p.GprSize > 0
                                                ? string.Format(L["{0} @{1}MHz ({2} bits)"],
                                                                p.Name,
                                                                p.Speed,
                                                                p.GprSize)
                                                : string.Format(L["{0} @{1}MHz"], p.Name, p.Speed);
                          }

                          return new ProcessorListItem
                          {
                              Id          = p.Id ?? 0,
                              DisplayName = displayName,
                              Company     = p.Company ?? string.Empty
                          };
                      })
                     .OrderBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase)
                     .ToList();

        _filteredProcessors = _processors;
        _loaded             = true;
        StateHasChanged();
    }

    void OnSearchChanged()
    {
        _filteredProcessors = string.IsNullOrWhiteSpace(_searchText)
                                  ? _processors
                                  : _processors
                                   .Where(p => p.DisplayName.Contains(_searchText,
                                                                      StringComparison.OrdinalIgnoreCase) ||
                                               p.Company.Contains(_searchText,
                                                                   StringComparison.OrdinalIgnoreCase))
                                   .ToList();
    }

    sealed class ProcessorListItem
    {
        public int    Id          { get; init; }
        public string DisplayName { get; init; }
        public string Company     { get; init; }
    }
}
