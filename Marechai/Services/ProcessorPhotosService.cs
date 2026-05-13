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

public class ProcessorPhotosService(Marechai.ApiClient.Client client, IRequestAdapter requestAdapter)
{
    public async Task<List<Guid>> GetGuidsByProcessorAsync(int processorId)
    {
        try
        {
            List<Guid?> guids = await client.Processors[processorId].Photos.GetAsync();

            return guids?.Where(g => g.HasValue).Select(g => g!.Value).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<ProcessorPhotoDto> GetAsync(Guid id)
    {
        try
        {
            return await client.Processors.Photos[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(ProcessorPhotoDto photo, string error)> UploadPhotoAsync(int    processorId, int licenseId,
                                                                                  string source,     byte[] fileBytes,
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
            body.AddOrReplacePart("processorId", "text/plain", processorId.ToString());
            body.AddOrReplacePart("licenseId", "text/plain", licenseId.ToString());

            if(!string.IsNullOrEmpty(source))
                body.AddOrReplacePart("source", "text/plain", source);

            var pathParams = new Dictionary<string, object> { { "baseurl", requestAdapter.BaseUrl } };

            var requestInfo = new RequestInformation(Method.POST,
                "{+baseurl}/processors/photos/upload", pathParams);

            requestInfo.Headers.TryAdd("Accept", "application/json");
            requestInfo.SetContentFromParsable(requestAdapter, "multipart/form-data", body);

            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", ProblemDetails.CreateFromDiscriminatorValue },
                { "401", ProblemDetails.CreateFromDiscriminatorValue }
            };

            ProcessorPhotoDto result = await requestAdapter.SendAsync(requestInfo,
                ProcessorPhotoDto.CreateFromDiscriminatorValue, errorMapping);

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
            await client.Processors.Photos[id].DeleteAsync();

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

    /// <summary>
    ///     Delete a pending processor photo (a not-yet-submitted file the collaborator
    ///     staged via <c>POST /processors/photos/pending</c>). Used by the suggestion
    ///     dialog when the user removes a photo from the staging list before submission,
    ///     OR when they cancel the dialog with photos still staged.
    /// </summary>
    public async Task<bool> DeletePendingPhotoAsync(Guid guid)
    {
        try
        {
            await client.Processors.Photos.Pending[guid].DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
