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

    public async Task<List<DocumentDto>> GetDocumentsByLetterAsync(char c)
    {
        try
        {
            List<DocumentDto>? documents = await client.Documents.ByLetter[c.ToString()].GetAsync();

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
            List<DocumentDto>? documents = await client.Documents.ByYear[year].GetAsync();

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
            List<DocumentDto>? documents = await client.Documents.GetAsync();

            return documents ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<DocumentDto?> GetDocumentAsync(long id)
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

    public async Task<DocumentSynopsisDto?> GetDocumentSynopsisAsync(long id)
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
            List<PersonByDocumentDto>? people = await client.Documents[id].People.GetAsync();

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
            List<CompanyByDocumentDto>? companies = await client.Documents[id].Companies.GetAsync();

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
            List<DocumentByMachineDto>? machines = await client.Documents[id].Machines.GetAsync();

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
            List<DocumentByMachineFamilyDto>? families = await client.Documents[id].MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch
        {
            return [];
        }
    }
}
