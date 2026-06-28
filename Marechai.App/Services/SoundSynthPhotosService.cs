#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

public class SoundSynthPhotosService
{
    readonly Client                           _apiClient;
    readonly ILogger<SoundSynthPhotosService> _logger;

    public SoundSynthPhotosService(Client apiClient, ILogger<SoundSynthPhotosService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<List<Guid>> GetPhotoIdsAsync(int soundSynthId)
    {
        try
        {
            _logger.LogInformation("Fetching photo IDs for sound synth {SoundSynthId}", soundSynthId);
            List<Guid?>? photos = await _apiClient.SoundSynths[soundSynthId].Photos.GetAsync();

            if(photos is null || photos.Count == 0)
                return [];

            return photos.Where(p => p.HasValue).Select(p => p!.Value).ToList();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching photo IDs for sound synth {SoundSynthId}", soundSynthId);

            return [];
        }
    }

    public async Task<SoundSynthPhotoDto?> GetPhotoDetailsAsync(Guid photoId)
    {
        try
        {
            _logger.LogInformation("Fetching sound synth photo details for {PhotoId}", photoId);

            return await _apiClient.SoundSynths.Photos[photoId].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching sound synth photo details for {PhotoId}", photoId);

            return null;
        }
    }

    public async Task<(AdminPendingSoundSynthPhotoUploadDto? Result, string? Error)> StageAdminPendingPhotoAsync(
        int soundSynthId, byte[] fileBytes, string fileName, string? contentType)
    {
        try
        {
            _logger.LogInformation("Staging admin sound synth photo for sound synth {SoundSynthId}", soundSynthId);

            var body = new MultipartBody();
            body.AddOrReplacePart("file", GetContentType(fileName, contentType), new MemoryStream(fileBytes), fileName);

            AdminPendingSoundSynthPhotoUploadDto? result =
                await _apiClient.SoundSynths.Photos.Admin.Pending.PostAsync(
                    body, config => config.QueryParameters.SoundSynthId = soundSynthId);

            return (result, null);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "Error staging admin sound synth photo for sound synth {SoundSynthId}", soundSynthId);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex,
                             "Unexpected error staging admin sound synth photo for sound synth {SoundSynthId}",
                             soundSynthId);

            return (null, ex.Message);
        }
    }

    public async Task<bool> DeleteAdminPendingPhotoAsync(Guid pendingId)
    {
        try
        {
            _logger.LogInformation("Deleting staged admin sound synth photo {PendingId}", pendingId);
            await _apiClient.SoundSynths.Photos.Admin.Pending[pendingId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting staged admin sound synth photo {PendingId}", pendingId);

            return false;
        }
    }

    public async Task<(AdminSoundSynthPhotoBatchJobStatusDto? Result, string? Error)> CommitAdminBatchAsync(
        AdminSoundSynthPhotoBatchCommitRequestDto request)
    {
        try
        {
            _logger.LogInformation("Committing admin sound synth photo batch for sound synth {SoundSynthId}",
                                   request.SoundSynthId);
            AdminSoundSynthPhotoBatchJobStatusDto? result =
                await _apiClient.SoundSynths.Photos.Admin.Batch.Commit.PostAsync(request);

            return (result, null);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex,
                             "Error committing admin sound synth photo batch for sound synth {SoundSynthId}",
                             request.SoundSynthId);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex,
                             "Unexpected error committing admin sound synth photo batch for sound synth {SoundSynthId}",
                             request.SoundSynthId);

            return (null, ex.Message);
        }
    }

    public async Task<AdminSoundSynthPhotoBatchJobStatusDto?> GetAdminBatchStatusAsync(Guid jobId)
    {
        try
        {
            _logger.LogInformation("Fetching admin sound synth photo batch status for {JobId}", jobId);

            return await _apiClient.SoundSynths.Photos.Admin.Batch[jobId].Status.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin sound synth photo batch status for {JobId}", jobId);

            return null;
        }
    }

    public async Task<bool> DeletePhotoAsync(Guid photoId)
    {
        try
        {
            _logger.LogInformation("Deleting sound synth photo {PhotoId}", photoId);
            await _apiClient.SoundSynths.Photos[photoId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting sound synth photo {PhotoId}", photoId);

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
