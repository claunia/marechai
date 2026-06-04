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

public class BooksService(Marechai.ApiClient.Client client, ReferenceDataCache referenceData)
{
    public async Task<int> GetBooksCountAsync(IReadOnlyList<string> filters = null,
                                              CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Books.Count.GetAsync(config =>
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
            List<CompanyDto> companies = await client.Books.Companies.GetAsync();

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
            List<CompanyDto> companies = await client.Books.Companies.Letter[c.ToString()].GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<BookDto>> GetBooksByLetterAsync(char c, int? skip = null, int? take = null,
                                                           CancellationToken cancellationToken = default)
    {
        try
        {
            List<BookDto> books = await client.Books.ByLetter[c.ToString()].GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return books ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetBooksByLetterCountAsync(char c, CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Books.ByLetter[c.ToString()].Count.GetAsync(cancellationToken: cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<BookDto>> GetBooksByYearAsync(int year, int? skip = null, int? take = null,
                                                         CancellationToken cancellationToken = default)
    {
        try
        {
            List<BookDto> books = await client.Books.ByYear[year].GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return books ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetBooksByYearCountAsync(int year, CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Books.ByYear[year].Count.GetAsync(cancellationToken: cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<BookDto>> GetBooksAsync(int? skip = null, int? take = null,
                                                   CancellationToken cancellationToken = default)
    {
        try
        {
            List<BookDto> books = await client.Books.GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return books ?? [];
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
    ///     <c>FilterDefinition</c> shape; <c>BooksController.ApplyFilters</c>
    ///     translates them into LINQ predicates.
    /// </summary>
    public async Task<List<BookDto>> GetPagedAsync(int skip, int take, string sortBy, bool sortDescending,
                                                   IReadOnlyList<string> filters,
                                                   CancellationToken cancellationToken = default)
    {
        try
        {
            List<BookDto> books = await client.Books.GetAsync(config =>
            {
                config.QueryParameters.Skip = skip;
                config.QueryParameters.Take = take;
                if(!string.IsNullOrWhiteSpace(sortBy)) config.QueryParameters.SortBy = sortBy;
                if(sortDescending) config.QueryParameters.SortDescending = true;
                if(filters is { Count: > 0 }) config.QueryParameters.Filters = filters.ToArray();
            }, cancellationToken);

            return books ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<BookDto> GetBookAsync(long id)
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

    public async Task<DocumentSynopsisDto> GetBookSynopsisAsync(long id)
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

    /// <summary>
    ///     Fetch the full synopsis DTO so the caller can detect language fallback (the
    ///     <c>LanguageCode</c> on the returned object is the language actually served, not the
    ///     language requested). Returns <c>null</c> when no synopsis exists in any language.
    /// </summary>
    public async Task<DocumentSynopsisDto> GetSynopsisAsync(long id, string lang = "eng")
    {
        try
        {
            return await client.Books[id].Synopsis.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });
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
            List<PersonByBookDto> people = await client.Books[id].People.GetAsync();

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
            List<CompanyByBookDto> companies = await client.Books[id].Companies.GetAsync();

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
            List<BookByMachineDto> machines = await client.Books[id].Machines.GetAsync();

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
            List<BookByMachineFamilyDto> families = await client.Books[id].MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Consolidated fetch for the public /book/{Id} view page. Calls the new
    /// /books/{id}/full endpoint which returns the head + previous/source titles,
    /// the language-aware synopsis (with English fallback collapsed server-side),
    /// and all four child collections in a single HTTP round-trip. Replaces what
    /// used to be 6–8 sequential service calls.
    /// </summary>
    public async Task<BookFullDto> GetBookFullAsync(long id, string lang = "eng")
    {
        try
        {
            return await client.Books[id].Full.GetAsync(rc => rc.QueryParameters.Lang = lang);
        }
        catch
        {
            return null;
        }
    }

    // --- CRUD methods ---

    public async Task<(long? id, string error)> CreateAsync(BookDto dto)
    {
        try
        {
            long? id = await client.Books.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> UpdateAsync(long id, BookDto dto)
    {
        try
        {
            await client.Books[id].PutAsync(dto);

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

    public async Task<(bool succeeded, string error)> DeleteAsync(long id)
    {
        try
        {
            await client.Books[id].DeleteAsync();

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

    // --- Synopsis methods ---

    public async Task<List<DocumentSynopsisDto>> GetSynopsesAsync(long bookId)
    {
        try
        {
            List<DocumentSynopsisDto> synopses = await client.Books[bookId].Synopses.GetAsync();

            return synopses ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> UpsertSynopsisAsync(long bookId, DocumentSynopsisDto dto)
    {
        try
        {
            long? id = await client.Books[bookId].Synopsis.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> DeleteSynopsisAsync(long bookId, string languageCode)
    {
        try
        {
            await client.Books[bookId].Synopsis[languageCode].DeleteAsync();

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

    // --- People junction methods ---

    public async Task<(long? id, string error)> AddPersonToBookAsync(PersonByBookDto dto)
    {
        try
        {
            long? id = await client.PeopleByBook.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemovePersonFromBookAsync(long id)
    {
        try
        {
            await client.PeopleByBook[id].DeleteAsync();

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

    // --- Companies junction methods ---

    public async Task<(long? id, string error)> AddCompanyToBookAsync(CompanyByBookDto dto)
    {
        try
        {
            long? id = await client.Books.Companies.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveCompanyFromBookAsync(long id)
    {
        try
        {
            await client.Books.Companies[id].DeleteAsync();

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

    // --- Machines junction methods ---

    public async Task<(int? id, string error)> AddMachineToBookAsync(BookByMachineDto dto)
    {
        try
        {
            int? id = await client.Machines.Books.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveMachineFromBookAsync(long id)
    {
        try
        {
            await client.Machines.Books[id].DeleteAsync();

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

    // --- Machine Families junction methods ---

    public async Task<(int? id, string error)> AddMachineFamilyToBookAsync(BookByMachineFamilyDto dto)
    {
        try
        {
            int? id = await client.MachineFamilies.Books.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveMachineFamilyFromBookAsync(long id)
    {
        try
        {
            await client.MachineFamilies.Books[id].DeleteAsync();

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

    // --- Cover methods ---

    public async Task<(bool succeeded, string error)> DeleteCoverAsync(long bookId)
    {
        try
        {
            await client.Books[bookId].Cover.DeleteAsync();

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

    public async Task<BookDto> UploadCoverAsync(long bookId, Microsoft.Kiota.Abstractions.MultipartBody body)
    {
        try
        {
            return await client.Books[bookId].Cover.Upload.PostAsync(body);
        }
        catch(ProblemDetails)
        {
            return null;
        }
    }

    // --- Picker helper methods ---

    public async Task<List<DocumentRoleDto>> GetDocumentRolesAsync()
    {
        try
        {
            List<DocumentRoleDto> roles = await client.Documents.Roles.Enabled.GetAsync();

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
            List<PersonDto> people = await client.People.GetAsync();

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
            List<CompanyDto> companies = await client.Companies.GetAsync();

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
            List<MachineDto> machines = await client.Machines.GetAsync();

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public Task<List<MachineFamilyDto>> GetAllMachineFamiliesAsync() => referenceData.GetMachineFamiliesAsync();

    public Task<List<Iso31661NumericDto>> GetCountriesAsync() => referenceData.GetCountriesAsync();

    static string ExtractDetail(ApiException ex)
    {
        // Kiota maps server error responses to a typed ProblemDetails (which inherits from
        // ApiException). The base Exception.Message just returns "Exception of type 'X' was
        // thrown." — the real, user-facing text lives on Detail / Title. Surface those when
        // present, falling back to Message only if the server gave us nothing useful.
        if(ex is ProblemDetails pd)
        {
            if(!string.IsNullOrWhiteSpace(pd.Detail)) return pd.Detail;
            if(!string.IsNullOrWhiteSpace(pd.Title))  return pd.Title;
        }

        if(ex is { ResponseStatusCode: 0 } || string.IsNullOrWhiteSpace(ex.Message)) return "Unknown error";

        return ex.Message;
    }
}
