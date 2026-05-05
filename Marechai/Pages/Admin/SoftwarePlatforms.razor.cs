using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwarePlatforms
{
    string                    _errorMessage;
    bool                       _isLoading = true;
    List<SoftwarePlatformDto> _platforms;
    string                    _successMessage;

    protected override async Task OnInitializedAsync() => await LoadDataAsync();

    async Task LoadDataAsync()
    {
        _isLoading = true;
        _platforms = await SoftwarePlatformsService.GetAllAsync();
        _isLoading = false;
    }

    async Task OpenAddDialog()
    {
        DialogParameters<SoftwarePlatformDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwarePlatformDialog>(L["Add Platform"], parameters,
                                                                                       new DialogOptions
                                                                                       {
                                                                                           MaxWidth  = MaxWidth.Small,
                                                                                           FullWidth = true
                                                                                       });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwarePlatformDialogResult data })
        {
            var dto = new SoftwarePlatformDto
            {
                Name = data.Name
            };

            (int? id, string errorMessage) = await SoftwarePlatformsService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Platform created successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(SoftwarePlatformDto platform)
    {
        DialogParameters<SoftwarePlatformDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.Name, platform.Name }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwarePlatformDialog>(L["Edit Platform"], parameters,
                                                                                       new DialogOptions
                                                                                       {
                                                                                           MaxWidth  = MaxWidth.Small,
                                                                                           FullWidth = true
                                                                                       });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwarePlatformDialogResult data })
        {
            var dto = new SoftwarePlatformDto
            {
                Id   = platform.Id,
                Name = data.Name
            };

            (bool succeeded, string errorMessage) =
                await SoftwarePlatformsService.UpdateAsync(platform.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Platform updated successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDelete(SoftwarePlatformDto platform)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete platform '{0}'? This action cannot be undone."],
                              platform.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Platform"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await SoftwarePlatformsService.DeleteAsync(platform.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Platform deleted successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
