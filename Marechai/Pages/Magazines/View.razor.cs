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
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Marechai.Helpers;
using Marechai.Pages.Suggestions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Marechai.Pages.Magazines;

public partial class View
{
    List<CompanyByMagazineDto>       _companies;
    List<int?>                       _issueYears;
    long                             _lastId;
    bool                             _loaded;
    MagazineDto                      _magazine;
    DocumentSynopsisDto              _synopsis;
    string                           _descriptionLanguageServed;
    bool                             _descriptionFellBack;
    bool                             _hasAnyDescription;
    HashSet<string>                  _existingDescriptionLangs = new(StringComparer.Ordinal);
    HashSet<string>                  _pendingDescriptionLangs  = new(StringComparer.Ordinal);

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

        _magazine = await Service.GetMagazineAsync(Id);

        if(_magazine is null)
        {
            _loaded = true;
            StateHasChanged();

            return;
        }

        // Load the per-language synopsis state with English fallback detection. Replaces the
        // previous single GetMagazineSynopsisAsync call which couldn't tell whether the served
        // language matched the requested UI language.
        await LoadDescriptionStateAsync();

        // Fan-out the remaining child collections in parallel — they are independent server-side
        // (each is a separate /magazines/{id}/<x> endpoint) so a single Task.WhenAll
        // collapses two round-trips into one wall-clock RTT.
        Task<List<CompanyByMagazineDto>> companiesTask  = Service.GetCompaniesByMagazineAsync(Id);
        Task<List<int?>>                 issueYearsTask = Service.GetIssueYearsAsync(Id);

        await Task.WhenAll(companiesTask, issueYearsTask);

        _companies  = companiesTask.Result;
        _issueYears = issueYearsTask.Result;

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
        if(_magazine is null) return;

        // Lesson 1: capture the route parameter Id, NOT _magazine.Id.Value. Async loads can leave
        // _magazine.Id null/0 in race scenarios and the cast silently produces 0 → server treats
        // it as an addition request and rejects with 400.
        long magazineId = Id;

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
        DocumentSynopsisDto existing = await Service.GetSynopsisAsync(magazineId, langCode);
        bool   isEdit    = existing is not null && string.Equals(existing.LanguageCode, langCode, StringComparison.Ordinal);
        string initialMd = isEdit ? (existing?.Text ?? string.Empty) : string.Empty;

        var editorParams = new DialogParameters
        {
            ["EntityType"]          = SuggestionEntityType.MagazineSynopsis,
            ["EntityId"]            = magazineId,
            ["Subkey"]              = langCode,
            ["EntityDisplayName"]   = _magazine.Title ?? $"#{magazineId}",
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
            if(s.EntityType == (int?)SuggestionEntityType.MagazineSynopsis
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

    /// <summary>
    ///     Open the dedicated Magazine entity-edit suggestion dialog. Lets any authenticated
    ///     user propose changes to scalar fields (title, native title, sort title, ISSN, first
    ///     publication, published, country) AND atomic add/remove on the Companies junction.
    ///     Each change is queued as an independent operation (scalar field set, junction add,
    ///     junction remove) and serialised under a unique field-name key so each operation is
    ///     accepted or rejected independently by an admin.
    /// </summary>
    async Task OpenMagazineSuggestionDialogAsync()
    {
        if(_magazine is null) return;

        // Lesson 1 (canonical): capture the route parameter Id, NOT _magazine.Id.Value. Async
        // loads can leave _magazine.Id null/0 in race scenarios and the cast silently produces
        // 0 → server treats it as an addition request and rejects with 400.
        long magazineId = Id;

        var dlgParams = new DialogParameters
        {
            ["EntityId"]   = magazineId,
            ["CurrentDto"] = _magazine
        };
        var dlgOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Large
        };

        var dlgRef = await DialogService.ShowAsync<MagazineSuggestionDialog>(
            L["Suggest changes"], dlgParams, dlgOptions);

        // Match Book/Document/Machine View behaviour: don't refetch on success — the snackbar
        // inside the dialog provides feedback, and the suggestion is invisible to the public
        // page until an admin accepts it.
        await dlgRef.Result;
    }

    /// <summary>
    ///     Open the dedicated MagazineIssue suggestion dialog in addition mode (entityId=null,
    ///     pre-filled with this magazine's id as the parent). Lets any authenticated user
    ///     propose a brand-new issue for this magazine. The dialog handles cover upload,
    ///     scalar fields, and people/machines/machine-families/software junctions; on accept
    ///     by an admin a new MagazineIssue row materialises with the parent magazine
    ///     pre-set. Magazine selection is locked to the current magazine — re-parenting an
    ///     issue is admin-only.
    /// </summary>
    async Task OpenSuggestNewIssueDialog()
    {
        if(_magazine is null) return;

        // Capture the route parameter Id (NOT _magazine.Id.Value) for the same reason as
        // the edit-mode handler above: async loads can leave _magazine.Id null/0 in race
        // scenarios.
        long magazineId = Id;

        var parameters = new DialogParameters<MagazineIssueSuggestionDialog>
        {
            { x => x.EntityId,                  0L                       },
            { x => x.CurrentDto,                (MagazineIssueDto)null   },
            { x => x.IsCreation,                true                     },
            { x => x.PrefillMagazineId,         (long?)magazineId        },
            { x => x.PrefillMagazineLabel,      _magazine.Title          },
            { x => x.PrefillPublished,          (DateTime?)null          },
            { x => x.PrefillPublishedPrecision, (DatePrecision?)null     }
        };

        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Large
        };

        await DialogService.ShowAsync<MagazineIssueSuggestionDialog>(
            L["Suggest new magazine issue"], parameters, options);
    }
}
