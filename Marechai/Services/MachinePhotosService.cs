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
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Machines.Photos.Upload;
using Marechai.ApiClient.Models;

namespace Marechai.Services;

public class MachinePhotosService(Marechai.ApiClient.Client client)
{
    public async Task<List<Guid>> GetGuidsByMachineAsync(int machineId)
    {
        try
        {
            List<Guid?>? guids = await client.Machines[machineId].Photos.GetAsync();

            return guids?.Where(g => g.HasValue).Select(g => g!.Value).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<MachinePhotoDto?> GetAsync(Guid id)
    {
        try
        {
            return await client.Machines.Photos[id.ToString()].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(MachinePhotoDto? photo, string? error)> UploadPhotoAsync(int    machineId, int licenseId,
                                                                                string? source,   byte[] fileBytes)
    {
        try
        {
            var body = new UploadPostRequestBody
            {
                MachineId = machineId,
                LicenseId = licenseId,
                Source    = source,
                File     = fileBytes
            };

            MachinePhotoDto? result = await client.Machines.Photos.Upload.PostAsync(body);

            return (result, null);
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string? error)> DeletePhotoAsync(Guid id)
    {
        try
        {
            await client.Machines.Photos[id.ToString()].DeleteAsync();

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
            List<LicenseDto>? licenses = await client.Licenses.GetAsync();

            return licenses?.OrderBy(l => l.Name).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }
}
