#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

public class SoftwareScreenshotsService
{
    readonly Client                                _apiClient;
    readonly ILogger<SoftwareScreenshotsService>   _logger;

    public SoftwareScreenshotsService(Client apiClient, ILogger<SoftwareScreenshotsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<(AdminPendingSoftwareScreenshotUploadDto? Result, string? Error)> StageAdminPendingScreenshotAsync(
        int softwareId, byte[] fileBytes, string fileName, string? contentType)
    {
        try
        {
            _logger.LogInformation("Staging admin screenshot for software {SoftwareId}", softwareId);

            var body = new MultipartBody();
            body.AddOrReplacePart("file", GetContentType(fileName, contentType), new MemoryStream(fileBytes), fileName);

            AdminPendingSoftwareScreenshotUploadDto? result = await _apiClient.Software.Screenshots.Admin.Pending.PostAsync(
                body, config => config.QueryParameters.SoftwareId = softwareId);

            return (result, null);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "Error staging admin screenshot for software {SoftwareId}", softwareId);

            return (null, ExtractDetail(ex));
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
