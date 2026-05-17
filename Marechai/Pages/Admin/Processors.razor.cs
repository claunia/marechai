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

public partial class Processors
{
    string                    _errorMessage;
    string                    _successMessage;
    MudDataGrid<ProcessorDto> _dataGrid;

    async Task<GridData<ProcessorDto>> ServerReload(GridState<ProcessorDto> state, CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        string sortBy         = null;
        bool   sortDescending = false;

        SortDefinition<ProcessorDto> sort = state.SortDefinitions.FirstOrDefault();

        if(sort is not null)
        {
            sortBy         = sort.SortBy;
            sortDescending = sort.Descending;
        }

        // Translate MudBlazor FilterDefinitions to the "{Column}||{Operator}||{Value}"
        // wire format consumed by ProcessorsController.ApplyFilters. Column comes from
        // PropertyColumn binding via fd.Column?.PropertyName which matches the
        // case labels in the controller switch (Name/Company/ModelCode/Introduced/Speed).
        List<string> filters = null;

        foreach(IFilterDefinition<ProcessorDto> fd in state.FilterDefinitions)
        {
            string column = fd.Column?.PropertyName;
            string op     = fd.Operator;
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

        Task<int>                countTask = ProcessorsService.GetCountAsync(filters, cancellationToken);
        Task<List<ProcessorDto>> dataTask  =
            ProcessorsService.GetPagedAsync(skip, take, sortBy, sortDescending, filters, cancellationToken);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<ProcessorDto>
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

    string FormatSpeed(double? speed) => speed is null ? "" : string.Format(L["{0} MHz"], speed.Value);

    async Task OpenAddProcessorDialog()
    {
        DialogParameters<ProcessorDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<ProcessorDialog>(L["Add Processor"], parameters,
                                                                                 new DialogOptions
                                                                                 {
                                                                                     MaxWidth  = MaxWidth.Large,
                                                                                     FullWidth = true
                                                                                 });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: ProcessorDialogResult data })
        {
            var dto = new ProcessorDto
            {
                Name             = data.Name,
                CompanyId        = data.CompanyId,
                ModelCode        = data.ModelCode,

                IntroducedPrecision = data.IntroducedPrecision,
                Introduced       = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value, TimeSpan.Zero) : null,
                InstructionSetId = data.InstructionSetId,
                Speed            = data.Speed,
                Package          = data.Package,
                Process          = data.Process,
                ProcessNm        = data.ProcessNm,
                DieSize          = data.DieSize,
                Transistors      = data.Transistors,
                Cores            = data.Cores,
                ThreadsPerCore   = data.ThreadsPerCore,
                DataBus          = data.DataBus,
                AddressBus       = data.AddressBus,
                Gprs             = data.Gprs,
                GprSize          = data.GprSize,
                Fprs             = data.Fprs,
                FprSize          = data.FprSize,
                SimdRegisters    = data.SimdRegisters,
                SimdSize         = data.SimdSize,
                L1Instruction    = data.L1Instruction,
                L1Data           = data.L1Data,
                L2               = data.L2,
                L3               = data.L3
            };

            (long? id, string errorMessage) = await ProcessorsService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Processor created successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditProcessorDialog(ProcessorDto processor)
    {
        DialogParameters<ProcessorDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.ProcessorId, processor.Id ?? 0 },
            { x => x.Name, processor.Name },
            { x => x.CompanyId, processor.CompanyId },
            { x => x.ModelCode, processor.ModelCode },
            { x => x.Introduced, processor.Introduced?.UtcDateTime },
            { x => x.IntroducedPrecision, processor.IntroducedPrecision ?? 0 },
            { x => x.InstructionSetId, processor.InstructionSetId },
            { x => x.Speed, processor.Speed },
            { x => x.Package, processor.Package },
            { x => x.Process, processor.Process },
            { x => x.ProcessNm, processor.ProcessNm },
            { x => x.DieSize, processor.DieSize },
            { x => x.Transistors, processor.Transistors },
            { x => x.Cores, processor.Cores },
            { x => x.ThreadsPerCore, processor.ThreadsPerCore },
            { x => x.DataBus, processor.DataBus },
            { x => x.AddressBus, processor.AddressBus },
            { x => x.Gprs, processor.Gprs },
            { x => x.GprSize, processor.GprSize },
            { x => x.Fprs, processor.Fprs },
            { x => x.FprSize, processor.FprSize },
            { x => x.SimdRegisters, processor.SimdRegisters },
            { x => x.SimdSize, processor.SimdSize },
            { x => x.L1Instruction, processor.L1Instruction },
            { x => x.L1Data, processor.L1Data },
            { x => x.L2, processor.L2 },
            { x => x.L3, processor.L3 }
        };

        IDialogReference dialog = await DialogService.ShowAsync<ProcessorDialog>(L["Edit Processor"], parameters,
                                                                                 new DialogOptions
                                                                                 {
                                                                                     MaxWidth  = MaxWidth.Large,
                                                                                     FullWidth = true
                                                                                 });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: ProcessorDialogResult data })
        {
            var dto = new ProcessorDto
            {
                Id               = processor.Id,
                Name             = data.Name,
                CompanyId        = data.CompanyId,

                IntroducedPrecision = data.IntroducedPrecision,
                ModelCode        = data.ModelCode,
                Introduced       = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value, TimeSpan.Zero) : null,
                InstructionSetId = data.InstructionSetId,
                Speed            = data.Speed,
                Package          = data.Package,
                Process          = data.Process,
                ProcessNm        = data.ProcessNm,
                DieSize          = data.DieSize,
                Transistors      = data.Transistors,
                Cores            = data.Cores,
                ThreadsPerCore   = data.ThreadsPerCore,
                DataBus          = data.DataBus,
                AddressBus       = data.AddressBus,
                Gprs             = data.Gprs,
                GprSize          = data.GprSize,
                Fprs             = data.Fprs,
                FprSize          = data.FprSize,
                SimdRegisters    = data.SimdRegisters,
                SimdSize         = data.SimdSize,
                L1Instruction    = data.L1Instruction,
                L1Data           = data.L1Data,
                L2               = data.L2,
                L3               = data.L3
            };

            (bool succeeded, string errorMessage) = await ProcessorsService.UpdateAsync(processor.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Processor updated successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDeleteProcessor(ProcessorDto processor)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete processor '{0}'? This action cannot be undone."],
                              processor.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Processor"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await ProcessorsService.DeleteAsync(processor.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Processor deleted successfully."];
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
        IDialogReference dialog = await DialogService.ShowAsync<ProcessorImportDialog>(L["Import Processors"],
                                                                                       new DialogOptions
                                                                                       {
                                                                                           MaxWidth  = MaxWidth.ExtraLarge,
                                                                                           FullWidth = true
                                                                                       });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
            await _dataGrid.ReloadServerData();
    }

    async Task OpenDescriptionsDialog(ProcessorDto processor)
    {
        DialogParameters<ProcessorDescriptionDialog> parameters = new()
        {
            { x => x.ProcessorId, processor.Id ?? 0 },
            { x => x.ProcessorName, processor.Name }
        };

        await DialogService.ShowAsync<ProcessorDescriptionDialog>(L["Processor Descriptions"], parameters,
                                                                   new DialogOptions
                                                                   {
                                                                       MaxWidth  = MaxWidth.Medium,
                                                                       FullWidth = true
                                                                   });
    }
}
