#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

public class GpuPhotosService
{
    readonly Client                    _apiClient;
    readonly ILogger<GpuPhotosService> _logger;

    public GpuPhotosService(Client apiClient, ILogger<GpuPhotosService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<List<Guid>> GetPhotoIdsAsync(int gpuId)
    {
        try
        {
            _logger.LogInformation("Fetching photo IDs for GPU {GpuId}", gpuId);
            List<Guid?>? photos = await _apiClient.Gpus[gpuId].Photos.GetAsync();

            if(photos is null || photos.Count == 0)
                return [];

            return photos.Where(p => p.HasValue).Select(p => p!.Value).ToList();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching photo IDs for GPU {GpuId}", gpuId);

            return [];
        }
    }

    public async Task<GpuPhotoDto?> GetPhotoDetailsAsync(Guid photoId)
    {
        try
        {
            _logger.LogInformation("Fetching GPU photo details for {PhotoId}", photoId);

            return await _apiClient.Gpus.Photos[photoId].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching GPU photo details for {PhotoId}", photoId);

            return null;
        }
    }

    public async Task<(AdminPendingGpuPhotoUploadDto? Result, string? Error)> StageAdminPendingPhotoAsync(
        int gpuId, byte[] fileBytes, string fileName, string? contentType)
    {
        try
        {
            _logger.LogInformation("Staging admin GPU photo for GPU {GpuId}", gpuId);

            var body = new MultipartBody();
            body.AddOrReplacePart("file", GetContentType(fileName, contentType), new MemoryStream(fileBytes), fileName);

            AdminPendingGpuPhotoUploadDto? result = await _apiClient.Gpus.Photos.Admin.Pending.PostAsync(
                body, config => config.QueryParameters.GpuId = gpuId);

            return (result, null);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "Error staging admin GPU photo for GPU {GpuId}", gpuId);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error staging admin GPU photo for GPU {GpuId}", gpuId);

            return (null, ex.Message);
        }
    }

    public async Task<bool> DeleteAdminPendingPhotoAsync(Guid pendingId)
    {
        try
        {
            _logger.LogInformation("Deleting staged admin GPU photo {PendingId}", pendingId);
            await _apiClient.Gpus.Photos.Admin.Pending[pendingId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting staged admin GPU photo {PendingId}", pendingId);

            return false;
        }
    }

    public async Task<(AdminGpuPhotoBatchJobStatusDto? Result, string? Error)> CommitAdminBatchAsync(
        AdminGpuPhotoBatchCommitRequestDto request)
    {
        try
        {
            _logger.LogInformation("Committing admin GPU photo batch for GPU {GpuId}", request.GpuId);
            AdminGpuPhotoBatchJobStatusDto? result = await _apiClient.Gpus.Photos.Admin.Batch.Commit.PostAsync(request);

            return (result, null);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "Error committing admin GPU photo batch for GPU {GpuId}", request.GpuId);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected error committing admin GPU photo batch for GPU {GpuId}", request.GpuId);

            return (null, ex.Message);
        }
    }

    public async Task<AdminGpuPhotoBatchJobStatusDto?> GetAdminBatchStatusAsync(Guid jobId)
    {
        try
        {
            _logger.LogInformation("Fetching admin GPU photo batch status for {JobId}", jobId);

            return await _apiClient.Gpus.Photos.Admin.Batch[jobId].Status.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin GPU photo batch status for {JobId}", jobId);

            return null;
        }
    }

    public async Task<bool> DeletePhotoAsync(Guid photoId)
    {
        try
        {
            _logger.LogInformation("Deleting GPU photo {PhotoId}", photoId);
            await _apiClient.Gpus.Photos[photoId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting GPU photo {PhotoId}", photoId);

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
