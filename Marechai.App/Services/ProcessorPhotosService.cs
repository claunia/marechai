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

public class ProcessorPhotosService
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    readonly Client                          _apiClient;
    readonly ILogger<ProcessorPhotosService> _logger;
    readonly ITokenService                   _tokenService;
    readonly IConfiguration                  _configuration;

    public ProcessorPhotosService(Client apiClient, ILogger<ProcessorPhotosService> logger,
                                   ITokenService tokenService, IConfiguration configuration)
    {
        _apiClient     = apiClient;
        _logger        = logger;
        _tokenService  = tokenService;
        _configuration = configuration;
    }

    public async Task<List<Guid>> GetPhotoIdsAsync(int processorId)
    {
        try
        {
            _logger.LogInformation("Fetching photo IDs for processor {ProcessorId}", processorId);
            List<Guid?>? photos = await _apiClient.Processors[processorId].Photos.GetAsync();

            if(photos is null || photos.Count == 0)
                return [];

            return photos.Where(p => p.HasValue).Select(p => p!.Value).ToList();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching photo IDs for processor {ProcessorId}", processorId);

            return [];
        }
    }

    public async Task<ProcessorPhotoDto?> GetPhotoDetailsAsync(Guid photoId)
    {
        try
        {
            _logger.LogInformation("Fetching processor photo details for {PhotoId}", photoId);

            return await _apiClient.Processors.Photos[photoId].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching processor photo details for {PhotoId}", photoId);

            return null;
        }
    }

    public async Task<(AdminPendingProcessorPhotoUploadDto? Result, string? Error)> StageAdminPendingPhotoAsync(
        int processorId, byte[] fileBytes, string fileName, string? contentType)
    {
        try
        {
            _logger.LogInformation("Staging admin processor photo for processor {ProcessorId}", processorId);

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
                await httpClient.PostAsync($"/processors/photos/admin/pending?processorId={processorId}", content);

            string json = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error staging admin processor photo for processor {ProcessorId}: {StatusCode} {Body}",
                                  processorId, response.StatusCode, json);

                return (null, ExtractProblemDetail(json) ?? $"Request failed with status {(int)response.StatusCode}");
            }

            AdminPendingProcessorPhotoUploadDto? result = string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<AdminPendingProcessorPhotoUploadDto>(json, JsonOptions);

            return (result, null);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error staging admin processor photo for processor {ProcessorId}", processorId);

            return (null, ex.Message);
        }
    }

    public async Task<bool> DeleteAdminPendingPhotoAsync(Guid pendingId)
    {
        try
        {
            _logger.LogInformation("Deleting staged admin processor photo {PendingId}", pendingId);
            await _apiClient.Processors.Photos.Admin.Pending[pendingId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting staged admin processor photo {PendingId}", pendingId);

            return false;
        }
    }

    public async Task<(AdminProcessorPhotoBatchJobStatusDto? Result, string? Error)> CommitAdminBatchAsync(
        AdminProcessorPhotoBatchCommitRequestDto request)
    {
        try
        {
            _logger.LogInformation("Committing admin processor photo batch for processor {ProcessorId}",
                                   request.ProcessorId);
            AdminProcessorPhotoBatchJobStatusDto? result = await _apiClient.Processors.Photos.Admin.Batch.Commit.PostAsync(request);

            return (result, null);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex,
                             "Error committing admin processor photo batch for processor {ProcessorId}",
                             request.ProcessorId);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex,
                             "Unexpected error committing admin processor photo batch for processor {ProcessorId}",
                             request.ProcessorId);

            return (null, ex.Message);
        }
    }

    public async Task<AdminProcessorPhotoBatchJobStatusDto?> GetAdminBatchStatusAsync(Guid jobId)
    {
        try
        {
            _logger.LogInformation("Fetching admin processor photo batch status for {JobId}", jobId);

            return await _apiClient.Processors.Photos.Admin.Batch[jobId].Status.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin processor photo batch status for {JobId}", jobId);

            return null;
        }
    }

    public async Task<bool> DeletePhotoAsync(Guid photoId)
    {
        try
        {
            _logger.LogInformation("Deleting processor photo {PhotoId}", photoId);
            await _apiClient.Processors.Photos[photoId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting processor photo {PhotoId}", photoId);

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
