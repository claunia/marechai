using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareReleases
{
    string                            _errorMessage;
    int?                              _parentSoftwareId;
    string                            _searchText;
    string                            _successMessage;
    string                            _versionName;
    bool                              _isVersionContext;
    bool                              _isSoftwareContext;
    MudDataGrid<SoftwareReleaseDto>   _dataGrid;

    [Parameter] public int VersionId  { get; set; }
    [Parameter] public int SoftwareId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _isVersionContext  = VersionId  > 0;
        _isSoftwareContext = SoftwareId > 0;

        if(_isVersionContext)
        {
            SoftwareVersionDto version = await SoftwareVersionsService.GetByIdAsync(VersionId);
            _versionName      = version is not null ? $"{version.Software} - {version.VersionString}" : null;
            _parentSoftwareId = version?.SoftwareId;
        }
        else if(_isSoftwareContext)
        {
            _parentSoftwareId = SoftwareId;
        }
    }

    async Task<GridData<SoftwareReleaseDto>> ServerReload(GridState<SoftwareReleaseDto> state,
                                                          CancellationToken              cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        string sortBy         = null;
        bool   sortDescending = false;

        SortDefinition<SoftwareReleaseDto> sort = state.SortDefinitions.FirstOrDefault();

        if(sort is not null)
        {
            sortBy         = sort.SortBy;
            sortDescending = sort.Descending;
        }

        Task<int>                        countTask;
        Task<List<SoftwareReleaseDto>>   dataTask;

        if(_isVersionContext)
        {
            countTask = SoftwareReleasesService.GetCountByVersionAsync(VersionId, _searchText);
            dataTask  = SoftwareReleasesService.GetPagedByVersionAsync(VersionId, skip, take, _searchText);
        }
        else if(_isSoftwareContext)
        {
            countTask = SoftwareReleasesService.GetCountBySoftwareAsync(SoftwareId, _searchText);
            dataTask  = SoftwareReleasesService.GetPagedBySoftwareAsync(SoftwareId, skip, take, _searchText);
        }
        else
        {
            countTask = SoftwareReleasesService.GetCountAsync(_searchText);
            dataTask  = SoftwareReleasesService.GetPagedAsync(skip, take, _searchText, sortBy, sortDescending);
        }

        await Task.WhenAll(countTask, dataTask);

        return new GridData<SoftwareReleaseDto>
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

    static string FormatDate(DateTimeOffset? date, int? precision = 0)
    {
        if(date is null) return "";
        if((precision ?? 0) == 2) return date.Value.Year.ToString();
        if((precision ?? 0) == 1) return date.Value.ToString("MMMM yyyy");
        return date.Value.Date.ToShortDateString();
    }

    void GoBack()
    {
        if(!_isVersionContext)
        {
            NavigationManager.NavigateTo("/admin/software");

            return;
        }

        if(_parentSoftwareId.HasValue)
            NavigationManager.NavigateTo($"/admin/software/{_parentSoftwareId.Value}/versions");
        else
            NavigationManager.NavigateTo("/admin/software");
    }

    void NavigateToCovers(SoftwareReleaseDto release)
    {
        if(release?.Id is null)
            return;

        int softwareId = release.SoftwareId ?? _parentSoftwareId ?? 0;

        if(softwareId <= 0)
            return;

        NavigationManager.NavigateTo($"/admin/software/{softwareId}/covers/{release.Id.Value}");
    }

    async Task OpenAddDialog()
    {
        bool isCompilation = !_isVersionContext && !_isSoftwareContext;

        DialogParameters<SoftwareReleaseDialog> parameters = new()
        {
            { x => x.IsNew, true },
            { x => x.ParentVersionId, VersionId },
            { x => x.IsCompilation, isCompilation },
            { x => x.SoftwareId, _isSoftwareContext ? SoftwareId : (_isVersionContext ? _parentSoftwareId : null) },
            { x => x.SoftwareVersionId, _isVersionContext ? (int?)VersionId : null }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<SoftwareReleaseDialog>(L["Add Release"], parameters,
                                                                  new DialogOptions
                                                                  {
                                                                      MaxWidth  = MaxWidth.Large,
                                                                      FullWidth = true
                                                                  });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareReleaseDialogResult data })
        {
            var dto = new SoftwareReleaseDto
            {
                Title             = data.Title,
                IsCompilation     = data.IsCompilation,
                SoftwareId        = data.SoftwareId,
                SoftwareVersionId = data.SoftwareVersionId,
                PlatformId        = data.PlatformId,
                PublisherId       = data.PublisherId,

                ReleaseDatePrecision = data.ReleaseDatePrecision,
                ReleaseDate       = data.ReleaseDate.HasValue ? new DateTimeOffset(data.ReleaseDate.Value, TimeSpan.Zero) : null
            };

            (int? id, string errorMessage) = await SoftwareReleasesService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Release created successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(SoftwareReleaseDto release)
    {
        SoftwareReleaseDto full = await SoftwareReleasesService.GetByIdAsync(release.Id ?? 0);

        if(full is null)
        {
            _errorMessage = L["Failed to load release details."];

            return;
        }

        DialogParameters<SoftwareReleaseDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.ReleaseId, full.Id ?? 0 },
            { x => x.ParentVersionId, VersionId },
            { x => x.Title, full.Title },
            { x => x.IsCompilation, full.IsCompilation == true },
            { x => x.SoftwareId, full.SoftwareId },
            { x => x.SoftwareVersionId, full.SoftwareVersionId },
            { x => x.PlatformId, full.PlatformId },
            { x => x.PublisherId, full.PublisherId },
            { x => x.ReleaseDate, full.ReleaseDate?.UtcDateTime },
            { x => x.ReleaseDatePrecision, full.ReleaseDatePrecision ?? 0 },
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<SoftwareReleaseDialog>(L["Edit Release"], parameters,
                                                                  new DialogOptions
                                                                  {
                                                                      MaxWidth  = MaxWidth.Large,
                                                                      FullWidth = true
                                                                  });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareReleaseDialogResult data })
        {
            var dto = new SoftwareReleaseDto
            {
                Id                = full.Id,
                Title             = data.Title,
                IsCompilation     = data.IsCompilation,
                SoftwareId        = full.SoftwareId,
                SoftwareVersionId = data.SoftwareVersionId,
                PlatformId        = data.PlatformId,

                ReleaseDatePrecision = data.ReleaseDatePrecision,
                PublisherId       = data.PublisherId,
                ReleaseDate       = data.ReleaseDate.HasValue ? new DateTimeOffset(data.ReleaseDate.Value, TimeSpan.Zero) : null
            };

            (bool succeeded, string errorMessage) = await SoftwareReleasesService.UpdateAsync(full.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Release updated successfully."];
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
        IDialogReference dialog =
            await DialogService.ShowAsync<SoftwareReleaseImportDialog>(L["Import CSV"],
                                                                       new DialogOptions
                                                                       {
                                                                           MaxWidth  = MaxWidth.ExtraLarge,
                                                                           FullWidth = true
                                                                       });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
            await _dataGrid.ReloadServerData();
    }

    async Task ConfirmDelete(SoftwareReleaseDto release)
    {
        string displayName = release.Title ?? $"{release.Platform} / {release.Publisher}";

        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete release '{0}'? This action cannot be undone."],
                              displayName)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Release"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await SoftwareReleasesService.DeleteAsync(release.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Release deleted successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
