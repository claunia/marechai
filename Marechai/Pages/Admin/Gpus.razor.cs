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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Gpus
{
    string                _errorMessage;
    string                _successMessage;
    MudDataGrid<GpuDto>   _dataGrid;

    async Task<GridData<GpuDto>> ServerReload(GridState<GpuDto> state, CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        string sortBy         = null;
        bool   sortDescending = false;

        SortDefinition<GpuDto> sort = state.SortDefinitions.FirstOrDefault();

        if(sort is not null)
        {
            sortBy         = sort.SortBy;
            sortDescending = sort.Descending;
        }

        // Translate MudBlazor FilterDefinitions to the "{Column}||{Operator}||{Value}"
        // wire format consumed by GpusController.ApplyFilters. Column comes from
        // PropertyColumn binding via fd.Column?.PropertyName which matches the
        // case labels in the controller switch (Name/Company/ModelCode/Introduced).
        List<string> filters = null;

        foreach(IFilterDefinition<GpuDto> fd in state.FilterDefinitions)
        {
            string column   = fd.Column?.PropertyName;
            string op       = fd.Operator;
            if(string.IsNullOrEmpty(column) || string.IsNullOrEmpty(op)) continue;

            bool isEmptyOp = op is "is empty" or "is not empty";

            string value;

            switch(fd.Value)
            {
                case null:
                    if(!isEmptyOp) continue;
                    value = string.Empty;
                    break;
                case DateTime dt:
                    value = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    break;
                case DateTimeOffset dto:
                    value = dto.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    break;
                case IFormattable f:
                    value = f.ToString(null, CultureInfo.InvariantCulture);
                    break;
                default:
                    value = fd.Value.ToString();
                    break;
            }

            // Skip non-empty-check operators with no meaningful value so a freshly
            // opened (but unfilled) filter UI doesn't accidentally drop every row.
            if(!isEmptyOp && string.IsNullOrEmpty(value)) continue;

            filters ??= [];
            filters.Add($"{column}||{op}||{value}");
        }

        Task<int>          countTask = GpusService.GetCountAsync(filters, cancellationToken);
        Task<List<GpuDto>> dataTask  = GpusService.GetPagedAsync(skip, take, sortBy, sortDescending, filters, cancellationToken);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<GpuDto>
        {
            Items      = dataTask.Result,
            TotalItems = countTask.Result
        };
    }

    static string FormatDate(DateTimeOffset? date, int? precision = 0)
    {
        if(date is null) return "";
        if((precision ?? 0) == 2) return date.Value.Year.ToString();
        if((precision ?? 0) == 1) return date.Value.ToString("MMMM yyyy");
        return date.Value.Date.ToShortDateString();
    }

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

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: GpuDialogResult data })
        {
            var dto = new GpuDto
            {
                Name        = data.Name,
                CompanyId   = data.CompanyId,
                ModelCode   = data.ModelCode,

                IntroducedPrecision = data.IntroducedPrecision,
                Introduced  = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value, TimeSpan.Zero) : null,
                Package     = data.Package,
                Process     = data.Process,
                ProcessNm   = data.ProcessNm,
                DieSize     = data.DieSize,
                Transistors = data.Transistors
            };

            (long? id, string errorMessage) = await GpusService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["GPU created successfully."];
                await _dataGrid.ReloadServerData();
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
            { x => x.Introduced, gpu.Introduced?.UtcDateTime },
            { x => x.IntroducedPrecision, gpu.IntroducedPrecision ?? 0 },
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

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: GpuDialogResult data })
        {
            var dto = new GpuDto
            {
                Id          = gpu.Id,
                Name        = data.Name,
                CompanyId   = data.CompanyId,

                IntroducedPrecision = data.IntroducedPrecision,
                ModelCode   = data.ModelCode,
                Introduced  = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value, TimeSpan.Zero) : null,
                Package     = data.Package,
                Process     = data.Process,
                ProcessNm   = data.ProcessNm,
                DieSize     = data.DieSize,
                Transistors = data.Transistors
            };

            (bool succeeded, string errorMessage) = await GpusService.UpdateAsync(gpu.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["GPU updated successfully."];
                await _dataGrid.ReloadServerData();
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

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await GpusService.DeleteAsync(gpu.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["GPU deleted successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenImportDialog()
    {
        IDialogReference dialog = await DialogService.ShowAsync<GpuImportDialog>(L["Import CSV"],
                                                                                 new DialogOptions
                                                                                 {
                                                                                     MaxWidth  = MaxWidth.ExtraLarge,
                                                                                     FullWidth = true
                                                                                 });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
            await _dataGrid.ReloadServerData();
    }

    async Task OpenDescriptionsDialog(GpuDto gpu)
    {
        DialogParameters<GpuDescriptionDialog> parameters = new()
        {
            { x => x.GpuId, gpu.Id ?? 0 },
            { x => x.GpuName, gpu.Name }
        };

        await DialogService.ShowAsync<GpuDescriptionDialog>(L["GPU Descriptions"], parameters,
                                                             new DialogOptions
                                                             {
                                                                 MaxWidth  = MaxWidth.Medium,
                                                                 FullWidth = true
                                                             });
    }
}
