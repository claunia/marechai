using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Documents
{
    string                   _errorMessage;
    string                   _successMessage;
    MudDataGrid<DocumentDto> _dataGrid;

    async Task<GridData<DocumentDto>> ServerReload(GridState<DocumentDto> state, CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        string sortBy         = null;
        bool   sortDescending = false;

        SortDefinition<DocumentDto> sort = state.SortDefinitions.FirstOrDefault();

        if(sort is not null)
        {
            sortBy         = sort.SortBy;
            sortDescending = sort.Descending;
        }

        // Translate MudBlazor FilterDefinitions to the "{Column}||{Operator}||{Value}"
        // wire format consumed by DocumentsController.ApplyFilters. Column comes from
        // PropertyColumn binding via fd.Column?.PropertyName which matches the
        // case labels in the controller switch (Title/Published/Country/InternetArchiveUrl).
        List<string> filters = null;

        foreach(IFilterDefinition<DocumentDto> fd in state.FilterDefinitions)
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

        Task<int>               countTask = DocumentsService.GetDocumentsCountAsync(filters, cancellationToken);
        Task<List<DocumentDto>> dataTask  =
            DocumentsService.GetPagedAsync(skip, take, sortBy, sortDescending, filters, cancellationToken);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<DocumentDto>
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

    async Task OpenAddDocumentDialog()
    {
        DialogParameters<DocumentDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DocumentDialog>(L["Add Document"], parameters,
                                                                                new DialogOptions
                                                                                {
                                                                                    MaxWidth  = MaxWidth.Large,
                                                                                    FullWidth = true
                                                                                });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: DocumentDialogResult data })
        {
            var dto = new DocumentDto
            {
                Title       = data.Title,
                NativeTitle = data.NativeTitle,
                SortTitle   = data.SortTitle,
                CountryId   = data.CountryId,

                PublishedPrecision = data.PublishedPrecision,
                Published   = data.Published.HasValue ? new DateTimeOffset(data.Published.Value, TimeSpan.Zero) : null,
                InternetArchiveUrl = data.InternetArchiveUrl
            };

            (long? id, string errorMessage) = await DocumentsService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Document created successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDocumentDialog(DocumentDto document)
    {
        // Fetch full details by ID to get FK IDs (list endpoint may omit them)
        DocumentDto fullDocument = document.Id.HasValue
            ? await DocumentsService.GetDocumentAsync(document.Id.Value)
            : document;

        fullDocument ??= document;

        DialogParameters<DocumentDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.DocumentId, fullDocument.Id ?? 0 },
            { x => x.Title, fullDocument.Title ?? string.Empty },
            { x => x.NativeTitle, fullDocument.NativeTitle },
            { x => x.SortTitle, fullDocument.SortTitle },
            { x => x.CountryId, fullDocument.CountryId },
            { x => x.Published, fullDocument.Published?.UtcDateTime },
            { x => x.PublishedPrecision, fullDocument.PublishedPrecision ?? 0 },
            { x => x.InternetArchiveUrl, fullDocument.InternetArchiveUrl }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DocumentDialog>(L["Edit Document"], parameters,
                                                                                new DialogOptions
                                                                                {
                                                                                    MaxWidth  = MaxWidth.Large,
                                                                                    FullWidth = true
                                                                                });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: DocumentDialogResult data })
        {
            var dto = new DocumentDto
            {
                Id          = document.Id,
                Title       = data.Title,
                NativeTitle = data.NativeTitle,
                SortTitle   = data.SortTitle,

                PublishedPrecision = data.PublishedPrecision,
                CountryId   = data.CountryId,
                Published   = data.Published.HasValue ? new DateTimeOffset(data.Published.Value, TimeSpan.Zero) : null,
                InternetArchiveUrl = data.InternetArchiveUrl
            };

            (bool succeeded, string errorMessage) = await DocumentsService.UpdateAsync(document.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Document updated successfully."];
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
        IDialogReference dialog = await DialogService.ShowAsync<DocumentImportDialog>(
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

    async Task ConfirmDeleteDocument(DocumentDto document)
    {
        string displayName = document.Title ?? $"Document #{document.Id}";

        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete document '{0}'? This action cannot be undone."],
                              displayName)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Document"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await DocumentsService.DeleteAsync(document.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Document deleted successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
