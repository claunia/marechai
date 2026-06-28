#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

public class MachinePhotosService
{
    readonly Client                        _apiClient;
    readonly ILogger<MachinePhotosService> _logger;

    public MachinePhotosService(Client apiClient, ILogger<MachinePhotosService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
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

            var body = new MultipartBody();
            body.AddOrReplacePart("file", GetContentType(fileName, contentType), new MemoryStream(fileBytes), fileName);

            AdminPendingMachinePhotoUploadDto? result = await _apiClient.Machines.Photos.Admin.Pending.PostAsync(
                body, config => config.QueryParameters.MachineId = machineId);

            return (result, null);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "Error staging admin machine photo for machine {MachineId}", machineId);

            return (null, ExtractDetail(ex));
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
