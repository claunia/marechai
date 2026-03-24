#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Machines.Photos.Upload;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

public class MachinePhotosService
{
    private readonly Client                        _apiClient;
    private readonly ILogger<MachinePhotosService> _logger;

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

            var body = new UploadPostRequestBody
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
}
