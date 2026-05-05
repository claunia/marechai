using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareReleases
{
    string                    _errorMessage;
    bool                       _isLoading = true;
    int?                       _parentSoftwareId;
    List<SoftwareReleaseDto>  _releases;
    string                    _successMessage;
    string                    _versionName;
    string                    _softwareName;
    bool                       _isVersionContext;
    bool                       _isSoftwareContext;

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

        await LoadDataAsync();
    }

    async Task LoadDataAsync()
    {
        _isLoading = true;

        if(_isVersionContext)
            _releases = await SoftwareReleasesService.GetByVersionAsync(VersionId);
        else if(_isSoftwareContext)
            _releases = await SoftwareReleasesService.GetBySoftwareAsync(SoftwareId);
        else
            _releases = await SoftwareReleasesService.GetAllAsync();

        _isLoading = false;
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
                await LoadDataAsync();
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
                await LoadDataAsync();
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
            await LoadDataAsync();
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
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
