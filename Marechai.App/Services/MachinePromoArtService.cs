#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Marechai.ApiClient.Machines.PromoArt.Upload;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

public sealed class MachinePromoArtService
{
    readonly Client                          _apiClient;
    readonly ILogger<MachinePromoArtService> _logger;

    public MachinePromoArtService(Client apiClient, ILogger<MachinePromoArtService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<List<MachinePromoArtDto>> GetPromoArtByMachineAsync(int machineId)
    {
        try
        {
            string lang = GetIso639CodeFromCulture();

            List<MachinePromoArtDto>? promoArt = await _apiClient.Machines[machineId].PromoArt.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return promoArt ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching promo art for machine {MachineId}", machineId);
            return [];
        }
    }

    public async Task<MachinePromoArtDto?> GetPromoArtDetailsAsync(Guid promoArtId)
    {
        try
        {
            string lang = GetIso639CodeFromCulture();

            return await _apiClient.Machines.PromoArt[promoArtId.ToString()].GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching promo art details for {PromoArtId}", promoArtId);
            return null;
        }
    }

    public async Task<List<SoftwarePromoArtGroupDto>> GetGroupsAsync()
    {
        try
        {
            string lang = GetIso639CodeFromCulture();

            List<SoftwarePromoArtGroupDto>? groups = await _apiClient.Machines.PromoArt.Groups.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return groups ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching promo art groups");
            return [];
        }
    }

    public async Task<MachinePromoArtDto?> UploadPromoArtAsync(int machineId, byte[] fileBytes, string groupName, string? caption)
    {
        try
        {
            var body = new UploadPostRequestBody
            {
                MachineId = machineId,
                File      = fileBytes,
                GroupName = groupName,
                Caption   = caption
            };

            return await _apiClient.Machines.PromoArt.Upload.PostAsync(body);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error uploading promo art for machine {MachineId}", machineId);
            return null;
        }
    }

    public async Task<bool> UpdatePromoArtAsync(Guid id, string? groupName, string? caption)
    {
        try
        {
            var body = new UpdateMachinePromoArtRequest
            {
                GroupName = groupName,
                Caption   = caption
            };

            await _apiClient.Machines.PromoArt[id].PutAsync(body);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating promo art {PromoArtId}", id);
            return false;
        }
    }

    public async Task<bool> DeletePromoArtAsync(Guid id)
    {
        try
        {
            await _apiClient.Machines.PromoArt[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting promo art {PromoArtId}", id);
            return false;
        }
    }

    static string GetIso639CodeFromCulture()
    {
        string twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        return twoLetter switch
        {
            "en" => "eng",
            "es" => "spa",
            "de" => "deu",
            "fr" => "fra",
            "la" => "lat",
            "pt" => "por",
            _    => "eng"
        };
    }
}
