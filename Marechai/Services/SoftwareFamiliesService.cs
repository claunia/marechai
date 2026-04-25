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

public class SoftwareFamiliesService(Marechai.ApiClient.Client client)
{
    public async Task<List<SoftwareFamilyDto>> GetAllAsync()
    {
        try
        {
            List<SoftwareFamilyDto>? families = await client.Software.Families.GetAsync();

            return families ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoftwareFamilyDto?> GetByIdAsync(int id)
    {
        try
        {
            return await client.Software.Families[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(int? id, string? error)> CreateAsync(SoftwareFamilyDto dto)
    {
        try
        {
            int? id = await client.Software.Families.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> UpdateAsync(int id, SoftwareFamilyDto dto)
    {
        try
        {
            await client.Software.Families[id].PutAsync(dto);

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
            await client.Software.Families[id].DeleteAsync();

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

    // ── Company role junction ──

    public async Task<List<CompanyBySoftwareFamilyDto>> GetCompanyRolesAsync(int familyId)
    {
        try
        {
            List<CompanyBySoftwareFamilyDto>? companies =
                await client.Software.Families[familyId].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(int? id, string? error)> AddCompanyRoleAsync(CompanyBySoftwareFamilyDto dto)
    {
        try
        {
            int? id = await client.Software.Families.Companies.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> RemoveCompanyRoleAsync(int id)
    {
        try
        {
            await client.Software.Families.Companies[id].DeleteAsync();

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

    // ── Pickers ──

    public async Task<List<CompanyDto>> GetAllCompaniesAsync()
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

    public async Task<List<SoftwareRoleDto>> GetSoftwareRolesAsync()
    {
        try
        {
            List<SoftwareRoleDto>? roles = await client.Software.Roles.Enabled.GetAsync();

            return roles ?? [];
        }
        catch
        {
            return [];
        }
    }
}
