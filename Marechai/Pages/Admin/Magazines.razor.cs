using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Magazines
{
    string                   _errorMessage;
    string                   _successMessage;
    MudDataGrid<MagazineDto> _dataGrid;

    async Task<GridData<MagazineDto>> ServerReload(GridState<MagazineDto> state, CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        string sortBy         = null;
        bool   sortDescending = false;

        SortDefinition<MagazineDto> sort = state.SortDefinitions.FirstOrDefault();

        if(sort is not null)
        {
            sortBy         = sort.SortBy;
            sortDescending = sort.Descending;
        }

        // Translate MudBlazor FilterDefinitions to the "{Column}||{Operator}||{Value}"
        // wire format consumed by MagazinesController.ApplyFilters. Column comes from
        // PropertyColumn binding via fd.Column?.PropertyName which matches the
        // case labels in the controller switch (Title/Issn/FirstPublication/Published/Country).
        List<string> filters = null;

        foreach(IFilterDefinition<MagazineDto> fd in state.FilterDefinitions)
        {
            string column = fd.Column?.PropertyName;
            string op     = fd.Operator;
            if(string.IsNullOrEmpty(column) || string.IsNullOrEmpty(op)) continue;

            bool isEmptyOp = op is "is empty" or "is not empty";

            string value;

            switch(fd.Value)
            {
                case null:
                    if(!isEmptyOp) continue;
                    value = string.Empty;
                    break;
                case DateTime dt:
                    value = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    break;
                case DateTimeOffset dto:
                    value = dto.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    break;
                case IFormattable f:
                    value = f.ToString(null, CultureInfo.InvariantCulture);
                    break;
                default:
                    value = fd.Value.ToString();
                    break;
            }

            // Skip non-empty-check operators with no meaningful value so a freshly
            // opened (but unfilled) filter UI doesn't accidentally drop every row.
            if(!isEmptyOp && string.IsNullOrEmpty(value)) continue;

            filters ??= [];
            filters.Add($"{column}||{op}||{value}");
        }

        Task<int>               countTask = MagazinesService.GetMagazinesCountAsync(filters, cancellationToken);
        Task<List<MagazineDto>> dataTask  =
            MagazinesService.GetMagazinesPagedAsync(skip, take, sortBy, sortDescending, filters, cancellationToken);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<MagazineDto>
        {
            Items      = dataTask.Result,
            TotalItems = countTask.Result
        };
    }

    static string FormatDate(DateTimeOffset? date, int? precision = 0)
    {
        if(date is null) return "";
        if((precision ?? 0) == 2) return date.Value.Year.ToString();
        if((precision ?? 0) == 1) return date.Value.ToString("MMMM yyyy");
        return date.Value.Date.ToShortDateString();
    }

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

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: MagazineDialogResult data })
        {
            var dto = new MagazineDto
            {
                Title            = data.Title,
                NativeTitle      = data.NativeTitle,
                SortTitle        = data.SortTitle,
                Issn             = data.Issn,
                CountryId        = data.CountryId,
                Published        = data.Published.HasValue ? new DateTimeOffset(data.Published.Value, TimeSpan.Zero) : null,
                PublishedPrecision = data.PublishedPrecision,
                FirstPublication = data.FirstPublication.HasValue ? new DateTimeOffset(data.FirstPublication.Value, TimeSpan.Zero) : null,
                FirstPublicationPrecision = data.FirstPublicationPrecision
            };

            (long? id, string errorMessage) = await MagazinesService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Magazine created successfully."];
                await _dataGrid.ReloadServerData();
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
        MagazineDto fullMagazine = magazine.Id.HasValue ? await MagazinesService.GetMagazineAsync(magazine.Id.Value) : magazine;
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
            { x => x.Published, fullMagazine.Published?.UtcDateTime },
            { x => x.PublishedPrecision, fullMagazine.PublishedPrecision ?? 0 },
            { x => x.FirstPublication, fullMagazine.FirstPublication?.UtcDateTime },
            { x => x.FirstPublicationPrecision, fullMagazine.FirstPublicationPrecision ?? 0 },
        };

        IDialogReference dialog = await DialogService.ShowAsync<MagazineDialog>(L["Edit Magazine"], parameters,
                                                                                new DialogOptions
                                                                                {
                                                                                    MaxWidth  = MaxWidth.Large,
                                                                                    FullWidth = true
                                                                                });

        DialogResult result = await dialog.Result;

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
                Published        = data.Published.HasValue ? new DateTimeOffset(data.Published.Value, TimeSpan.Zero) : null,
                PublishedPrecision = data.PublishedPrecision,
                FirstPublication = data.FirstPublication.HasValue ? new DateTimeOffset(data.FirstPublication.Value, TimeSpan.Zero) : null,
                FirstPublicationPrecision = data.FirstPublicationPrecision
            };

            (bool succeeded, string errorMessage) = await MagazinesService.UpdateAsync(magazine.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Magazine updated successfully."];
                await _dataGrid.ReloadServerData();
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

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await MagazinesService.DeleteAsync(magazine.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Magazine deleted successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenImportDialog()
    {
        IDialogReference dialog = await DialogService.ShowAsync<MagazineImportDialog>(
            L["Import CSV"],
            new DialogOptions
            {
                MaxWidth  = MaxWidth.ExtraLarge,
                FullWidth = true
            });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
            await _dataGrid.ReloadServerData();
    }
}
