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
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Marechai.Services;

public class SoftwareService(Marechai.ApiClient.Client client, IRequestAdapter requestAdapter)
{
    // ── CRUD methods ──

    public async Task<(int? id, string? error)> CreateAsync(SoftwareDto dto)
    {
        try
        {
            int? id = await client.Software.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> UpdateAsync(int id, SoftwareDto dto)
    {
        try
        {
            await client.Software[id].PutAsync(dto);

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
            await client.Software[id].DeleteAsync();

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

    // ── Company role junction methods ──

    public async Task<List<SoftwareCompanyRoleDto>> GetCompanyRolesAsync(int softwareId)
    {
        try
        {
            List<SoftwareCompanyRoleDto>? roles = await client.Software[softwareId].CompanyRoles.GetAsync();

            return roles ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string? error)> AddCompanyRoleAsync(SoftwareCompanyRoleDto dto)
    {
        try
        {
            await client.Software.CompanyRoles.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> RemoveCompanyRoleAsync(int softwareId, int companyId,
                                                                              string roleId)
    {
        try
        {
            await client.Software.CompanyRoles[softwareId][companyId][roleId].DeleteAsync();

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

    // ── Picker methods for admin ──

    public async Task<List<SoftwareRoleDto>> GetSoftwareRolesAsync()
    {
        try
        {
            List<SoftwareRoleDto>? roles = await client.Software.Roles.Enabled.GetAsync();

            return roles ?? [];
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

    public async Task<List<SoftwareFamilyDto>> GetAllFamiliesAsync()
    {
        try
        {
            List<SoftwareFamilyDto>? families = await client.Software.Families.GetAsync();

            return families ?? [];
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

    // ── Screenshot methods ──

    public async Task<SoftwareScreenshotDto?> UploadScreenshotAsync(int     softwareId, byte[] fileBytes,
                                                                     string  fileName,
                                                                     ulong?  softwarePlatformId = null,
                                                                     ulong?  softwareVersionId  = null,
                                                                     string? caption             = null)
    {
        try
        {
            string contentType = Path.GetExtension(fileName)?.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"            => "image/png",
                ".webp"           => "image/webp",
                ".tiff" or ".tif" => "image/tiff",
                ".bmp"            => "image/bmp",
                _                 => "application/octet-stream"
            };

            var body = new MultipartBody();
            body.AddOrReplacePart("file", contentType, new MemoryStream(fileBytes), fileName);
            body.AddOrReplacePart("softwareId", "text/plain", softwareId.ToString());

            if(softwarePlatformId.HasValue)
                body.AddOrReplacePart("softwarePlatformId", "text/plain", softwarePlatformId.Value.ToString());

            if(softwareVersionId.HasValue)
                body.AddOrReplacePart("softwareVersionId", "text/plain", softwareVersionId.Value.ToString());

            if(!string.IsNullOrEmpty(caption))
                body.AddOrReplacePart("caption", "text/plain", caption);

            var pathParams = new Dictionary<string, object> { { "baseurl", requestAdapter.BaseUrl } };

            var requestInfo = new RequestInformation(Method.POST,
                "{+baseurl}/software/screenshots/upload", pathParams);

            requestInfo.Headers.TryAdd("Accept", "application/json");
            requestInfo.SetContentFromParsable(requestAdapter, "multipart/form-data", body);

            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", ProblemDetails.CreateFromDiscriminatorValue },
                { "401", ProblemDetails.CreateFromDiscriminatorValue }
            };

            return await requestAdapter.SendAsync(requestInfo,
                SoftwareScreenshotDto.CreateFromDiscriminatorValue, errorMapping);
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool succeeded, string? error)> DeleteScreenshotAsync(Guid screenshotId)
    {
        try
        {
            await client.Software.Screenshots[screenshotId.ToString()].DeleteAsync();

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

    // ── Company browsing methods ──

    public async Task<List<CompanyDto>> GetCompaniesAsync()
    {
        try
        {
            List<CompanyDto>? companies = await client.Software.Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyDto>> GetCompaniesByLetterAsync(char c)
    {
        try
        {
            List<CompanyDto>? companies = await client.Software.Companies.Letter[c.ToString()].GetAsync();

            return companies ?? [];
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

    public async Task<List<PersonBySoftwareDto>> GetCreditsBySoftwareAsync(int softwareId)
    {
        try
        {
            List<PersonBySoftwareDto>? credits = await client.Software[softwareId].Credits.GetAsync();

            return credits ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareGenreDto>> GetGenresAsync(int softwareId)
    {
        try
        {
            List<SoftwareGenreDto>? genres = await client.Software[softwareId].Genres.GetAsync();

            return genres ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareAttributeDto>> GetAttributesAsync(int softwareId)
    {
        try
        {
            List<SoftwareAttributeDto>? attributes = await client.Software[softwareId].Attributes.GetAsync();

            return attributes ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareAttributeDto>> GetReleaseAttributesAsync(int releaseId)
    {
        try
        {
            List<SoftwareAttributeDto>? attributes =
                await client.Software.Releases[releaseId].Attributes.GetAsync();

            return attributes ?? [];
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

    public async Task<List<SoftwareScreenshotDto>> GetScreenshotsBySoftwareAsync(int softwareId)
    {
        try
        {
            List<Guid?>? ids = await client.Software[softwareId].Screenshots.GetAsync();

            if(ids is null or { Count: 0 }) return [];

            var screenshots = new List<SoftwareScreenshotDto>();

            foreach(Guid? id in ids)
            {
                if(!id.HasValue) continue;

                SoftwareScreenshotDto? dto = await GetScreenshotDetailsAsync(id.Value);

                if(dto is not null) screenshots.Add(dto);
            }

            return screenshots;
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string? error)> UpdateScreenshotAsync(Guid id, SoftwareScreenshotDto dto)
    {
        try
        {
            await client.Software.Screenshots[id.ToString()].PutAsync(dto);

            return (true, null);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
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

    public async Task<List<SoftwareVersionBySoftwareReleaseDto>> GetIncludedVersionsAsync(int releaseId)
    {
        try
        {
            List<SoftwareVersionBySoftwareReleaseDto>? versions =
                await client.Software.Releases[releaseId].Versions.GetAsync();

            return versions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetCompilationsForSoftwareAsync(int softwareId)
    {
        try
        {
            List<SoftwareReleaseDto>? compilations = await client.Software[softwareId].Compilations.GetAsync();

            return compilations ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetReleasesBySoftwareAsync(int softwareId)
    {
        try
        {
            List<SoftwareReleaseDto>? releases = await client.Software[softwareId].Releases.GetAsync();

            return releases ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareBySoftwareReleaseDto>> GetIncludedSoftwareAsync(int releaseId)
    {
        try
        {
            List<SoftwareBySoftwareReleaseDto>? software =
                await client.Software.Releases[releaseId].Software.GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ── Description methods ──

    public async Task<string?> GetDescriptionTextAsync(int id)
    {
        try
        {
            var desc = await client.Software[id].Description.GetAsync();

            return desc?.Html ?? desc?.Markdown;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SoftwareDescriptionDto>> GetDescriptionsAsync(int softwareId)
    {
        try
        {
            List<SoftwareDescriptionDto>? descriptions = await client.Software[softwareId].Descriptions.GetAsync();

            return descriptions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string? error)> CreateOrUpdateDescriptionAsync(int                    softwareId,
                                                                                      SoftwareDescriptionDto dto)
    {
        try
        {
            await client.Software[softwareId].Description.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> DeleteDescriptionAsync(int softwareId, string languageCode)
    {
        try
        {
            await client.Software[softwareId].Description[languageCode].DeleteAsync();

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

    // ── Cover methods ──

    public async Task<List<SoftwareCoverDto>> GetCoversBySoftwareAsync(int softwareId)
    {
        try
        {
            List<SoftwareCoverDto>? covers = await client.Software[softwareId].Covers.GetAsync();

            return covers ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<Guid?>> GetCoverIdsByReleaseAsync(int releaseId)
    {
        try
        {
            List<Guid?>? ids = await client.Software.Releases[releaseId].Covers.GetAsync();

            return ids ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoftwareCoverDto?> GetCoverDetailsAsync(Guid id)
    {
        try
        {
            return await client.Software.Covers[id.ToString()].GetAsync();
        }
        catch
        {
            return null;
        }
    }
}
