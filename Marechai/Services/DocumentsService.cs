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

public class DocumentsService(Marechai.ApiClient.Client client)
{
    public async Task<int> GetDocumentsCountAsync()
    {
        try
        {
            int? count = await client.Documents.Count.GetAsync();

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
            int? year = await client.Documents.MinimumYear.GetAsync();

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
            int? year = await client.Documents.MaximumYear.GetAsync();

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
            List<CompanyDto> companies = await client.Documents.Companies.GetAsync();

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
            List<CompanyDto> companies = await client.Documents.Companies.Letter[c.ToString()].GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<DocumentDto>> GetDocumentsByLetterAsync(char c)
    {
        try
        {
            List<DocumentDto> documents = await client.Documents.ByLetter[c.ToString()].GetAsync();

            return documents ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<DocumentDto>> GetDocumentsByYearAsync(int year)
    {
        try
        {
            List<DocumentDto> documents = await client.Documents.ByYear[year].GetAsync();

            return documents ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<DocumentDto>> GetDocumentsAsync()
    {
        try
        {
            List<DocumentDto> documents = await client.Documents.GetAsync();

            return documents ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<DocumentDto> GetDocumentAsync(long id)
    {
        try
        {
            return await client.Documents[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<DocumentSynopsisDto> GetDocumentSynopsisAsync(long id)
    {
        try
        {
            return await client.Documents[id].Synopsis.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<PersonByDocumentDto>> GetPeopleByDocumentAsync(long id)
    {
        try
        {
            List<PersonByDocumentDto> people = await client.Documents[id].People.GetAsync();

            return people ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyByDocumentDto>> GetCompaniesByDocumentAsync(long id)
    {
        try
        {
            List<CompanyByDocumentDto> companies = await client.Documents[id].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<DocumentByMachineDto>> GetMachinesByDocumentAsync(long id)
    {
        try
        {
            List<DocumentByMachineDto> machines = await client.Documents[id].Machines.GetAsync();

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<DocumentByMachineFamilyDto>> GetMachineFamiliesByDocumentAsync(long id)
    {
        try
        {
            List<DocumentByMachineFamilyDto> families = await client.Documents[id].MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch
        {
            return [];
        }
    }

    // --- CRUD methods ---

    public async Task<(long? id, string error)> CreateAsync(DocumentDto dto)
    {
        try
        {
            long? id = await client.Documents.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> UpdateAsync(long id, DocumentDto dto)
    {
        try
        {
            await client.Documents[id].PutAsync(dto);

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

    public async Task<(bool succeeded, string error)> DeleteAsync(long id)
    {
        try
        {
            await client.Documents[id].DeleteAsync();

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

    public async Task<List<DocumentSynopsisDto>> GetSynopsesAsync(long documentId)
    {
        try
        {
            List<DocumentSynopsisDto> synopses = await client.Documents[documentId].Synopses.GetAsync();

            return synopses ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(long? id, string error)> UpsertSynopsisAsync(long documentId, DocumentSynopsisDto dto)
    {
        try
        {
            long? id = await client.Documents[documentId].Synopsis.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> DeleteSynopsisAsync(long documentId, string languageCode)
    {
        try
        {
            await client.Documents[documentId].Synopsis[languageCode].DeleteAsync();

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

    public async Task<(long? id, string error)> AddPersonToDocumentAsync(PersonByDocumentDto dto)
    {
        try
        {
            long? id = await client.PeopleByDocument.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemovePersonFromDocumentAsync(long id)
    {
        try
        {
            await client.PeopleByDocument[id].DeleteAsync();

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

    public async Task<(long? id, string error)> AddCompanyToDocumentAsync(CompanyByDocumentDto dto)
    {
        try
        {
            long? id = await client.Documents.Companies.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveCompanyFromDocumentAsync(long id)
    {
        try
        {
            await client.Documents.Companies[id].DeleteAsync();

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

    public async Task<(long? id, string error)> AddMachineToDocumentAsync(DocumentByMachineDto dto)
    {
        try
        {
            long? id = await client.Machines.Documents.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveMachineFromDocumentAsync(long id)
    {
        try
        {
            await client.Machines.Documents[id].DeleteAsync();

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

    public async Task<(long? id, string error)> AddMachineFamilyToDocumentAsync(DocumentByMachineFamilyDto dto)
    {
        try
        {
            long? id = await client.MachineFamilies.Documents.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> RemoveMachineFamilyFromDocumentAsync(long id)
    {
        try
        {
            await client.MachineFamilies.Documents[id].DeleteAsync();

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

    public async Task<List<MachineFamilyDto>> GetAllMachineFamiliesAsync()
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
}
