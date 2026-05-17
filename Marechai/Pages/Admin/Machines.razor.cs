using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Machines
{
    string                  _errorMessage;
    string                  _successMessage;
    MudDataGrid<MachineDto> _dataGrid;

    async Task<GridData<MachineDto>> ServerReload(GridState<MachineDto> state, CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        string sortBy         = null;
        bool   sortDescending = false;

        SortDefinition<MachineDto> sort = state.SortDefinitions.FirstOrDefault();

        if(sort is not null)
        {
            sortBy         = sort.SortBy;
            sortDescending = sort.Descending;
        }

        // Translate MudBlazor FilterDefinitions to the "{Column}||{Operator}||{Value}"
        // wire format consumed by MachinesController.ApplyFilters. Column comes from
        // PropertyColumn binding via fd.Column?.PropertyName which matches the
        // case labels in the controller switch (Name/Company/Model/Type/Prototype/
        // Introduced/Family).
        List<string> filters = null;

        foreach(IFilterDefinition<MachineDto> fd in state.FilterDefinitions)
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

        Task<int>              countTask = MachinesService.GetCountAsync(filters, cancellationToken);
        Task<List<MachineDto>> dataTask  =
            MachinesService.GetPagedAsync(skip, take, sortBy, sortDescending, filters, cancellationToken);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<MachineDto>
        {
            Items      = dataTask.Result,
            TotalItems = countTask.Result
        };
    }

    string FormatType(int? type) => type switch
    {
        1 => L["Computer"],
        2 => L["Console"],
        3 => L["Smartphone"],
        4 => L["PDA"],
        5 => L["Tablet"],
        _ => L["Unknown"]
    };

    static string FormatDate(DateTimeOffset? date, int? precision = 0)
    {
        if(date is null) return "";
        if((precision ?? 0) == 2) return date.Value.Year.ToString();
        if((precision ?? 0) == 1) return date.Value.ToString("MMMM yyyy");
        return date.Value.Date.ToShortDateString();
    }

    async Task OpenAddMachineDialog()
    {
        DialogParameters<MachineDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<MachineDialog>(L["Add Machine"], parameters,
                                                                               new DialogOptions
                                                                               {
                                                                                   MaxWidth  = MaxWidth.Large,
                                                                                   FullWidth = true
                                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: MachineDialogResult data })
        {
            var dto = new MachineDto
            {
                Name       = data.Name,
                Model      = data.Model,
                CompanyId  = data.CompanyId,
                Type       = data.Type,
                Prototype  = data.Prototype,
                Introduced = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value, TimeSpan.Zero) : null,
                IntroducedPrecision = data.IntroducedPrecision,
                FamilyId   = data.FamilyId
            };

            (long? id, string errorMessage) = await MachinesService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Machine created successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditMachineDialog(MachineDto machine)
    {
        // List endpoint doesn't return CompanyId/FamilyId, fetch full details
        MachineDto fullMachine = await MachinesService.GetByIdAsync(machine.Id ?? 0);

        if(fullMachine is null)
        {
            _errorMessage = "Failed to load machine details.";

            return;
        }

        DialogParameters<MachineDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.MachineId, fullMachine.Id ?? 0 },
            { x => x.Name, fullMachine.Name },
            { x => x.Model, fullMachine.Model },
            { x => x.CompanyId, fullMachine.CompanyId },
            { x => x.Type, fullMachine.Type ?? 0 },
            { x => x.Introduced, fullMachine.Introduced?.UtcDateTime },
            { x => x.IntroducedPrecision, fullMachine.IntroducedPrecision ?? 0 },
            { x => x.Prototype, fullMachine.Prototype ?? false },
            { x => x.FamilyId, fullMachine.FamilyId }
        };

        IDialogReference dialog = await DialogService.ShowAsync<MachineDialog>(L["Edit Machine"], parameters,
                                                                               new DialogOptions
                                                                               {
                                                                                   MaxWidth  = MaxWidth.Large,
                                                                                   FullWidth = true
                                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: MachineDialogResult data })
        {
            var dto = new MachineDto
            {
                Id         = machine.Id,
                Name       = data.Name,
                Model      = data.Model,
                CompanyId  = data.CompanyId,
                Type       = data.Type,
                Prototype  = data.Prototype,
                Introduced = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value, TimeSpan.Zero) : null,
                IntroducedPrecision = data.IntroducedPrecision,
                FamilyId   = data.FamilyId
            };

            (bool succeeded, string errorMessage) = await MachinesService.UpdateAsync(machine.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Machine updated successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDeleteMachine(MachineDto machine)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete machine '{0}'? This action cannot be undone."],
                              machine.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Machine"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await MachinesService.DeleteAsync(machine.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Machine deleted successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenDescriptionsDialog(MachineDto machine)
    {
        DialogParameters<MachineDescriptionDialog> parameters = new()
        {
            { x => x.MachineId, machine.Id ?? 0 },
            { x => x.MachineName, machine.Name }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<MachineDescriptionDialog>(L["Machine Descriptions"], parameters,
                                                                     new DialogOptions
                                                                     {
                                                                         MaxWidth  = MaxWidth.Medium,
                                                                         FullWidth = true
                                                                     });

        await dialog.Result;
    }

    async Task OpenImportDialog()
    {
        IDialogReference dialog = await DialogService.ShowAsync<MachineImportDialog>(L["Import CSV"],
            new DialogOptions { MaxWidth = MaxWidth.ExtraLarge, FullWidth = true });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
            await _dataGrid.ReloadServerData();
    }
}
