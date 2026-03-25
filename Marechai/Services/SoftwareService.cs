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

namespace Marechai.Services;

public class SoftwareService(Marechai.ApiClient.Client client)
{
    // ── Picker methods ──

    public async Task<int> GetSoftwareCountAsync()
    {
        try
        {
            int? count = await client.Software.Count.GetAsync();

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> GetMinimumYearAsync()
    {
        try
        {
            int? year = await client.Software.MinimumYear.GetAsync();

            return year ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> GetMaximumYearAsync()
    {
        try
        {
            int? year = await client.Software.MaximumYear.GetAsync();

            return year ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<SoftwarePlatformDto>> GetPlatformsAsync()
    {
        try
        {
            List<SoftwarePlatformDto>? platforms = await client.Software.Platforms.GetAsync();

            return platforms ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ── Search methods ──

    public async Task<List<SoftwareDto>> GetSoftwareByLetterAsync(char c)
    {
        try
        {
            List<SoftwareDto>? software = await client.Software.ByLetter[c.ToString()].GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetSoftwareByYearAsync(int year)
    {
        try
        {
            List<SoftwareDto>? software = await client.Software.ByYear[year].GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetSoftwareByPlatformAsync(int platformId)
    {
        try
        {
            List<SoftwareDto>? software = await client.Software.ByPlatform[platformId].GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetAllSoftwareAsync()
    {
        try
        {
            List<SoftwareDto>? software = await client.Software.GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ── Detail methods ──

    public async Task<SoftwareDto?> GetSoftwareByIdAsync(int id)
    {
        try
        {
            return await client.Software[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SoftwareCompanyRoleDto>> GetCompaniesAsync(int softwareId)
    {
        try
        {
            List<SoftwareCompanyRoleDto>? companies = await client.Software[softwareId].CompanyRoles.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareVersionDto>> GetVersionsAsync(int softwareId)
    {
        try
        {
            List<SoftwareVersionDto>? versions = await client.Software[softwareId].Versions.GetAsync();

            return versions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetReleasesByVersionAsync(int versionId)
    {
        try
        {
            List<SoftwareReleaseDto>? releases = await client.Software.Versions[versionId].Releases.GetAsync();

            return releases ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<Guid?>> GetScreenshotIdsAsync(int softwareId)
    {
        try
        {
            List<Guid?>? ids = await client.Software[softwareId].Screenshots.GetAsync();

            return ids ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoftwareScreenshotDto?> GetScreenshotDetailsAsync(Guid id)
    {
        try
        {
            return await client.Software.Screenshots[id.ToString()].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    // ── Release detail methods ──

    public async Task<SoftwareVersionDto?> GetVersionByIdAsync(int versionId)
    {
        try
        {
            return await client.Software.Versions[versionId].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<SoftwareReleaseDto?> GetReleaseByIdAsync(int releaseId)
    {
        try
        {
            return await client.Software.Releases[releaseId].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SoftwareBarcodeDto>> GetBarcodesAsync(int releaseId)
    {
        try
        {
            List<SoftwareBarcodeDto>? barcodes = await client.Software.Releases[releaseId].Barcodes.GetAsync();

            return barcodes ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareProductCodeDto>> GetProductCodesAsync(int releaseId)
    {
        try
        {
            List<SoftwareProductCodeDto>? codes =
                await client.Software.Releases[releaseId].ProductCodes.GetAsync();

            return codes ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<GpuBySoftwareReleaseDto>> GetMinimumGpusAsync(int releaseId)
    {
        try
        {
            List<GpuBySoftwareReleaseDto>? gpus =
                await client.Software.Releases[releaseId].MinimumGpus.GetAsync();

            return gpus ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<GpuBySoftwareReleaseDto>> GetRecommendedGpusAsync(int releaseId)
    {
        try
        {
            List<GpuBySoftwareReleaseDto>? gpus =
                await client.Software.Releases[releaseId].RecommendedGpus.GetAsync();

            return gpus ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoundSynthBySoftwareReleaseDto>> GetSoundSynthsAsync(int releaseId)
    {
        try
        {
            List<SoundSynthBySoftwareReleaseDto>? synths =
                await client.Software.Releases[releaseId].SoundSynths.GetAsync();

            return synths ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ── Company aggregation methods (for release detail) ──

    public async Task<List<CompanyBySoftwareVersionDto>> GetCompaniesByVersionAsync(int versionId)
    {
        try
        {
            List<CompanyBySoftwareVersionDto>? companies =
                await client.Software.Versions[versionId].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyBySoftwareFamilyDto>> GetCompaniesByFamilyAsync(int familyId)
    {
        try
        {
            List<CompanyBySoftwareFamilyDto>? companies =
                await client.Software.Families[familyId].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyBySoftwareVariantDto>> GetCompaniesByVariantAsync(int variantId)
    {
        try
        {
            List<CompanyBySoftwareVariantDto>? companies =
                await client.Software.Variants[variantId].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }
}
