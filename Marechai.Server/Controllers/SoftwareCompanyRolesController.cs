/*******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ---------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] -----------------------------------------------------------
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
// ---------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/software/company-roles")]
[ApiController]
public class SoftwareCompanyRolesController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/software/{softwareId:ulong}/company-roles")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareCompanyRoleDto>> GetBySoftwareAsync(ulong softwareId) => context.SoftwareCompanyRoles
       .Where(r => r.SoftwareId == softwareId)
       .OrderBy(r => r.Company.Name)
       .Select(r => new SoftwareCompanyRoleDto
        {
            SoftwareId = r.SoftwareId,
            Software   = r.Software.Name,
            CompanyId  = r.CompanyId,
            Company    = r.Company.Name,
            Role       = r.Role
        })
       .ToListAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateAsync([FromBody] SoftwareCompanyRoleDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwareCompanyRole
        {
            SoftwareId = dto.SoftwareId,
            CompanyId  = dto.CompanyId,
            Role       = dto.Role
        };

        await context.SoftwareCompanyRoles.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{softwareId:ulong}/{companyId:int}/{role}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong softwareId, int companyId, string role)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareCompanyRole item =
            await context.SoftwareCompanyRoles.FindAsync(softwareId, companyId, role);

        if(item is null) return NotFound();

        context.SoftwareCompanyRoles.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
