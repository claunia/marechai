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
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Marechai.Services;

public class SoundSynthPhotosService(Marechai.ApiClient.Client client, IRequestAdapter requestAdapter)
{
    public async Task<List<Guid>> GetGuidsBySoundSynthAsync(int soundSynthId)
    {
        try
        {
            List<Guid?> guids = await client.SoundSynths[soundSynthId].Photos.GetAsync();

            return guids?.Where(g => g.HasValue).Select(g => g!.Value).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoundSynthPhotoDto> GetAsync(Guid id)
    {
        try
        {
            return await client.SoundSynths.Photos[id.ToString()].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(SoundSynthPhotoDto photo, string error)> UploadPhotoAsync(int    soundSynthId, int licenseId,
                                                                                   string source,      byte[] fileBytes,
                                                                                   string  fileName)
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
            body.AddOrReplacePart("soundSynthId", "text/plain", soundSynthId.ToString());
            body.AddOrReplacePart("licenseId", "text/plain", licenseId.ToString());

            if(!string.IsNullOrEmpty(source))
                body.AddOrReplacePart("source", "text/plain", source);

            var pathParams = new Dictionary<string, object> { { "baseurl", requestAdapter.BaseUrl } };

            var requestInfo = new RequestInformation(Method.POST,
                "{+baseurl}/sound-synths/photos/upload", pathParams);

            requestInfo.Headers.TryAdd("Accept", "application/json");
            requestInfo.SetContentFromParsable(requestAdapter, "multipart/form-data", body);

            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", ProblemDetails.CreateFromDiscriminatorValue },
                { "401", ProblemDetails.CreateFromDiscriminatorValue }
            };

            SoundSynthPhotoDto result = await requestAdapter.SendAsync(requestInfo,
                SoundSynthPhotoDto.CreateFromDiscriminatorValue, errorMapping);

            return (result, null);
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeletePhotoAsync(Guid id)
    {
        try
        {
            await client.SoundSynths.Photos[id.ToString()].DeleteAsync();

            return (true, null);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<List<LicenseDto>> GetAllLicensesAsync()
    {
        try
        {
            List<LicenseDto> licenses = await client.Licenses.GetAsync();

            return licenses?.OrderBy(l => l.Name).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }
}
