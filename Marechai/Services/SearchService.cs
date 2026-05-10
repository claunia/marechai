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
using Marechai.ApiClient;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

public sealed class SearchService(Client client)
{
    public async Task<List<SearchResultDto>> AutocompleteAsync(string query, int? entityType = null, int take = 8)
    {
        if(string.IsNullOrWhiteSpace(query) || query.Length < 3) return [];

        try
        {
            List<SearchResultDto> result = await client.Search.Autocomplete.GetAsync(req =>
            {
                req.QueryParameters.Q          = query;
                req.QueryParameters.Take       = take;
                if(entityType.HasValue) req.QueryParameters.EntityType = entityType;
            });

            return result ?? [];
        }
        catch(ApiException)        { return []; }
        catch(Exception)           { return []; }
    }

    public async Task<SearchResultsPageDto> SearchAsync(SearchRequestDto request)
    {
        // Filter-only searches (no query) are valid; let the server decide whether the request is empty.
        if(request == null) return new SearchResultsPageDto();

        try
        {
            return await client.Search.Results.PostAsync(request) ?? new SearchResultsPageDto();
        }
        catch(ApiException) { return new SearchResultsPageDto(); }
        catch(Exception)    { return new SearchResultsPageDto(); }
    }
}
