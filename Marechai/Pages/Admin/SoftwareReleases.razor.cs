using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareReleases
{
    string?                    _errorMessage;
    bool                       _isLoading = true;
    int?                       _parentSoftwareId;
    List<SoftwareReleaseDto>?  _releases;
    string?                    _successMessage;
    string?                    _versionName;
    bool                       _isVersionContext;

    [Parameter] public int VersionId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _isVersionContext = VersionId > 0;

        if(_isVersionContext)
        {
            SoftwareVersionDto? version = await SoftwareVersionsService.GetByIdAsync(VersionId);
            _versionName      = version is not null ? $"{version.Software} - {version.VersionString}" : null;
            _parentSoftwareId = version?.SoftwareId;
        }

        await LoadDataAsync();
    }

    async Task LoadDataAsync()
    {
        _isLoading = true;

        _releases = _isVersionContext
                        ? await SoftwareReleasesService.GetByVersionAsync(VersionId)
                        : await SoftwareReleasesService.GetAllAsync();

        _isLoading = false;
    }

    static string FormatDate(DateTimeOffset? date) => date is null ? "" : date.Value.Date.ToShortDateString();

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
        DialogParameters<SoftwareReleaseDialog> parameters = new()
        {
            { x => x.IsNew, true },
            { x => x.ParentVersionId, VersionId },
            { x => x.IsCompilation, !_isVersionContext }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<SoftwareReleaseDialog>(L["Add Release"], parameters,
                                                                  new DialogOptions
                                                                  {
                                                                      MaxWidth  = MaxWidth.Large,
                                                                      FullWidth = true
                                                                  });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareReleaseDialogResult data })
        {
            var dto = new SoftwareReleaseDto
            {
                Title             = data.Title,
                SoftwareVersionId = data.IsCompilation ? null : VersionId,
                VariantId         = data.VariantId,
                SubvariantId      = data.SubvariantId,
                PlatformId        = data.PlatformId,
                RegionId          = data.RegionId,
                PublisherId       = data.PublisherId,
                ReleaseDate       = data.ReleaseDate.HasValue ? new DateTimeOffset(data.ReleaseDate.Value) : null
            };

            (int? id, string? errorMessage) = await SoftwareReleasesService.CreateAsync(dto);

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
        SoftwareReleaseDto? full = await SoftwareReleasesService.GetByIdAsync(release.Id ?? 0);

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
            { x => x.IsCompilation, full.SoftwareVersionId is null },
            { x => x.VariantId, full.VariantId },
            { x => x.SubvariantId, full.SubvariantId },
            { x => x.PlatformId, full.PlatformId },
            { x => x.RegionId, full.RegionId },
            { x => x.PublisherId, full.PublisherId },
            { x => x.ReleaseDate, full.ReleaseDate?.DateTime }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<SoftwareReleaseDialog>(L["Edit Release"], parameters,
                                                                  new DialogOptions
                                                                  {
                                                                      MaxWidth  = MaxWidth.Large,
                                                                      FullWidth = true
                                                                  });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareReleaseDialogResult data })
        {
            var dto = new SoftwareReleaseDto
            {
                Id                = full.Id,
                Title             = data.Title,
                SoftwareVersionId = data.IsCompilation ? null : (int?)VersionId,
                VariantId         = data.VariantId,
                SubvariantId      = data.SubvariantId,
                PlatformId        = data.PlatformId,
                RegionId          = data.RegionId,
                PublisherId       = data.PublisherId,
                ReleaseDate       = data.ReleaseDate.HasValue ? new DateTimeOffset(data.ReleaseDate.Value) : null
            };

            (bool succeeded, string? errorMessage) = await SoftwareReleasesService.UpdateAsync(full.Id ?? 0, dto);

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

    async Task ConfirmDelete(SoftwareReleaseDto release)
    {
        string displayName = $"{release.Platform} / {release.Region}";

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

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await SoftwareReleasesService.DeleteAsync(release.Id ?? 0);

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
