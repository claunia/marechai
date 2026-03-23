using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.App.Models;

namespace Marechai.App.Services;

public class SoftwareBrowsingService
{
    private readonly ApiClient                        _apiClient;
    private readonly ILogger<SoftwareBrowsingService> _logger;

    public SoftwareBrowsingService(ApiClient apiClient, ILogger<SoftwareBrowsingService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<int> GetSoftwareCountAsync()
    {
        try
        {
            _logger.LogInformation("Fetching software count from API");
            int? result = await _apiClient.Software.Count.GetAsync();
            int  count  = result ?? 0;
            _logger.LogInformation("Successfully fetched software count: {Count}", count);

            return count;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software count from API");

            return 0;
        }
    }

    public async Task<int> GetMinimumYearAsync()
    {
        try
        {
            _logger.LogInformation("Fetching software minimum year from API");
            int? result = await _apiClient.Software.MinimumYear.GetAsync();
            int  year   = result ?? 0;
            _logger.LogInformation("Successfully fetched software minimum year: {Year}", year);

            return year;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software minimum year from API");

            return 0;
        }
    }

    public async Task<int> GetMaximumYearAsync()
    {
        try
        {
            _logger.LogInformation("Fetching software maximum year from API");
            int? result = await _apiClient.Software.MaximumYear.GetAsync();
            int  year   = result ?? 0;
            _logger.LogInformation("Successfully fetched software maximum year: {Year}", year);

            return year;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software maximum year from API");

            return 0;
        }
    }

    public async Task<List<SoftwareDto>> GetSoftwareByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Fetching software starting with '{Letter}' from API", letter);

            List<SoftwareDto> software = await _apiClient.Software.ByLetter[letter.ToString()].GetAsync();

            if(software == null) return [];

            _logger.LogInformation("Successfully fetched {Count} software starting with '{Letter}'",
                                   software.Count,
                                   letter);

            return software;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software by letter '{Letter}' from API", letter);

            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetSoftwareByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Fetching software from year {Year} from API", year);

            List<SoftwareDto> software = await _apiClient.Software.ByYear[year].GetAsync();

            if(software == null) return [];

            _logger.LogInformation("Successfully fetched {Count} software from year {Year}", software.Count, year);

            return software;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software by year {Year} from API", year);

            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetSoftwareByPlatformAsync(int platformId)
    {
        try
        {
            _logger.LogInformation("Fetching software for platform {PlatformId} from API", platformId);

            List<SoftwareDto> software = await _apiClient.Software.ByPlatform[platformId].GetAsync();

            if(software == null) return [];

            _logger.LogInformation("Successfully fetched {Count} software for platform {PlatformId}",
                                   software.Count,
                                   platformId);

            return software;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software by platform {PlatformId} from API", platformId);

            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetAllSoftwareAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all software from API");

            List<SoftwareDto> software = await _apiClient.Software.GetAsync();

            if(software == null) return [];

            _logger.LogInformation("Successfully fetched {Count} total software", software.Count);

            return software;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all software from API");

            return [];
        }
    }

    public async Task<SoftwareDto?> GetSoftwareByIdAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching software {SoftwareId} from API", softwareId);

            SoftwareDto? software = await _apiClient.Software[softwareId].GetAsync();

            if(software == null)
            {
                _logger.LogWarning("Software {SoftwareId} not found", softwareId);

                return null;
            }

            _logger.LogInformation("Successfully fetched software {SoftwareId}: {Name}", softwareId, software.Name);

            return software;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software {SoftwareId} from API", softwareId);

            return null;
        }
    }

    public async Task<List<SoftwareVersionDto>> GetVersionsAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching versions for software {SoftwareId} from API", softwareId);

            List<SoftwareVersionDto> versions = await _apiClient.Software[softwareId].Versions.GetAsync();

            if(versions == null) return [];

            _logger.LogInformation("Successfully fetched {Count} versions for software {SoftwareId}",
                                   versions.Count,
                                   softwareId);

            return versions;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching versions for software {SoftwareId} from API", softwareId);

            return [];
        }
    }

    public async Task<List<SoftwareCompanyRoleDto>> GetCompaniesAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching companies for software {SoftwareId} from API", softwareId);

            List<SoftwareCompanyRoleDto> companies =
                await _apiClient.Software[softwareId].Companies.GetAsync();

            if(companies == null) return [];

            _logger.LogInformation("Successfully fetched {Count} companies for software {SoftwareId}",
                                   companies.Count,
                                   softwareId);

            return companies;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies for software {SoftwareId} from API", softwareId);

            return [];
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetReleasesByVersionAsync(int versionId)
    {
        try
        {
            _logger.LogInformation("Fetching releases for version {VersionId} from API", versionId);

            List<SoftwareReleaseDto> releases =
                await _apiClient.Software.Versions[versionId].Releases.GetAsync();

            if(releases == null) return [];

            _logger.LogInformation("Successfully fetched {Count} releases for version {VersionId}",
                                   releases.Count,
                                   versionId);

            return releases;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching releases for version {VersionId} from API", versionId);

            return [];
        }
    }

    public async Task<SoftwareReleaseDto?> GetReleaseByIdAsync(int releaseId)
    {
        try
        {
            _logger.LogInformation("Fetching release {ReleaseId} from API", releaseId);

            SoftwareReleaseDto? release = await _apiClient.Software.Releases[releaseId].GetAsync();

            if(release == null)
            {
                _logger.LogWarning("Release {ReleaseId} not found", releaseId);

                return null;
            }

            _logger.LogInformation("Successfully fetched release {ReleaseId}", releaseId);

            return release;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching release {ReleaseId} from API", releaseId);

            return null;
        }
    }

    public async Task<List<SoftwareBarcodeDto>> GetBarcodesAsync(int releaseId)
    {
        try
        {
            _logger.LogInformation("Fetching barcodes for release {ReleaseId} from API", releaseId);

            List<SoftwareBarcodeDto> barcodes =
                await _apiClient.Software.Releases[releaseId].Barcodes.GetAsync();

            if(barcodes == null) return [];

            _logger.LogInformation("Successfully fetched {Count} barcodes for release {ReleaseId}",
                                   barcodes.Count,
                                   releaseId);

            return barcodes;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching barcodes for release {ReleaseId} from API", releaseId);

            return [];
        }
    }

    public async Task<List<SoftwareProductCodeDto>> GetProductCodesAsync(int releaseId)
    {
        try
        {
            _logger.LogInformation("Fetching product codes for release {ReleaseId} from API", releaseId);

            List<SoftwareProductCodeDto> productCodes =
                await _apiClient.Software.Releases[releaseId].ProductCodes.GetAsync();

            if(productCodes == null) return [];

            _logger.LogInformation("Successfully fetched {Count} product codes for release {ReleaseId}",
                                   productCodes.Count,
                                   releaseId);

            return productCodes;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching product codes for release {ReleaseId} from API", releaseId);

            return [];
        }
    }

    public async Task<List<SoftwarePlatformDto>> GetAllPlatformsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all software platforms from API");

            List<SoftwarePlatformDto> platforms = await _apiClient.Software.Platforms.GetAsync();

            if(platforms == null) return [];

            _logger.LogInformation("Successfully fetched {Count} software platforms", platforms.Count);

            return platforms;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software platforms from API");

            return [];
        }
    }

    public async Task<List<CompanyBySoftwareVersionDto>> GetCompaniesByVersionAsync(int versionId)
    {
        try
        {
            List<CompanyBySoftwareVersionDto> items =
                await _apiClient.Software.Versions[versionId].Companies.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies for version {VersionId}", versionId);

            return [];
        }
    }

    public async Task<List<CompanyBySoftwareFamilyDto>> GetCompaniesByFamilyAsync(int familyId)
    {
        try
        {
            List<CompanyBySoftwareFamilyDto> items =
                await _apiClient.Software.Families[familyId].Companies.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies for family {FamilyId}", familyId);

            return [];
        }
    }

    public async Task<List<CompanyBySoftwareVariantDto>> GetCompaniesByVariantAsync(int variantId)
    {
        try
        {
            List<CompanyBySoftwareVariantDto> items =
                await _apiClient.Software.Variants[variantId].Companies.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies for variant {VariantId}", variantId);

            return [];
        }
    }

    public async Task<SoftwareVersionDto?> GetVersionByIdAsync(int versionId)
    {
        try
        {
            return await _apiClient.Software.Versions[versionId].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching version {VersionId}", versionId);

            return null;
        }
    }
}
