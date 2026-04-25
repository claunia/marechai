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
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

public class SoftwareReleasesService(Marechai.ApiClient.Client client)
{
    public async Task<List<SoftwareReleaseDto>> GetAllAsync()
    {
        try
        {
            List<SoftwareReleaseDto>? releases = await client.Software.Releases.GetAsync();

            return releases ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetByVersionAsync(int versionId)
    {
        try
        {
            List<SoftwareReleaseDto>? releases =
                await client.Software.Versions[versionId].Releases.GetAsync();

            return releases ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoftwareReleaseDto?> GetByIdAsync(int id)
    {
        try
        {
            return await client.Software.Releases[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(int? id, string? error)> CreateAsync(SoftwareReleaseDto dto)
    {
        try
        {
            int? id = await client.Software.Releases.PostAsync(dto);

            return (id, null);
        }
        catch(ApiException ex)
        {
            return (null, ex.Message);
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string? error)> UpdateAsync(int id, SoftwareReleaseDto dto)
    {
        try
        {
            await client.Software.Releases[id].PutAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string? error)> DeleteAsync(int id)
    {
        try
        {
            await client.Software.Releases[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Barcode junction ──

    public async Task<List<SoftwareBarcodeDto>> GetBarcodesAsync(int releaseId)
    {
        try
        {
            List<SoftwareBarcodeDto>? barcodes =
                await client.Software.Releases[releaseId].Barcodes.GetAsync();

            return barcodes ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(int? id, string? error)> AddBarcodeAsync(SoftwareBarcodeDto dto)
    {
        try
        {
            int? id = await client.Software.Barcodes.PostAsync(dto);

            return (id, null);
        }
        catch(ApiException ex)
        {
            return (null, ex.Message);
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string? error)> RemoveBarcodeAsync(int id)
    {
        try
        {
            await client.Software.Barcodes[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Product code junction ──

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

    public async Task<(int? id, string? error)> AddProductCodeAsync(SoftwareProductCodeDto dto)
    {
        try
        {
            int? id = await client.Software.ProductCodes.PostAsync(dto);

            return (id, null);
        }
        catch(ApiException ex)
        {
            return (null, ex.Message);
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string? error)> RemoveProductCodeAsync(int id)
    {
        try
        {
            await client.Software.ProductCodes[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Minimum GPU junction ──

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

    public async Task<(bool succeeded, string? error)> AddMinimumGpuAsync(GpuBySoftwareReleaseDto dto)
    {
        try
        {
            await client.Software.Releases.MinimumGpus.PostAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string? error)> RemoveMinimumGpuAsync(int releaseId, int gpuId)
    {
        try
        {
            await client.Software.Releases.MinimumGpus[releaseId][gpuId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Recommended GPU junction ──

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

    public async Task<(bool succeeded, string? error)> AddRecommendedGpuAsync(GpuBySoftwareReleaseDto dto)
    {
        try
        {
            await client.Software.Releases.RecommendedGpus.PostAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string? error)> RemoveRecommendedGpuAsync(int releaseId, int gpuId)
    {
        try
        {
            await client.Software.Releases.RecommendedGpus[releaseId][gpuId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Sound synth junction ──

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

    public async Task<(bool succeeded, string? error)> AddSoundSynthAsync(SoundSynthBySoftwareReleaseDto dto)
    {
        try
        {
            await client.Software.Releases.SoundSynths.PostAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string? error)> RemoveSoundSynthAsync(int releaseId, int soundSynthId)
    {
        try
        {
            await client.Software.Releases.SoundSynths[releaseId][soundSynthId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Pickers ──

    public async Task<List<SoftwareVersionDto>> GetAllVersionsAsync()
    {
        try
        {
            List<SoftwareVersionDto>? versions = await client.Software.Versions.GetAsync();

            return versions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwarePlatformDto>> GetAllPlatformsAsync()
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

    public async Task<List<SoftwareVariantDto>> GetAllVariantsAsync()
    {
        try
        {
            List<SoftwareVariantDto>? variants = await client.Software.Variants.GetAsync();

            return variants ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyDto>> GetAllCompaniesAsync()
    {
        try
        {
            List<CompanyDto>? companies = await client.Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<Iso31661NumericDto>> GetCountriesAsync()
    {
        try
        {
            List<Iso31661NumericDto>? countries = await client.Iso31661Numeric.GetAsync();

            return countries ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<GpuDto>> GetAllGpusAsync()
    {
        try
        {
            List<GpuDto>? gpus = await client.Gpus.GetAsync();

            return gpus ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoundSynthDto>> GetAllSoundSynthsAsync()
    {
        try
        {
            List<SoundSynthDto>? synths = await client.SoundSynths.GetAsync();

            return synths ?? [];
        }
        catch
        {
            return [];
        }
    }
}
