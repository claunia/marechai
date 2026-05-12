/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Marechai.Helpers;
using Marechai.Pages.Suggestions;
using Marechai.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Marechai.Pages.Books;

public partial class View
{
    BookDto                      _book;
    List<CompanyByBookDto>       _companies;
    long                         _lastId;
    bool                         _isCollected;
    PhotoLightbox                _lightbox;
    bool                         _loaded;
    List<BookByMachineFamilyDto> _machineFamilies;
    List<BookByMachineDto>       _machines;
    List<PersonByBookDto>        _people;
    BookDto                      _previousBook;
    BookDto                      _sourceBook;
    DocumentSynopsisDto          _synopsis;
    string                       _descriptionLanguageServed;
    bool                         _descriptionFellBack;
    bool                         _hasAnyDescription;
    HashSet<string>              _existingDescriptionLangs = new(StringComparer.Ordinal);
    HashSet<string>              _pendingDescriptionLangs  = new(StringComparer.Ordinal);
    bool                         _togglingCollection;

    [CascadingParameter]
    Task<AuthenticationState> AuthState { get; set; }

    [Parameter]
    public long Id { get; set; }

    protected override void OnParametersSet()
    {
        if(Id == _lastId) return;

        _lastId = Id;
        _loaded = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        if(Id <= 0)
        {
            _loaded = true;

            return;
        }

        // Single consolidated fetch: head + previous/source titles + synopsis +
        // all four child collections (people, companies, machines, machine
        // families) in one HTTP round-trip. Replaces 6 sequential service
        // calls plus the two conditional Previous/Source book fetches that
        // used to dominate the page-load critical path.
        BookFullDto full = await Service.GetBookFullAsync(Id, UiLanguage.GetIso639_3());

        if(full?.Book is null)
        {
            _loaded = true;
            StateHasChanged();

            return;
        }

        _book            = full.Book;
        _people          = full.People          ?? [];
        _companies       = full.Companies       ?? [];
        _machines        = full.Machines        ?? [];
        _machineFamilies = full.MachineFamilies ?? [];

        // Load the per-language synopsis state separately so we can detect language
        // fallback (the BookFullDto only carries the rendered synopsis text). The
        // existing-languages HashSet feeds the language picker chips.
        await LoadDescriptionStateAsync();

        // Materialize lightweight stand-ins for the previous/source book chips.
        // The markup only renders the link target (Id) and the button label
        // (Title), so projecting just those two fields server-side is enough.
        if(_book.PreviousId.HasValue && full.PreviousBookTitle is not null)
            _previousBook = new BookDto { Id = _book.PreviousId.Value, Title = full.PreviousBookTitle };

        if(_book.SourceId.HasValue && full.SourceBookTitle is not null)
            _sourceBook = new BookDto { Id = _book.SourceId.Value, Title = full.SourceBookTitle };

        AuthenticationState authState = await AuthState;

        if(authState.User.Identity?.IsAuthenticated == true)
            _isCollected = await CollectionSvc.IsBookCollectedAsync(Id);

        _loaded = true;
        StateHasChanged();
    }

    /// <summary>
    ///     Load the synopsis for the current UI language with English fallback, plus the
    ///     metadata (which languages already have a synopsis) used to drive the picker
    ///     status chips. Sets <see cref="_descriptionFellBack" /> when the served language
    ///     differs from the requested UI language.
    /// </summary>
    async Task LoadDescriptionStateAsync()
    {
        string requested = UiLanguage.GetIso639_3();
        DocumentSynopsisDto served = await Service.GetSynopsisAsync(Id, requested);

        _synopsis                  = served;
        _descriptionLanguageServed = served?.LanguageCode;
        _hasAnyDescription         = served is not null && !string.IsNullOrWhiteSpace(served.Text);
        _descriptionFellBack       = _hasAnyDescription
                                  && _descriptionLanguageServed is not null
                                  && !string.Equals(_descriptionLanguageServed, requested, StringComparison.Ordinal);

        // Lightweight metadata fetch so the picker can show "Has synopsis" / "Empty" chips.
        List<DocumentSynopsisDto> all = await Service.GetSynopsesAsync(Id);
        _existingDescriptionLangs = new HashSet<string>(StringComparer.Ordinal);
        foreach(DocumentSynopsisDto d in all ?? new List<DocumentSynopsisDto>())
        {
            if(!string.IsNullOrEmpty(d.LanguageCode))
                _existingDescriptionLangs.Add(d.LanguageCode);
        }

        // If the picker had no served synopsis but other languages exist, the page should
        // still go down the "fallback / pencil" branch rather than the "no synopsis" branch.
        if(!_hasAnyDescription && _existingDescriptionLangs.Count > 0)
            _hasAnyDescription = true;
    }

