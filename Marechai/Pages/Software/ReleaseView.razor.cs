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
using Humanizer;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Marechai.Pages.Suggestions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Marechai.Pages.Software;

public partial class ReleaseView
{
    List<(int CompanyId, string CompanyName, string Role)> _aggregatedCompanies = [];
    List<SoftwareAttributeDto>                             _releaseAttributes = [];
    List<SoftwareAttributeDto>                             _releaseSpecs = [];
    List<SoftwareAttributeDto>                             _releaseRatings = [];
    List<SoftwareBarcodeDto>                               _barcodes = [];
    int                                                    _lastId;
    List<SoftwareVersionBySoftwareReleaseDto>               _includedVersions = [];
    List<SoftwareBySoftwareReleaseDto>                     _includedSoftware = [];
    bool                                                   _isCollected;
    bool                                                   _loaded;
    List<GpuBySoftwareReleaseDto>                          _minimumGpus = [];
    List<SoftwareProductCodeDto>                           _productCodes = [];
    List<GpuBySoftwareReleaseDto>                          _recommendedGpus = [];
    SoftwareReleaseDto                                     _release;
    string                                                 _softwareName;
    List<SoundSynthBySoftwareReleaseDto>                   _soundSynths = [];
    List<SoftwareCoverDto>                                 _covers = [];
    Dictionary<string, List<SoftwareCoverDto>>              _coversByType = new();
    SoftwareCoverDto                                      _fullscreenCover;
    SoftwareCoverDto                                      _heroCover;
    bool                                                   _togglingCollection;

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

        _release = await Service.GetReleaseByIdAsync(Id);

        if(_release is null)
        {
            _loaded = true;
            StateHasChanged();

            return;
        }

        // Get software name from version (versioned single releases) or software (versionless single releases)
        if(_release.SoftwareVersionId.HasValue)
        {
            SoftwareVersionDto version = await Service.GetVersionByIdAsync(_release.SoftwareVersionId.Value);
            _softwareName = version?.Software;
        }
        else if(_release.IsCompilation == true)
        {
            // Compilation: load included versions and/or software
            _includedVersions = await Service.GetIncludedVersionsAsync(Id);
            _includedSoftware = await Service.GetIncludedSoftwareAsync(Id);
        }
        else
        {
            // Versionless single release: use software name from DTO
            _softwareName = _release.Software;
        }

        _barcodes        = await Service.GetBarcodesAsync(Id);
        _productCodes    = await Service.GetProductCodesAsync(Id);
        _minimumGpus     = await Service.GetMinimumGpusAsync(Id);
        _recommendedGpus = await Service.GetRecommendedGpusAsync(Id);
        _soundSynths     = await Service.GetSoundSynthsAsync(Id);

        // Load attributes for this release
        _releaseAttributes = await Service.GetReleaseAttributesAsync(Id);
        _releaseSpecs      = _releaseAttributes.Where(a => a.Category == "Spec").ToList();
        _releaseRatings    = _releaseAttributes.Where(a => a.Category == "Rating").ToList();

        // Load covers for this release
        List<Guid?> coverIds = await Service.GetCoverIdsByReleaseAsync(Id);
        _covers = [];

        foreach(Guid? coverId in coverIds)
        {
            if(coverId.HasValue && coverId.Value != Guid.Empty)
            {
                SoftwareCoverDto detail = await Service.GetCoverDetailsAsync(coverId.Value);

                if(detail != null) _covers.Add(detail);
            }
        }

        _coversByType = _covers
                        .GroupBy(c => c.TypeName ?? "Other")
                        .OrderBy(g => g.Key)
                        .ToDictionary(g => g.Key, g => g.ToList());

        // Pick a random front cover for the hero header
        var frontCovers = _covers.Where(c => c.Type == 0).ToList();

        if(frontCovers.Count > 0)
            _heroCover = frontCovers[Random.Shared.Next(frontCovers.Count)];

        // Aggregate companies from multiple levels
        await LoadAggregatedCompaniesAsync();

        AuthenticationState authState = await AuthState;

        if(authState.User.Identity?.IsAuthenticated == true)
            _isCollected = await CollectionSvc.IsSoftwareReleaseCollectedAsync(Id);

