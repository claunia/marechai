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
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Marechai.Pages.People;

public partial class View
{
    List<PersonByBookDto>     _books;
    List<PersonByCompanyDto>  _companies;
    string                    _description;
    string                    _descriptionLanguageServed;
    bool                      _descriptionFellBack;
    bool                      _hasAnyDescription;
    HashSet<string>           _existingDescriptionLangs = new(StringComparer.Ordinal);
    HashSet<string>           _pendingDescriptionLangs  = new(StringComparer.Ordinal);
    List<PersonByDocumentDto> _documents;
    int                       _lastId;
    bool                      _loaded;
    List<PersonByMagazineDto> _magazines;
    PersonDto                 _person;
    List<PersonBySoftwareDto> _softwareCredits;

    [CascadingParameter]
    Task<AuthenticationState> AuthState { get; set; }

    [Parameter]
    public int Id { get; set; }

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

        // Single consolidated /people/{id}/full call replaces six sequential
        // round-trips. The /full endpoint also resolves the language-aware
        // biography server-side (with English fallback collapsed into one
        // ordered query), mirroring the GPU/Processor/SoundSynth pattern.
        try
        {
            PersonFullDto full = await Service.GetPersonFullAsync(Id, UiLanguage.GetIso639_3());

            if(full?.Person is null)
            {
                _loaded = true;
                StateHasChanged();

                return;
            }

            _person          = full.Person;
            _companies       = full.Companies       ?? [];
            _books           = full.Books           ?? [];
            _documents       = full.Documents       ?? [];
            _magazines       = full.Magazines       ?? [];
            _softwareCredits = full.SoftwareCredits ?? [];
            _description     = full.DescriptionHtml ?? full.DescriptionText;

            _descriptionLanguageServed = full.DescriptionLanguageCode;

            string requested = UiLanguage.GetIso639_3();
            _hasAnyDescription   = !string.IsNullOrWhiteSpace(_description);
            _descriptionFellBack = _hasAnyDescription
                                && _descriptionLanguageServed is not null
                                && !string.Equals(_descriptionLanguageServed, requested, StringComparison.Ordinal);

            // Lightweight metadata fetch so the picker can show "Has biography" / "Empty" chips.
            List<PersonDescriptionDto> all = await Service.GetDescriptionsAsync(Id);
            _existingDescriptionLangs = new HashSet<string>(StringComparer.Ordinal);
            foreach(PersonDescriptionDto d in all ?? new List<PersonDescriptionDto>())
            {
                if(!string.IsNullOrEmpty(d.LanguageCode))
                    _existingDescriptionLangs.Add(d.LanguageCode);
            }
        }
        catch(ObjectDisposedException)
        {
            // Component was disposed mid-load (navigation away). Nothing to do.
            return;
        }

        _loaded = true;
        StateHasChanged();
    }

    /// <summary>
    ///     Open the language picker. On selection, opens the markdown editor pre-loaded with
    ///     the existing biography for that language (or empty for a new translation). Refreshes
    ///     the biography card on success so the user immediately sees their pending suggestion's
    ///     status (the biography itself only updates when the admin accepts).
    /// </summary>
    async Task OpenDescriptionPickerAsync()
    {
        if(_person is null) return;

        // Use the route parameter Id (always valid — the page wouldn't have rendered otherwise)
        // rather than _person.Id.Value. This sidesteps the OwningComponentBase trap where the
        // inner-scope DTO load could in theory leave Id unpopulated.
        long personId = Id;

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
            L["Choose language for biography"], pickerParams, pickerOptions);
        var pickerResult = await pickerRef.Result;

        if(pickerResult.Canceled || pickerResult.Data is not string langCode) return;

        // Pre-fetch the existing markdown for the chosen language (may be null/empty).
        PersonDescriptionDto existing = await Service.GetDescriptionAsync(Id, langCode);
        bool   isEdit    = existing is not null && string.Equals(existing.LanguageCode, langCode, StringComparison.Ordinal);
        string initialMd = isEdit ? (existing?.Markdown ?? string.Empty) : string.Empty;

        string displayName = _person.DisplayName ?? _person.Alias ?? $"{_person.Name} {_person.Surname}".Trim();
        if(string.IsNullOrWhiteSpace(displayName)) displayName = $"#{personId}";

        var editorParams = new DialogParameters
        {
            ["EntityType"]          = SuggestionEntityType.PersonDescription,
            ["EntityId"]            = personId,
            ["Subkey"]              = langCode,
            ["EntityDisplayName"]   = displayName,
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
            L["Suggest biography"], editorParams, editorOptions);
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
            if(s.EntityType == (int?)SuggestionEntityType.PersonDescription
            && s.EntityId == (long?)Id
            && s.Status == (int?)SuggestionStatus.Pending
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
}
