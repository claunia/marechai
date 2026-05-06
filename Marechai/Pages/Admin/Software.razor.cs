using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Software
{
    string                   _errorMessage;
    string                   _searchText;
    string                   _successMessage;
    MudDataGrid<SoftwareDto> _dataGrid;

    async Task<GridData<SoftwareDto>> ServerReload(GridState<SoftwareDto> state, CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        string sortBy         = null;
        bool   sortDescending = false;

        SortDefinition<SoftwareDto> sort = state.SortDefinitions.FirstOrDefault();

        if(sort is not null)
        {
            sortBy         = sort.SortBy;
            sortDescending = sort.Descending;
        }

        Task<int>               countTask = SoftwareService.GetCountAsync(_searchText);
        Task<List<SoftwareDto>> dataTask  = SoftwareService.GetPagedAsync(skip, take, _searchText, sortBy, sortDescending);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<SoftwareDto>
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

    void NavigateToVersions(SoftwareDto software) =>
        NavigationManager.NavigateTo($"/admin/software/{software.Id}/versions");

    void NavigateToReleases(SoftwareDto software) =>
        NavigationManager.NavigateTo($"/admin/software/{software.Id}/releases");

    void NavigateToScreenshots(SoftwareDto software) =>
        NavigationManager.NavigateTo($"/admin/software/{software.Id}/screenshots");

    void NavigateToCovers(SoftwareDto software) =>
        NavigationManager.NavigateTo($"/admin/software/{software.Id}/covers");

    async Task OpenDescriptionsDialog(SoftwareDto software)
    {
        DialogParameters<SoftwareDescriptionDialog> parameters = new()
        {
            { x => x.SoftwareId, software.Id ?? 0 },
            { x => x.SoftwareName, software.Name }
        };

        await DialogService.ShowAsync<SoftwareDescriptionDialog>(L["Descriptions"], parameters,
                                                                 new DialogOptions
                                                                 {
                                                                     MaxWidth  = MaxWidth.Medium,
                                                                     FullWidth = true
                                                                 });
    }

    async Task OpenAddDialog()
    {
        DialogParameters<SoftwareDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareDialog>(L["Add Software"], parameters,
                                                                                new DialogOptions
                                                                                {
                                                                                    MaxWidth  = MaxWidth.Large,
                                                                                    FullWidth = true
                                                                                });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareDialogResult data })
        {
            var dto = new SoftwareDto
            {
                Name              = data.Name,
                FamilyId          = data.FamilyId,
                PredecessorId     = data.PredecessorId,
                BaseSoftwareId    = data.BaseSoftwareId,
                Kind              = (int)data.Kind
            };

            (int? id, string errorMessage) = await SoftwareService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Software created successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(SoftwareDto software)
    {
        // Fetch full details for FK IDs
        SoftwareDto full = await SoftwareService.GetSoftwareByIdAsync(software.Id ?? 0);

        if(full is null)
        {
            _errorMessage = L["Failed to load software details."];

            return;
        }

        DialogParameters<SoftwareDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.SoftwareId, full.Id ?? 0 },
            { x => x.Name, full.Name },
            { x => x.FamilyId, full.FamilyId },
            { x => x.PredecessorId, full.PredecessorId },
            { x => x.PredecessorName, full.Predecessor },
            { x => x.BaseSoftwareId, full.BaseSoftwareId },
            { x => x.BaseSoftwareName, full.BaseSoftware },
            { x => x.Kind, (Marechai.Data.SoftwareKind)(full.Kind ?? 0) }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareDialog>(L["Edit Software"], parameters,
                                                                                new DialogOptions
                                                                                {
                                                                                    MaxWidth  = MaxWidth.Large,
                                                                                    FullWidth = true
                                                                                });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareDialogResult data })
        {
            var dto = new SoftwareDto
            {
                Id                = full.Id,
                Name              = data.Name,
                FamilyId          = data.FamilyId,
                PredecessorId     = data.PredecessorId,
                BaseSoftwareId    = data.BaseSoftwareId,
                Kind              = (int)data.Kind
            };

            (bool succeeded, string errorMessage) = await SoftwareService.UpdateAsync(full.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Software updated successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenMergeDialog(SoftwareDto software)
    {
        DialogParameters<SoftwareMergeDialog> parameters = new()
        {
            { x => x.SourceId, (int)(software.Id ?? 0) },
            { x => x.SourceName, software.Name }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareMergeDialog>(L["Merge Software"], parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth  = MaxWidth.Medium,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            _successMessage = string.Format(L["Software '{0}' merged successfully."], software.Name);
            await _dataGrid.ReloadServerData();
        }
    }

    async Task ConfirmDelete(SoftwareDto software)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete software '{0}'? This action cannot be undone."],
                              software.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Software"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await SoftwareService.DeleteAsync(software.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Software deleted successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