        _loaded = true;
        StateHasChanged();
    }

    async Task ToggleCollectionAsync()
    {
        _togglingCollection = true;

        if(_isCollected)
        {
            (bool success, _) = await CollectionSvc.RemoveSoftwareReleaseFromCollectionAsync(Id);

            if(success) _isCollected = false;
        }
        else
        {
            (bool success, _) = await CollectionSvc.AddSoftwareReleaseToCollectionAsync(Id);

            if(success) _isCollected = true;
        }

        _togglingCollection = false;
    }

    async Task LoadAggregatedCompaniesAsync()
    {
        var seen = new HashSet<(int, string)>();
        var companies = new List<(int CompanyId, string CompanyName, string Role)>();

        // Companies from version level
        if(_release.SoftwareVersionId.HasValue)
        {
            List<CompanyBySoftwareVersionDto> versionCompanies =
                await Service.GetCompaniesByVersionAsync(_release.SoftwareVersionId.Value);

            foreach(CompanyBySoftwareVersionDto c in versionCompanies)
            {
                if(c.CompanyId.HasValue && seen.Add((c.CompanyId.Value, c.RoleId ?? "")))
                    companies.Add((c.CompanyId.Value, c.Company ?? "", c.Role ?? ""));
            }
        }

        _aggregatedCompanies = companies.OrderBy(c => c.CompanyName).ToList();
    }

    static string GetBarcodeTypeName(int type) => ((BarcodeType)type).Humanize();

    static string GetProductCodeIssuerName(int issuer) => ((ProductCodeIssuer)issuer).Humanize();

    /// <summary>
    ///     Open the collaborative suggestion dialog scoped to this software release.
    ///     Captures the route parameter <see cref="Id" /> directly (NOT <c>_release.Id</c>
    ///     \u2014 async race lesson: OwningComponentBase + the async DTO load can momentarily
    ///     leave the DTO null/0 and the cast to long would silently produce 0 which the
    ///     server treats as an addition request).
    /// </summary>
    async Task OpenSoftwareReleaseSuggestionDialogAsync()
    {
        if(_release is null) return;
        long releaseId = Id;
        if(releaseId <= 0) return;

        var parameters = new DialogParameters
        {
            ["EntityId"]   = releaseId,
            ["CurrentDto"] = _release
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true };
        IDialogReference dialog = await DialogService.ShowAsync<SoftwareReleaseSuggestionDialog>(
            L["Suggest changes"], parameters, options);
        await dialog.Result;
    }

    /// <summary>
    ///     Open the dedicated dialog for the Specifications card. Pre-passes the existing
    ///     spec rows so the dialog doesn't need to re-fetch — it loads only the distinct
    ///     keys/values picker corpora. After close, refresh the local attributes list to
    ///     reflect any rows the suggestion server may already have applied (admin viewing).
    /// </summary>
    async Task OpenSoftwareReleaseSpecsSuggestionDialogAsync()
    {
        long releaseId = Id;
        if(releaseId <= 0) return;

        var parameters = new DialogParameters
        {
            ["EntityId"]     = releaseId,
            ["ExistingRows"] = _releaseSpecs ?? new List<SoftwareAttributeDto>()
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
        IDialogReference dialog = await DialogService.ShowAsync<SoftwareReleaseSpecsSuggestionDialog>(
            L["Suggest specifications changes"], parameters, options);
        await dialog.Result;
        await RefreshAttributesAsync();
    }

    /// <summary>Open the dedicated dialog for the Ratings card. See specs counterpart.</summary>
    async Task OpenSoftwareReleaseRatingsSuggestionDialogAsync()
    {
        long releaseId = Id;
        if(releaseId <= 0) return;

        var parameters = new DialogParameters
        {
            ["EntityId"]     = releaseId,
            ["ExistingRows"] = _releaseRatings ?? new List<SoftwareAttributeDto>()
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
        IDialogReference dialog = await DialogService.ShowAsync<SoftwareReleaseRatingsSuggestionDialog>(
            L["Suggest ratings changes"], parameters, options);
        await dialog.Result;
        await RefreshAttributesAsync();
    }

    async Task RefreshAttributesAsync()
    {
        _releaseAttributes = await Service.GetReleaseAttributesAsync(Id);
        _releaseSpecs      = _releaseAttributes.Where(a => a.Category == "Spec").ToList();
        _releaseRatings    = _releaseAttributes.Where(a => a.Category == "Rating").ToList();
        StateHasChanged();
    }

    /// <summary>
    ///     Open the collaborative cover-upload dialog for this release. Lets a logged-in
    ///     user stage 1-30 pending cover images (jpg/png/webp), pick a mandatory type per
    ///     image, optionally caption each one, then submit ONE Suggestion row that an
    ///     administrator reviews per-image.
    /// </summary>
    async Task OpenSuggestCoversDialog()
    {
        AuthenticationState authState = await AuthState;
        if(authState.User.Identity?.IsAuthenticated != true) return;
        if(_release is null) return;

        var parameters = new DialogParameters
        {
            ["SoftwareReleaseId"] = (ulong)_release.Id,
            ["ReleaseTitle"]      = _release.Title ?? _softwareName ?? string.Empty
        };
        var options = new DialogOptions
        {
            MaxWidth          = MaxWidth.Large,
            FullWidth         = true,
            CloseOnEscapeKey  = true
        };
        IDialogReference dialog = await DialogService.ShowAsync<SoftwareCoversSuggestionDialog>(
            L["Suggest covers"], parameters, options);
        await dialog.Result;
    }
}
