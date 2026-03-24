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

namespace Marechai.Pages.Gpus;

public partial class Index
{
    List<GpuListItem> _filteredGpus;
    List<GpuListItem> _gpus;
    bool              _loaded;
    string            _searchText;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        List<GpuDto> allGpus = await Service.GetAllAsync();

        var specialGpus = new List<GpuListItem>();
        var regularGpus = new List<GpuListItem>();

        foreach(GpuDto gpu in allGpus)
        {
            string displayName = gpu.Name ?? string.Empty;
            bool   isSpecial   = false;

            switch(displayName)
            {
                case "DB_FRAMEBUFFER":
                    displayName = L["Framebuffer"];
                    isSpecial   = true;

                    break;
                case "DB_SOFTWARE":
                    displayName = L["Software"];
                    isSpecial   = true;

                    break;
                case "DB_NONE":
                    displayName = L["None"];
                    isSpecial   = true;

                    break;
            }

            var item = new GpuListItem
            {
                Id          = gpu.Id ?? 0,
                DisplayName = displayName,
                Company     = gpu.Company ?? string.Empty,
                IsSpecial   = isSpecial,
                SortOrder   = gpu.Name switch
                {
                    "DB_FRAMEBUFFER" => 0,
                    "DB_SOFTWARE"    => 1,
                    "DB_NONE"        => 2,
                    _                => 3
                }
            };

            if(isSpecial)
                specialGpus.Add(item);
            else
                regularGpus.Add(item);
        }

        specialGpus.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
        regularGpus.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

        _gpus = [..specialGpus, ..regularGpus];
        _filteredGpus = _gpus;
        _loaded       = true;
        StateHasChanged();
    }

    void OnSearchChanged()
    {
        _filteredGpus = string.IsNullOrWhiteSpace(_searchText)
                            ? _gpus
                            : _gpus.Where(g => g.DisplayName.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                                               g.Company.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                                   .ToList();
    }

    sealed class GpuListItem
    {
        public int    Id          { get; init; }
        public string DisplayName { get; init; }
        public string Company     { get; init; }
        public bool   IsSpecial   { get; init; }
        public int    SortOrder   { get; init; }
    }
}
