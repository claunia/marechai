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

public class InstructionSetExtensionsService(Marechai.ApiClient.Client client)
{
    public async Task<List<InstructionSetExtensionDto>> GetAllAsync()
    {
        try
        {
            List<InstructionSetExtensionDto> extensions = await client.InstructionSetExtensions.GetAsync();

            return extensions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<InstructionSetExtensionDto> GetByIdAsync(int id)
    {
        try
        {
            return await client.InstructionSetExtensions[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(int? id, string error)> CreateAsync(InstructionSetExtensionDto dto)
    {
        try
        {
            int? id = await client.InstructionSetExtensions.PostAsync(dto);

            return (id, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, InstructionSetExtensionDto dto)
    {
        try
        {
            await client.InstructionSetExtensions[id].PutAsync(dto);

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
            await client.InstructionSetExtensions[id].DeleteAsync();

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

    public async Task<bool> VerifyUniqueAsync(string extension)
    {
        try
        {
            bool? isUnique = await client.InstructionSetExtensions.VerifyUnique[extension].GetAsync();

            return isUnique ?? false;
        }
        catch
        {
            return false;
        }
    }

    static string ExtractDetail(ApiException ex)
    {
        // Kiota maps server error responses to a typed ProblemDetails (which inherits from
        // ApiException). The base Exception.Message just returns "Exception of type 'X' was
        // thrown." — the real, user-facing text lives on Detail / Title. Surface those when
        // present, falling back to Message only if the server gave us nothing useful.
        if(ex is ProblemDetails pd)
        {
            if(!string.IsNullOrWhiteSpace(pd.Detail)) return pd.Detail;
            if(!string.IsNullOrWhiteSpace(pd.Title))  return pd.Title;
        }

        if(ex is { ResponseStatusCode: 0 } || string.IsNullOrWhiteSpace(ex.Message)) return "Unknown error";

        return ex.Message;
    }
}
