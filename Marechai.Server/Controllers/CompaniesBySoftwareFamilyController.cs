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
// Copyright © 2003-2025 Natalia Portillo
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

[Route("/companies-by-software-family")]
[ApiController]
public class CompaniesBySoftwareFamilyController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<CompanyBySoftwareFamilyDto>> GetBySoftwareFamily(ulong softwareFamilyId) => await context
       .CompaniesBySoftwareFamilies.Where(p => p.SoftwareFamilyId == softwareFamilyId)
       .Select(p => new CompanyBySoftwareFamilyDto
        {
            Id               = p.Id,
            Company          = p.Company.Name,
            CompanyId        = p.CompanyId,
            RoleId           = p.RoleId,
            Role             = p.Role.Name,
            SoftwareFamilyId = p.SoftwareFamilyId
        })
       .OrderBy(p => p.Company)
       .ThenBy(p => p.Role)
       .ToListAsync();

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        CompaniesBySoftwareFamily item = await context.CompaniesBySoftwareFamilies.FindAsync(id);

        if(item is null) return NotFound();

        context.CompaniesBySoftwareFamilies.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync(int companyId, ulong softwareFamilyId, string roleId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var item = new CompaniesBySoftwareFamily
        {
            CompanyId        = companyId,
            SoftwareFamilyId = softwareFamilyId,
            RoleId           = roleId
        };

        await context.CompaniesBySoftwareFamilies.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return item.Id;
    }
}