    /// <summary>
    ///     Open the language picker. On selection, opens the markdown editor pre-loaded with
    ///     the existing synopsis for that language (or empty for a new translation). Refreshes
    ///     the synopsis card on success so the user immediately sees their pending suggestion's
    ///     status (the synopsis itself only updates when the admin accepts).
    /// </summary>
    async Task OpenDescriptionPickerAsync()
    {
        if(_book is null) return;

        // Lesson 1: capture the route parameter Id, NOT _book.Id.Value. Async loads can leave
        // _book.Id null/0 in race scenarios and the cast silently produces 0 → server treats it
        // as an addition request and rejects with 400.
        long bookId = Id;

        // Refresh per-user pending list lazily so anonymous users never hit /auth/me/suggestions.
        await RefreshPendingDescriptionLangsAsync();

        var pickerParams = new DialogParameters
        {
            ["ExistingLanguages"] = _existingDescriptionLangs,
            ["PendingLanguages"]  = _pendingDescriptionLangs,
            ["DefaultLanguage"]   = UiLanguage.GetIso639_3()
        };
        var pickerOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Small
        };

        var pickerRef = await DialogService.ShowAsync<LanguagePickerDialog>(
            L["Choose language for synopsis"], pickerParams, pickerOptions);
        var pickerResult = await pickerRef.Result;

        if(pickerResult.Canceled || pickerResult.Data is not string langCode) return;

        // Pre-fetch the existing markdown for the chosen language (may be null/empty).
        DocumentSynopsisDto existing = await Service.GetSynopsisAsync(bookId, langCode);
        bool   isEdit    = existing is not null && string.Equals(existing.LanguageCode, langCode, StringComparison.Ordinal);
        string initialMd = isEdit ? (existing?.Text ?? string.Empty) : string.Empty;

        var editorParams = new DialogParameters
        {
            ["EntityType"]          = SuggestionEntityType.BookSynopsis,
            ["EntityId"]            = bookId,
            ["Subkey"]              = langCode,
            ["EntityDisplayName"]   = _book.Title ?? $"#{bookId}",
            ["LanguageDisplayName"] = LanguageDisplayName(langCode),
            ["InitialMarkdown"]     = initialMd,
            ["IsEdit"]              = isEdit
        };
        var editorOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Large
        };

        var editorRef = await DialogService.ShowAsync<MarkdownSuggestionDialog>(
            L["Suggest synopsis"], editorParams, editorOptions);
        var editorResult = await editorRef.Result;

        if(!editorResult.Canceled && editorResult.Data is not null)
        {
            // Add the just-submitted language to the pending set so a follow-up click on the
            // same language disables the button without a round-trip.
            _pendingDescriptionLangs.Add(langCode);
            StateHasChanged();
        }
    }

    async Task RefreshPendingDescriptionLangsAsync()
    {
        AuthenticationState auth = await AuthState;
        if(!(auth.User?.Identity?.IsAuthenticated ?? false))
        {
            _pendingDescriptionLangs.Clear();
            return;
        }

        List<SuggestionDto> mine = await Suggestions.GetMyAsync();
        var fresh = new HashSet<string>(StringComparer.Ordinal);

        foreach(SuggestionDto s in mine ?? new List<SuggestionDto>())
        {
            if(s.EntityType == (int?)SuggestionEntityType.BookSynopsis
            && s.EntityId   == Id
            && s.Status     == (int?)SuggestionStatus.Pending
            && !string.IsNullOrEmpty(s.Subkey))
                fresh.Add(s.Subkey);
        }

        _pendingDescriptionLangs = fresh;
    }

    static string LanguageDisplayName(string iso639_3) => iso639_3 switch
    {
        "eng" => "English",
        "spa" => "Spanish",
        "deu" => "German",
        "fra" => "French",
        "ita" => "Italian",
        "lat" => "Latin",
        "por" => "Portuguese",
        _     => iso639_3
    };

    async Task ToggleCollectionAsync()
    {
        _togglingCollection = true;

        if(_isCollected)
        {
            (bool success, _) = await CollectionSvc.RemoveBookFromCollectionAsync(Id);

            if(success) _isCollected = false;
        }
        else
        {
            (bool success, _) = await CollectionSvc.AddBookToCollectionAsync(Id);

            if(success) _isCollected = true;
        }

        _togglingCollection = false;
    }
}
