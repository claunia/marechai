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

    public async Task<List<CompanyDto>> GetCompaniesAsync()
    {
        try
        {
            List<CompanyDto>? companies = await client.Books.Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyDto>> GetCompaniesByLetterAsync(char c)
    {
        try
        {
            List<CompanyDto>? companies = await client.Books.Companies.Letter[c.ToString()].GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
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

    // --- CRUD methods ---

    public async Task<(long? id, string? error)> CreateAsync(BookDto dto)
    {
        try
        {
            long? id = await client.Books.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> UpdateAsync(long id, BookDto dto)
    {
        try
        {
            await client.Books[id].PutAsync(dto);

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

    public async Task<(bool succeeded, string? error)> DeleteAsync(long id)
    {
        try
        {
            await client.Books[id].DeleteAsync();

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

    // --- Synopsis methods ---

    public async Task<List<DocumentSynopsisDto>> GetSynopsesAsync(long bookId)
    {
        try
        {
            List<DocumentSynopsisDto>? synopses = await client.Books[bookId].Synopses.GetAsync();

            return synopses ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string? error)> UpsertSynopsisAsync(long bookId, DocumentSynopsisDto dto)
    {
        try
        {
            long? id = await client.Books[bookId].Synopsis.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> DeleteSynopsisAsync(long bookId, string languageCode)
    {
        try
        {
            await client.Books[bookId].Synopsis[languageCode].DeleteAsync();

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

    // --- People junction methods ---

    public async Task<(long? id, string? error)> AddPersonToBookAsync(PersonByBookDto dto)
    {
        try
        {
            long? id = await client.PeopleByBook.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> RemovePersonFromBookAsync(long id)
    {
        try
        {
            await client.PeopleByBook[id].DeleteAsync();

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

    // --- Companies junction methods ---

    public async Task<(long? id, string? error)> AddCompanyToBookAsync(CompanyByBookDto dto)
    {
        try
        {
            long? id = await client.Books.Companies.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> RemoveCompanyFromBookAsync(long id)
    {
        try
        {
            await client.Books.Companies[id].DeleteAsync();

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

    // --- Machines junction methods ---

    public async Task<(int? id, string? error)> AddMachineToBookAsync(BookByMachineDto dto)
    {
        try
        {
            int? id = await client.Machines.Books.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> RemoveMachineFromBookAsync(long id)
    {
        try
        {
            await client.Machines.Books[id].DeleteAsync();

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

    // --- Machine Families junction methods ---

    public async Task<(int? id, string? error)> AddMachineFamilyToBookAsync(BookByMachineFamilyDto dto)
    {
        try
        {
            int? id = await client.MachineFamilies.Books.PostAsync(dto);

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

    public async Task<(bool succeeded, string? error)> RemoveMachineFamilyFromBookAsync(long id)
    {
        try
        {
            await client.MachineFamilies.Books[id].DeleteAsync();

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

    // --- Cover methods ---

    public async Task<(bool succeeded, string? error)> DeleteCoverAsync(long bookId)
    {
        try
        {
            await client.Books[bookId].Cover.DeleteAsync();

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

    public async Task<BookDto?> UploadCoverAsync(long bookId, Microsoft.Kiota.Abstractions.MultipartBody body)
    {
        try
        {
            return await client.Books[bookId].Cover.Upload.PostAsync(body);
        }
        catch(ProblemDetails ex)
        {
            return null;
        }
    }

    // --- Picker helper methods ---

    public async Task<List<DocumentRoleDto>> GetDocumentRolesAsync()
    {
        try
        {
            List<DocumentRoleDto>? roles = await client.Documents.Roles.Enabled.GetAsync();

            return roles ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<PersonDto>> GetAllPeopleAsync()
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

    public async Task<List<CompanyDto>> GetAllCompaniesAsync()
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

    public async Task<List<MachineDto>> GetAllMachinesAsync()
    {
        try
        {
            List<MachineDto>? machines = await client.Machines.GetAsync();

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MachineFamilyDto>> GetAllMachineFamiliesAsync()
    {
        try
        {
            List<MachineFamilyDto>? families = await client.MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<Iso31661NumericDto>> GetCountriesAsync()
    {
        try
        {
            List<Iso31661NumericDto>? countries = await client.Iso31661Numeric.GetAsync();

            return countries ?? [];
        }
        catch
        {
            return [];
        }
    }
}
