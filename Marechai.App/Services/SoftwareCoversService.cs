#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.App.Services.Authentication;
using Marechai.ApiClient.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

public class SoftwareCoversService
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    readonly Client                         _apiClient;
    readonly ILogger<SoftwareCoversService> _logger;
    readonly ITokenService                  _tokenService;
    readonly IConfiguration                 _configuration;

    public SoftwareCoversService(Client apiClient, ILogger<SoftwareCoversService> logger,
                                  ITokenService tokenService, IConfiguration configuration)
    {
        _apiClient     = apiClient;
        _logger        = logger;
        _tokenService  = tokenService;
        _configuration = configuration;
    }

    public async Task<List<Guid>> GetCoverIdsByReleaseAsync(int releaseId)
    {
        try
        {
            _logger.LogInformation("Fetching cover IDs for release {ReleaseId}", releaseId);
            List<Guid?>? covers = await _apiClient.Software.Releases[releaseId].Covers.GetAsync();

            if(covers is null || covers.Count == 0)
                return [];

            return covers.Where(c => c.HasValue).Select(c => c!.Value).ToList();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching cover IDs for release {ReleaseId}", releaseId);

            return [];
        }
    }

    public async Task<SoftwareCoverDto?> GetCoverDetailsAsync(Guid coverId)
    {
        try
        {
            _logger.LogInformation("Fetching cover details for {CoverId}", coverId);

            return await _apiClient.Software.Covers[coverId].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching cover details for {CoverId}", coverId);

            return null;
        }
    }

    public async Task<(AdminPendingCoverUploadDto? Result, string? Error)> StageAdminPendingCoverAsync(
        int releaseId, byte[] fileBytes, string fileName, string? contentType)
    {
        try
        {
            _logger.LogInformation("Staging admin cover for release {ReleaseId}", releaseId);

            // The Kiota-generated client's typed response deserialization throws
            // ArgumentOutOfRangeException in Microsoft.Kiota.Serialization.Json for this
            // endpoint's response, so the request/response is handled manually here instead.
            string baseUrl = _configuration.GetValue<string>("ApiClient:Url") ?? "http://localhost:5023";

            using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

            string token = _tokenService.GetToken();

            if(!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(fileName, contentType));
            content.Add(fileContent, "file", fileName);

            using HttpResponseMessage response =
                await httpClient.PostAsync($"/software/covers/admin/pending?releaseId={releaseId}", content);

            string json = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error staging admin cover for release {ReleaseId}: {StatusCode} {Body}", releaseId,
                                  response.StatusCode, json);

                return (null, ExtractProblemDetail(json) ?? $"Request failed with status {(int)response.StatusCode}");
            }

            AdminPendingCoverUploadDto? result = string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<AdminPendingCoverUploadDto>(json, JsonOptions);

            return (result, null);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error staging admin cover for release {ReleaseId}", releaseId);

            return (null, ex.Message);
        }
    }

    public async Task<bool> DeleteAdminPendingCoverAsync(Guid pendingId)
    {
        try
        {
            _logger.LogInformation("Deleting staged admin cover {PendingId}", pendingId);
            await _apiClient.Software.Covers.Admin.Pending[pendingId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting staged admin cover {PendingId}", pendingId);

            return false;
        }
    }

    public async Task<(AdminBatchJobStatusDto? Result, string? Error)> CommitAdminBatchAsync(
        AdminBatchCommitRequestDto request)
    {
        try
        {
            _logger.LogInformation("Committing admin cover batch for release {ReleaseId}", request.SoftwareReleaseId);
            AdminBatchJobStatusDto? result = await _apiClient.Software.Covers.Admin.Batch.Commit.PostAsync(request);

            return (result, null);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "Error committing admin cover batch for release {ReleaseId}", request.SoftwareReleaseId);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error committing admin cover batch for release {ReleaseId}", request.SoftwareReleaseId);

            return (null, ex.Message);
        }
    }

    public async Task<AdminBatchJobStatusDto?> GetAdminBatchStatusAsync(Guid jobId)
    {
        try
        {
            _logger.LogInformation("Fetching admin cover batch status for {JobId}", jobId);

            return await _apiClient.Software.Covers.Admin.Batch[jobId].Status.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin cover batch status for {JobId}", jobId);

            return null;
        }
    }

    public async Task<bool> UpdateCoverAsync(Guid id, SoftwareCoverDto dto)
    {
        try
        {
            _logger.LogInformation("Updating cover {Id}", id);
            await _apiClient.Software.Covers[id].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating cover {Id}", id);

            return false;
        }
    }

    public async Task<bool> DeleteCoverAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting cover {Id}", id);
            await _apiClient.Software.Covers[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting cover {Id}", id);

            return false;
        }
    }

    static string GetContentType(string fileName, string? contentType)
    {
        if(!string.IsNullOrWhiteSpace(contentType))
            return contentType;

        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".webp"           => "image/webp",
            ".avif"           => "image/avif",
            ".bmp"            => "image/bmp",
            ".tif" or ".tiff" => "image/tiff",
            _                 => "application/octet-stream"
        };
    }

    static string? ExtractProblemDetail(string json)
    {
        if(string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);

            if(doc.RootElement.TryGetProperty("detail", out JsonElement detail) &&
               detail.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(detail.GetString()))
                return detail.GetString();

            if(doc.RootElement.TryGetProperty("title", out JsonElement title) &&
               title.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(title.GetString()))
                return title.GetString();
        }
        catch(JsonException)
        {
            // response body wasn't a ProblemDetails-shaped JSON document
        }

        return null;
    }

    static string ExtractDetail(ApiException ex)
    {
        if(ex is ProblemDetails pd)
        {
            if(!string.IsNullOrWhiteSpace(pd.Detail)) return pd.Detail;
            if(!string.IsNullOrWhiteSpace(pd.Title))  return pd.Title;
        }

        if(string.IsNullOrWhiteSpace(ex.Message))
            return "Unknown error";

        return ex.Message;
    }
}
