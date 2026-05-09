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

public class PeopleService(Marechai.ApiClient.Client client)
{
    public async Task<int> GetPeopleCountAsync()
    {
        try
        {
            int? count = await client.People.Count.GetAsync();

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> GetMinimumYearAsync()
    {
        try
        {
            int? year = await client.People.MinimumYear.GetAsync();

            return year ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> GetMaximumYearAsync()
    {
        try
        {
            int? year = await client.People.MaximumYear.GetAsync();

            return year ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<PersonDto>> GetPeopleByLetterAsync(char c, int? skip = null, int? take = null,
                                                              CancellationToken cancellationToken = default)
    {
        try
        {
            List<PersonDto> people = await client.People.ByLetter[c.ToString()].GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return people ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetPeopleByLetterCountAsync(char c, CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.People.ByLetter[c.ToString()].Count.GetAsync(cancellationToken: cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<PersonDto>> GetPeopleByYearAsync(int year, int? skip = null, int? take = null,
                                                            CancellationToken cancellationToken = default)
    {
        try
        {
            List<PersonDto> people = await client.People.ByYear[year].GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return people ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetPeopleByYearCountAsync(int year, CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.People.ByYear[year].Count.GetAsync(cancellationToken: cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<PersonDto>> GetPeopleAsync(int? skip = null, int? take = null,
                                                      CancellationToken cancellationToken = default)
    {
        try
        {
            List<PersonDto> people = await client.People.GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return people ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<PersonDto> GetPersonAsync(int id)
    {
        try
        {
            return await client.People[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<PersonByCompanyDto>> GetCompaniesByPersonAsync(int id)
    {
        try
        {
            List<PersonByCompanyDto> companies = await client.People[id].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<PersonByBookDto>> GetBooksByPersonAsync(int id)
    {
        try
        {
            List<PersonByBookDto> books = await client.People[id].Books.GetAsync();

            return books ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<PersonByDocumentDto>> GetDocumentsByPersonAsync(int id)
    {
        try
        {
            List<PersonByDocumentDto> documents = await client.People[id].Documents.GetAsync();

            return documents ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<PersonByMagazineDto>> GetMagazinesByPersonAsync(int id)
    {
        try
        {
            List<PersonByMagazineDto> magazines = await client.People[id].Magazines.GetAsync();

            return magazines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<PersonBySoftwareDto>> GetSoftwareByPersonAsync(int id)
    {
        try
        {
            List<PersonBySoftwareDto> software = await client.People[id].Software.GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Wrapper for the consolidated /people/{id}/full endpoint. Returns the
    /// person head plus all five child collections (companies, books, documents,
    /// magazines, software credits) in a single HTTP round-trip. Replaces what
    /// used to be 6 sequential service calls in the public /person/{Id} view.
    /// </summary>
    public async Task<PersonFullDto> GetPersonFullAsync(int id, string lang = "eng")
    {
        try
        {
            return await client.People[id].Full.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<(long? id, string error)> CreateAsync(PersonDto dto)
    {
        try
        {
            long? id = await client.People.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, PersonDto dto)
    {
        try
        {
            await client.People[id].PutAsync(dto);

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
            await client.People[id].DeleteAsync();

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

    public async Task<List<Iso31661NumericDto>> GetCountriesAsync()
    {
        try
        {
            List<Iso31661NumericDto> countries = await client.Iso31661Numeric.GetAsync();

            return countries ?? [];
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
            var desc = await client.People[id].Description.GetAsync(rc =>
            {
                rc.QueryParameters.Lang = lang;
            });

            return desc?.Html ?? desc?.Markdown;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<PersonDescriptionDto>> GetDescriptionsAsync(int personId)
    {
        try
        {
            List<PersonDescriptionDto> descriptions = await client.People[personId].Descriptions.GetAsync();

            return descriptions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> CreateOrUpdateDescriptionAsync(int                  personId,
                                                                                     PersonDescriptionDto dto)
    {
        try
        {
            await client.People[personId].Description.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> DeleteDescriptionAsync(int personId, string languageCode)
    {
        try
        {
            await client.People[personId].Description[languageCode].DeleteAsync();

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
