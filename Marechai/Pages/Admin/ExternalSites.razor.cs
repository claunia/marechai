using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class ExternalSites
{
    string                _errorMessage;
    bool                   _isLoading = true;
    List<ExternalSiteDto> _sites;
    string                _successMessage;

    protected override async Task OnInitializedAsync() => await LoadDataAsync();

    async Task LoadDataAsync()
    {
        _isLoading = true;
        _sites     = await ExternalSitesService.GetAllAsync();
        _isLoading = false;
    }

    async Task OpenAddDialog()
    {
        DialogParameters<ExternalSiteDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<ExternalSiteDialog>(L["Add Site"], parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth  = MaxWidth.Small,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: ExternalSiteDialogResult data })
        {
            var dto = new ExternalSiteDto
            {
                Name        = data.Name,
                UrlTemplate = data.UrlTemplate
            };

            (long? id, string errorMessage) = await ExternalSitesService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Site created successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(ExternalSiteDto site)
    {
        DialogParameters<ExternalSiteDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.Id, site.Id ?? 0 },
            { x => x.Name, site.Name },
            { x => x.UrlTemplate, site.UrlTemplate }
        };

        IDialogReference dialog = await DialogService.ShowAsync<ExternalSiteDialog>(L["Edit Site"], parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth  = MaxWidth.Small,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: ExternalSiteDialogResult data })
        {
            var dto = new ExternalSiteDto
            {
                Id          = site.Id,
                Name        = data.Name,
                UrlTemplate = data.UrlTemplate
            };

            (bool succeeded, string errorMessage) = await ExternalSitesService.UpdateAsync(site.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Site updated successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDelete(ExternalSiteDto site)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete site '{0}'? This action cannot be undone."],
                              site.Name)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Site"], parameters,
                                                                new DialogOptions
                                                                {
                                                                    MaxWidth  = MaxWidth.ExtraSmall,
                                                                    FullWidth = true
                                                                });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await ExternalSitesService.DeleteAsync(site.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Site deleted successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
