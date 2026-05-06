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
using Microsoft.Extensions.Logging;

namespace Marechai.Services;

public sealed class ReviewReportService(Marechai.ApiClient.Client client, ILogger<ReviewReportService> logger)
{
    public async Task<List<ReviewReportDto>> GetReportsAsync(bool? resolved = false)
    {
        try
        {
            List<ReviewReportDto> reports = await client.Admin.ReviewReports.GetAsync(config =>
            {
                config.QueryParameters.Resolved = resolved;
            });

            return reports ?? [];
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading review reports");

            return [];
        }
    }

    public async Task<bool> ResolveReportAsync(long id)
    {
        try
        {
            await client.Admin.ReviewReports[id].Resolve.PutAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteReportAsync(long id)
    {
        try
        {
            await client.Admin.ReviewReports[id].DeleteAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}
