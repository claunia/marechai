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
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Resolutions
{
    string              _errorMessage;
    bool                 _isLoading = true;
    List<ResolutionDto> _items;
    string              _successMessage;

    protected override async Task OnInitializedAsync() => await LoadItemsAsync();

    async Task LoadItemsAsync()
    {
        _isLoading = true;
        _items     = await ResolutionsService.GetAllAsync();
        _isLoading = false;
    }

    async Task OpenAddDialog()
    {
        DialogParameters<ResolutionDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<ResolutionDialog>(L["Add Resolution"],
                                                                                  parameters,
                                                                                  new DialogOptions
                                                                                  {
                                                                                      MaxWidth  = MaxWidth.Medium,
                                                                                      FullWidth = true
                                                                                  });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: ResolutionDialogResult data })
        {
            var dto = new ResolutionDto
            {
                Width     = data.Width,
                Height    = data.Height,
                Colors    = data.Colors,
                Palette   = data.Palette,
                Chars     = data.Chars,
                Grayscale = data.Grayscale
            };

            (long? id, string errorMessage) = await ResolutionsService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Resolution created successfully."];
                await LoadItemsAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(ResolutionDto item)
    {
        DialogParameters<ResolutionDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.Width, item.Width ?? 1 },
            { x => x.Height, item.Height ?? 1 },
            { x => x.Colors, item.Colors },
            { x => x.Palette, item.Palette },
            { x => x.Chars, item.Chars ?? false },
            { x => x.Grayscale, item.Grayscale ?? false }
        };

        IDialogReference dialog = await DialogService.ShowAsync<ResolutionDialog>(L["Edit Resolution"],
                                                                                  parameters,
                                                                                  new DialogOptions
                                                                                  {
                                                                                      MaxWidth  = MaxWidth.Medium,
                                                                                      FullWidth = true
                                                                                  });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: ResolutionDialogResult data })
        {
            var dto = new ResolutionDto
            {
                Id        = item.Id,
                Width     = data.Width,
                Height    = data.Height,
                Colors    = data.Colors,
                Palette   = data.Palette,
                Chars     = data.Chars,
                Grayscale = data.Grayscale
            };

            (bool succeeded, string errorMessage) = await ResolutionsService.UpdateAsync(item.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Resolution updated successfully."];
                await LoadItemsAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDelete(ResolutionDto item)
    {
        string displayName = $"{item.Width}x{item.Height}";

        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(
                    L["Are you sure you want to delete resolution '{0}'? This action cannot be undone."],
                    displayName)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Resolution"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await ResolutionsService.DeleteAsync(item.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Resolution deleted successfully."];
                await LoadItemsAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
