#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

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
