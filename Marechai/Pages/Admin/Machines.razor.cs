using System;
using System.Collections.Generic;
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
        int page     = state.Page + 1;
        int pageSize = state.PageSize;

        MachinePageDto result = await MachinesService.GetPagedAsync(page, pageSize);

        return new GridData<MachineDto>
        {
            Items      = result?.Items ?? new List<MachineDto>(),
            TotalItems = result?.TotalCount ?? 0
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
