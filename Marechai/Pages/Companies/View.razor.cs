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

using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Marechai.Helpers;
using Marechai.Pages.Suggestions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Companies;

public partial class View
{
    CompanyDto           _company;
    List<MachineDto>     _computers;
    List<MachineDto>     _consoles;
    List<MachineDto>     _smartphones;
    List<MachineDto>     _tablets;
    List<MachineDto>     _pdas;
    string               _description;
    string               _descriptionLanguageServed;
    bool                 _descriptionFellBack;
    bool                 _hasAnyDescription;
    HashSet<string>      _existingDescriptionLangs = new(StringComparer.Ordinal);
    HashSet<string>      _pendingDescriptionLangs  = new(StringComparer.Ordinal);
    List<GpuDto>         _gpus;
    List<SoundSynthDto>  _soundSynths;
    List<ProcessorDto>   _processors;
    List<MachineFamilyDto> _machineFamilies;
    List<BookDto>        _books;
    List<DocumentDto>    _documents;
    List<MagazineDto>    _magazines;
    List<SoftwareDto>    _software;
    List<PersonByCompanyDto> _people;
    int                  _lastId;
    bool                 _loaded;
    List<CompanyLogoDto> _logos;
    int                  _selectedIndex;
    CompanyDto           _soldTo;

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

        try
        {

        _company = await Service.GetAsync(Id);
        List<MachineDto> machines = await Service.GetMachinesAsync(Id);

        _computers       = machines.Where(m => m.Type == (int)MachineType.Computer).ToList();
        _consoles        = machines.Where(m => m.Type == (int)MachineType.Console).ToList();
        _smartphones     = machines.Where(m => m.Type == (int)MachineType.Smartphone).ToList();
        _tablets         = machines.Where(m => m.Type == (int)MachineType.Tablet).ToList();
        _pdas            = machines.Where(m => m.Type == (int)MachineType.Pda).ToList();
        _gpus            = await Service.GetGpusAsync(Id);
        _soundSynths     = await Service.GetSoundSynthsAsync(Id);
        _processors      = await Service.GetProcessorsAsync(Id);
        _machineFamilies = await Service.GetMachineFamiliesAsync(Id);
        _books           = await Service.GetBooksAsync(Id);
        _documents       = await Service.GetDocumentsAsync(Id);
        _magazines       = await Service.GetMagazinesAsync(Id);
        _software        = await Service.GetSoftwareAsync(Id);
        _people          = await Service.GetPeopleAsync(Id);

        await LoadDescriptionStateAsync();
        _soldTo      = await Service.GetSoldToAsync(_company.SoldToId);
        _logos       = await CompanyLogosService.GetByCompany(Id);

        _loaded = true;
        StateHasChanged();
        }
        catch(ObjectDisposedException)
        {
            // Component was disposed during async loading — ignore
        }
    }

    /// <summary>
    ///     Load the description for the current UI language with English fallback, plus the
    ///     metadata (which languages already have a description) used to drive the picker
    ///     status chips. Sets <see cref="_descriptionFellBack" /> when the served language
    ///     differs from the requested UI language.
    /// </summary>
    async Task LoadDescriptionStateAsync()
    {
        string requested = UiLanguage.GetIso639_3();
        CompanyDescriptionDto served = await Service.GetDescriptionAsync(Id, requested);

        _description               = served?.Html ?? served?.Markdown ?? string.Empty;
        _description               = HtmlFragmentFixer.FixFragmentLinks(_description, $"/company/{Id}");
        _descriptionLanguageServed = served?.LanguageCode;
        _hasAnyDescription         = !string.IsNullOrWhiteSpace(_description);
        _descriptionFellBack       = _hasAnyDescription
                                  && _descriptionLanguageServed is not null
                                  && !string.Equals(_descriptionLanguageServed, requested, StringComparison.Ordinal);

        // Lightweight metadata fetch so the picker can show "Has description" / "Empty" chips.
        List<CompanyDescriptionDto> all = await Service.GetDescriptionsAsync(Id);
        _existingDescriptionLangs = new HashSet<string>(StringComparer.Ordinal);
        foreach(CompanyDescriptionDto d in all ?? new List<CompanyDescriptionDto>())
        {
            if(!string.IsNullOrEmpty(d.LanguageCode))
                _existingDescriptionLangs.Add(d.LanguageCode);
        }
    }

    async Task OpenSuggestionDialog()
    {
        if(_company is null) return;

        var parameters = new DialogParameters
        {
            ["EntityType"] = SuggestionEntityType.Company,
            ["EntityId"]   = (long)_company.Id.GetValueOrDefault(),
            ["CurrentDto"] = (object)_company
        };
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Medium
        };

        await DialogService.ShowAsync<SuggestionDialog>(L["Suggest changes"], parameters, options);
    }

    /// <summary>
    ///     Open the language picker. On selection, opens the markdown editor pre-loaded with
    ///     the existing description for that language (or empty for a new translation).
    ///     Refreshes the description card on success so the user immediately sees their pending
    ///     suggestion's status (the description itself only updates when the admin accepts).
    /// </summary>
    async Task OpenDescriptionPickerAsync()
    {
        if(_company is null || !_company.Id.HasValue) return;

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
            L["Choose language for description"], pickerParams, pickerOptions);
        var pickerResult = await pickerRef.Result;

        if(pickerResult.Canceled || pickerResult.Data is not string langCode) return;

        // Pre-fetch the existing markdown for the chosen language (may be null/empty).
        CompanyDescriptionDto existing = await Service.GetDescriptionAsync(_company.Id.Value, langCode);
        bool isEdit       = existing is not null && string.Equals(existing.LanguageCode, langCode, StringComparison.Ordinal);
        string initialMd  = isEdit ? (existing?.Markdown ?? string.Empty) : string.Empty;

        var editorParams = new DialogParameters
        {
            ["EntityType"]          = SuggestionEntityType.CompanyDescription,
            ["EntityId"]            = (long)_company.Id.Value,
            ["Subkey"]              = langCode,
            ["EntityDisplayName"]   = _company.Name ?? $"#{_company.Id.Value}",
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
            L["Suggest description"], editorParams, editorOptions);
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
        var auth = await AuthState.GetAuthenticationStateAsync();
        if(!(auth.User?.Identity?.IsAuthenticated ?? false))
        {
            _pendingDescriptionLangs.Clear();
            return;
        }

        List<SuggestionDto> mine = await Suggestions.GetMyAsync();
        var fresh = new HashSet<string>(StringComparer.Ordinal);

        foreach(SuggestionDto s in mine ?? new List<SuggestionDto>())
        {
            if(s.EntityType == (int?)SuggestionEntityType.CompanyDescription
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