using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareSubvariants
{
    string?                       _errorMessage;
    bool                          _isLoading = true;
    int?                          _parentSoftwareId;
    List<SoftwareSubvariantDto>?  _subvariants;
    string?                       _successMessage;
    string?                       _variantName;

    [Parameter] public int VariantId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        SoftwareVariantDto? variant = await SoftwareVariantsService.GetByIdAsync(VariantId);
        _variantName      = variant?.Name;
        _parentSoftwareId = variant?.SoftwareId;
        await LoadDataAsync();
    }

    async Task LoadDataAsync()
    {
        _isLoading   = true;
        _subvariants = await SoftwareSubvariantsService.GetByVariantAsync(VariantId);
        _isLoading   = false;
    }

    void GoBack()
    {
        if(_parentSoftwareId.HasValue)
            NavigationManager.NavigateTo($"/admin/software/{_parentSoftwareId.Value}/variants");
        else
            NavigationManager.NavigateTo("/admin/software");
    }

    async Task OpenAddDialog()
    {
        DialogParameters<SoftwareSubvariantDialog> parameters = new()
        {
            { x => x.IsNew, true },
            { x => x.ParentVariantId, VariantId }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<SoftwareSubvariantDialog>(L["Add Subvariant"], parameters,
                                                                     new DialogOptions
                                                                     {
                                                                         MaxWidth  = MaxWidth.Medium,
                                                                         FullWidth = true
                                                                     });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareSubvariantDialogResult data })
        {
            var dto = new SoftwareSubvariantDto
            {
                VariantId = VariantId,
                Name      = data.Name
            };

            (int? id, string? errorMessage) = await SoftwareSubvariantsService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Subvariant created successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(SoftwareSubvariantDto subvariant)
    {
        SoftwareSubvariantDto? full = await SoftwareSubvariantsService.GetByIdAsync(subvariant.Id ?? 0);

        if(full is null)
        {
            _errorMessage = L["Failed to load subvariant details."];

            return;
        }

        DialogParameters<SoftwareSubvariantDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.SubvariantId, full.Id ?? 0 },
            { x => x.ParentVariantId, VariantId },
            { x => x.Name, full.Name }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<SoftwareSubvariantDialog>(L["Edit Subvariant"], parameters,
                                                                     new DialogOptions
                                                                     {
                                                                         MaxWidth  = MaxWidth.Medium,
                                                                         FullWidth = true
                                                                     });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareSubvariantDialogResult data })
        {
            var dto = new SoftwareSubvariantDto
            {
                Id        = full.Id,
                VariantId = VariantId,
                Name      = data.Name
            };

            (bool succeeded, string? errorMessage) =
                await SoftwareSubvariantsService.UpdateAsync(full.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Subvariant updated successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDelete(SoftwareSubvariantDto subvariant)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete subvariant '{0}'? This action cannot be undone."],
                              subvariant.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Subvariant"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) =
                await SoftwareSubvariantsService.DeleteAsync(subvariant.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Subvariant deleted successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
