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
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

public class ProcessorsService(Marechai.ApiClient.Client client)
{
    public async Task<List<ProcessorDto>> GetAllAsync(int? skip = null, int? take = null,
                                                      CancellationToken cancellationToken = default)
    {
        try
        {
            List<ProcessorDto> processors = await client.Processors.GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return processors ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Processors.Count.GetAsync(cancellationToken: cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<ProcessorDto> GetByIdAsync(int id)
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

    /// <summary>
    /// Consolidated fetch for the public /processor/{Id} view page. Replaces five
    /// sequential HTTP round-trips (head + machines + description + photos + videos)
    /// with a single backend call. Returns null on transport failure or a 404 from
    /// the server (the page treats null as "processor not found").
    /// </summary>
    public async Task<ProcessorFullDto> GetProcessorFullAsync(int id, string lang = "eng")
    {
        try
        {
            return await client.Processors[id].Full.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<(long? id, string error)> CreateAsync(ProcessorDto dto)
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

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, ProcessorDto dto)
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

    public async Task<(bool succeeded, string error)> DeleteAsync(int id)
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
            List<MachineDto> machines = await client.Processors[processorId].Machines.GetAsync();

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
            List<CompanyDto> companies = await client.Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    ///     Returns every instruction set extension known to the server. Used by the
    ///     collaborative <c>ProcessorSuggestionDialog</c> autocomplete picker when queuing
    ///     a junction-add operation.
    /// </summary>
    public async Task<List<InstructionSetExtensionDto>> GetAllExtensionsAsync()
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

    /// <summary>
    ///     Returns every instruction set known to the server. Used by the collaborative
    ///     <c>ProcessorSuggestionDialog</c> autocomplete picker for the scalar
    ///     <c>instruction_set_id</c> field.
    /// </summary>
    public async Task<List<InstructionSetDto>> GetAllInstructionSetsAsync()
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

    public async Task<List<InstructionSetExtensionByProcessorDto>> GetExtensionsByProcessorAsync(int processorId)
    {
        try
        {
            List<InstructionSetExtensionByProcessorDto> extensions =
                await client.Processor[processorId].InstructionSetExtensions.GetAsync();

            return extensions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> AddExtensionToProcessorAsync(
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

    public async Task<(bool succeeded, string error)> RemoveExtensionFromProcessorAsync(int id)
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
    public async Task<string> GetDescriptionTextAsync(int id, string lang = "eng")
    {
        try
        {
            ProcessorDescriptionDto desc = await client.Processors[id].Description.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });

            return desc?.Html ?? desc?.Markdown;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Fetch the full description DTO so the caller can detect language fallback (the
    ///     <c>LanguageCode</c> on the returned object is the language actually served, not the
    ///     language requested). Returns <c>null</c> when no description exists in any language.
    /// </summary>
    public async Task<ProcessorDescriptionDto> GetDescriptionAsync(int id, string lang = "eng")
    {
        try
        {
            return await client.Processors[id].Description.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });
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
            List<ProcessorDescriptionDto> descriptions = await client.Processors[processorId].Descriptions.GetAsync();

            return descriptions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> CreateOrUpdateDescriptionAsync(int processorId,
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

    public async Task<(bool succeeded, string error)> DeleteDescriptionAsync(int processorId, string languageCode)
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

    // ── Videos ──

    static string ExtractErrorMessage(ApiException ex)
    {
        if(ex is ProblemDetails pd) return pd.Detail ?? pd.Title ?? ex.Message;

        return ex.Message;
    }

    public async Task<List<ProcessorVideoDto>> GetVideosByProcessorAsync(int processorId)
    {
        try
        {
            var result = await client.Processors[processorId].Videos.GetAsync();

            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(ProcessorVideoDto dto, string error)> CreateVideoAsync(int    processorId, string provider,
                                                                              string videoId,
                                                                              string title)
    {
        try
        {
            var dto = await client.Processors[processorId]
                                  .Videos.PostAsync(new CreateProcessorVideoRequest
                                                    {
                                                        Provider = provider,
                                                        VideoId  = videoId,
                                                        Title    = title
                                                    });

            return (dto, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> UpdateVideoTitleAsync(long id, string title)
    {
        try
        {
            await client.Processors.Videos[id].PutAsync(new UpdateProcessorVideoRequest
                                                        {
                                                            Title = title
                                                        });

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteVideoAsync(long id)
    {
        try
        {
            await client.Processors.Videos[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
