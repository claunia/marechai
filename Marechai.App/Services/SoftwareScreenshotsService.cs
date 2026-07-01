#nullable enable

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.App.Services.Authentication;
using Marechai.ApiClient.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

public class SoftwareScreenshotsService
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    readonly Client                              _apiClient;
    readonly ILogger<SoftwareScreenshotsService> _logger;
    readonly ITokenService                       _tokenService;
    readonly IConfiguration                      _configuration;

    public SoftwareScreenshotsService(Client apiClient, ILogger<SoftwareScreenshotsService> logger,
                                       ITokenService tokenService, IConfiguration configuration)
    {
        _apiClient     = apiClient;
        _logger        = logger;
        _tokenService  = tokenService;
        _configuration = configuration;
    }

    public async Task<(AdminPendingSoftwareScreenshotUploadDto? Result, string? Error)> StageAdminPendingScreenshotAsync(
        int softwareId, byte[] fileBytes, string fileName, string? contentType)
    {
        try
        {
            _logger.LogInformation("Staging admin screenshot for software {SoftwareId}", softwareId);

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
                await httpClient.PostAsync($"/software/screenshots/admin/pending?softwareId={softwareId}", content);

            string json = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error staging admin screenshot for software {SoftwareId}: {StatusCode} {Body}",
                                  softwareId, response.StatusCode, json);

                return (null, ExtractProblemDetail(json) ?? $"Request failed with status {(int)response.StatusCode}");
            }

            AdminPendingSoftwareScreenshotUploadDto? result = string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<AdminPendingSoftwareScreenshotUploadDto>(json, JsonOptions);

            return (result, null);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error staging admin screenshot for software {SoftwareId}", softwareId);

            return (null, ex.Message);
        }
    }

    public async Task<bool> DeleteAdminPendingScreenshotAsync(Guid pendingId)
    {
        try
        {
            _logger.LogInformation("Deleting staged admin screenshot {PendingId}", pendingId);
            await _apiClient.Software.Screenshots.Admin.Pending[pendingId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting staged admin screenshot {PendingId}", pendingId);

            return false;
        }
    }

    public async Task<(AdminSoftwareScreenshotBatchJobStatusDto? Result, string? Error)> CommitAdminBatchAsync(
        AdminSoftwareScreenshotBatchCommitRequestDto request)
    {
        try
        {
            _logger.LogInformation("Committing admin screenshot batch for software {SoftwareId}", request.SoftwareId);
            AdminSoftwareScreenshotBatchJobStatusDto? result =
                await _apiClient.Software.Screenshots.Admin.Batch.Commit.PostAsync(request);

            return (result, null);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "Error committing admin screenshot batch for software {SoftwareId}", request.SoftwareId);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error committing admin screenshot batch for software {SoftwareId}", request.SoftwareId);

            return (null, ex.Message);
        }
    }

    public async Task<AdminSoftwareScreenshotBatchJobStatusDto?> GetAdminBatchStatusAsync(Guid jobId)
    {
        try
        {
            _logger.LogInformation("Fetching admin screenshot batch status for {JobId}", jobId);

            return await _apiClient.Software.Screenshots.Admin.Batch[jobId].Status.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin screenshot batch status for {JobId}", jobId);

            return null;
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
