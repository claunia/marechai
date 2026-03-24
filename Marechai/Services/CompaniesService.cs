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
using Microsoft.Extensions.Localization;

namespace Marechai.Services;

public class CompaniesService(Marechai.ApiClient.Client client, IStringLocalizer<CompaniesService> localizer)
{
    public async Task<List<CompanyDto>> GetAsync()
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

    public async Task<CompanyDto?> GetAsync(int id)
    {
        try
        {
            return await client.Companies[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<MachineDto>> GetMachinesAsync(int id)
    {
        try
        {
            List<MachineDto>? machines = await client.Companies[id].Machines.GetAsync();

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<string?> GetDescriptionTextAsync(int id)
    {
        try
        {
            return await client.Companies[id].Description.Text.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<CompanyDto?> GetSoldToAsync(int? id)
    {
        if(id is null) return null;

        try
        {
            return await client.Companies[id.Value].Soldto.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetCountryNameAsync(int id)
    {
        try
        {
            return await client.Countries[id].Name.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<CompanyDto>> GetCompaniesByCountryAsync(int countryId)
    {
        try
        {
            List<CompanyDto>? companies = await client.Countries[countryId].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyDto>> GetCompaniesByLetterAsync(char id)
    {
        try
        {
            List<CompanyDto>? companies = await client.Companies.Letter[id.ToString()].GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }
}
