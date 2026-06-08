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

public class CompaniesService(Marechai.ApiClient.Client client, ReferenceDataCache referenceData, IndexNowService indexNow)
{
    public async Task<List<CompanyDto>> GetAsync()
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

    public async Task<List<CompanyDto>> GetPagedAsync(int skip, int take, string search = null,
                                                       string sortBy = null, bool sortDescending = false)
    {
        try
        {
            List<CompanyDto> companies = await client.Companies.GetAsync(config =>
            {
                config.QueryParameters.Skip           = skip;
                config.QueryParameters.Take           = take;
                config.QueryParameters.Search         = search;
                config.QueryParameters.SortBy         = sortBy;
                config.QueryParameters.SortDescending = sortDescending;
            });

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetCountAsync(string search = null)
    {
        try
        {
            int? count = await client.Companies.Count.GetAsync(config =>
            {
                config.QueryParameters.Search = search;
            });

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<CompanyDto> GetAsync(int id)
    {
        try
        {
            return await client.Companies[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(int? id, string error)> CreateAsync(CompanyDto dto)
    {
        try
        {
            int? id = await client.Companies.PostAsync(dto);

            if(id.HasValue) indexNow.EnqueueUrl($"/company/{id}");

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

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, CompanyDto dto)
    {
        try
        {
            await client.Companies[id].PutAsync(dto);

            indexNow.EnqueueUrl($"/company/{id}");

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
            await client.Companies[id].DeleteAsync();

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

    public async Task<List<MachineDto>> GetMachinesAsync(int id)
    {
        try
        {
            List<MachineDto> machines = await client.Companies[id].Machines.GetAsync();

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<string> GetDescriptionTextAsync(int id, string lang = "eng")
    {
        try
        {
            var desc = await client.Companies[id].Description.GetAsync(rc =>
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
    public async Task<CompanyDescriptionDto> GetDescriptionAsync(int id, string lang = "eng")
    {
        try
        {
            return await client.Companies[id].Description.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<CompanyDescriptionDto>> GetDescriptionsAsync(int companyId)
    {
        try
        {
            List<CompanyDescriptionDto> descriptions = await client.Companies[companyId].Descriptions.GetAsync();

            return descriptions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> CreateOrUpdateDescriptionAsync(int             companyId,
                                                                                      CompanyDescriptionDto dto)
    {
        try
        {
            await client.Companies[companyId].Description.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> DeleteDescriptionAsync(int companyId, string languageCode)
    {
        try
        {
            await client.Companies[companyId].Description[languageCode].DeleteAsync();

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

    public Task<List<Iso31661NumericDto>> GetCountriesAsync() => referenceData.GetCountriesAsync();

    public async Task<CompanyDto> GetSoldToAsync(int? id)
    {
        if(id is null) return null;

        try
        {
            return await client.Companies[id.Value].Soldto.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<string> GetCountryNameAsync(int id)
    {
        try
        {
            return await client.Countries[id].Name.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<CompanyDto>> GetCompaniesByCountryAsync(int countryId)
    {
        try
        {
            List<CompanyDto> companies = await client.Countries[countryId].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyDto>> GetCompaniesByLetterAsync(char id)
    {
        try
        {
            List<CompanyDto> companies = await client.Companies.Letter[id.ToString()].GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<GpuDto>> GetGpusAsync(int id)
    {
        try
        {
            List<GpuDto> gpus = await client.Companies[id].Gpus.GetAsync();

            return gpus ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoundSynthDto>> GetSoundSynthsAsync(int id)
    {
        try
        {
            List<SoundSynthDto> synths = await client.Companies[id].SoundSynths.GetAsync();

            return synths ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<ProcessorDto>> GetProcessorsAsync(int id)
    {
        try
        {
            List<ProcessorDto> processors = await client.Companies[id].Processors.GetAsync();

            return processors ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MachineFamilyDto>> GetMachineFamiliesAsync(int id)
    {
        try
        {
            List<MachineFamilyDto> families = await client.Companies[id].MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<BookDto>> GetBooksAsync(int id)
    {
        try
        {
            List<BookDto> books = await client.Companies[id].Books.GetAsync();

            return books ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<DocumentDto>> GetDocumentsAsync(int id)
    {
        try
        {
            List<DocumentDto> documents = await client.Companies[id].Documents.GetAsync();

            return documents ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MagazineDto>> GetMagazinesAsync(int id)
    {
        try
        {
            List<MagazineDto> magazines = await client.Companies[id].Magazines.GetAsync();

            return magazines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetSoftwareAsync(int id)
    {
        try
        {
            List<SoftwareDto> software = await client.Companies[id].Software.GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<PersonByCompanyDto>> GetPeopleAsync(int id)
    {
        try
        {
            List<PersonByCompanyDto> people = await client.Companies[id].People.GetAsync();

            return people ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<CompanyMergePreviewDto> GetMergePreviewAsync(int targetId, int sourceId)
    {
        try
        {
            return await client.Companies[targetId].MergePreview[sourceId].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool succeeded, string error)> MergeCompaniesAsync(int                     targetId,
                                                                          int                     sourceId,
                                                                          CompanyMergeRequestDto  request)
    {
        try
        {
            await client.Companies[targetId].Merge[sourceId].PostAsync(request);

            return (true, null);
        }
        catch(ApiException ex)
        {
            string detail = ex is ProblemDetails pd ? pd.Detail ?? pd.Title ?? ex.Message : ex.Message;

            return (false, detail);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
