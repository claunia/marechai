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

public partial class Gpus
{
    string?        _errorMessage;
    bool           _isLoading = true;
    string?        _successMessage;
    List<GpuDto>?  _gpus;

    protected override async Task OnInitializedAsync() => await LoadGpusAsync();

    async Task LoadGpusAsync()
    {
        _isLoading = true;
        _gpus      = await GpusService.GetAllAsync();
        _isLoading = false;
    }

    Func<GpuDto, bool> QuickFilter => _ => true;

    static string FormatDate(DateTimeOffset? date) => date is null ? "" : date.Value.Date.ToShortDateString();

    async Task OpenAddGpuDialog()
    {
        DialogParameters<GpuDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<GpuDialog>(L["Add GPU"], parameters,
                                                                           new DialogOptions
                                                                           {
                                                                               MaxWidth  = MaxWidth.Medium,
                                                                               FullWidth = true
                                                                           });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: GpuDialogResult data })
        {
            var dto = new GpuDto
            {
                Name        = data.Name,
                CompanyId   = data.CompanyId,
                ModelCode   = data.ModelCode,
                Introduced  = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value) : null,
                Package     = data.Package,
                Process     = data.Process,
                ProcessNm   = data.ProcessNm,
                DieSize     = data.DieSize,
                Transistors = data.Transistors
            };

            (long? id, string? errorMessage) = await GpusService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["GPU created successfully."];
                await LoadGpusAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditGpuDialog(GpuDto gpu)
    {
        DialogParameters<GpuDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.GpuId, gpu.Id ?? 0 },
            { x => x.Name, gpu.Name },
            { x => x.CompanyId, gpu.CompanyId },
            { x => x.ModelCode, gpu.ModelCode },
            { x => x.Introduced, gpu.Introduced?.DateTime },
            { x => x.Package, gpu.Package },
            { x => x.Process, gpu.Process },
            { x => x.ProcessNm, gpu.ProcessNm },
            { x => x.DieSize, gpu.DieSize },
            { x => x.Transistors, gpu.Transistors }
        };

        IDialogReference dialog = await DialogService.ShowAsync<GpuDialog>(L["Edit GPU"], parameters,
                                                                           new DialogOptions
                                                                           {
                                                                               MaxWidth  = MaxWidth.Medium,
                                                                               FullWidth = true
                                                                           });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: GpuDialogResult data })
        {
            var dto = new GpuDto
            {
                Id          = gpu.Id,
                Name        = data.Name,
                CompanyId   = data.CompanyId,
                ModelCode   = data.ModelCode,
                Introduced  = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value) : null,
                Package     = data.Package,
                Process     = data.Process,
                ProcessNm   = data.ProcessNm,
                DieSize     = data.DieSize,
                Transistors = data.Transistors
            };

            (bool succeeded, string? errorMessage) = await GpusService.UpdateAsync(gpu.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["GPU updated successfully."];
                await LoadGpusAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDeleteGpu(GpuDto gpu)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete GPU '{0}'? This action cannot be undone."],
                              gpu.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete GPU"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await GpusService.DeleteAsync(gpu.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["GPU deleted successfully."];
                await LoadGpusAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
