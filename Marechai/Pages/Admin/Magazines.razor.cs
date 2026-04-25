using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Magazines
{
    string?              _errorMessage;
    bool                 _isLoading = true;
    string?              _successMessage;
    List<MagazineDto>?   _magazines;

    protected override async Task OnInitializedAsync() => await LoadMagazinesAsync();

    async Task LoadMagazinesAsync()
    {
        _isLoading = true;
        _magazines = await MagazinesService.GetMagazinesAsync();
        _isLoading = false;
    }

    Func<MagazineDto, bool> QuickFilter => magazine =>
    {
        return true;
    };

    static string FormatDate(DateTimeOffset? date) => date is null ? "" : date.Value.Date.ToShortDateString();

    async Task OpenAddMagazineDialog()
    {
        DialogParameters<MagazineDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<MagazineDialog>(L["Add Magazine"], parameters,
                                                                                new DialogOptions
                                                                                {
                                                                                    MaxWidth  = MaxWidth.Large,
                                                                                    FullWidth = true
                                                                                });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: MagazineDialogResult data })
        {
            var dto = new MagazineDto
            {
                Title            = data.Title,
                NativeTitle      = data.NativeTitle,
                SortTitle        = data.SortTitle,
                Issn             = data.Issn,
                CountryId        = data.CountryId,
                Published        = data.Published.HasValue ? new DateTimeOffset(data.Published.Value) : null,
                FirstPublication = data.FirstPublication.HasValue ? new DateTimeOffset(data.FirstPublication.Value) : null
            };

            (long? id, string? errorMessage) = await MagazinesService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Magazine created successfully."];
                await LoadMagazinesAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditMagazineDialog(MagazineDto magazine)
    {
        // Fetch full details by ID to get FK IDs (list endpoint may omit them)
        MagazineDto? fullMagazine = magazine.Id.HasValue ? await MagazinesService.GetMagazineAsync(magazine.Id.Value) : magazine;
        fullMagazine ??= magazine;

        DialogParameters<MagazineDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.MagazineId, fullMagazine.Id ?? 0 },
            { x => x.Title, fullMagazine.Title ?? string.Empty },
            { x => x.NativeTitle, fullMagazine.NativeTitle },
            { x => x.SortTitle, fullMagazine.SortTitle },
            { x => x.Issn, fullMagazine.Issn },
            { x => x.CountryId, fullMagazine.CountryId },
            { x => x.Published, fullMagazine.Published?.DateTime },
            { x => x.FirstPublication, fullMagazine.FirstPublication?.DateTime }
        };

        IDialogReference dialog = await DialogService.ShowAsync<MagazineDialog>(L["Edit Magazine"], parameters,
                                                                                new DialogOptions
                                                                                {
                                                                                    MaxWidth  = MaxWidth.Large,
                                                                                    FullWidth = true
                                                                                });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false, Data: MagazineDialogResult data })
        {
            var dto = new MagazineDto
            {
                Id               = magazine.Id,
                Title            = data.Title,
                NativeTitle      = data.NativeTitle,
                SortTitle        = data.SortTitle,
                Issn             = data.Issn,
                CountryId        = data.CountryId,
                Published        = data.Published.HasValue ? new DateTimeOffset(data.Published.Value) : null,
                FirstPublication = data.FirstPublication.HasValue ? new DateTimeOffset(data.FirstPublication.Value) : null
            };

            (bool succeeded, string? errorMessage) = await MagazinesService.UpdateAsync(magazine.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Magazine updated successfully."];
                await LoadMagazinesAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDeleteMagazine(MagazineDto magazine)
    {
        string displayName = magazine.Title ?? $"Magazine #{magazine.Id}";

        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete magazine '{0}'? This action cannot be undone."], displayName)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Magazine"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await MagazinesService.DeleteAsync(magazine.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Magazine deleted successfully."];
                await LoadMagazinesAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
