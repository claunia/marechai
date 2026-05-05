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

public class MagazinesService(Marechai.ApiClient.Client client)
{
    public async Task<int> GetMagazinesCountAsync()
    {
        try
        {
            int? count = await client.Magazines.Count.GetAsync();

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

    public async Task<List<MagazineDto>> GetMagazinesByLetterAsync(char c)
    {
        try
        {
            List<MagazineDto> magazines = await client.Magazines.ByLetter[c.ToString()].GetAsync();

            return magazines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MagazineDto>> GetMagazinesByYearAsync(int year)
    {
        try
        {
            List<MagazineDto> magazines = await client.Magazines.ByYear[year].GetAsync();

            return magazines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MagazineDto>> GetMagazinesAsync()
    {
        try
        {
            List<MagazineDto> magazines = await client.Magazines.GetAsync();

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

    // --- CRUD methods ---

    public async Task<(long? id, string error)> CreateAsync(MagazineDto dto)
    {
        try
        {
            long? id = await client.Magazines.PostAsync(dto);

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

    public async Task<(bool succeeded, string error)> UpdateAsync(long id, MagazineDto dto)
    {
        try
        {
            await client.Magazines[id].PutAsync(dto);

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
            await client.Magazines[id].DeleteAsync();

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
            return (null, ex.Message);
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
            return (false, ex.Message);
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
            return (null, ex.Message);
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
            return (false, ex.Message);
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
            return (null, ex.Message);
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
            return (false, ex.Message);
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
            return (null, ex.Message);
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
            return (false, ex.Message);
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
            return (null, ex.Message);
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
