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

public partial class Screens
{
    string          _errorMessage;
    bool             _isLoading = true;
    string          _successMessage;
    List<ScreenDto> _screens;

    protected override async Task OnInitializedAsync() => await LoadScreensAsync();

    async Task LoadScreensAsync()
    {
        _isLoading = true;
        _screens   = await ScreensService.GetAllAsync();
        _isLoading = false;
    }

    string FormatNativeResolution(ScreenDto screen)
    {
        ResolutionDto r = screen.NativeResolution?.ResolutionDto;

        if(r is null)
            return string.Empty;

        return $"{r.Width}×{r.Height}";
    }

    async Task OpenAddScreenDialog()
    {
        DialogParameters<ScreenDialog> parameters = new()
        {
            { x => x.IsNew, true },
            { x => x.Diagonal, 0.0 }
        };

        IDialogReference dialog = await DialogService.ShowAsync<ScreenDialog>(L["Add Screen"], parameters,
                                                                              new DialogOptions
                                                                              {
                                                                                  MaxWidth  = MaxWidth.Medium,
                                                                                  FullWidth = true
                                                                              });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: ScreenDialogResult data })
        {
            var dto = new ScreenDto
            {
                Diagonal           = data.Diagonal,
                Width              = data.Width,
                Height             = data.Height,
                EffectiveColors    = data.EffectiveColors,
                Type               = data.Type,
                NativeResolutionId = data.NativeResolutionId
            };

            (long? id, string errorMessage) = await ScreensService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Screen created successfully."];
                await LoadScreensAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditScreenDialog(ScreenDto screen)
    {
        DialogParameters<ScreenDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.ScreenId, screen.Id ?? 0 },
            { x => x.Diagonal, screen.Diagonal ?? 0.0 },
            { x => x.Width, screen.Width },
            { x => x.Height, screen.Height },
            { x => x.EffectiveColors, screen.EffectiveColors },
            { x => x.Type, screen.Type },
            { x => x.NativeResolutionId, screen.NativeResolutionId }
        };

        IDialogReference dialog = await DialogService.ShowAsync<ScreenDialog>(L["Edit Screen"], parameters,
                                                                              new DialogOptions
                                                                              {
                                                                                  MaxWidth  = MaxWidth.Medium,
                                                                                  FullWidth = true
                                                                              });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: ScreenDialogResult data })
        {
            var dto = new ScreenDto
            {
                Id                 = screen.Id,
                Diagonal           = data.Diagonal,
                Width              = data.Width,
                Height             = data.Height,
                EffectiveColors    = data.EffectiveColors,
                Type               = data.Type,
                NativeResolutionId = data.NativeResolutionId
            };

            (bool succeeded, string errorMessage) = await ScreensService.UpdateAsync(screen.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Screen updated successfully."];
                await LoadScreensAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDeleteScreen(ScreenDto screen)
    {
        string displayName = screen.Diagonal is not null ? $"{screen.Diagonal}\"" : $"#{screen.Id}";

        if(!string.IsNullOrWhiteSpace(screen.Type))
            displayName = $"{screen.Type} {displayName}";

        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete screen '{0}'? This action cannot be undone."],
                              displayName)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Screen"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await ScreensService.DeleteAsync(screen.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Screen deleted successfully."];
                await LoadScreensAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
