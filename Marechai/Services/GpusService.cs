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

public class GpusService(Marechai.ApiClient.Client client)
{
    public async Task<List<GpuDto>> GetAllAsync()
    {
        try
        {
            List<GpuDto>? gpus = await client.Gpus.GetAsync();

            return gpus ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<GpuDto?> GetByIdAsync(int id)
    {
        try
        {
            return await client.Gpus[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(long? id, string? error)> CreateAsync(GpuDto dto)
    {
        try
        {
            long? id = await client.Gpus.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> UpdateAsync(int id, GpuDto dto)
    {
        try
        {
            await client.Gpus[id].PutAsync(dto);

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

    public async Task<(bool succeeded, string? error)> DeleteAsync(int id)
    {
        try
        {
            await client.Gpus[id].DeleteAsync();

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

    public async Task<List<ResolutionByGpuDto>> GetResolutionsByGpuAsync(int gpuId)
    {
        try
        {
            List<ResolutionByGpuDto>? resolutions =
                await client.ResolutionsByGpu.Gpus[gpuId].Resolutions.GetAsync();

            return resolutions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<ResolutionDto>> GetAllResolutionsAsync()
    {
        try
        {
            List<ResolutionDto>? resolutions = await client.Resolutions.GetAsync();

            return resolutions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string? error)> AddResolutionToGpuAsync(ResolutionByGpuDto dto)
    {
        try
        {
            long? id = await client.ResolutionsByGpu.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> RemoveResolutionFromGpuAsync(long id)
    {
        try
        {
            await client.ResolutionsByGpu[id].DeleteAsync();

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

    public async Task<ResolutionDto?> GetResolutionByIdAsync(int resolutionId)
    {
        try
        {
            return await client.Resolutions[resolutionId].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<MachineDto>> GetMachinesByGpuAsync(int gpuId)
    {
        try
        {
            List<MachineDto>? machines = await client.Gpus[gpuId].Machines.GetAsync();

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyDto>> GetCompaniesAsync()
    {
        try
        {
            List<CompanyDto>? companies = await client.Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }
}
