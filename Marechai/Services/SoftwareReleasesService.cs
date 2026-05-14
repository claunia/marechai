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
            List<SoftwareReleaseDto> releases = await client.Software.Releases.GetAsync();

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
            List<SoftwareReleaseDto> releases =
                await client.Software.Versions[versionId].Releases.GetAsync();

            return releases ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoftwareReleaseDto> GetByIdAsync(int id)
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

    public async Task<(int? id, string error)> CreateAsync(SoftwareReleaseDto dto)
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

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, SoftwareReleaseDto dto)
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

    public async Task<(bool succeeded, string error)> DeleteAsync(int id)
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
            List<SoftwareBarcodeDto> barcodes =
                await client.Software.Releases[releaseId].Barcodes.GetAsync();

            return barcodes ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(int? id, string error)> AddBarcodeAsync(SoftwareBarcodeDto dto)
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

    public async Task<(bool succeeded, string error)> RemoveBarcodeAsync(int id)
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
            List<SoftwareProductCodeDto> codes =
                await client.Software.Releases[releaseId].ProductCodes.GetAsync();

            return codes ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(int? id, string error)> AddProductCodeAsync(SoftwareProductCodeDto dto)
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

    public async Task<(bool succeeded, string error)> RemoveProductCodeAsync(int id)
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
            List<GpuBySoftwareReleaseDto> gpus =
                await client.Software.Releases[releaseId].MinimumGpus.GetAsync();

            return gpus ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> AddMinimumGpuAsync(GpuBySoftwareReleaseDto dto)
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

    public async Task<(bool succeeded, string error)> RemoveMinimumGpuAsync(int releaseId, int gpuId)
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
            List<GpuBySoftwareReleaseDto> gpus =
                await client.Software.Releases[releaseId].RecommendedGpus.GetAsync();

            return gpus ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> AddRecommendedGpuAsync(GpuBySoftwareReleaseDto dto)
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

    public async Task<(bool succeeded, string error)> RemoveRecommendedGpuAsync(int releaseId, int gpuId)
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
            List<SoundSynthBySoftwareReleaseDto> synths =
                await client.Software.Releases[releaseId].SoundSynths.GetAsync();

            return synths ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> AddSoundSynthAsync(SoundSynthBySoftwareReleaseDto dto)
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

    public async Task<(bool succeeded, string error)> RemoveSoundSynthAsync(int releaseId, int soundSynthId)
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
            List<SoftwareVersionDto> versions = await client.Software.Versions.GetAsync();

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
            List<SoftwarePlatformDto> platforms = await client.Software.Platforms.GetAsync();

            return platforms ?? [];
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
            List<CompanyDto> companies = await client.Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<UnM49Dto>> GetAllUnM49Async()
    {
        try
        {
            List<UnM49Dto> regions = await client.UnM49.GetAsync();

            return regions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<UnM49BySoftwareReleaseDto>> GetReleaseRegionsAsync(int releaseId)
    {
        try
        {
            List<UnM49BySoftwareReleaseDto> regions =
                await client.Software.Releases[releaseId].Regions.GetAsync();

            return regions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> AddRegionToReleaseAsync(int releaseId, int regionId)
    {
        try
        {
            await client.Software.Releases[releaseId].Regions.PostAsync(new UnM49BySoftwareReleaseDto
            {
                UnM49Id = regionId
            });

            return (true, null);
        }
        catch(ApiException e)
        {
            return (false, e.Message);
        }
        catch(Exception e)
        {
            return (false, e.Message);
        }
    }

    public async Task<(bool succeeded, string error)> RemoveRegionFromReleaseAsync(int releaseId, int regionId)
    {
        try
        {
            await client.Software.Releases[releaseId].Regions[regionId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException e)
        {
            return (false, e.Message);
        }
        catch(Exception e)
        {
            return (false, e.Message);
        }
    }

    public async Task<List<GpuDto>> GetAllGpusAsync()
    {
        try
        {
            List<GpuDto> gpus = await client.Gpus.GetAsync();

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
            List<SoundSynthDto> synths = await client.SoundSynths.GetAsync();

            return synths ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ── Compilation junction methods ──

    public async Task<List<SoftwareVersionBySoftwareReleaseDto>> GetIncludedVersionsAsync(int releaseId)
    {
        try
        {
            List<SoftwareVersionBySoftwareReleaseDto> versions =
                await client.Software.Releases[releaseId].Versions.GetAsync();

            return versions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> AddIncludedVersionAsync(SoftwareVersionBySoftwareReleaseDto dto)
    {
        try
        {
            await client.Software.Releases[(int)(dto.ReleaseId ?? 0)].Versions.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveIncludedVersionAsync(int releaseId, int versionId)
    {
        try
        {
            await client.Software.Releases[releaseId].Versions[versionId].DeleteAsync();

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

    public async Task<List<SoftwareVersionDto>> GetAllSoftwareVersionsForPickerAsync()
    {
        try
        {
            List<SoftwareVersionDto> versions = await client.Software.Versions.GetAsync();

            return versions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetCompilationsAsync()
    {
        try
        {
            List<SoftwareReleaseDto> compilations = await client.Software.Releases.Compilations.GetAsync();

            return compilations ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetBySoftwareAsync(int softwareId)
    {
        try
        {
            List<SoftwareReleaseDto> releases =
                await client.Software[softwareId].Releases.GetAsync();

            return releases ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ── Paged / count methods ──

    public async Task<List<SoftwareReleaseDto>> GetPagedAsync(int skip, int take, string search = null,
                                                               string sortBy = null, bool sortDescending = false)
    {
        try
        {
            List<SoftwareReleaseDto> releases = await client.Software.Releases.GetAsync(config =>
            {
                config.QueryParameters.Skip           = skip;
                config.QueryParameters.Take           = take;
                config.QueryParameters.Search         = search;
                config.QueryParameters.SortBy         = sortBy;
                config.QueryParameters.SortDescending = sortDescending;
            });

            return releases ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetCountAsync(string search = null)
    {
        try
        {
            int? count = await client.Software.Releases.Count.GetAsync(config =>
            {
                config.QueryParameters.Search = search;
            });

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetPagedByVersionAsync(int versionId, int skip, int take,
                                                                       string search = null)
    {
        try
        {
            List<SoftwareReleaseDto> releases =
                await client.Software.Versions[versionId].Releases.GetAsync(config =>
                {
                    config.QueryParameters.Skip   = skip;
                    config.QueryParameters.Take   = take;
                    config.QueryParameters.Search = search;
                });

            return releases ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetCountByVersionAsync(int versionId, string search = null)
    {
        try
        {
            int? count = await client.Software.Versions[versionId].Releases.Count.GetAsync(config =>
            {
                config.QueryParameters.Search = search;
            });

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetPagedBySoftwareAsync(int softwareId, int skip, int take,
                                                                        string search = null)
    {
        try
        {
            List<SoftwareReleaseDto> releases =
                await client.Software[softwareId].Releases.GetAsync(config =>
                {
                    config.QueryParameters.Skip   = skip;
                    config.QueryParameters.Take   = take;
                    config.QueryParameters.Search = search;
                });

            return releases ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetCountBySoftwareAsync(int softwareId, string search = null)
    {
        try
        {
            int? count = await client.Software[softwareId].Releases.Count.GetAsync(config =>
            {
                config.QueryParameters.Search = search;
            });

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    // ── Versionless compilation junction methods ──

    public async Task<List<SoftwareBySoftwareReleaseDto>> GetIncludedSoftwareAsync(int releaseId)
    {
        try
        {
            List<SoftwareBySoftwareReleaseDto> software =
                await client.Software.Releases[releaseId].Software.GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> AddIncludedSoftwareAsync(SoftwareBySoftwareReleaseDto dto)
    {
        try
        {
            await client.Software.Releases[(int)(dto.ReleaseId ?? 0)].Software.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveIncludedSoftwareAsync(int releaseId, int softwareId)
    {
        try
        {
            await client.Software.Releases[releaseId].Software[softwareId].DeleteAsync();

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

    public async Task<List<SoftwareDto>> GetAllSoftwareForPickerAsync()
    {
        try
        {
            List<SoftwareDto> software = await client.Software.GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<Iso639Dto>> GetAllLanguagesAsync()
    {
        try
        {
            List<Iso639Dto> languages = await client.Languages.GetAsync();

            return languages ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<LanguageBySoftwareReleaseDto>> GetReleaseLanguagesAsync(int releaseId)
    {
        try
        {
            List<LanguageBySoftwareReleaseDto> languages =
                await client.Software.Releases[releaseId].Languages.GetAsync();

            return languages ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> AddLanguageToReleaseAsync(int releaseId, string languageCode)
    {
        try
        {
            await client.Software.Releases[releaseId].Languages.PostAsync(new LanguageBySoftwareReleaseDto
            {
                LanguageCode = languageCode
            });

            return (true, null);
        }
        catch(ApiException e)
        {
            return (false, e.Message);
        }
        catch(Exception e)
        {
            return (false, e.Message);
        }
    }

    public async Task<(bool succeeded, string error)> RemoveLanguageFromReleaseAsync(int releaseId,
                                                                                      string languageCode)
    {
        try
        {
            await client.Software.Releases[releaseId].Languages[languageCode].DeleteAsync();

            return (true, null);
        }
        catch(ApiException e)
        {
            return (false, e.Message);
        }
        catch(Exception e)
        {
            return (false, e.Message);
        }
    }

    /// <summary>
    ///     Delete a pending Software cover image (a not-yet-submitted file the collaborator
    ///     staged via <c>POST /software/covers/pending</c>). Used by the suggestion dialog
    ///     when the user removes an image from the staging list before submission, OR when
    ///     they cancel the dialog with images still staged.
    /// </summary>
    public async Task<bool> DeletePendingCoverAsync(Guid guid)
    {
        try
        {
            await client.Software.Covers.Pending[guid].DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
