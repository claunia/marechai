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

public class ProcessorsService(Marechai.ApiClient.Client client)
{
    public async Task<List<ProcessorDto>> GetAllAsync()
    {
        try
        {
            List<ProcessorDto>? processors = await client.Processors.GetAsync();

            return processors ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<ProcessorDto?> GetByIdAsync(int id)
    {
        try
        {
            return await client.Processors[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(long? id, string? error)> CreateAsync(ProcessorDto dto)
    {
        try
        {
            long? id = await client.Processors.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> UpdateAsync(int id, ProcessorDto dto)
    {
        try
        {
            await client.Processors[id].PutAsync(dto);

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
            await client.Processors[id].DeleteAsync();

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

    public async Task<List<MachineDto>> GetMachinesByProcessorAsync(int processorId)
    {
        try
        {
            List<MachineDto>? machines = await client.Processors[processorId].Machines.GetAsync();

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

    public async Task<List<InstructionSetExtensionByProcessorDto>> GetExtensionsByProcessorAsync(int processorId)
    {
        try
        {
            List<InstructionSetExtensionByProcessorDto>? extensions =
                await client.Processor[processorId].InstructionSetExtensions.GetAsync();

            return extensions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string? error)> AddExtensionToProcessorAsync(
        InstructionSetExtensionByProcessorDto dto)
    {
        try
        {
            long? id = await client.InstructionSetExtensionsByProcessor.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> RemoveExtensionFromProcessorAsync(int id)
    {
        try
        {
            await client.InstructionSetExtensionsByProcessor[id].DeleteAsync();

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

    // Description management
    public async Task<string?> GetDescriptionTextAsync(int id)
    {
        try
        {
            ProcessorDescriptionDto? desc = await client.Processors[id].Description.GetAsync();

            return desc?.Html ?? desc?.Markdown;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<ProcessorDescriptionDto>> GetDescriptionsAsync(int processorId)
    {
        try
        {
            List<ProcessorDescriptionDto>? descriptions = await client.Processors[processorId].Descriptions.GetAsync();

            return descriptions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string? error)> CreateOrUpdateDescriptionAsync(int processorId,
        ProcessorDescriptionDto dto)
    {
        try
        {
            await client.Processors[processorId].Description.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> DeleteDescriptionAsync(int processorId, string languageCode)
    {
        try
        {
            await client.Processors[processorId].Description[languageCode].DeleteAsync();

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
