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
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class CompanyMergeDialog
{
    CompanyMergePreviewDto _preview;
    CompanyDto             _selectedTarget;
    CompanyDto             _targetFull;
    CompanyDto             _sourceFull;
    string                 _targetSoldToName;
    string                 _sourceSoldToName;
    string                 _errorMessage;
    bool                   _isLoadingPreview;
    bool                   _isMerging;
    CompanyDto             _previousTarget;

    // Per-field "use source" toggles. Default false means keep target.
    bool _fieldUseSource_Name;
    bool _fieldUseSource_LegalName;
    bool _fieldUseSource_Status;
    bool _fieldUseSource_Founded;
    bool _fieldUseSource_Sold;
    bool _fieldUseSource_SoldTo;
    bool _fieldUseSource_Country;
    bool _fieldUseSource_Address;
    bool _fieldUseSource_City;
    bool _fieldUseSource_Province;
    bool _fieldUseSource_PostalCode;
    bool _fieldUseSource_Website;
    bool _fieldUseSource_Twitter;
    bool _fieldUseSource_Facebook;

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public int    SourceId   { get; set; }
    [Parameter] public string SourceName { get; set; } = string.Empty;

    [Inject] CompaniesService CompaniesService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_selectedTarget is not null && _selectedTarget != _previousTarget && _selectedTarget.Id != SourceId)
        {
            _previousTarget = _selectedTarget;
            await LoadPreview();
            StateHasChanged();
        }
        else if(_selectedTarget is null && _previousTarget is not null)
        {
            _previousTarget    = null;
            _preview           = null;
            _targetFull        = null;
            _sourceFull        = null;
            _targetSoldToName  = null;
            _sourceSoldToName  = null;
            _errorMessage      = null;
            StateHasChanged();
        }
    }

    async Task<IEnumerable<CompanyDto>> SearchCompanies(string value, CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(value)) return [];

        List<CompanyDto> results = await CompaniesService.GetPagedAsync(0, 20, value);

        return results?.Where(c => c.Id != SourceId) ?? [];
    }

    async Task LoadPreview()
    {
        _isLoadingPreview = true;
        _errorMessage     = null;
        _preview          = null;
        _targetFull       = null;
        _sourceFull       = null;
        _targetSoldToName = null;
        _sourceSoldToName = null;

        int targetId = (int)(_selectedTarget.Id ?? 0);

        Task<CompanyMergePreviewDto> previewTask = CompaniesService.GetMergePreviewAsync(targetId, SourceId);
        Task<CompanyDto>             targetTask  = CompaniesService.GetAsync(targetId);
        Task<CompanyDto>             sourceTask  = CompaniesService.GetAsync(SourceId);

        await Task.WhenAll(previewTask, targetTask, sourceTask);

        _preview    = previewTask.Result;
        _targetFull = targetTask.Result;
        _sourceFull = sourceTask.Result;

        if(_preview is null || _targetFull is null || _sourceFull is null)
        {
            _errorMessage     = L["Failed to load merge preview. Please try again."];
            _isLoadingPreview = false;

            return;
        }

        // Resolve sold-to names (best-effort) for human-friendly display.
        Task<CompanyDto> targetSoldToTask = _targetFull.SoldToId.HasValue
                                                ? CompaniesService.GetSoldToAsync(_targetFull.SoldToId)
                                                : Task.FromResult<CompanyDto>(null);

        Task<CompanyDto> sourceSoldToTask = _sourceFull.SoldToId.HasValue
                                                ? CompaniesService.GetSoldToAsync(_sourceFull.SoldToId)
                                                : Task.FromResult<CompanyDto>(null);

        await Task.WhenAll(targetSoldToTask, sourceSoldToTask);

        _targetSoldToName = targetSoldToTask.Result?.Name;
        _sourceSoldToName = sourceSoldToTask.Result?.Name;

        // Initialise per-field defaults: keep target unless target is empty/null and source has a value.
        _fieldUseSource_Name       = ShouldPreferSource(_targetFull.Name, _sourceFull.Name);
        _fieldUseSource_LegalName  = ShouldPreferSource(_targetFull.LegalName, _sourceFull.LegalName);
        _fieldUseSource_Status     = _targetFull.Status is null && _sourceFull.Status is not null;
        _fieldUseSource_Founded    = _targetFull.Founded is null && _sourceFull.Founded is not null;
        _fieldUseSource_Sold       = _targetFull.Sold is null && _sourceFull.Sold is not null;
        _fieldUseSource_SoldTo     = _targetFull.SoldToId is null && _sourceFull.SoldToId is not null;
        _fieldUseSource_Country    = _targetFull.CountryId is null && _sourceFull.CountryId is not null;
        _fieldUseSource_Address    = ShouldPreferSource(_targetFull.Address, _sourceFull.Address);
        _fieldUseSource_City       = ShouldPreferSource(_targetFull.City, _sourceFull.City);
        _fieldUseSource_Province   = ShouldPreferSource(_targetFull.Province, _sourceFull.Province);
        _fieldUseSource_PostalCode = ShouldPreferSource(_targetFull.PostalCode, _sourceFull.PostalCode);
        _fieldUseSource_Website    = ShouldPreferSource(_targetFull.Website, _sourceFull.Website);
        _fieldUseSource_Twitter    = ShouldPreferSource(_targetFull.Twitter, _sourceFull.Twitter);
        _fieldUseSource_Facebook   = ShouldPreferSource(_targetFull.Facebook, _sourceFull.Facebook);

        _isLoadingPreview = false;
    }

    static bool ShouldPreferSource(string target, string source) =>
        string.IsNullOrWhiteSpace(target) && !string.IsNullOrWhiteSpace(source);

    bool HasAnyScalarDiff() =>
        !StringsEqual(_targetFull.Name, _sourceFull.Name)               ||
        !StringsEqual(_targetFull.LegalName, _sourceFull.LegalName)     ||
        _targetFull.Status           != _sourceFull.Status              ||
        _targetFull.Founded          != _sourceFull.Founded             ||
        _targetFull.FoundedPrecision != _sourceFull.FoundedPrecision    ||
        _targetFull.Sold             != _sourceFull.Sold                ||
        _targetFull.SoldPrecision    != _sourceFull.SoldPrecision       ||
        _targetFull.SoldToId         != _sourceFull.SoldToId            ||
        _targetFull.CountryId        != _sourceFull.CountryId           ||
        !StringsEqual(_targetFull.Address, _sourceFull.Address)         ||
        !StringsEqual(_targetFull.City, _sourceFull.City)               ||
        !StringsEqual(_targetFull.Province, _sourceFull.Province)       ||
        !StringsEqual(_targetFull.PostalCode, _sourceFull.PostalCode)   ||
        !StringsEqual(_targetFull.Website, _sourceFull.Website)         ||
        !StringsEqual(_targetFull.Twitter, _sourceFull.Twitter)         ||
        !StringsEqual(_targetFull.Facebook, _sourceFull.Facebook);

    static bool StringsEqual(string a, string b) =>
        (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) ||
        string.Equals(a, b, StringComparison.Ordinal);

    // RenderFragment helper for one diff row. Renders nothing when target == source.
    RenderFragment RenderScalarDiff(string label, string targetValue, string sourceValue,
                                    bool currentUseSource, Action<bool> setUseSource,
                                    bool? forceShow = null) =>
        builder =>
        {
            bool show = forceShow ?? !StringsEqual(targetValue, sourceValue);

            if(!show) return;

            int seq = 0;

            builder.OpenElement(seq++, "tr");

            builder.OpenElement(seq++, "td");
            builder.AddContent(seq++, label);
            builder.CloseElement();

            // Target cell with radio
            builder.OpenElement(seq++, "td");
            builder.OpenComponent<MudRadio<bool>>(seq++);
            builder.AddComponentParameter(seq++, "Value", false);
            builder.AddComponentParameter(seq++, "T", typeof(bool));
            builder.AddComponentParameter(seq++, "Color", Color.Primary);
            builder.AddComponentParameter(seq++, "Dense", true);
            builder.AddComponentParameter(seq++, "Checked", !currentUseSource);
            builder.AddComponentParameter(seq++, "CheckedChanged",
                EventCallback.Factory.Create<bool>(this, isChecked =>
                {
                    if(isChecked) setUseSource(false);
                }));
            builder.AddAttribute(seq++, "ChildContent",
                (RenderFragment)(rb =>
                {
                    rb.AddContent(0, FormatValue(targetValue));
                }));
            builder.CloseComponent();
            builder.CloseElement();

            // Source cell with radio
            builder.OpenElement(seq++, "td");
            builder.OpenComponent<MudRadio<bool>>(seq++);
            builder.AddComponentParameter(seq++, "Value", true);
            builder.AddComponentParameter(seq++, "T", typeof(bool));
            builder.AddComponentParameter(seq++, "Color", Color.Primary);
            builder.AddComponentParameter(seq++, "Dense", true);
            builder.AddComponentParameter(seq++, "Checked", currentUseSource);
            builder.AddComponentParameter(seq++, "CheckedChanged",
                EventCallback.Factory.Create<bool>(this, isChecked =>
                {
                    if(isChecked) setUseSource(true);
                }));
            builder.AddAttribute(seq++, "ChildContent",
                (RenderFragment)(rb =>
                {
                    rb.AddContent(0, FormatValue(sourceValue));
                }));
            builder.CloseComponent();
            builder.CloseElement();

            builder.CloseElement();
        };

    string FormatValue(string v) => string.IsNullOrWhiteSpace(v) ? L["(none)"] : v;

    string GetStatusText(int? status) => status switch
                                         {
                                             0 => L["Unknown"],
                                             1 => L["Active"],
                                             2 => L["Sold"],
                                             3 => L["Merged"],
                                             4 => L["Bankrupt"],
                                             5 => L["Defunct"],
                                             6 => L["Renamed"],
                                             _ => L["Unknown"]
                                         };

    static string FormatDate(DateTimeOffset? date, int? precision)
    {
        if(date is null) return "";
        if((precision ?? 0) == 2) return date.Value.Year.ToString();
        if((precision ?? 0) == 1) return date.Value.ToString("MMMM yyyy");

        return date.Value.Date.ToShortDateString();
    }

    async Task ConfirmMerge()
    {
        if(_preview is null || _selectedTarget is null || _targetFull is null || _sourceFull is null) return;

        _isMerging    = true;
        _errorMessage = null;

        var request = new CompanyMergeRequestDto
        {
            Name             = _fieldUseSource_Name       ? _sourceFull.Name       : _targetFull.Name,
            LegalName        = _fieldUseSource_LegalName  ? _sourceFull.LegalName  : _targetFull.LegalName,
            Status           = _fieldUseSource_Status     ? _sourceFull.Status     : _targetFull.Status,
            Founded          = _fieldUseSource_Founded    ? _sourceFull.Founded    : _targetFull.Founded,
            FoundedPrecision = _fieldUseSource_Founded    ? _sourceFull.FoundedPrecision : _targetFull.FoundedPrecision,
            Sold             = _fieldUseSource_Sold       ? _sourceFull.Sold       : _targetFull.Sold,
            SoldPrecision    = _fieldUseSource_Sold       ? _sourceFull.SoldPrecision    : _targetFull.SoldPrecision,
            SoldToId         = _fieldUseSource_SoldTo     ? _sourceFull.SoldToId   : _targetFull.SoldToId,
            CountryId        = _fieldUseSource_Country    ? _sourceFull.CountryId  : _targetFull.CountryId,
            Address          = _fieldUseSource_Address    ? _sourceFull.Address    : _targetFull.Address,
            City             = _fieldUseSource_City       ? _sourceFull.City       : _targetFull.City,
            Province         = _fieldUseSource_Province   ? _sourceFull.Province   : _targetFull.Province,
            PostalCode       = _fieldUseSource_PostalCode ? _sourceFull.PostalCode : _targetFull.PostalCode,
            Website          = _fieldUseSource_Website    ? _sourceFull.Website    : _targetFull.Website,
            Twitter          = _fieldUseSource_Twitter    ? _sourceFull.Twitter    : _targetFull.Twitter,
            Facebook         = _fieldUseSource_Facebook   ? _sourceFull.Facebook   : _targetFull.Facebook
        };

        (bool succeeded, string error) =
            await CompaniesService.MergeCompaniesAsync((int)(_selectedTarget.Id ?? 0), SourceId, request);

        _isMerging = false;

        if(succeeded)
            MudDialog.Close(DialogResult.Ok(true));
        else
            _errorMessage = string.Format(L["Merge failed: {0}"], error);
    }

    void Cancel() => MudDialog.Cancel();
}
