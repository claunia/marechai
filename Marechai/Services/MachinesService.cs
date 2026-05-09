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

public class MachinesService(Marechai.ApiClient.Client client)
{
    static string ExtractErrorMessage(ApiException ex)
    {
        if(ex is ProblemDetails pd) return pd.Detail ?? pd.Title ?? ex.Message;

        return ex.Message;
    }
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

    public async Task<MachinePageDto> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            return await client.Machines.Paged.GetAsync(rc =>
            {
                rc.QueryParameters.Page     = page;
                rc.QueryParameters.PageSize = pageSize;
            });
        }
        catch
        {
            return new MachinePageDto
            {
                Items      = new List<MachineDto>(),
                TotalCount = 0
            };
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

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, MachineDto dto)
    {
        try
        {
            await client.Machines[id].PutAsync(dto);

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
            await client.Machines[id].DeleteAsync();

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
            return (null, ex.Message);
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
            return (false, ex.Message);
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
            return (null, ex.Message);
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
            return (false, ex.Message);
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
            return (null, ex.Message);
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
            return (false, ex.Message);
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
            return (null, ex.Message);
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
            return (false, ex.Message);
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
            return (null, ex.Message);
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
            return (false, ex.Message);
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
            return (null, ex.Message);
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
            return (false, ex.Message);
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
            return (null, ex.Message);
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
            return (false, ex.Message);
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
            return (false, ex.Message);
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
            await client.Machines.Videos[id].PutAsync(new UpdateMachineVideoRequest
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
            await client.Machines.Videos[id].DeleteAsync();

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
