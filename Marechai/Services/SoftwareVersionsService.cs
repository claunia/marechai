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

public class SoftwareVersionsService(Marechai.ApiClient.Client client)
{
    public async Task<List<SoftwareVersionDto>> GetAllAsync()
    {
        try
        {
            List<SoftwareVersionDto> versions = await client.Software.Versions.GetAsync();

            return versions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareVersionDto>> GetBySoftwareAsync(int softwareId)
    {
        try
        {
            List<SoftwareVersionDto> versions = await client.Software[softwareId].Versions.GetAsync();

            return versions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoftwareVersionDto> GetByIdAsync(int id)
    {
        try
        {
            return await client.Software.Versions[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(int? id, string error)> CreateAsync(SoftwareVersionDto dto)
    {
        try
        {
            int? id = await client.Software.Versions.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, SoftwareVersionDto dto)
    {
        try
        {
            await client.Software.Versions[id].PutAsync(dto);

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
            await client.Software.Versions[id].DeleteAsync();

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

    public async Task<List<CompanyBySoftwareVersionDto>> GetCompanyRolesAsync(int versionId)
    {
        try
        {
            List<CompanyBySoftwareVersionDto> companies =
                await client.Software.Versions[versionId].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(int? id, string error)> AddCompanyRoleAsync(CompanyBySoftwareVersionDto dto)
    {
        try
        {
            int? id = await client.Software.Versions.Companies.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveCompanyRoleAsync(int id)
    {
        try
        {
            await client.Software.Versions.Companies[id].DeleteAsync();

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
            List<CompanyDto> companies = await client.Companies.GetAsync();

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
            List<SoftwareRoleDto> roles = await client.Software.Roles.Enabled.GetAsync();

            return roles ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<LicenseDto>> GetAllLicensesAsync()
    {
        try
        {
            List<LicenseDto> licenses = await client.Licenses.GetAsync();

            return licenses ?? [];
        }
        catch
        {
            return [];
        }
    }
}
