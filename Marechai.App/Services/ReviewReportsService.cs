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

namespace Marechai.App.Services;

/// <summary>
///     Admin-only client wrapper around <c>/admin/review-reports</c>. Lists, resolves and deletes reports filed
///     against software reviews. The server enforces <c>Admin,UberAdmin</c> role membership; this wrapper just
///     surfaces calls + standardised error handling.
/// </summary>
public sealed class ReviewReportsService(Client client)
{
    public async Task<(List<ReviewReportDto> Reports, string? Error)> GetReportsAsync(bool? resolved = null)
    {
        try
        {
            List<ReviewReportDto>? reports = await client.Admin.ReviewReports.GetAsync(rc =>
            {
                if(resolved.HasValue) rc.QueryParameters.Resolved = resolved.Value;
            });

            return (reports ?? [], null);
        }
        catch(ApiException ex)
        {
            return ([], ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return ([], ex.Message);
        }
    }

    public async Task<(bool Succeeded, string? Error)> ResolveAsync(long id)
    {
        try
        {
            await client.Admin.ReviewReports[id].Resolve.PutAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, string? Error)> DeleteAsync(long id)
    {
        try
        {
            await client.Admin.ReviewReports[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    static string ExtractErrorMessage(ApiException ex) =>
        ex is ProblemDetails pd ? pd.Detail ?? pd.Title ?? ex.Message : ex.Message;
}
