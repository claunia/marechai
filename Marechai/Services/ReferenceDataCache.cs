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
using Microsoft.Extensions.Caching.Memory;

namespace Marechai.Services;

/// <summary>
///     Process-wide memo for slow-changing reference data fetched from the API
///     (countries, languages, licenses, ISO standards, machine families). Each
///     Blazor circuit (per user) gets a scoped instance but the underlying
///     <see cref="IMemoryCache"/> is the application-wide singleton, so the
///     first user to hit any of these pays the round-trip and everyone else
///     reads from the in-process cache until the TTL expires.
///
///     The matching server endpoints already cache too (5 min for slow-changing
///     catalogs, 24 h for ISO standards), so this layer's primary win is
///     avoiding the HTTPS round-trip + JSON parse on every page navigation, not
///     reducing DB load.
///
///     Failure path: if the API call throws, we cache an empty list briefly so
///     a transient outage doesn't turn into a stampede of retries from every
///     page in the circuit. The TTL is short enough (5 min) that recovery is
///     prompt without manual invalidation.
/// </summary>
public sealed class ReferenceDataCache(IMemoryCache cache, Marechai.ApiClient.Client client)
{
    const string COUNTRIES_KEY        = "ref:iso31661-numeric";
    const string LANGUAGES_KEY        = "ref:languages";
    const string LICENSES_KEY         = "ref:licenses";
    const string ISO4217_KEY          = "ref:iso4217";
    const string UN_M49_ALL_KEY       = "ref:un-m49:all";
    const string UN_M49_REGIONS_KEY   = "ref:un-m49:regions";
    const string UN_M49_COUNTRIES_KEY = "ref:un-m49:countries";
    const string MACHINE_FAMILIES_KEY = "ref:machine-families";

    static readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);

    public Task<List<Iso31661NumericDto>> GetCountriesAsync() =>
        GetOrFetchAsync(COUNTRIES_KEY, () => client.Iso31661Numeric.GetAsync());

    public Task<List<Iso639Dto>> GetLanguagesAsync() =>
        GetOrFetchAsync(LANGUAGES_KEY, () => client.Languages.GetAsync());

    public Task<List<LicenseDto>> GetLicensesAsync() =>
        GetOrFetchAsync(LICENSES_KEY, () => client.Licenses.GetAsync());

    public Task<List<Iso4217Dto>> GetIso4217Async() =>
        GetOrFetchAsync(ISO4217_KEY, () => client.Iso4217.GetAsync());

    public Task<List<UnM49Dto>> GetUnM49Async() =>
        GetOrFetchAsync(UN_M49_ALL_KEY, () => client.UnM49.GetAsync());

    public Task<List<UnM49Dto>> GetUnM49RegionsAsync() =>
        GetOrFetchAsync(UN_M49_REGIONS_KEY, () => client.UnM49.Regions.GetAsync());

    public Task<List<UnM49Dto>> GetUnM49CountriesAsync() =>
        GetOrFetchAsync(UN_M49_COUNTRIES_KEY, () => client.UnM49.Countries.GetAsync());

    public Task<List<MachineFamilyDto>> GetMachineFamiliesAsync() =>
        GetOrFetchAsync(MACHINE_FAMILIES_KEY, () => client.MachineFamilies.GetAsync());

    /// <summary>
    ///     Drop the machine-families memo when admin code creates/edits/deletes
    ///     a family so subsequent dropdowns reflect the change immediately
    ///     rather than waiting up to 5 minutes for the TTL.
    /// </summary>
    public void InvalidateMachineFamilies() => cache.Remove(MACHINE_FAMILIES_KEY);

    async Task<List<T>> GetOrFetchAsync<T>(string key, Func<Task<List<T>>> fetcher)
    {
        if(cache.TryGetValue(key, out List<T> cached) && cached is not null) return cached;

        List<T> list;

        try
        {
            list = await fetcher() ?? [];
        }
        catch
        {
            list = [];
        }

        cache.Set(key, list, _ttl);

        return list;
    }
}
