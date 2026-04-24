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

public partial class InstructionSets
{
    string?                   _errorMessage;
    bool                      _isLoading = true;
    List<InstructionSetDto>?  _items;
    string?                   _successMessage;

    protected override async Task OnInitializedAsync() => await LoadItemsAsync();

    async Task LoadItemsAsync()
    {
        _isLoading = true;
        _items     = await InstructionSetsService.GetAllAsync();
        _isLoading = false;
    }

    Func<InstructionSetDto, bool> QuickFilter => _ => true;

    async Task OpenAddDialog()
    {
        DialogParameters<InstructionSetDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<InstructionSetDialog>(L["Add Instruction Set"],
                                                                                      parameters,
                                                                                      new DialogOptions
                                                                                      {
                                                                                          MaxWidth  = MaxWidth.Small,
                                                                                          FullWidth = true
                                                                                      });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: InstructionSetDialogResult data })
        {
            var dto = new InstructionSetDto
            {
                Name = data.Name
            };

            (int? id, string? errorMessage) = await InstructionSetsService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Instruction set created successfully."];
                await LoadItemsAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(InstructionSetDto item)
    {
        DialogParameters<InstructionSetDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.Name, item.Name },
            { x => x.OriginalName, item.Name }
        };

        IDialogReference dialog = await DialogService.ShowAsync<InstructionSetDialog>(L["Edit Instruction Set"],
                                                                                      parameters,
                                                                                      new DialogOptions
                                                                                      {
                                                                                          MaxWidth  = MaxWidth.Small,
                                                                                          FullWidth = true
                                                                                      });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: InstructionSetDialogResult data })
        {
            var dto = new InstructionSetDto
            {
                Id   = item.Id,
                Name = data.Name
            };

            (bool succeeded, string? errorMessage) = await InstructionSetsService.UpdateAsync(item.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Instruction set updated successfully."];
                await LoadItemsAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDelete(InstructionSetDto item)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(
                    L["Are you sure you want to delete instruction set '{0}'? This action cannot be undone."],
                    item.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Instruction Set"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await InstructionSetsService.DeleteAsync(item.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Instruction set deleted successfully."];
                await LoadItemsAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
