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

public class MachinePhotosService
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    readonly Client                        _apiClient;
    readonly ILogger<MachinePhotosService> _logger;
    readonly ITokenService                 _tokenService;
    readonly IConfiguration                _configuration;

    public MachinePhotosService(Client apiClient, ILogger<MachinePhotosService> logger, ITokenService tokenService,
                                 IConfiguration configuration)
    {
        _apiClient     = apiClient;
        _logger        = logger;
        _tokenService  = tokenService;
        _configuration = configuration;
    }

    public async Task<List<Guid>> GetPhotoIdsAsync(int machineId)
    {
        try
        {
            _logger.LogInformation("Fetching photo IDs for machine {MachineId}", machineId);
            List<Guid?>? photos = await _apiClient.Machines[machineId].Photos.GetAsync();

            if(photos is null || photos.Count == 0)
                return [];

            return photos.Where(p => p.HasValue).Select(p => p!.Value).ToList();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching photo IDs for machine {MachineId}", machineId);

            return [];
        }
    }

    public async Task<MachinePhotoDto?> GetPhotoDetailsAsync(Guid photoId)
    {
        try
        {
            _logger.LogInformation("Fetching photo details for {PhotoId}", photoId);

            return await _apiClient.Machines.Photos[photoId].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching photo details for {PhotoId}", photoId);

            return null;
        }
    }

    public async Task<MachinePhotoDto?> UploadPhotoAsync(int     machineId,
                                                         int     licenseId,
                                                         string? source,
                                                         byte[]  fileBytes,
                                                         string  fileName)
    {
        try
        {
            _logger.LogInformation("Uploading photo for machine {MachineId}", machineId);

            var body = new Marechai.ApiClient.Machines.Photos.Upload.UploadPostRequestBody
            {
                MachineId = machineId,
                LicenseId = licenseId,
                Source    = source,
                File      = fileBytes
            };

            MachinePhotoDto? result = await _apiClient.Machines.Photos.Upload.PostAsync(body);

            _logger.LogInformation("Successfully uploaded photo for machine {MachineId}", machineId);

            return result;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo for machine {MachineId}", machineId);

            return null;
        }
    }

    public async Task<(AdminPendingMachinePhotoUploadDto? Result, string? Error)> StageAdminPendingPhotoAsync(
        int machineId, byte[] fileBytes, string fileName, string? contentType)
    {
        try
        {
            _logger.LogInformation("Staging admin machine photo for machine {MachineId}", machineId);

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
                await httpClient.PostAsync($"/machines/photos/admin/pending?machineId={machineId}", content);

            string json = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error staging admin machine photo for machine {MachineId}: {StatusCode} {Body}",
                                  machineId, response.StatusCode, json);

                return (null, ExtractProblemDetail(json) ?? $"Request failed with status {(int)response.StatusCode}");
            }

            AdminPendingMachinePhotoUploadDto? result = string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<AdminPendingMachinePhotoUploadDto>(json, JsonOptions);

            return (result, null);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error staging admin machine photo for machine {MachineId}", machineId);

            return (null, ex.Message);
        }
    }

    public async Task<bool> DeleteAdminPendingPhotoAsync(Guid pendingId)
    {
        try
        {
            _logger.LogInformation("Deleting staged admin machine photo {PendingId}", pendingId);
            await _apiClient.Machines.Photos.Admin.Pending[pendingId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting staged admin machine photo {PendingId}", pendingId);

            return false;
        }
    }

    public async Task<(AdminMachinePhotoBatchJobStatusDto? Result, string? Error)> CommitAdminBatchAsync(
        AdminMachinePhotoBatchCommitRequestDto request)
    {
        try
        {
            _logger.LogInformation("Committing admin machine photo batch for machine {MachineId}", request.MachineId);
            AdminMachinePhotoBatchJobStatusDto? result =
                await _apiClient.Machines.Photos.Admin.Batch.Commit.PostAsync(request);

            return (result, null);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "Error committing admin machine photo batch for machine {MachineId}", request.MachineId);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error committing admin machine photo batch for machine {MachineId}",
                             request.MachineId);

            return (null, ex.Message);
        }
    }

    public async Task<AdminMachinePhotoBatchJobStatusDto?> GetAdminBatchStatusAsync(Guid jobId)
    {
        try
        {
            _logger.LogInformation("Fetching admin machine photo batch status for {JobId}", jobId);

            return await _apiClient.Machines.Photos.Admin.Batch[jobId].Status.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin machine photo batch status for {JobId}", jobId);

            return null;
        }
    }

    public async Task<bool> DeletePhotoAsync(Guid photoId)
    {
        try
        {
            _logger.LogInformation("Deleting photo {PhotoId}", photoId);
            await _apiClient.Machines.Photos[photoId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo {PhotoId}", photoId);

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
