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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Models;
using Marechai.Database.Models;
using Marechai.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[ApiController]
[Route("invitation-codes")]
[Authorize(Roles = "Admin,UberAdmin")]
public class InvitationCodesController(MarechaiContext            context,
                                       InvitationCodeGenerator    generator) : ControllerBase
{
    /// <summary>
    ///     Lists the current user's own invitation codes (those they created/were granted). For self-service use;
    ///     users are not told who redeemed their codes.
    /// </summary>
    [HttpGet("mine")]
    [Authorize]
    [ProducesResponseType(typeof(List<MyInvitationCodeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    public Task<List<MyInvitationCodeDto>> GetMineAsync()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Task.FromResult(new List<MyInvitationCodeDto>());

        return context.InvitationCodes.AsNoTracking()
                      .Where(c => c.CreatedById == userId)
                      .OrderByDescending(c => c.CreatedOn)
                      .Select(c => new MyInvitationCodeDto
                       {
                           Code    = c.Code,
                           IsUsed  = c.UsedById != null
                       })
                      .ToListAsync();
    }

    /// <summary>
    ///     Lists invitation codes. Admins can filter to only-unused via <paramref name="unusedOnly" />=true.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<InvitationCodeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Produces("application/json")]
    public Task<List<InvitationCodeDto>> GetAllAsync([FromQuery] bool? unusedOnly = null)
    {
        IQueryable<InvitationCode> q = context.InvitationCodes.AsNoTracking();

        if(unusedOnly == true) q = q.Where(c => c.UsedById == null);

        return q.OrderByDescending(c => c.CreatedOn)
                .Select(c => new InvitationCodeDto
                 {
                     Code              = c.Code,
                     CreatedOn         = c.CreatedOn,
                     CreatedByUserName = c.CreatedBy.UserName,
                     UsedOn            = c.UsedOn,
                     UsedByUserName    = c.UsedBy != null ? c.UsedBy.UserName : null,
                     IsUsed            = c.UsedById != null
                 })
                .ToListAsync();
    }

    /// <summary>
    ///     Generates a fresh invitation code attributed to the calling admin. Retries up to 5× on PK collision
    ///     (extremely unlikely with 31^8 ≈ 8.5×10^11 codes but tighter on a populated table).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(InvitationCodeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<ActionResult<InvitationCodeDto>> CreateAsync()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        for(int attempt = 0; attempt < 5; attempt++)
        {
            string code = generator.Generate();

            var entity = new InvitationCode
            {
                Code        = code,
                CreatedById = userId,
                CreatedOn   = DateTime.UtcNow,
                RowVersion  = Guid.NewGuid()
            };

            context.InvitationCodes.Add(entity);

            try
            {
                await context.SaveChangesWithUserAsync(userId);

                string createdByUserName = await context.Users.Where(u => u.Id == userId)
                                                        .Select(u => u.UserName)
                                                        .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetAllAsync),
                                       new InvitationCodeDto
                                       {
                                           Code              = code,
                                           CreatedOn         = entity.CreatedOn,
                                           CreatedByUserName = createdByUserName,
                                           IsUsed            = false
                                       });
            }
            catch(DbUpdateException)
            {
                // PK collision (or other unique constraint) — drop tracked entity and retry with a new code.
                context.Entry(entity).State = EntityState.Detached;
            }
        }

        return Problem("Failed to generate a unique invitation code after multiple attempts.",
                       statusCode: StatusCodes.Status500InternalServerError,
                       title: "INVITATION_CODE_GENERATION_FAILED");
    }

    /// <summary>
    ///     Revokes (deletes) an unused invitation code. Used codes return 409 Conflict because deleting them
    ///     would cascade-null the consuming user's <c>UsedById</c> link, hiding the audit trail.
    /// </summary>
    [HttpDelete("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RevokeAsync(string code)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        InvitationCode entity = await context.InvitationCodes.FirstOrDefaultAsync(c => c.Code == code);

        if(entity is null) return NotFound();

        if(entity.UsedById is not null)
            return Problem("This invitation code has already been redeemed and cannot be revoked.",
                           statusCode: StatusCodes.Status409Conflict,
                           title: "INVITATION_CODE_ALREADY_USED");

        context.InvitationCodes.Remove(entity);
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }
}
