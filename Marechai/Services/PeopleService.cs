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

using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

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

    public async Task<List<PersonDto>> GetPeopleByLetterAsync(char c)
    {
        try
        {
            List<PersonDto>? people = await client.People.ByLetter[c.ToString()].GetAsync();

            return people ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<PersonDto>> GetPeopleByYearAsync(int year)
    {
        try
        {
            List<PersonDto>? people = await client.People.ByYear[year].GetAsync();

            return people ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<PersonDto>> GetPeopleAsync()
    {
        try
        {
            List<PersonDto>? people = await client.People.GetAsync();

            return people ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<PersonDto?> GetPersonAsync(int id)
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
            List<PersonByCompanyDto>? companies = await client.People[id].Companies.GetAsync();

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
            List<PersonByBookDto>? books = await client.People[id].Books.GetAsync();

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
            List<PersonByDocumentDto>? documents = await client.People[id].Documents.GetAsync();

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
            List<PersonByMagazineDto>? magazines = await client.People[id].Magazines.GetAsync();

            return magazines ?? [];
        }
        catch
        {
            return [];
        }
    }
}
