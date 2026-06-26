using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareCompilations
{
    string                             _errorMessage;
    string                             _searchText;
    string                             _successMessage;
    MudDataGrid<SoftwareCompilationDto> _dataGrid;

    async Task<GridData<SoftwareCompilationDto>> ServerReload(GridState<SoftwareCompilationDto> state,
                                                               CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        Task<int>                            countTask = SoftwareCompilationsService.GetCountAsync(_searchText);
        Task<List<SoftwareCompilationDto>>   dataTask  = SoftwareCompilationsService.GetPagedAsync(skip, take, _searchText);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<SoftwareCompilationDto>
        {
            Items      = dataTask.Result,
            TotalItems = countTask.Result
        };
    }

    async Task OnSearch(string text)
    {
        _searchText = text;
        await _dataGrid.ReloadServerData();
    }

    void NavigateToView(SoftwareCompilationDto compilation) =>
        NavigationManager.NavigateTo($"/software-compilation/{compilation.Id}");

    async Task OpenAddDialog()
    {
        DialogParameters<SoftwareCompilationDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareCompilationDialog>(L["Add Compilation"], parameters,
                                                                                            new DialogOptions
                                                                                            {
                                                                                                MaxWidth  = MaxWidth.Large,
                                                                                                FullWidth = true
                                                                                            });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareCompilationDialogResult data })
        {
            var dto = new SoftwareCompilationDto
            {
                Name             = data.Name,
                SoftwareId       = data.SoftwareId,
                MachineId        = data.MachineId,
                PredecessorId    = data.PredecessorId,
                RelationshipType = (int)data.RelationshipType
            };

            (bool succeeded, int? id, string errorMessage) = await SoftwareCompilationsService.CreateAsync(dto);

            if(succeeded)
            {
                _successMessage = L["Compilation created successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(SoftwareCompilationDto compilation)
    {
        SoftwareCompilationDto full = await SoftwareCompilationsService.GetAsync(compilation.Id ?? 0);

        if(full is null)
        {
            _errorMessage = L["Failed to load compilation details."];

            return;
        }

        DialogParameters<SoftwareCompilationDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.CompilationId, full.Id ?? 0 },
            { x => x.Name, full.Name },
            { x => x.SoftwareId, full.SoftwareId },
            { x => x.SoftwareName, full.Software },
            { x => x.MachineId, full.MachineId },
            { x => x.MachineName, full.Machine },
            { x => x.PredecessorId, full.PredecessorId },
            { x => x.PredecessorName, full.Predecessor },
            { x => x.RelationshipType, (Marechai.Data.SoftwareRelationshipType)(full.RelationshipType ?? 0) }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareCompilationDialog>(L["Edit Compilation"], parameters,
                                                                                            new DialogOptions
                                                                                            {
                                                                                                MaxWidth  = MaxWidth.Large,
                                                                                                FullWidth = true
                                                                                            });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareCompilationDialogResult data })
        {
            var dto = new SoftwareCompilationDto
            {
                Id               = full.Id,
                Name             = data.Name,
                SoftwareId       = data.SoftwareId,
                MachineId        = data.MachineId,
                PredecessorId    = data.PredecessorId,
                RelationshipType = (int)data.RelationshipType
            };

            (bool succeeded, string errorMessage) = await SoftwareCompilationsService.UpdateAsync(full.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Compilation updated successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDelete(SoftwareCompilationDto compilation)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete compilation '{0}'? This action cannot be undone."],
                              compilation.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Compilation"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await SoftwareCompilationsService.DeleteAsync(compilation.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Compilation deleted successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
