#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

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
