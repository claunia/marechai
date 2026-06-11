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

public class GpuPhotosService(Marechai.ApiClient.Client client, IRequestAdapter requestAdapter, ReferenceDataCache referenceData)
{
    public async Task<List<Guid>> GetGuidsByGpuAsync(int gpuId)
    {
        try
        {
            List<Guid?> guids = await client.Gpus[gpuId].Photos.GetAsync();

            return guids?.Where(g => g.HasValue).Select(g => g!.Value).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<GpuPhotoDto> GetAsync(Guid id)
    {
        try
        {
            return await client.Gpus.Photos[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(GpuPhotoDto photo, string error)> UploadPhotoAsync(int    gpuId,     int licenseId,
                                                                            string source,   byte[] fileBytes,
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
            body.AddOrReplacePart("gpuId", "text/plain", gpuId.ToString());
            body.AddOrReplacePart("licenseId", "text/plain", licenseId.ToString());

            if(!string.IsNullOrEmpty(source))
                body.AddOrReplacePart("source", "text/plain", source);

            var pathParams = new Dictionary<string, object> { { "baseurl", requestAdapter.BaseUrl } };

            var requestInfo = new RequestInformation(Method.POST,
                "{+baseurl}/gpus/photos/upload", pathParams);

            requestInfo.Headers.TryAdd("Accept", "application/json");
            requestInfo.SetContentFromParsable(requestAdapter, "multipart/form-data", body);

            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", ProblemDetails.CreateFromDiscriminatorValue },
                { "401", ProblemDetails.CreateFromDiscriminatorValue }
            };

            GpuPhotoDto result = await requestAdapter.SendAsync(requestInfo,
                GpuPhotoDto.CreateFromDiscriminatorValue, errorMapping);

            return (result, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractDetail(ex));
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
            await client.Gpus.Photos[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public Task<List<LicenseDto>> GetAllLicensesAsync() => referenceData.GetLicensesAsync();

    /// <summary>
    ///     Delete a pending GPU photo (a not-yet-submitted file the collaborator staged via
    ///     <c>POST /gpus/photos/pending</c>). Used by the suggestion dialog when the user
    ///     removes a photo from the staging list before submission, OR when they cancel the
    ///     dialog with photos still staged.
    /// </summary>
    public async Task<bool> DeletePendingPhotoAsync(Guid guid)
    {
        try
        {
            await client.Gpus.Photos.Pending[guid].DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Delete a single admin-staged pending GPU-photo image (the admin
    ///     batch-upload dialog uses this when the admin removes a card before committing
    ///     or cancels the dialog entirely). Returns false on any failure — caller decides
    ///     whether to surface it.
    /// </summary>
    public async Task<bool> DeleteAdminPendingPhotoAsync(Guid guid)
    {
        try
        {
            await client.Gpus.Photos.Admin.Pending[guid].DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Commit a batch of admin-staged pending GPU-photo images. Returns the
    ///     initial job snapshot (state=Queued) plus any error message; the caller is
    ///     responsible for polling <see cref="GetAdminBatchStatusAsync" /> until the job
    ///     reaches a terminal state.
    /// </summary>
    public async Task<(AdminGpuPhotoBatchJobStatusDto job, string error)> CommitAdminBatchAsync(
        AdminGpuPhotoBatchCommitRequestDto request)
    {
        try
        {
            AdminGpuPhotoBatchJobStatusDto job =
                await client.Gpus.Photos.Admin.Batch.Commit.PostAsync(request);
            return (job, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    /// <summary>
    ///     Fetch the current status of an in-flight admin GPU-photo batch-commit job.
    ///     Returns null when the job id is unknown to the server (caller treats null as
    ///     a terminal failure).
    /// </summary>
    public async Task<AdminGpuPhotoBatchJobStatusDto> GetAdminBatchStatusAsync(Guid jobId)
    {
        try
        {
            return await client.Gpus.Photos.Admin.Batch[jobId].Status.GetAsync();
        }
        catch
        {
            return null;
        }
    
    }

    static string ExtractDetail(ApiException ex)
    {
        if(ex is ProblemDetails pd)
        {
            if(!string.IsNullOrWhiteSpace(pd.Detail)) return pd.Detail;
            if(!string.IsNullOrWhiteSpace(pd.Title))  return pd.Title;
        }

        if(ex is { ResponseStatusCode: 0 } || string.IsNullOrWhiteSpace(ex.Message)) return "Unknown error";

        return ex.Message;
    }
}
