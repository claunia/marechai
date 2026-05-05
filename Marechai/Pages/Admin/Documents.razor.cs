using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class Documents
{
    string              _errorMessage;
    bool                 _isLoading = true;
    string              _successMessage;
    List<DocumentDto>   _documents;

    protected override async Task OnInitializedAsync() => await LoadDocumentsAsync();

    async Task LoadDocumentsAsync()
    {
        _isLoading = true;
        _documents = await DocumentsService.GetDocumentsAsync();
        _isLoading = false;
    }

    Func<DocumentDto, bool> QuickFilter => document =>
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
                Published   = data.Published.HasValue ? new DateTimeOffset(data.Published.Value, TimeSpan.Zero) : null
            };

            (long? id, string errorMessage) = await DocumentsService.CreateAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Document created successfully."];
                await LoadDocumentsAsync();
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
                Published   = data.Published.HasValue ? new DateTimeOffset(data.Published.Value, TimeSpan.Zero) : null
            };

            (bool succeeded, string errorMessage) = await DocumentsService.UpdateAsync(document.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Document updated successfully."];
                await LoadDocumentsAsync();
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
            await LoadDocumentsAsync();
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
                await LoadDocumentsAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
