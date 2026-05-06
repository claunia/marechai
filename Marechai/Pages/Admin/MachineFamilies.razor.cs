using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class MachineFamilies
{
    string                  _errorMessage;
    bool                     _isLoading = true;
    string                  _successMessage;
    List<MachineFamilyDto>  _families;

    protected override async Task OnInitializedAsync() => await LoadDataAsync();

    async Task LoadDataAsync()
    {
        _isLoading = true;
        _families  = await MachineFamiliesService.GetAllAsync();
        _isLoading = false;
    }

    async Task OpenAddDialog()
    {
        DialogParameters<MachineFamilyDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<MachineFamilyDialog>(L["Add Machine Family"], parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth  = MaxWidth.Small,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: MachineFamilyDialogResult data })
        {
            var dto = new MachineFamilyDto
            {
                Name      = data.Name,
                CompanyId = data.CompanyId
            };

            (long? id, string errorMessage) = await MachineFamiliesService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Machine family created successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(MachineFamilyDto family)
    {
        // List endpoint doesn't return CompanyId, fetch full details
        MachineFamilyDto fullFamily = await MachineFamiliesService.GetByIdAsync(family.Id ?? 0);

        if(fullFamily is null)
        {
            _errorMessage = "Failed to load machine family details.";

            return;
        }

        DialogParameters<MachineFamilyDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.FamilyId, fullFamily.Id ?? 0 },
            { x => x.Name, fullFamily.Name },
            { x => x.CompanyId, fullFamily.CompanyId }
        };

        IDialogReference dialog = await DialogService.ShowAsync<MachineFamilyDialog>(L["Edit Machine Family"], parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth  = MaxWidth.Small,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: MachineFamilyDialogResult data })
        {
            var dto = new MachineFamilyDto
            {
                Id        = family.Id,
                Name      = data.Name,
                CompanyId = data.CompanyId
            };

            (bool succeeded, string errorMessage) = await MachineFamiliesService.UpdateAsync(family.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Machine family updated successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDelete(MachineFamilyDto family)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete machine family '{0}'? This action cannot be undone."],
                              family.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Machine Family"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await MachineFamiliesService.DeleteAsync(family.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Machine family deleted successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
