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
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

/// <summary>
///     Admin-only client wrapper around <c>/invitation-codes</c>. Lists, generates and revokes single-use
///     registration invitation codes. The server enforces <c>Admin,UberAdmin</c> role membership; this
///     wrapper just surfaces calls + standardised error handling.
/// </summary>
public sealed class InvitationCodesService(Marechai.ApiClient.Client client, ILogger<InvitationCodesService> logger)
{
    public async Task<List<InvitationCodeDto>> GetAllAsync(bool? unusedOnly = null)
    {
        try
        {
            List<InvitationCodeDto> codes = await client.InvitationCodes.GetAsync(rc =>
            {
                if(unusedOnly.HasValue) rc.QueryParameters.UnusedOnly = unusedOnly.Value;
            });

            return codes ?? [];
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading invitation codes");

            return [];
        }
    }

    public async Task<(InvitationCodeDto Created, string ErrorMessage)> CreateAsync()
    {
        try
        {
            InvitationCodeDto created = await client.InvitationCodes.PostAsync();

            return (created, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Create invitation code failed");

            return (null, ex.Detail ?? ex.Title ?? "Failed to create invitation code.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Create invitation code failed");

            return (null, "An error occurred while creating an invitation code.");
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> RevokeAsync(string code)
    {
        try
        {
            await client.InvitationCodes[code].DeleteAsync();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Revoke invitation code failed");

            return (false, ex.Detail ?? ex.Title ?? "Failed to revoke invitation code.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Revoke invitation code failed");

            return (false, "An error occurred while revoking the invitation code.");
        }
    }
}
