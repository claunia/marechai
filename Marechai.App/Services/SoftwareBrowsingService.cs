#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

public class SoftwareBrowsingService
{
    private readonly Client                        _apiClient;
    private readonly ILogger<SoftwareBrowsingService> _logger;

    public SoftwareBrowsingService(Client apiClient, ILogger<SoftwareBrowsingService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<List<SoftwareVideoDto>> GetVideosBySoftwareAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching videos for software {SoftwareId}", softwareId);

            List<SoftwareVideoDto>? videos = await _apiClient.Software[softwareId].Videos.GetAsync();

            _logger.LogInformation("Successfully fetched {Count} videos for software {SoftwareId}",
                                    videos?.Count ?? 0,
                                    softwareId);

            return videos ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching videos for software {SoftwareId}", softwareId);

            return [];
        }
    }

    public async Task<List<SoftwareCriticReviewDto>> GetCriticReviewsBySoftwareAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching critic reviews for software {SoftwareId}", softwareId);

            List<SoftwareCriticReviewDto>? reviews = await _apiClient.Software[softwareId].CriticReviews.GetAsync();

            _logger.LogInformation("Successfully fetched {Count} critic reviews for software {SoftwareId}",
                                    reviews?.Count ?? 0,
                                    softwareId);

            return reviews ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching critic reviews for software {SoftwareId}", softwareId);

            return [];
        }
    }

    public async Task<CriticReviewSummaryDto?> GetCriticReviewSummaryBySoftwareAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching critic review summary for software {SoftwareId}", softwareId);

            return await _apiClient.Software[softwareId].CriticReviews.Summary.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching critic review summary for software {SoftwareId}", softwareId);

            return null;
        }
    }

    public async Task<List<SoftwareUserReviewDto>> GetUserReviewsBySoftwareAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching user reviews for software {SoftwareId}", softwareId);

            List<SoftwareUserReviewDto>? reviews = await _apiClient.Software[softwareId].UserReviews.GetAsync();

            _logger.LogInformation("Successfully fetched {Count} user reviews for software {SoftwareId}",
                                    reviews?.Count ?? 0,
                                    softwareId);

            return reviews ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching user reviews for software {SoftwareId}", softwareId);

            return [];
        }
    }

    public async Task<UserReviewSummaryDto?> GetUserReviewSummaryBySoftwareAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching user review summary for software {SoftwareId}", softwareId);

            return await _apiClient.Software[softwareId].UserRatings.Summary.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching user review summary for software {SoftwareId}", softwareId);

            return null;
        }
    }

    public async Task<SoftwareUserRatingDto?> GetMyUserRatingAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching current user rating for software {SoftwareId}", softwareId);

            return await _apiClient.Software[softwareId].UserRatings.Me.GetAsync();
        }
        catch(ApiException ex) when (ex.ResponseStatusCode == 204)
        {
            _logger.LogInformation("No current user rating found for software {SoftwareId}", softwareId);

            return null;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching current user rating for software {SoftwareId}", softwareId);

            return null;
        }
    }

    public async Task<(SoftwareUserReviewDto? Review, string? ErrorMessage)> CreateUserReviewAsync(
        int softwareId, SoftwareUserReviewDto review)
    {
        try
        {
            _logger.LogInformation("Creating user review for software {SoftwareId}", softwareId);

            return (await _apiClient.Software[softwareId].UserReviews.PostAsync(review), null);
        }
        catch(ProblemDetails ex)
        {
            _logger.LogError(ex, "Problem creating user review for software {SoftwareId}", softwareId);

            return (null, ex.Detail ?? ex.Title ?? ex.Message);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "API error creating user review for software {SoftwareId}", softwareId);

            return (null, ex.Message);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating user review for software {SoftwareId}", softwareId);

            return (null, ex.Message);
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateUserReviewAsync(
        int softwareId, long reviewId, SoftwareUserReviewDto review)
    {
        try
        {
            _logger.LogInformation("Updating user review {ReviewId} for software {SoftwareId}", reviewId, softwareId);

            await _apiClient.Software[softwareId].UserReviews[reviewId].PutAsync(review);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            _logger.LogError(ex,
                             "Problem updating user review {ReviewId} for software {SoftwareId}",
                             reviewId,
                             softwareId);

            return (false, ex.Detail ?? ex.Title ?? ex.Message);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "API error updating user review {ReviewId} for software {SoftwareId}",
                             reviewId, softwareId);

            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating user review {ReviewId} for software {SoftwareId}",
                             reviewId, softwareId);

            return (false, ex.Message);
        }
    }

    public async Task<bool> VoteUserReviewAsync(int softwareId, long reviewId, bool isUpvote)
    {
        try
        {
            _logger.LogInformation("Voting on review {ReviewId} for software {SoftwareId}: upvote={IsUpvote}",
                                   reviewId, softwareId, isUpvote);

            await _apiClient.Software[softwareId].UserReviews[reviewId].Vote.PostAsync(
                new SoftwareUserReviewVoteDto { IsUpvote = isUpvote });

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error voting on review {ReviewId} for software {SoftwareId}", reviewId, softwareId);

            return false;
        }
    }

    public async Task<bool> RemoveUserReviewVoteAsync(int softwareId, long reviewId)
    {
        try
        {
            _logger.LogInformation("Removing vote on review {ReviewId} for software {SoftwareId}",
                                   reviewId, softwareId);

            await _apiClient.Software[softwareId].UserReviews[reviewId].Vote.DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex,
                             "Error removing vote on review {ReviewId} for software {SoftwareId}",
                             reviewId,
                             softwareId);

            return false;
        }
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

    public async Task<List<SoftwareDto>> GetSoftwareByLetterAsync(char letter, SoftwareKind? kind = null)
    {
        try
        {
            _logger.LogInformation("Fetching software starting with '{Letter}' from API", letter);

            List<SoftwareDto> software = await _apiClient.Software.ByLetter[letter.ToString()].GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            });

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

    public async Task<List<SoftwareDto>> GetSoftwareByYearAsync(int year, SoftwareKind? kind = null)
    {
        try
        {
            _logger.LogInformation("Fetching software from year {Year} from API", year);

            List<SoftwareDto> software = await _apiClient.Software.ByYear[year].GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            });

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

    public async Task<List<SoftwareDto>> GetSoftwareByPlatformAsync(int platformId, SoftwareKind? kind = null)
    {
        try
        {
            _logger.LogInformation("Fetching software for platform {PlatformId} from API", platformId);

            List<SoftwareDto> software = await _apiClient.Software.ByPlatform[platformId].GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            });

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

    public async Task<List<SoftwareDto>> GetAllSoftwareAsync(SoftwareKind? kind = null)
    {
        try
        {
            _logger.LogInformation("Fetching all software from API");

            List<SoftwareDto> software = await _apiClient.Software.GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            });

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

    public async Task<List<SoftwareSpecKeyDto>> GetSpecificationsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching software specifications from API");

            string lang = GetIso639CodeFromCulture();

            List<SoftwareSpecKeyDto> specs = await _apiClient.Software.Specifications.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            if(specs == null) return [];

            _logger.LogInformation("Successfully fetched {Count} specification keys", specs.Count);

            return specs;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software specifications from API");

            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetSoftwareBySpecAsync(string key, string value, SoftwareKind? kind = null)
    {
        try
        {
            _logger.LogInformation("Fetching software by spec {Key}={Value} from API", key, value);

            List<SoftwareDto> software = await _apiClient.Software.BySpec.GetAsync(config =>
            {
                config.QueryParameters.Key   = key;
                config.QueryParameters.Value = value;
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            });

            if(software == null) return [];

            _logger.LogInformation("Successfully fetched {Count} software for spec {Key}={Value}",
                software.Count, key, value);

            return software;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software by spec {Key}={Value} from API", key, value);

            return [];
        }
    }

    public async Task<SoftwareDto> GetSoftwareByIdAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching software {SoftwareId} from API", softwareId);

            SoftwareDto software = await _apiClient.Software[softwareId].GetAsync();

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

    public async Task<SoftwareReleaseDto> GetReleaseByIdAsync(int releaseId)
    {
        try
        {
            _logger.LogInformation("Fetching release {ReleaseId} from API", releaseId);

            SoftwareReleaseDto release = await _apiClient.Software.Releases[releaseId].GetAsync();

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
            _logger.LogInformation("Fetching software platforms with software from API");

            List<SoftwarePlatformDto> platforms = await _apiClient.Software.Platforms.GetAsync(config =>
            {
                config.QueryParameters.IncludeUnused = false;
            });

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

    public async Task<SoftwareVersionDto> GetVersionByIdAsync(int versionId)
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

    public async Task<List<Guid>> GetScreenshotIdsAsync(int softwareId)
    {
        try
        {
            List<Guid?> result = await _apiClient.Software[softwareId].Screenshots.GetAsync();

            return result?.Where(g => g.HasValue).Select(g => g!.Value).ToList() ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching screenshot IDs for software {SoftwareId}", softwareId);

            return [];
        }
    }

    public async Task<SoftwareScreenshotDto> GetScreenshotDetailsAsync(Guid screenshotId)
    {
        try
        {
            string lang = GetIso639CodeFromCulture();

            return await _apiClient.Software.Screenshots[screenshotId.ToString()].GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching screenshot {ScreenshotId}", screenshotId);

            return null;
        }
    }

    public async Task<List<SoftwareCoverDto>> GetCoversAsync(int softwareId)
    {
        try
        {
            string lang = GetIso639CodeFromCulture();

            List<SoftwareCoverDto> covers = await _apiClient.Software[softwareId].Covers.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return covers ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching covers for software {SoftwareId}", softwareId);

            return [];
        }
    }

    public async Task<SoftwareCoverDto> GetCoverByIdAsync(Guid coverId)
    {
        try
        {
            string lang = GetIso639CodeFromCulture();

            return await _apiClient.Software.Covers[coverId.ToString()].GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching cover {CoverId}", coverId);

            return null;
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetCompilationsForSoftwareAsync(int softwareId)
    {
        try
        {
            List<SoftwareReleaseDto> compilations =
                await _apiClient.Software[softwareId].Compilations.GetAsync();

            return compilations ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching compilations for software {SoftwareId}", softwareId);

            return [];
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetReleasesBySoftwareAsync(int softwareId)
    {
        try
        {
            List<SoftwareReleaseDto> releases =
                await _apiClient.Software[softwareId].Releases.GetAsync();

            return releases ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching releases for software {SoftwareId}", softwareId);

            return [];
        }
    }

    public async Task<SoftwareDescriptionDto> GetDescriptionAsync(int softwareId, string languageCode)
    {
        try
        {
            _logger.LogInformation("Fetching description for software {SoftwareId} in language {Lang}",
                                   softwareId,
                                   languageCode);

            SoftwareDescriptionDto desc = await _apiClient.Software[softwareId].Description.GetAsync(
                                               config => config.QueryParameters.Lang = languageCode);

            return desc;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching description for software {SoftwareId}", softwareId);

            return null;
        }
    }

    public async Task<List<PersonBySoftwareDto>> GetCreditsAsync(int softwareId, string lang = null)
    {
        try
        {
            _logger.LogInformation("Fetching credits for software {SoftwareId} (lang={Lang}) from API",
                                   softwareId,
                                   lang ?? "default");

            List<PersonBySoftwareDto> credits = await _apiClient.Software[softwareId].Credits.GetAsync(config =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) config.QueryParameters.Lang = lang;
            });

            if(credits == null) return [];

            _logger.LogInformation("Successfully fetched {Count} credits for software {SoftwareId}",
                                   credits.Count,
                                   softwareId);

            return credits;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching credits for software {SoftwareId} from API", softwareId);

            return [];
        }
    }

    public async Task<List<SoftwareGenreDto>> GetGenresAsync(int softwareId, string lang = null)
    {
        try
        {
            _logger.LogInformation("Fetching genres for software {SoftwareId} (lang={Lang}) from API",
                                   softwareId, lang ?? "default");

            List<SoftwareGenreDto> genres = await _apiClient.Software[softwareId].Genres.GetAsync(config =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) config.QueryParameters.Lang = lang;
            });

            if(genres == null) return [];

            _logger.LogInformation("Successfully fetched {Count} genres for software {SoftwareId}",
                                   genres.Count,
                                   softwareId);

            return genres;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching genres for software {SoftwareId} from API", softwareId);

            return [];
        }
    }

    public async Task<List<SoftwareSimilarToDto>> GetSimilarSoftwareAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching similar software for software {SoftwareId} from API", softwareId);

            List<SoftwareSimilarToDto> similar = await _apiClient.Software[softwareId].Similar.GetAsync();

            return similar ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching similar software for software {SoftwareId} from API", softwareId);

            return [];
        }
    }

    public async Task<List<SoftwareAttributeDto>> GetAttributesAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching attributes for software {SoftwareId} from API", softwareId);

            string lang = GetIso639CodeFromCulture();

            List<SoftwareAttributeDto> attributes = await _apiClient.Software[softwareId].Attributes.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            if(attributes == null) return [];

            _logger.LogInformation("Successfully fetched {Count} attributes for software {SoftwareId}",
                                   attributes.Count,
                                   softwareId);

            return attributes;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching attributes for software {SoftwareId} from API", softwareId);

            return [];
        }
    }

    private static string GetIso639CodeFromCulture()
    {
        string twoLetter = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        return twoLetter switch
        {
            "en" => "eng",
            "es" => "spa",
            "de" => "deu",
            "fr" => "fra",
            "it" => "ita",
            "la" => "lat",
            "pt" => "por",
            _    => "eng"
        };
    }
}
