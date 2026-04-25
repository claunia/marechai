using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareFamilies
{
    string?                   _errorMessage;
    List<SoftwareFamilyDto>?  _families;
    bool                      _isLoading = true;
    string?                   _successMessage;

    protected override async Task OnInitializedAsync() => await LoadDataAsync();

    async Task LoadDataAsync()
    {
        _isLoading = true;
        _families  = await SoftwareFamiliesService.GetAllAsync();
        _isLoading = false;
    }

    Func<SoftwareFamilyDto, bool> QuickFilter => _ => true;

    static string FormatDate(DateTimeOffset? date, int? precision = 0)
    {
        if(date is null) return "";
        if((precision ?? 0) == 2) return date.Value.Year.ToString();
        if((precision ?? 0) == 1) return date.Value.ToString("MMMM yyyy");
        return date.Value.Date.ToShortDateString();
    }

    async Task OpenAddDialog()
    {
        DialogParameters<SoftwareFamilyDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareFamilyDialog>(L["Add Family"], parameters,
                                                                                      new DialogOptions
                                                                                      {
                                                                                          MaxWidth  = MaxWidth.Large,
                                                                                          FullWidth = true
                                                                                      });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareFamilyDialogResult data })
        {
            var dto = new SoftwareFamilyDto
            {
                Name       = data.Name,
                ParentId   = data.ParentId,

                IntroducedPrecision = data.IntroducedPrecision,
                Introduced = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value) : null
            };

            (int? id, string? errorMessage) = await SoftwareFamiliesService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Family created successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(SoftwareFamilyDto family)
    {
        SoftwareFamilyDto? full = await SoftwareFamiliesService.GetByIdAsync(family.Id ?? 0);

        if(full is null)
        {
            _errorMessage = L["Failed to load family details."];

            return;
        }

        DialogParameters<SoftwareFamilyDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.FamilyId, full.Id ?? 0 },
            { x => x.Name, full.Name },
            { x => x.ParentId, full.ParentId },
            { x => x.Introduced, full.Introduced?.DateTime },
            { x => x.IntroducedPrecision, full.IntroducedPrecision ?? 0 },
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareFamilyDialog>(L["Edit Family"], parameters,
                                                                                      new DialogOptions
                                                                                      {
                                                                                          MaxWidth  = MaxWidth.Large,
                                                                                          FullWidth = true
                                                                                      });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareFamilyDialogResult data })
        {
            var dto = new SoftwareFamilyDto
            {
                Id         = full.Id,
                Name       = data.Name,

                IntroducedPrecision = data.IntroducedPrecision,
                ParentId   = data.ParentId,
                Introduced = data.Introduced.HasValue ? new DateTimeOffset(data.Introduced.Value) : null
            };

            (bool succeeded, string? errorMessage) = await SoftwareFamiliesService.UpdateAsync(full.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Family updated successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDelete(SoftwareFamilyDto family)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete family '{0}'? This action cannot be undone."],
                              family.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Family"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await SoftwareFamiliesService.DeleteAsync(family.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Family deleted successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
