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

public class GpusService(Marechai.ApiClient.Client client)
{
    public async Task<List<GpuDto>> GetAllAsync(int? skip = null, int? take = null,
                                                CancellationToken cancellationToken = default)
    {
        try
        {
            List<GpuDto> gpus = await client.Gpus.GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return gpus ?? [];
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
    ///     <c>FilterDefinition</c> shape; <see cref="GpusController.ApplyFilters"/>
    ///     translates them into LINQ predicates.
    /// </summary>
    public async Task<List<GpuDto>> GetPagedAsync(int skip, int take, string sortBy, bool sortDescending,
                                                  IReadOnlyList<string> filters,
                                                  CancellationToken cancellationToken = default)
    {
        try
        {
            List<GpuDto> gpus = await client.Gpus.GetAsync(config =>
            {
                config.QueryParameters.Skip = skip;
                config.QueryParameters.Take = take;
                if(!string.IsNullOrWhiteSpace(sortBy)) config.QueryParameters.SortBy = sortBy;
                if(sortDescending) config.QueryParameters.SortDescending = true;
                if(filters is { Count: > 0 }) config.QueryParameters.Filters = filters.ToArray();
            }, cancellationToken);

            return gpus ?? [];
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
            int? count = await client.Gpus.Count.GetAsync(config =>
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

    public async Task<GpuDto> GetByIdAsync(int id)
    {
        try
        {
            return await client.Gpus[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Consolidated fetch for the public /gpu/{Id} view page. Replaces six
    /// sequential HTTP round-trips (head + resolutions + machines + description +
    /// photos + videos) with a single backend call. Returns null on transport
    /// failure or a 404 from the server (the page treats null as "GPU not found").
    /// </summary>
    public async Task<GpuFullDto> GetGpuFullAsync(int id, string lang = "eng")
    {
        try
        {
            return await client.Gpus[id].Full.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<(long? id, string error)> CreateAsync(GpuDto dto)
    {
        try
        {
            long? id = await client.Gpus.PostAsync(dto);

            return (id, null);
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

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, GpuDto dto)
    {
        try
        {
            await client.Gpus[id].PutAsync(dto);

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

    public async Task<(bool succeeded, string error)> DeleteAsync(int id)
    {
        try
        {
            await client.Gpus[id].DeleteAsync();

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

    public async Task<List<ResolutionByGpuDto>> GetResolutionsByGpuAsync(int gpuId)
    {
        try
        {
            List<ResolutionByGpuDto> resolutions =
                await client.ResolutionsByGpu.Gpus[gpuId].Resolutions.GetAsync();

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
            List<ResolutionDto> resolutions = await client.Resolutions.GetAsync();

            return resolutions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> AddResolutionToGpuAsync(ResolutionByGpuDto dto)
    {
        try
        {
            long? id = await client.ResolutionsByGpu.PostAsync(dto);

            return (id, null);
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

    public async Task<(bool succeeded, string error)> RemoveResolutionFromGpuAsync(long id)
    {
        try
        {
            await client.ResolutionsByGpu[id].DeleteAsync();

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

    public async Task<ResolutionDto> GetResolutionByIdAsync(int resolutionId)
    {
        try
        {
            return await client.Resolutions[resolutionId].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<MachineDto>> GetMachinesByGpuAsync(int gpuId)
    {
        try
        {
            List<MachineDto> machines = await client.Gpus[gpuId].Machines.GetAsync();

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

    // Description management
    public async Task<string> GetDescriptionTextAsync(int id, string lang = "eng")
    {
        try
        {
            GpuDescriptionDto desc = await client.Gpus[id].Description.GetAsync(rc =>
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
    public async Task<GpuDescriptionDto> GetDescriptionAsync(int id, string lang = "eng")
    {
        try
        {
            return await client.Gpus[id].Description.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<GpuDescriptionDto>> GetDescriptionsAsync(int gpuId)
    {
        try
        {
            List<GpuDescriptionDto> descriptions = await client.Gpus[gpuId].Descriptions.GetAsync();

            return descriptions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> CreateOrUpdateDescriptionAsync(int gpuId,
        GpuDescriptionDto dto)
    {
        try
        {
            await client.Gpus[gpuId].Description.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> DeleteDescriptionAsync(int gpuId, string languageCode)
    {
        try
        {
            await client.Gpus[gpuId].Description[languageCode].DeleteAsync();

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

    // ── Videos ──

    static string ExtractErrorMessage(ApiException ex)
    {
        if(ex is ProblemDetails pd) return pd.Detail ?? pd.Title ?? ex.Message;

        return ex.Message;
    }

    public async Task<List<GpuVideoDto>> GetVideosByGpuAsync(int gpuId)
    {
        try
        {
            var result = await client.Gpus[gpuId].Videos.GetAsync();

            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(GpuVideoDto dto, string error)> CreateVideoAsync(int    gpuId, string provider,
                                                                        string videoId,
                                                                        string title)
    {
        try
        {
            var dto = await client.Gpus[gpuId]
                                  .Videos.PostAsync(new CreateGpuVideoRequest
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
            await client.Gpus.Videos[id].PutAsync(new UpdateGpuVideoRequest
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
            await client.Gpus.Videos[id].DeleteAsync();

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
