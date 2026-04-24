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

public class ScreensService(Marechai.ApiClient.Client client)
{
    public async Task<List<ScreenDto>> GetAllAsync()
    {
        try
        {
            List<ScreenDto>? screens = await client.Screens.GetAsync();

            return screens ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<ScreenDto?> GetByIdAsync(int id)
    {
        try
        {
            return await client.Screens[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(long? id, string? error)> CreateAsync(ScreenDto dto)
    {
        try
        {
            long? id = await client.Screens.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> UpdateAsync(int id, ScreenDto dto)
    {
        try
        {
            await client.Screens[id].PutAsync(dto);

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
            await client.Screens[id].DeleteAsync();

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

    public async Task<List<ResolutionByScreenDto>> GetResolutionsByScreenAsync(int screenId)
    {
        try
        {
            List<ResolutionByScreenDto>? resolutions = await client.Screens[screenId].Resolutions.GetAsync();

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

    public async Task<(long? id, string? error)> AddResolutionToScreenAsync(ResolutionByScreenDto dto)
    {
        try
        {
            long? id = await client.ResolutionsByScreen.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> RemoveResolutionFromScreenAsync(long id)
    {
        try
        {
            await client.ResolutionsByScreen[id].DeleteAsync();

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
