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
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.Services;

public class SmartphonesService(Marechai.ApiClient.Client client)
{
    public async Task<int> GetSmartphonesCountAsync()
    {
        try
        {
            int? count = await client.Smartphones.Count.GetAsync();

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
            int? year = await client.Smartphones.MinimumYear.GetAsync();

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
            int? year = await client.Smartphones.MaximumYear.GetAsync();

            return year ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<MachineDto>> GetSmartphonesByLetterAsync(char c, int? skip = null, int? take = null,
                                                                    CancellationToken cancellationToken = default)
    {
        try
        {
            List<MachineDto> machines = await client.Smartphones.ByLetter[c.ToString()].GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetSmartphonesByLetterCountAsync(char c, CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Smartphones.ByLetter[c.ToString()].Count
                                     .GetAsync(cancellationToken: cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<MachineDto>> GetSmartphonesByYearAsync(int year, int? skip = null, int? take = null,
                                                                  CancellationToken cancellationToken = default)
    {
        try
        {
            List<MachineDto> machines = await client.Smartphones.ByYear[year].GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetSmartphonesByYearCountAsync(int year, CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Smartphones.ByYear[year].Count.GetAsync(cancellationToken: cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<MachineDto>> GetSmartphonesAsync(int? skip = null, int? take = null,
                                                            CancellationToken cancellationToken = default)
    {
        try
        {
            List<MachineDto> machines = await client.Smartphones.GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<MachineDto>> GetPrototypesAsync(int? skip = null, int? take = null,
                                                           CancellationToken cancellationToken = default)
    {
        try
        {
            List<MachineDto> machines = await client.Smartphones.Prototypes.GetAsync(config =>
            {
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return machines ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetPrototypesCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Smartphones.Prototypes.Count.GetAsync(cancellationToken: cancellationToken);

            return count ?? 0;
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
            List<CompanyDto> companies = await client.Smartphones.Companies.GetAsync();

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
            List<CompanyDto> companies = await client.Smartphones.Companies.Letter[c.ToString()].GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }
}
