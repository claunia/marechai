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

public class SoftwareCompilationsService(Marechai.ApiClient.Client client)
{
    static string ExtractDetail(ApiException ex) => ex.Message;

    public async Task<List<SoftwareCompilationDto>> GetPagedAsync(int skip, int take, string search = null)
    {
        try
        {
            List<SoftwareCompilationDto> compilations = await client.SoftwareCompilations.GetAsync(config =>
            {
                config.QueryParameters.Skip   = skip;
                config.QueryParameters.Take   = take;
                config.QueryParameters.Search = search;
            });

            return compilations ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetCountAsync(string search = null)
    {
        try
        {
            int? count = await client.SoftwareCompilations.Count.GetAsync(config =>
            {
                config.QueryParameters.Search = search;
            });

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<SoftwareCompilationDto> GetAsync(int id)
    {
        try
        {
            return await client.SoftwareCompilations[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool succeeded, int? id, string error)> CreateAsync(SoftwareCompilationDto dto)
    {
        try
        {
            int? id = await client.SoftwareCompilations.PostAsync(dto);

            return (true, id, null);
        }
        catch(ApiException ex)
        {
            return (false, null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, SoftwareCompilationDto dto)
    {
        try
        {
            await client.SoftwareCompilations[id].PutAsync(dto);

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

    public async Task<(bool succeeded, string error)> DeleteAsync(int id)
    {
        try
        {
            await client.SoftwareCompilations[id].DeleteAsync();

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

    public async Task<List<SoftwareReleaseDto>> GetReleasesAsync(int id)
    {
        try
        {
            List<SoftwareReleaseDto> releases = await client.SoftwareCompilations[id].Releases.GetAsync();

            return releases ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MachineDto>> GetAllMachinesAsync()
    {
        try
        {
            List<MachineDto> machines = await client.Machines.GetAsync();

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareCoverDto>> GetCoversAsync(int id)
    {
        try
        {
            List<SoftwareCoverDto> covers = await client.SoftwareCompilations[id].Covers.GetAsync();

            return covers ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ── Included software / version junction methods ──

    public async Task<List<SoftwareBySoftwareCompilationDto>> GetIncludedSoftwareAsync(int id)
    {
        try
        {
            List<SoftwareBySoftwareCompilationDto> software = await client.SoftwareCompilations[id].Software.GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> AddIncludedSoftwareAsync(SoftwareBySoftwareCompilationDto dto)
    {
        try
        {
            await client.SoftwareCompilations[(int)(dto.SoftwareCompilationId ?? 0)].Software
                       .PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveIncludedSoftwareAsync(int id, int softwareId)
    {
        try
        {
            await client.SoftwareCompilations[id].Software[softwareId].DeleteAsync();

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

    public async Task<List<SoftwareVersionBySoftwareCompilationDto>> GetIncludedVersionsAsync(int id)
    {
        try
        {
            List<SoftwareVersionBySoftwareCompilationDto> versions =
                await client.SoftwareCompilations[id].Versions.GetAsync();

            return versions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> AddIncludedVersionAsync(SoftwareVersionBySoftwareCompilationDto dto)
    {
        try
        {
            await client.SoftwareCompilations[(int)(dto.SoftwareCompilationId ?? 0)].Versions
                       .PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveIncludedVersionAsync(int id, int versionId)
    {
        try
        {
            await client.SoftwareCompilations[id].Versions[versionId].DeleteAsync();

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

    // ── Nested-compilation junction methods ──

    public async Task<List<SoftwareCompilationDto>> GetIncludedCompilationsAsync(int id)
    {
        try
        {
            List<SoftwareCompilationDto> compilations = await client.SoftwareCompilations[id].Compilations.GetAsync();

            return compilations ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> AddIncludedCompilationAsync(int id, int childId)
    {
        try
        {
            await client.SoftwareCompilations[id].Compilations[childId].PostAsync();

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

    public async Task<(bool succeeded, string error)> RemoveIncludedCompilationAsync(int id, int childId)
    {
        try
        {
            await client.SoftwareCompilations[id].Compilations[childId].DeleteAsync();

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
}
