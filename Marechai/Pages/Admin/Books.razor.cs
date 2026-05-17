using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Books
{
    string              _errorMessage;
    string              _successMessage;
    MudDataGrid<BookDto> _dataGrid;

    async Task<GridData<BookDto>> ServerReload(GridState<BookDto> state, CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        string sortBy         = null;
        bool   sortDescending = false;

        SortDefinition<BookDto> sort = state.SortDefinitions.FirstOrDefault();

        if(sort is not null)
        {
            sortBy         = sort.SortBy;
            sortDescending = sort.Descending;
        }

        // Translate MudBlazor FilterDefinitions to the "{Column}||{Operator}||{Value}"
        // wire format consumed by BooksController.ApplyFilters. Column comes from
        // PropertyColumn binding via fd.Column?.PropertyName which matches the
        // case labels in the controller switch (Title/Isbn/Edition/Pages/Published/
        // Country/InternetArchiveUrl).
        List<string> filters = null;

        foreach(IFilterDefinition<BookDto> fd in state.FilterDefinitions)
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

        Task<int>           countTask = BooksService.GetBooksCountAsync(filters, cancellationToken);
        Task<List<BookDto>> dataTask  =
            BooksService.GetPagedAsync(skip, take, sortBy, sortDescending, filters, cancellationToken);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<BookDto>
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

    async Task OpenAddBookDialog()
    {
        DialogParameters<BookDialog> parameters = new()
        {
            { x => x.IsNew, true }
        };

        IDialogReference dialog = await DialogService.ShowAsync<BookDialog>(L["Add Book"], parameters,
                                                                            new DialogOptions
                                                                            {
                                                                                MaxWidth  = MaxWidth.Large,
                                                                                FullWidth = true
                                                                            });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: BookDialogResult data })
        {
            var dto = new BookDto
            {
                Title       = data.Title,
                NativeTitle = data.NativeTitle,
                SortTitle   = data.SortTitle,
                Isbn        = data.Isbn,
                Edition     = data.Edition,
                Pages       = data.Pages,
                CountryId   = data.CountryId,

                PublishedPrecision = data.PublishedPrecision,
                Published   = data.Published.HasValue ? new DateTimeOffset(data.Published.Value, TimeSpan.Zero) : null,
                PreviousId  = data.PreviousId,
                SourceId    = data.SourceId,
                InternetArchiveUrl = data.InternetArchiveUrl
            };

            (long? id, string errorMessage) = await BooksService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Book created successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditBookDialog(BookDto book)
    {
        // Fetch full details by ID to get FK IDs (list endpoint may omit them)
        BookDto fullBook = book.Id.HasValue ? await BooksService.GetBookAsync(book.Id.Value) : book;
        fullBook ??= book;

        DialogParameters<BookDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.BookId, fullBook.Id ?? 0 },
            { x => x.Title, fullBook.Title ?? string.Empty },
            { x => x.NativeTitle, fullBook.NativeTitle },
            { x => x.SortTitle, fullBook.SortTitle },
            { x => x.Isbn, fullBook.Isbn },
            { x => x.Edition, fullBook.Edition },
            { x => x.Pages, fullBook.Pages },
            { x => x.CountryId, fullBook.CountryId },
            { x => x.Published, fullBook.Published?.UtcDateTime },
            { x => x.PublishedPrecision, fullBook.PublishedPrecision ?? 0 },
            { x => x.PreviousId, fullBook.PreviousId },
            { x => x.SourceId, fullBook.SourceId },
            { x => x.HasCover, fullBook.CoverGuid is not null },
            { x => x.InternetArchiveUrl, fullBook.InternetArchiveUrl }
        };

        IDialogReference dialog = await DialogService.ShowAsync<BookDialog>(L["Edit Book"], parameters,
                                                                            new DialogOptions
                                                                            {
                                                                                MaxWidth  = MaxWidth.Large,
                                                                                FullWidth = true
                                                                            });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: BookDialogResult data })
        {
            var dto = new BookDto
            {
                Id          = book.Id,
                Title       = data.Title,
                NativeTitle = data.NativeTitle,
                SortTitle   = data.SortTitle,
                Isbn        = data.Isbn,
                Edition     = data.Edition,
                Pages       = data.Pages,

                PublishedPrecision = data.PublishedPrecision,
                CountryId   = data.CountryId,
                Published   = data.Published.HasValue ? new DateTimeOffset(data.Published.Value, TimeSpan.Zero) : null,
                PreviousId  = data.PreviousId,
                SourceId    = data.SourceId,
                InternetArchiveUrl = data.InternetArchiveUrl
            };

            (bool succeeded, string errorMessage) = await BooksService.UpdateAsync(book.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Book updated successfully."];
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
        IDialogReference dialog = await DialogService.ShowAsync<BookImportDialog>(
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

    async Task ConfirmDeleteBook(BookDto book)
    {
        string displayName = book.Title ?? $"Book #{book.Id}";

        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                string.Format(L["Are you sure you want to delete book '{0}'? This action cannot be undone."], displayName)
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Book"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await BooksService.DeleteAsync(book.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Book deleted successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
