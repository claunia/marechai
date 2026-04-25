using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Books
{
    string?         _errorMessage;
    bool            _isLoading = true;
    string?         _successMessage;
    List<BookDto>?  _books;

    protected override async Task OnInitializedAsync() => await LoadBooksAsync();

    async Task LoadBooksAsync()
    {
        _isLoading = true;
        _books     = await BooksService.GetBooksAsync();
        _isLoading = false;
    }

    Func<BookDto, bool> QuickFilter => book =>
    {
        return true;
    };

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

        DialogResult? result = await dialog.Result;

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
                Published   = data.Published.HasValue ? new DateTimeOffset(data.Published.Value) : null,
                PreviousId  = data.PreviousId,
                SourceId    = data.SourceId
            };

            (long? id, string? errorMessage) = await BooksService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Book created successfully."];
                await LoadBooksAsync();
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
        BookDto? fullBook = book.Id.HasValue ? await BooksService.GetBookAsync(book.Id.Value) : book;
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
            { x => x.Published, fullBook.Published?.DateTime },
            { x => x.PublishedPrecision, fullBook.PublishedPrecision ?? 0 },
            { x => x.PreviousId, fullBook.PreviousId },
            { x => x.SourceId, fullBook.SourceId },
            { x => x.HasCover, fullBook.CoverGuid is not null }
        };

        IDialogReference dialog = await DialogService.ShowAsync<BookDialog>(L["Edit Book"], parameters,
                                                                            new DialogOptions
                                                                            {
                                                                                MaxWidth  = MaxWidth.Large,
                                                                                FullWidth = true
                                                                            });

        DialogResult? result = await dialog.Result;

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
                Published   = data.Published.HasValue ? new DateTimeOffset(data.Published.Value) : null,
                PreviousId  = data.PreviousId,
                SourceId    = data.SourceId
            };

            (bool succeeded, string? errorMessage) = await BooksService.UpdateAsync(book.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Book updated successfully."];
                await LoadBooksAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
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

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await BooksService.DeleteAsync(book.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Book deleted successfully."];
                await LoadBooksAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
