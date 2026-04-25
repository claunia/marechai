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

public class SoftwareSubvariantsService(Marechai.ApiClient.Client client)
{
    public async Task<List<SoftwareSubvariantDto>> GetByVariantAsync(int variantId)
    {
        try
        {
            List<SoftwareSubvariantDto>? subvariants =
                await client.Software.Variants[variantId].Subvariants.GetAsync();

            return subvariants ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoftwareSubvariantDto?> GetByIdAsync(int id)
    {
        try
        {
            return await client.Software.Subvariants[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(int? id, string? error)> CreateAsync(SoftwareSubvariantDto dto)
    {
        try
        {
            int? id = await client.Software.Subvariants.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> UpdateAsync(int id, SoftwareSubvariantDto dto)
    {
        try
        {
            await client.Software.Subvariants[id].PutAsync(dto);

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
            await client.Software.Subvariants[id].DeleteAsync();

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

    public async Task<List<SoftwareSubvariantLanguageDto>> GetLanguagesAsync(int subvariantId)
    {
        try
        {
            List<SoftwareSubvariantLanguageDto>? languages =
                await client.Software.Subvariants[subvariantId].Languages.GetAsync();

            return languages ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string? error)> AddLanguageAsync(SoftwareSubvariantLanguageDto dto)
    {
        try
        {
            await client.Software.SubvariantLanguages.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> RemoveLanguageAsync(int subvariantId, string languageCode)
    {
        try
        {
            await client.Software.SubvariantLanguages[subvariantId][languageCode].DeleteAsync();

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
}
