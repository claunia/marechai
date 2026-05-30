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

public class MachinePhotosService(Marechai.ApiClient.Client client, IRequestAdapter requestAdapter)
{
    public async Task<List<Guid>> GetGuidsByMachineAsync(int machineId)
    {
        try
        {
            List<Guid?> guids = await client.Machines[machineId].Photos.GetAsync();

            return guids?.Where(g => g.HasValue).Select(g => g!.Value).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<MachinePhotoDto> GetAsync(Guid id)
    {
        try
        {
            return await client.Machines.Photos[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(MachinePhotoDto photo, string error)> UploadPhotoAsync(int    machineId, int licenseId,
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
            body.AddOrReplacePart("machineId", "text/plain", machineId.ToString());
            body.AddOrReplacePart("licenseId", "text/plain", licenseId.ToString());

            if(!string.IsNullOrEmpty(source))
                body.AddOrReplacePart("source", "text/plain", source);

            var pathParams = new Dictionary<string, object> { { "baseurl", requestAdapter.BaseUrl } };

            var requestInfo = new RequestInformation(Method.POST,
                "{+baseurl}/machines/photos/upload", pathParams);

            requestInfo.Headers.TryAdd("Accept", "application/json");
            requestInfo.SetContentFromParsable(requestAdapter, "multipart/form-data", body);

            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", ProblemDetails.CreateFromDiscriminatorValue },
                { "401", ProblemDetails.CreateFromDiscriminatorValue }
            };

            MachinePhotoDto result = await requestAdapter.SendAsync(requestInfo,
                MachinePhotoDto.CreateFromDiscriminatorValue, errorMapping);

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
            await client.Machines.Photos[id].DeleteAsync();

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
    ///     Delete a pending machine photo (a not-yet-submitted file the collaborator
    ///     staged via <c>POST /machines/photos/pending</c>). Used by the suggestion
    ///     dialog when the user removes a photo from the staging list before submission,
    ///     OR when they cancel the dialog with photos still staged.
    /// </summary>
    public async Task<bool> DeletePendingPhotoAsync(Guid guid)
    {
        try
        {
            await client.Machines.Photos.Pending[guid].DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Delete a single admin-staged pending machine-photo image (the admin
    ///     batch-upload dialog uses this when the admin removes a card before committing
    ///     or cancels the dialog entirely). Returns false on any failure — caller decides
    ///     whether to surface it.
    /// </summary>
    public async Task<bool> DeleteAdminPendingPhotoAsync(Guid guid)
    {
        try
        {
            await client.Machines.Photos.Admin.Pending[guid].DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Commit a batch of admin-staged pending machine-photo images. Returns the
    ///     initial job snapshot (state=Queued) plus any error message; the caller is
    ///     responsible for polling <see cref="GetAdminBatchStatusAsync" /> until the job
    ///     reaches a terminal state.
    /// </summary>
    public async Task<(AdminMachinePhotoBatchJobStatusDto job, string error)> CommitAdminBatchAsync(
        AdminMachinePhotoBatchCommitRequestDto request)
    {
        try
        {
            AdminMachinePhotoBatchJobStatusDto job =
                await client.Machines.Photos.Admin.Batch.Commit.PostAsync(request);
            return (job, null);
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    /// <summary>
    ///     Fetch the current status of an in-flight admin machine-photo batch-commit job.
    ///     Returns null when the job id is unknown to the server (caller treats null as
    ///     a terminal failure).
    /// </summary>
    public async Task<AdminMachinePhotoBatchJobStatusDto> GetAdminBatchStatusAsync(Guid jobId)
    {
        try
        {
            return await client.Machines.Photos.Admin.Batch[jobId].Status.GetAsync();
        }
        catch
        {
            return null;
        }
    }
}
