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

using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Marechai.Pages.Suggestions;
using MudBlazor;

namespace Marechai.Pages.Tablets;

public partial class Index
{
    int  _tablets;
    bool _loaded;
    int  _maxYear;
    int  _minYear;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        _tablets = await Service.GetTabletsCountAsync();
        _minYear     = await Service.GetMinimumYearAsync();
        _maxYear     = await Service.GetMaximumYearAsync();

        _loaded = true;
        StateHasChanged();
    }

    /// <summary>
    ///     Open the unified Machine suggestion dialog in creation mode pre-selecting and locking
    ///     the type to <see cref="MachineType.Tablet" />. The dialog handles submission +
    ///     validation; this page does not refresh on success because the new machine only appears
    ///     in the listing once an admin accepts the suggestion.
    /// </summary>
    async Task OpenSuggestNewTabletDialog()
    {
        var parameters = new DialogParameters<MachineSuggestionDialog>
        {
            { x => x.EntityId,    0L                                    },
            { x => x.CurrentDto,  (MachineDto)null                       },
            { x => x.IsCreation,  true                                   },
            { x => x.PrefillType, (MachineType?)MachineType.Tablet   }
        };

        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Large
        };

        await DialogService.ShowAsync<MachineSuggestionDialog>(L["Suggest new tablet"], parameters, options);
    }
}
