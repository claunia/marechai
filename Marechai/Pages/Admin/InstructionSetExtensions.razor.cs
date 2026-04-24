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

public partial class InstructionSetExtensions
{
    string?                            _errorMessage;
    bool                               _isLoading = true;
    List<InstructionSetExtensionDto>?  _items;
    string?                            _successMessage;

    protected override async Task OnInitializedAsync() => await LoadItemsAsync();

    async Task LoadItemsAsync()
    {
        _isLoading = true;
        _items     = await InstructionSetExtensionsService.GetAllAsync();
        _isLoading = false;
    }

    Func<InstructionSetExtensionDto, bool> QuickFilter => _ => true;

    async Task OpenAddDialog()
    {
        DialogParameters<InstructionSetExtensionDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<InstructionSetExtensionDialog>(L["Add Extension"], parameters,
                                                                         new DialogOptions
                                                                         {
                                                                             MaxWidth  = MaxWidth.Small,
                                                                             FullWidth = true
                                                                         });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: InstructionSetExtensionDialogResult data })
        {
            var dto = new InstructionSetExtensionDto
            {
                Extension = data.Extension
            };

            (int? id, string? errorMessage) = await InstructionSetExtensionsService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Extension created successfully."];
                await LoadItemsAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(InstructionSetExtensionDto item)
    {
        DialogParameters<InstructionSetExtensionDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.Extension, item.Extension },
            { x => x.OriginalExtension, item.Extension }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<InstructionSetExtensionDialog>(L["Edit Extension"], parameters,
                                                                         new DialogOptions
                                                                         {
                                                                             MaxWidth  = MaxWidth.Small,
                                                                             FullWidth = true
                                                                         });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: InstructionSetExtensionDialogResult data })
        {
            var dto = new InstructionSetExtensionDto
            {
                Id        = item.Id,
                Extension = data.Extension
            };

            (bool succeeded, string? errorMessage) =
                await InstructionSetExtensionsService.UpdateAsync(item.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Extension updated successfully."];
                await LoadItemsAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDelete(InstructionSetExtensionDto item)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete extension '{0}'? This action cannot be undone."],
                              item.Extension)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Extension"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await InstructionSetExtensionsService.DeleteAsync(item.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Extension deleted successfully."];
                await LoadItemsAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
