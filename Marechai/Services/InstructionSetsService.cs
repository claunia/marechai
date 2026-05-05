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

public class InstructionSetsService(Marechai.ApiClient.Client client)
{
    public async Task<List<InstructionSetDto>> GetAllAsync()
    {
        try
        {
            List<InstructionSetDto> sets = await client.InstructionSets.GetAsync();

            return sets ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<InstructionSetDto> GetByIdAsync(int id)
    {
        try
        {
            return await client.InstructionSets[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(int? id, string error)> CreateAsync(InstructionSetDto dto)
    {
        try
        {
            int? id = await client.InstructionSets.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, InstructionSetDto dto)
    {
        try
        {
            await client.InstructionSets[id].PutAsync(dto);

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
            await client.InstructionSets[id].DeleteAsync();

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

    public async Task<bool> VerifyUniqueAsync(string name)
    {
        try
        {
            bool? isUnique = await client.InstructionSets.VerifyUnique[name].GetAsync();

            return isUnique ?? false;
        }
        catch
        {
            return false;
        }
    }
}
