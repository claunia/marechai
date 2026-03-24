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

    public async Task<List<MagazineDto>> GetMagazinesByLetterAsync(char c)
    {
        try
        {
            List<MagazineDto>? magazines = await client.Magazines.ByLetter[c.ToString()].GetAsync();

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
            List<MagazineDto>? magazines = await client.Magazines.ByYear[year].GetAsync();

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
            List<MagazineDto>? magazines = await client.Magazines.GetAsync();

            return magazines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<MagazineDto?> GetMagazineAsync(long id)
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

    public async Task<DocumentSynopsisDto?> GetMagazineSynopsisAsync(long id)
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
            List<PersonByMagazineDto>? people = await client.Magazines[id].People.GetAsync();

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
            List<CompanyByMagazineDto>? companies = await client.Magazines[id].Companies.GetAsync();

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
            List<MagazineByMachineDto>? machines = await client.Magazines[id].Machines.GetAsync();

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
            List<MagazineByMachineFamilyDto>? families = await client.Magazines[id].MachineFamilies.GetAsync();

            return families ?? [];
        }
        catch
        {
            return [];
        }
    }
}
