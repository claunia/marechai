using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareVersions
{
    string?                    _errorMessage;
    bool                       _isLoading = true;
    string?                    _softwareName;
    string?                    _successMessage;
    List<SoftwareVersionDto>?  _versions;

    [Parameter] public int SoftwareId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        SoftwareDto? software = await SoftwareService.GetSoftwareByIdAsync(SoftwareId);
        _softwareName = software?.Name;
        await LoadDataAsync();
    }

    async Task LoadDataAsync()
    {
        _isLoading = true;
        _versions  = await SoftwareVersionsService.GetBySoftwareAsync(SoftwareId);
        _isLoading = false;
    }

    void NavigateToReleases(SoftwareVersionDto version) =>
        NavigationManager.NavigateTo($"/admin/software/versions/{version.Id}/releases");

    async Task OpenAddDialog()
    {
        DialogParameters<SoftwareVersionDialog> parameters = new()
        {
            { x => x.IsNew, true },
            { x => x.ParentSoftwareId, SoftwareId }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareVersionDialog>(L["Add Version"], parameters,
                                                                                       new DialogOptions
                                                                                       {
                                                                                           MaxWidth  = MaxWidth.Large,
                                                                                           FullWidth = true
                                                                                       });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareVersionDialogResult data })
        {
            var dto = new SoftwareVersionDto
            {
                SoftwareId      = SoftwareId,
                VersionString   = data.VersionString,
                PublicVersion   = data.PublicVersion,
                Codename        = data.Codename,
                ParentVersionId = data.ParentVersionId,
                LicenseId       = data.LicenseId
            };

            (int? id, string? errorMessage) = await SoftwareVersionsService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Version created successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(SoftwareVersionDto version)
    {
        SoftwareVersionDto? full = await SoftwareVersionsService.GetByIdAsync(version.Id ?? 0);

        if(full is null)
        {
            _errorMessage = L["Failed to load version details."];

            return;
        }

        DialogParameters<SoftwareVersionDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.VersionId, full.Id ?? 0 },
            { x => x.ParentSoftwareId, SoftwareId },
            { x => x.VersionString, full.VersionString },
            { x => x.PublicVersion, full.PublicVersion },
            { x => x.Codename, full.Codename },
            { x => x.ParentVersionId, full.ParentVersionId },
            { x => x.LicenseId, full.LicenseId }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareVersionDialog>(L["Edit Version"], parameters,
                                                                                       new DialogOptions
                                                                                       {
                                                                                           MaxWidth  = MaxWidth.Large,
                                                                                           FullWidth = true
                                                                                       });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareVersionDialogResult data })
        {
            var dto = new SoftwareVersionDto
            {
                Id              = full.Id,
                SoftwareId      = SoftwareId,
                VersionString   = data.VersionString,
                PublicVersion   = data.PublicVersion,
                Codename        = data.Codename,
                ParentVersionId = data.ParentVersionId,
                LicenseId       = data.LicenseId
            };

            (bool succeeded, string? errorMessage) = await SoftwareVersionsService.UpdateAsync(full.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Version updated successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDelete(SoftwareVersionDto version)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete version '{0}'? This action cannot be undone."],
                              version.VersionString)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Version"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await SoftwareVersionsService.DeleteAsync(version.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Version deleted successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
