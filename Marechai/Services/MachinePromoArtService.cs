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
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Helpers;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Marechai.Services;

public class MachinePromoArtService(Marechai.ApiClient.Client client, IRequestAdapter requestAdapter)
{
    public async Task<List<MachinePromoArtDto>> GetPromoArtByMachineAsync(int machineId)
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            List<MachinePromoArtDto> result = await client.Machines[machineId].PromoArt.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwarePromoArtGroupDto>> GetPromoArtGroupsAsync()
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            List<SoftwarePromoArtGroupDto> result = await client.Machines.PromoArt.Groups.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<MachinePromoArtDto> UploadPromoArtAsync(ulong  machineId, string groupName, string caption,
                                                              byte[] fileBytes,  string fileName)
    {
        try
        {
            string contentType = Path.GetExtension(fileName)?.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"            => "image/png",
                ".webp"           => "image/webp",
                ".tiff" or ".tif" => "image/tiff",
                ".bmp"            => "image/bmp",
                _                 => "application/octet-stream"
            };

            var body = new MultipartBody();
            body.AddOrReplacePart("file", contentType, new MemoryStream(fileBytes), fileName);
            body.AddOrReplacePart("machineId", "text/plain", machineId.ToString());
            body.AddOrReplacePart("groupName", "text/plain", groupName);

            if(!string.IsNullOrEmpty(caption))
                body.AddOrReplacePart("caption", "text/plain", caption);

            var pathParams = new Dictionary<string, object> { { "baseurl", requestAdapter.BaseUrl } };

            var requestInfo = new RequestInformation(Method.POST,
                                                     "{+baseurl}/machines/promo-art/upload", pathParams);

            requestInfo.Headers.TryAdd("Accept", "application/json");
            requestInfo.SetContentFromParsable(requestAdapter, "multipart/form-data", body);

            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", ProblemDetails.CreateFromDiscriminatorValue },
                { "401", ProblemDetails.CreateFromDiscriminatorValue }
            };

            return await requestAdapter.SendAsync(requestInfo,
                                                  MachinePromoArtDto.CreateFromDiscriminatorValue, errorMapping);
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool succeeded, string error)> UpdatePromoArtAsync(Guid id, UpdateMachinePromoArtRequest dto)
    {
        try
        {
            await client.Machines.PromoArt[id].PutAsync(dto);

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

    public async Task<(bool succeeded, string error)> DeletePromoArtAsync(Guid id)
    {
        try
        {
            await client.Machines.PromoArt[id].DeleteAsync();

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

    public async Task<bool> DeletePendingPromoArtAsync(Guid guid)
    {
        try
        {
            await client.Machines.PromoArt.Pending[guid].DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    static string ExtractErrorMessage(ApiException ex)
    {
        if(ex is ProblemDetails problem)
        {
            if(!string.IsNullOrWhiteSpace(problem.Detail)) return problem.Detail;
            if(!string.IsNullOrWhiteSpace(problem.Title))  return problem.Title;
        }

        return string.IsNullOrWhiteSpace(ex.Message) ? "Unknown error" : ex.Message;
    }
}
