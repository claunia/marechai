using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareVariants
{
    string?                    _errorMessage;
    bool                       _isLoading = true;
    string?                    _softwareName;
    string?                    _successMessage;
    List<SoftwareVariantDto>?  _variants;

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
        _variants  = await SoftwareVariantsService.GetBySoftwareAsync(SoftwareId);
        _isLoading = false;
    }

    void NavigateToSubvariants(SoftwareVariantDto variant) =>
        NavigationManager.NavigateTo($"/admin/software/variants/{variant.Id}/subvariants");

    async Task OpenAddDialog()
    {
        DialogParameters<SoftwareVariantDialog> parameters = new()
        {
            { x => x.IsNew, true },
            { x => x.ParentSoftwareId, SoftwareId }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareVariantDialog>(L["Add Variant"], parameters,
                                                                                       new DialogOptions
                                                                                       {
                                                                                           MaxWidth  = MaxWidth.Large,
                                                                                           FullWidth = true
                                                                                       });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareVariantDialogResult data })
        {
            var dto = new SoftwareVariantDto
            {
                SoftwareId = SoftwareId,
                Name       = data.Name
            };

            (int? id, string? errorMessage) = await SoftwareVariantsService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Variant created successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(SoftwareVariantDto variant)
    {
        SoftwareVariantDto? full = await SoftwareVariantsService.GetByIdAsync(variant.Id ?? 0);

        if(full is null)
        {
            _errorMessage = L["Failed to load variant details."];

            return;
        }

        DialogParameters<SoftwareVariantDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.VariantId, full.Id ?? 0 },
            { x => x.ParentSoftwareId, SoftwareId },
            { x => x.Name, full.Name }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareVariantDialog>(L["Edit Variant"], parameters,
                                                                                       new DialogOptions
                                                                                       {
                                                                                           MaxWidth  = MaxWidth.Large,
                                                                                           FullWidth = true
                                                                                       });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareVariantDialogResult data })
        {
            var dto = new SoftwareVariantDto
            {
                Id         = full.Id,
                SoftwareId = SoftwareId,
                Name       = data.Name
            };

            (bool succeeded, string? errorMessage) = await SoftwareVariantsService.UpdateAsync(full.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Variant updated successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDelete(SoftwareVariantDto variant)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete variant '{0}'? This action cannot be undone."],
                              variant.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Variant"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await SoftwareVariantsService.DeleteAsync(variant.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Variant deleted successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
