#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.ApiClient.Software.PromoArt.Upload;

namespace Marechai.App.Services;

public sealed class SoftwarePromoArtService
{
    readonly Client                           _apiClient;
    readonly ILogger<SoftwarePromoArtService> _logger;

    public SoftwarePromoArtService(Client apiClient, ILogger<SoftwarePromoArtService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<List<SoftwarePromoArtDto>> GetPromoArtBySoftwareAsync(int softwareId)
    {
        try
        {
            string lang = GetIso639CodeFromCulture();

            List<SoftwarePromoArtDto>? promoArt = await _apiClient.Software[softwareId].PromoArt.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return promoArt ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching promo art for software {SoftwareId}", softwareId);
            return [];
        }
    }

    public async Task<SoftwarePromoArtDto?> GetPromoArtDetailsAsync(Guid promoArtId)
    {
        try
        {
            string lang = GetIso639CodeFromCulture();

            return await _apiClient.Software.PromoArt[promoArtId].GetAsync(config =>
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

            List<SoftwarePromoArtGroupDto>? groups = await _apiClient.Software.PromoArt.Groups.GetAsync(config =>
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

    public async Task<SoftwarePromoArtDto?> UploadPromoArtAsync(int softwareId, byte[] fileBytes, string groupName, string? caption)
    {
        try
        {
            var body = new UploadPostRequestBody
            {
                SoftwareId = softwareId,
                File       = fileBytes,
                GroupName  = groupName,
                Caption    = caption
            };

            return await _apiClient.Software.PromoArt.Upload.PostAsync(body);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error uploading promo art for software {SoftwareId}", softwareId);
            return null;
        }
    }

    public async Task<bool> UpdatePromoArtAsync(Guid id, string? groupName, string? caption)
    {
        try
        {
            var body = new UpdateSoftwarePromoArtRequest
            {
                GroupName = groupName,
                Caption   = caption
            };

            await _apiClient.Software.PromoArt[id].PutAsync(body);

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
            await _apiClient.Software.PromoArt[id].DeleteAsync();

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
