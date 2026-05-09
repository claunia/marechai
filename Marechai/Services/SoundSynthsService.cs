/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

public class SoundSynthsService(Marechai.ApiClient.Client client)
{
    static string ExtractErrorMessage(ApiException ex)
    {
        if(ex is ProblemDetails pd) return pd.Detail ?? pd.Title ?? ex.Message;

        return ex.Message;
    }

    public async Task<List<SoundSynthDto>> GetAllAsync()
    {
        try
        {
            List<SoundSynthDto> synths = await client.SoundSynths.GetAsync();

            return synths ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoundSynthDto> GetByIdAsync(int id)
    {
        try
        {
            return await client.SoundSynths[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Fetches the consolidated /soundsynth/{Id} payload (head + company logo + description
    /// + machines + photos + videos) in a single HTTP round-trip. Replaces the original
    /// 5-call sequence (GetByIdAsync + GetMachinesBySoundSynthAsync + GetDescriptionTextAsync
    /// + SoundSynthPhotosService.GetGuidsBySoundSynthAsync + GetVideosBySoundSynthAsync).
    /// Returns <c>null</c> on any transport-level failure or 404; the caller should treat
    /// null the same way it treated <c>GetByIdAsync</c> returning null.
    /// </summary>
    public async Task<SoundSynthFullDto> GetFullAsync(int id, string lang = "eng")
    {
        try
        {
            return await client.SoundSynths[id].Full.GetAsync(rc =>
            {
                if(!string.IsNullOrEmpty(lang)) rc.QueryParameters.Lang = lang;
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<(long? id, string error)> CreateAsync(SoundSynthDto dto)
    {
        try
        {
            long? id = await client.SoundSynths.PostAsync(dto);

            return (id, null);
        }
        catch(ApiException ex)
        {
            return (null, ex.Message);
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, SoundSynthDto dto)
    {
        try
        {
            await client.SoundSynths[id].PutAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteAsync(int id)
    {
        try
        {
            await client.SoundSynths[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<List<CompanyDto>> GetCompaniesAsync()
    {
        try
        {
            List<CompanyDto> companies = await client.Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MachineDto>> GetMachinesBySoundSynthAsync(int soundSynthId)
    {
        try
        {
            List<MachineDto> machines = await client.SoundSynths[soundSynthId].Machines.GetAsync();

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    // Description management
    public async Task<string> GetDescriptionTextAsync(int id, string lang = "eng")
    {
        try
        {
            SoundSynthDescriptionDto desc = await client.SoundSynths[id].Description.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });

            return desc?.Html ?? desc?.Markdown;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SoundSynthDescriptionDto>> GetDescriptionsAsync(int soundSynthId)
    {
        try
        {
            List<SoundSynthDescriptionDto> descriptions = await client.SoundSynths[soundSynthId].Descriptions.GetAsync();

            return descriptions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> CreateOrUpdateDescriptionAsync(int soundSynthId,
        SoundSynthDescriptionDto dto)
    {
        try
        {
            await client.SoundSynths[soundSynthId].Description.PostAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteDescriptionAsync(int soundSynthId, string languageCode)
    {
        try
        {
            await client.SoundSynths[soundSynthId].Description[languageCode].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Videos ──

    public async Task<List<SoundSynthVideoDto>> GetVideosBySoundSynthAsync(int soundSynthId)
    {
        try
        {
            var result = await client.SoundSynths[soundSynthId].Videos.GetAsync();

            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(SoundSynthVideoDto dto, string error)> CreateVideoAsync(int    soundSynthId, string provider,
                                                                               string videoId,
                                                                               string title)
    {
        try
        {
            var dto = await client.SoundSynths[soundSynthId]
                                  .Videos.PostAsync(new CreateSoundSynthVideoRequest
                                                    {
                                                        Provider = provider,
                                                        VideoId  = videoId,
                                                        Title    = title
                                                    });

            return (dto, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> UpdateVideoTitleAsync(long id, string title)
    {
        try
        {
            await client.SoundSynths.Videos[id].PutAsync(new UpdateSoundSynthVideoRequest
                                                         {
                                                             Title = title
                                                         });

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteVideoAsync(long id)
    {
        try
        {
            await client.SoundSynths.Videos[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
