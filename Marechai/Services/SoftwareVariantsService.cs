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

public class SoftwareVariantsService(Marechai.ApiClient.Client client)
{
    public async Task<List<SoftwareVariantDto>> GetAllAsync()
    {
        try
        {
            List<SoftwareVariantDto>? variants = await client.Software.Variants.GetAsync();

            return variants ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareVariantDto>> GetBySoftwareAsync(int softwareId)
    {
        try
        {
            List<SoftwareVariantDto>? variants = await client.Software[softwareId].Variants.GetAsync();

            return variants ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoftwareVariantDto?> GetByIdAsync(int id)
    {
        try
        {
            return await client.Software.Variants[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(int? id, string? error)> CreateAsync(SoftwareVariantDto dto)
    {
        try
        {
            int? id = await client.Software.Variants.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> UpdateAsync(int id, SoftwareVariantDto dto)
    {
        try
        {
            await client.Software.Variants[id].PutAsync(dto);

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
            await client.Software.Variants[id].DeleteAsync();

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

    // ── Language junction ──

    public async Task<List<SoftwareVariantLanguageDto>> GetLanguagesAsync(int variantId)
    {
        try
        {
            List<SoftwareVariantLanguageDto>? languages =
                await client.Software.Variants[variantId].Languages.GetAsync();

            return languages ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string? error)> AddLanguageAsync(SoftwareVariantLanguageDto dto)
    {
        try
        {
            await client.Software.VariantLanguages.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> RemoveLanguageAsync(int variantId, string languageCode)
    {
        try
        {
            await client.Software.VariantLanguages[variantId][languageCode].DeleteAsync();

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

    public async Task<List<CompanyBySoftwareVariantDto>> GetCompanyRolesAsync(int variantId)
    {
        try
        {
            List<CompanyBySoftwareVariantDto>? companies =
                await client.Software.Variants[variantId].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(int? id, string? error)> AddCompanyRoleAsync(CompanyBySoftwareVariantDto dto)
    {
        try
        {
            int? id = await client.Software.Variants.Companies.PostAsync(dto);

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
            await client.Software.Variants.Companies[id].DeleteAsync();

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
