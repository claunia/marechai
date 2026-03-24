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

public class BooksService(Marechai.ApiClient.Client client)
{
    public async Task<int> GetBooksCountAsync()
    {
        try
        {
            int? count = await client.Books.Count.GetAsync();

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
            int? year = await client.Books.MinimumYear.GetAsync();

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
            int? year = await client.Books.MaximumYear.GetAsync();

            return year ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<BookDto>> GetBooksByLetterAsync(char c)
    {
        try
        {
            List<BookDto>? books = await client.Books.ByLetter[c.ToString()].GetAsync();

            return books ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<BookDto>> GetBooksByYearAsync(int year)
    {
        try
        {
            List<BookDto>? books = await client.Books.ByYear[year].GetAsync();

            return books ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<BookDto>> GetBooksAsync()
    {
        try
        {
            List<BookDto>? books = await client.Books.GetAsync();

            return books ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<BookDto?> GetBookAsync(long id)
    {
        try
        {
            return await client.Books[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<DocumentSynopsisDto?> GetBookSynopsisAsync(long id)
    {
        try
        {
            return await client.Books[id].Synopsis.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<PersonByBookDto>> GetPeopleByBookAsync(long id)
    {
        try
        {
            List<PersonByBookDto>? people = await client.Books[id].People.GetAsync();

            return people ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyByBookDto>> GetCompaniesByBookAsync(long id)
    {
        try
        {
            List<CompanyByBookDto>? companies = await client.Books[id].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<BookByMachineDto>> GetMachinesByBookAsync(long id)
    {
        try
        {
            List<BookByMachineDto>? machines = await client.Books[id].Machines.GetAsync();

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<BookByMachineFamilyDto>> GetMachineFamiliesByBookAsync(long id)
    {
        try
        {
            List<BookByMachineFamilyDto>? families = await client.Books[id].MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch
        {
            return [];
        }
    }
}
