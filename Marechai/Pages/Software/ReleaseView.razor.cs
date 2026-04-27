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
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Marechai.Pages.Software;

public partial class ReleaseView
{
    List<(int CompanyId, string CompanyName, string Role)> _aggregatedCompanies = [];
    List<SoftwareBarcodeDto>                               _barcodes = [];
    int                                                    _id;
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
    bool                                                   _togglingCollection;

    [CascadingParameter]
    Task<AuthenticationState> AuthState { get; set; }

    [Parameter]
    public int Id
    {
        get => _id;
        set
        {
            if(_id == value) return;

            _id     = value;
            _loaded = false;
        }
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
            SoftwareVersionDto? version = await Service.GetVersionByIdAsync(_release.SoftwareVersionId.Value);
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
}
