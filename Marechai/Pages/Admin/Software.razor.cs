using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Software
{
    string?            _errorMessage;
    bool               _isLoading = true;
    List<SoftwareDto>? _softwareList;
    string?            _successMessage;

    protected override async Task OnInitializedAsync() => await LoadDataAsync();

    async Task LoadDataAsync()
    {
        _isLoading    = true;
        _softwareList = await SoftwareService.GetAllSoftwareAsync();
        _isLoading    = false;
    }

    Func<SoftwareDto, bool> QuickFilter => _ => true;

    void NavigateToVersions(SoftwareDto software) =>
        NavigationManager.NavigateTo($"/admin/software/{software.Id}/versions");

    void NavigateToScreenshots(SoftwareDto software) =>
        NavigationManager.NavigateTo($"/admin/software/{software.Id}/screenshots");

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

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareDialogResult data })
        {
            var dto = new SoftwareDto
            {
                Name              = data.Name,
                FamilyId          = data.FamilyId,
                IsOperatingSystem = data.IsOperatingSystem,
                IsGame            = data.IsGame
            };

            (int? id, string? errorMessage) = await SoftwareService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Software created successfully."];
                await LoadDataAsync();
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
        SoftwareDto? full = await SoftwareService.GetSoftwareByIdAsync(software.Id ?? 0);

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
            { x => x.IsOperatingSystem, full.IsOperatingSystem ?? false },
            { x => x.IsGame, full.IsGame ?? false }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareDialog>(L["Edit Software"], parameters,
                                                                                new DialogOptions
                                                                                {
                                                                                    MaxWidth  = MaxWidth.Large,
                                                                                    FullWidth = true
                                                                                });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareDialogResult data })
        {
            var dto = new SoftwareDto
            {
                Id                = full.Id,
                Name              = data.Name,
                FamilyId          = data.FamilyId,
                IsOperatingSystem = data.IsOperatingSystem,
                IsGame            = data.IsGame
            };

            (bool succeeded, string? errorMessage) = await SoftwareService.UpdateAsync(full.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Software updated successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
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

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await SoftwareService.DeleteAsync(software.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Software deleted successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
