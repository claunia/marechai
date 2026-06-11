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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

public class MachinesService(Marechai.ApiClient.Client client, IndexNowService indexNow)
{
    public async Task<MachineDto> GetMachine(int id)
    {
        try
        {
            return await client.Machines[id].Full.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<MachineDto>> GetAllAsync()
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

    /// <summary>
    ///     Server-side paged fetch used by the admin <c>MudDataGrid</c>. Pushes the
    ///     sort + filter spec all the way down to the SQL query so paging,
    ///     filtering and ordering happen in the database. Filters are
    ///     <c>"{Column}||{Operator}||{Value}"</c> triples mirroring MudBlazor's
    ///     <c>FilterDefinition</c> shape; the controller's <c>ApplyFilters</c>
    ///     helper translates them into LINQ predicates.
    /// </summary>
    public async Task<List<MachineDto>> GetPagedAsync(int skip, int take, string sortBy, bool sortDescending,
                                                      IReadOnlyList<string> filters,
                                                      CancellationToken cancellationToken = default)
    {
        try
        {
            List<MachineDto> machines = await client.Machines.GetAsync(config =>
            {
                config.QueryParameters.Skip = skip;
                config.QueryParameters.Take = take;
                if(!string.IsNullOrWhiteSpace(sortBy)) config.QueryParameters.SortBy = sortBy;
                if(sortDescending) config.QueryParameters.SortDescending = true;
                if(filters is { Count: > 0 }) config.QueryParameters.Filters = filters.ToArray();
            }, cancellationToken);

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetCountAsync(IReadOnlyList<string> filters = null,
                                         CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Machines.Count.GetAsync(config =>
            {
                if(filters is { Count: > 0 }) config.QueryParameters.Filters = filters.ToArray();
            }, cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<MachineDto> GetByIdAsync(int id)
    {
        try
        {
            return await client.Machines[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(long? id, string error)> CreateAsync(MachineDto dto)
    {
        try
        {
            long? id = await client.Machines.PostAsync(dto);

            if(id.HasValue) indexNow.EnqueueUrl($"/machine/{id}");

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

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, MachineDto dto)
    {
        try
        {
            await client.Machines[id].PutAsync(dto);

            indexNow.EnqueueUrl($"/machine/{id}");

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
            await client.Machines[id].DeleteAsync();

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

    public async Task<List<MachineFamilyDto>> GetFamiliesAsync()
    {
        try
        {
            List<MachineFamilyDto> families = await client.MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<MachineFamilyDto> GetFamilyByIdAsync(int id)
    {
        try
        {
            return await client.MachineFamilies[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    // GPU junction management
    public async Task<List<GpuByMachineDto>> GetGpusByMachineAsync(int machineId)
    {
        try
        {
            List<GpuByMachineDto> gpus = await client.Machines.Gpus.ByMachine[machineId].GetAsync();

            return gpus ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<GpuDto>> GetAllGpusAsync()
    {
        try
        {
            List<GpuDto> gpus = await client.Gpus.GetAsync();

            return gpus ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> AddGpuToMachineAsync(GpuByMachineDto dto)
    {
        try
        {
            long? id = await client.Machines.Gpus.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveGpuFromMachineAsync(long id)
    {
        try
        {
            await client.Machines.Gpus[id].DeleteAsync();

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

    // Processor junction management
    public async Task<List<ProcessorByMachineDto>> GetProcessorsByMachineAsync(int machineId)
    {
        try
        {
            List<ProcessorByMachineDto> processors = await client.ProcessorsByMachine.ByMachine[machineId].GetAsync();

            return processors ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<ProcessorDto>> GetAllProcessorsAsync()
    {
        try
        {
            List<ProcessorDto> processors = await client.Processors.GetAsync();

            return processors ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> AddProcessorToMachineAsync(ProcessorByMachineDto dto)
    {
        try
        {
            long? id = await client.ProcessorsByMachine.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveProcessorFromMachineAsync(long id)
    {
        try
        {
            await client.ProcessorsByMachine[id].DeleteAsync();

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

    // Sound synth junction management
    public async Task<List<SoundSynthByMachineDto>> GetSoundSynthsByMachineAsync(int machineId)
    {
        try
        {
            List<SoundSynthByMachineDto> synths = await client.SoundSynthsByMachine.ByMachine[machineId].GetAsync();

            return synths ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoundSynthDto>> GetAllSoundSynthsAsync()
    {
        try
        {
            List<SoundSynthDto> synths = await client.SoundSynths.GetAsync();

            return synths ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> AddSoundSynthToMachineAsync(SoundSynthByMachineDto dto)
    {
        try
        {
            long? id = await client.SoundSynthsByMachine.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveSoundSynthFromMachineAsync(long id)
    {
        try
        {
            await client.SoundSynthsByMachine[id].DeleteAsync();

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

    // Screen junction management
    public async Task<List<ScreenByMachineDto>> GetScreensByMachineAsync(int machineId)
    {
        try
        {
            List<ScreenByMachineDto> screens = await client.Machines[machineId].Screens.GetAsync();

            return screens ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<ScreenDto>> GetAllScreensAsync()
    {
        try
        {
            List<ScreenDto> screens = await client.Screens.GetAsync();

            return screens ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> AddScreenToMachineAsync(ScreenByMachineDto dto)
    {
        try
        {
            long? id = await client.ScreensByMachine.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveScreenFromMachineAsync(long id)
    {
        try
        {
            await client.ScreensByMachine[id].DeleteAsync();

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

    // Memory junction management
    public async Task<List<MemoryByMachineDto>> GetMemoryByMachineAsync(int machineId)
    {
        try
        {
            List<MemoryByMachineDto> memory = await client.Machines[machineId].Memories.GetAsync();

            return memory ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> AddMemoryToMachineAsync(MemoryByMachineDto dto)
    {
        try
        {
            long? id = await client.MemoriesByMachine.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveMemoryFromMachineAsync(long id)
    {
        try
        {
            await client.MemoriesByMachine[id].DeleteAsync();

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

    // Storage junction management
    public async Task<List<StorageByMachineDto>> GetStorageByMachineAsync(int machineId)
    {
        try
        {
            List<StorageByMachineDto> storage = await client.Machines[machineId].Storage.GetAsync();

            return storage ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> AddStorageToMachineAsync(StorageByMachineDto dto)
    {
        try
        {
            long? id = await client.StorageByMachine.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveStorageFromMachineAsync(long id)
    {
        try
        {
            await client.StorageByMachine[id].DeleteAsync();

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

    public async Task<List<SoftwareDto>> GetSoftwareByMachineAsync(int machineId)
    {
        try
        {
            List<SoftwareDto> software = await client.Machines[machineId].Software.GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetSoftwareByMachinePagedAsync(int machineId, int skip, int take,
                                                                         string search = null,
                                                                         string sortBy = null,
                                                                         bool sortDescending = false,
                                                                         CancellationToken cancellationToken = default)
    {
        try
        {
            List<SoftwareDto> software = await client.Machines[machineId].Software.GetAsync(config =>
            {
                config.QueryParameters.Skip           = skip;
                config.QueryParameters.Take           = take;
                if(!string.IsNullOrWhiteSpace(search)) config.QueryParameters.Search = search;
                if(!string.IsNullOrWhiteSpace(sortBy)) config.QueryParameters.SortBy = sortBy;
                if(sortDescending)                     config.QueryParameters.SortDescending = true;
            }, cancellationToken);

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetSoftwareByMachineCountAsync(int machineId, string search = null,
                                                           CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Machines[machineId].Software.Count.GetAsync(config =>
            {
                if(!string.IsNullOrWhiteSpace(search)) config.QueryParameters.Search = search;
            }, cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    // Software Platform junction management
    public async Task<List<SoftwarePlatformByMachineDto>> GetSoftwarePlatformsByMachineAsync(int machineId)
    {
        try
        {
            List<SoftwarePlatformByMachineDto> platforms =
                await client.SoftwarePlatformsByMachine.ByMachine[machineId].GetAsync();

            return platforms ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwarePlatformDto>> GetAllSoftwarePlatformsAsync()
    {
        try
        {
            List<SoftwarePlatformDto> platforms = await client.Software.Platforms.GetAsync();

            return platforms ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> AddSoftwarePlatformToMachineAsync(SoftwarePlatformByMachineDto dto)
    {
        try
        {
            long? id = await client.SoftwarePlatformsByMachine.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveSoftwarePlatformFromMachineAsync(long id)
    {
        try
        {
            await client.SoftwarePlatformsByMachine[id].DeleteAsync();

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

    // Description management
    public async Task<string> GetDescriptionTextAsync(int id, string lang = "eng")
    {
        try
        {
            MachineDescriptionDto desc = await client.Machines[id].Description.GetAsync(rc =>
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
    public async Task<MachineDescriptionDto> GetDescriptionAsync(int id, string lang = "eng")
    {
        try
        {
            return await client.Machines[id].Description.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<MachineDescriptionDto>> GetDescriptionsAsync(int machineId)
    {
        try
        {
            List<MachineDescriptionDto> descriptions = await client.Machines[machineId].Descriptions.GetAsync();

            return descriptions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> CreateOrUpdateDescriptionAsync(int machineId,
        MachineDescriptionDto dto)
    {
        try
        {
            await client.Machines[machineId].Description.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> DeleteDescriptionAsync(int machineId, string languageCode)
    {
        try
        {
            await client.Machines[machineId].Description[languageCode].DeleteAsync();

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

    // ── Videos ──

    public async Task<List<MachineVideoDto>> GetVideosByMachineAsync(int machineId)
    {
        try
        {
            var result = await client.Machines[machineId].Videos.GetAsync();

            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(MachineVideoDto dto, string error)> CreateVideoAsync(int    machineId, string provider,
                                                                            string videoId,
                                                                            string title)
    {
        try
        {
            var dto = await client.Machines[machineId]
                                  .Videos.PostAsync(new CreateMachineVideoRequest
                                                    {
                                                        Provider = provider,
                                                        VideoId  = videoId,
                                                        Title    = title
                                                    });

            return (dto, null);
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

    public async Task<(bool succeeded, string error)> UpdateVideoTitleAsync(long id, string title)
    {
        try
        {
            await client.Machines.Videos[id].PutAsync(new UpdateMachineVideoRequest
                                                      {
                                                          Title = title
                                                      });

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

    public async Task<(bool succeeded, string error)> DeleteVideoAsync(long id)
    {
        try
        {
            await client.Machines.Videos[id].DeleteAsync();

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

    static string ExtractDetail(ApiException ex)
    {
        if(ex is ProblemDetails pd)
        {
            if(!string.IsNullOrWhiteSpace(pd.Detail)) return pd.Detail;
            if(!string.IsNullOrWhiteSpace(pd.Title))  return pd.Title;
        }

        if(ex is { ResponseStatusCode: 0 } || string.IsNullOrWhiteSpace(ex.Message)) return "Unknown error";

        return ex.Message;
    }
}
