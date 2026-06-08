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
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

public class MagazinesService(Marechai.ApiClient.Client client, ReferenceDataCache referenceData, IndexNowService indexNow)
{
    public async Task<int> GetMagazinesCountAsync(IReadOnlyList<string> filters = null,
                                                  CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Magazines.Count.GetAsync(config =>
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
            int? year = await client.Magazines.MinimumYear.GetAsync();

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
            int? year = await client.Magazines.MaximumYear.GetAsync();

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
            List<CompanyDto> companies = await client.Magazines.Companies.GetAsync();

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
            List<CompanyDto> companies = await client.Magazines.Companies.Letter[c.ToString()].GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MagazineDto>> GetMagazinesByLetterAsync(char c, int? skip = null, int? take = null,
                                                                   CancellationToken cancellationToken = default)
    {
        try
        {
            List<MagazineDto> magazines = await client.Magazines.ByLetter[c.ToString()].GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return magazines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetMagazinesByLetterCountAsync(char c, CancellationToken cancellationToken = default)
    {
        try
        {
            int? count =
                await client.Magazines.ByLetter[c.ToString()].Count.GetAsync(cancellationToken: cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<MagazineDto>> GetMagazinesByYearAsync(int year, int? skip = null, int? take = null,
                                                                 CancellationToken cancellationToken = default)
    {
        try
        {
            List<MagazineDto> magazines = await client.Magazines.ByYear[year].GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return magazines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetMagazinesByYearCountAsync(int year, CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Magazines.ByYear[year].Count.GetAsync(cancellationToken: cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<MagazineDto>> GetMagazinesAsync(int? skip = null, int? take = null,
                                                           CancellationToken cancellationToken = default)
    {
        try
        {
            List<MagazineDto> magazines = await client.Magazines.GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return magazines ?? [];
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
    ///     <c>FilterDefinition</c> shape; <c>MagazinesController.ApplyFilters</c>
    ///     translates them into LINQ predicates.
    /// </summary>
    public async Task<List<MagazineDto>> GetMagazinesPagedAsync(int skip, int take, string sortBy, bool sortDescending,
                                                                IReadOnlyList<string> filters,
                                                                CancellationToken cancellationToken = default)
    {
        try
        {
            List<MagazineDto> magazines = await client.Magazines.GetAsync(config =>
            {
                config.QueryParameters.Skip = skip;
                config.QueryParameters.Take = take;
                if(!string.IsNullOrWhiteSpace(sortBy)) config.QueryParameters.SortBy = sortBy;
                if(sortDescending) config.QueryParameters.SortDescending = true;
                if(filters is { Count: > 0 }) config.QueryParameters.Filters = filters.ToArray();
            }, cancellationToken);

            return magazines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<MagazineDto> GetMagazineAsync(long id)
    {
        try
        {
            return await client.Magazines[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<DocumentSynopsisDto> GetMagazineSynopsisAsync(long id)
    {
        try
        {
            return await client.Magazines[id].Synopsis.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Fetches the synopsis for the given magazine in the requested language. Returns the
    ///     full DTO so callers can compare the served <c>LanguageCode</c> against the requested
    ///     code to detect a fallback (English when the requested language is missing).
    /// </summary>
    public async Task<DocumentSynopsisDto> GetSynopsisAsync(long id, string lang = "eng")
    {
        try
        {
            return await client.Magazines[id].Synopsis.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<PersonByMagazineDto>> GetPeopleByMagazineAsync(long id)
    {
        try
        {
            List<PersonByMagazineDto> people = await client.Magazines[id].People.GetAsync();

            return people ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyByMagazineDto>> GetCompaniesByMagazineAsync(long id)
    {
        try
        {
            List<CompanyByMagazineDto> companies = await client.Magazines[id].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MagazineByMachineDto>> GetMachinesByMagazineAsync(long id)
    {
        try
        {
            List<MagazineByMachineDto> machines = await client.Magazines[id].Machines.GetAsync();

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MagazineByMachineFamilyDto>> GetMachineFamiliesByMagazineAsync(long id)
    {
        try
        {
            List<MagazineByMachineFamilyDto> families = await client.Magazines[id].MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MagazineBySoftwareDto>> GetSoftwareByMagazineAsync(long id)
    {
        try
        {
            List<MagazineBySoftwareDto> software = await client.Magazines[id].Software.GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    // --- CRUD methods ---

    public async Task<(long? id, string error)> CreateAsync(MagazineDto dto)
    {
        try
        {
            long? id = await client.Magazines.PostAsync(dto);

            if(id.HasValue) indexNow.EnqueueUrl($"/magazine/{id}");

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

    public async Task<(bool succeeded, string error)> UpdateAsync(long id, MagazineDto dto)
    {
        try
        {
            await client.Magazines[id].PutAsync(dto);

            indexNow.EnqueueUrl($"/magazine/{id}");

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
            await client.Magazines[id].DeleteAsync();

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

    // --- Magazine Issue methods ---

    public async Task<List<MagazineIssueDto>> GetIssuesByMagazineAsync(long magazineId)
    {
        try
        {
            List<MagazineIssueDto> issues = await client.Magazines[magazineId].Issues.GetAsync();

            return issues ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<MagazineIssueDto> GetIssueByIdAsync(long id)
    {
        try
        {
            return await client.Magazines.Issues[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Loads the consolidated payload for the public /magazine/issue/{id} view page in a
    /// single round-trip (head + magazine title + machines + machine families + software).
    /// </summary>
    public async Task<MagazineIssueFullDto> GetIssueFullAsync(long id)
    {
        try
        {
            return await client.Magazines.Issues[id].Full.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Distinct list of publication years for the magazine's issues, descending. A trailing
    /// <c>null</c> entry means the magazine has issues with no publication date (the "Others"
    /// pill bucket on the magazine view page).
    /// </summary>
    public async Task<List<int?>> GetIssueYearsAsync(long magazineId)
    {
        try
        {
            List<int?> years = await client.Magazines[magazineId].IssueYears.GetAsync();

            return years ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MagazineIssueDto>> GetIssuesByYearAsync(long magazineId, int year)
    {
        try
        {
            List<MagazineIssueDto> issues = await client.Magazines[magazineId].Issues.ByYear[year].GetAsync();

            return issues ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MagazineIssueDto>> GetIssuesNoYearAsync(long magazineId)
    {
        try
        {
            List<MagazineIssueDto> issues = await client.Magazines[magazineId].Issues.NoYear.GetAsync();

            return issues ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> CreateIssueAsync(MagazineIssueDto dto)
    {
        try
        {
            long? id = await client.Magazines.Issues.PostAsync(dto);

            return (id, null);
        }
        catch(ProblemDetails ex)
        {
            return (null, FormatProblem(ex, "Failed to create the issue."));
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

    public async Task<(bool succeeded, string error)> UpdateIssueAsync(long id, MagazineIssueDto dto)
    {
        try
        {
            await client.Magazines.Issues[id].PutAsync(dto);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            return (false, FormatProblem(ex, "Failed to update the issue."));
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

    public async Task<(bool succeeded, string error)> DeleteIssueAsync(long id)
    {
        try
        {
            await client.Magazines.Issues[id].DeleteAsync();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            return (false, FormatProblem(ex, "Failed to delete the issue."));
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

    // --- Magazine Issue Cover methods ---

    public async Task<(bool succeeded, string error)> DeleteIssueCoverAsync(long issueId)
    {
        try
        {
            await client.Magazines.Issues[issueId].Cover.DeleteAsync();

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

    public async Task<MagazineIssueDto> UploadIssueCoverAsync(long issueId, MultipartBody body)
    {
        try
        {
            return await client.Magazines.Issues[issueId].Cover.Upload.PostAsync(body);
        }
        catch(ProblemDetails)
        {
            return null;
        }
    }

    // --- Synopsis methods ---

    public async Task<List<DocumentSynopsisDto>> GetSynopsesAsync(long magazineId)
    {
        try
        {
            List<DocumentSynopsisDto> synopses = await client.Magazines[magazineId].Synopses.GetAsync();

            return synopses ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> UpsertSynopsisAsync(long magazineId, DocumentSynopsisDto dto)
    {
        try
        {
            long? id = await client.Magazines[magazineId].Synopsis.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> DeleteSynopsisAsync(long magazineId, string languageCode)
    {
        try
        {
            await client.Magazines[magazineId].Synopsis[languageCode].DeleteAsync();

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

    public async Task<(long? id, string error)> AddPersonToMagazineAsync(PersonByMagazineDto dto)
    {
        try
        {
            long? id = await client.PeopleByMagazine.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemovePersonFromMagazineAsync(long id)
    {
        try
        {
            await client.PeopleByMagazine[id].DeleteAsync();

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

    public async Task<(long? id, string error)> AddCompanyToMagazineAsync(CompanyByMagazineDto dto)
    {
        try
        {
            long? id = await client.Magazines.Companies.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveCompanyFromMagazineAsync(long id)
    {
        try
        {
            await client.Magazines.Companies[id].DeleteAsync();

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

    public async Task<(long? id, string error)> AddMachineToMagazineAsync(MagazineByMachineDto dto)
    {
        try
        {
            long? id = await client.MagazinesByMachine.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveMachineFromMagazineAsync(long id)
    {
        try
        {
            await client.MagazinesByMachine[id].DeleteAsync();

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

    public async Task<(long? id, string error)> AddMachineFamilyToMagazineAsync(MagazineByMachineFamilyDto dto)
    {
        try
        {
            long? id = await client.MagazinesByMachineFamily.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveMachineFamilyFromMagazineAsync(long id)
    {
        try
        {
            await client.MagazinesByMachineFamily[id].DeleteAsync();

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

    // --- Software junction methods ---

    public async Task<(long? id, string error)> AddSoftwareToMagazineAsync(MagazineBySoftwareDto dto)
    {
        try
        {
            long? id = await client.MagazinesBySoftware.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveSoftwareFromMagazineAsync(long id)
    {
        try
        {
            await client.MagazinesBySoftware[id].DeleteAsync();

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

    public async Task<List<SoftwareDto>> GetAllSoftwareAsync()
    {
        try
        {
            List<SoftwareDto> software = await client.Software.GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

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

    /// <summary>
    /// Renders a Kiota-thrown <see cref="ProblemDetails"/> into a human-readable string for
    /// surfacing in toasts / inline error messages. The Kiota-generated <c>ProblemDetails</c>
    /// extends <see cref="Microsoft.Kiota.Abstractions.ApiException"/> but does NOT override
    /// <see cref="Exception.Message"/>, so calling <c>ex.Message</c> yields the useless
    /// <c>"Exception of type 'Marechai.ApiClient.Models.ProblemDetails' was thrown."</c>.
    /// Validation errors specifically arrive as <c>ValidationProblemDetails</c> (RFC 9457
    /// shape) with the per-field messages under <c>AdditionalData["errors"]</c>; we expand
    /// those into a single multi-line string so the user can see exactly which field failed.
    /// </summary>
    static string FormatProblem(ProblemDetails ex, string fallback)
    {
        var sb = new StringBuilder();

        if(!string.IsNullOrWhiteSpace(ex.Detail))
            sb.Append(ex.Detail);
        else if(!string.IsNullOrWhiteSpace(ex.Title))
            sb.Append(ex.Title);
        else
            sb.Append(fallback);

        if(ex.AdditionalData != null && ex.AdditionalData.TryGetValue("errors", out object errorsObj) &&
           errorsObj is JsonElement { ValueKind: JsonValueKind.Object } errorsJson)
        {
            foreach(JsonProperty fieldEntry in errorsJson.EnumerateObject())
            {
                if(fieldEntry.Value.ValueKind != JsonValueKind.Array) continue;

                IEnumerable<string> messages = fieldEntry.Value.EnumerateArray()
                                                         .Where(e => e.ValueKind == JsonValueKind.String)
                                                         .Select(e => e.GetString());

                foreach(string msg in messages) sb.Append(' ').Append(fieldEntry.Name).Append(": ").Append(msg);
            }
        }

        return sb.ToString();
    }
}
