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
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("admin/review-reports")]
[ApiController]
[Authorize(Roles = "Admin, UberAdmin")]
public class ReviewReportsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<ReviewReportDto>> GetReportsAsync([FromQuery] bool? resolved = false)
    {
        IQueryable<ReviewReport> query = context.ReviewReports;

        if(resolved.HasValue)
            query = query.Where(r => r.IsResolved == resolved.Value);

        return await query.OrderByDescending(r => r.CreatedOn)
                          .Select(r => new ReviewReportDto
                           {
                               Id                 = r.Id,
                               ReporterId         = r.ReporterId,
                               ReporterName       = r.Reporter != null ? r.Reporter.DisplayName ?? r.Reporter.UserName : null,
                               ReviewId           = r.ReviewId,
                               SoftwareId         = r.Review.SoftwareId,
                               SoftwareName       = r.Review.Software.Name,
                               ReviewerName       = r.Review.User != null ? r.Review.User.DisplayName ?? r.Review.User.UserName : null,
                               Reason             = r.Reason,
                               Explanation        = r.Explanation,
                               IsResolved         = r.IsResolved,
                               ResolvedByUserName = r.ResolvedBy != null ? r.ResolvedBy.DisplayName ?? r.ResolvedBy.UserName : null,
                               ResolvedOn         = r.ResolvedOn,
                               CreatedOn          = r.CreatedOn
                           })
                          .ToListAsync();
    }

    [HttpPut("{id:long}/resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveReportAsync(long id)
    {
        ReviewReport report = await context.ReviewReports.FindAsync(id);

        if(report is null) return NotFound();

        string userId = User.FindFirstValue(ClaimTypes.Sid);

        report.IsResolved       = true;
        report.ResolvedByUserId = userId;
        report.ResolvedOn       = DateTime.UtcNow;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteReportAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        ReviewReport report = await context.ReviewReports.FindAsync(id);

        if(report is not null)
        {
            context.ReviewReports.Remove(report);
            await context.SaveChangesWithUserAsync(userId);
        }

        return NoContent();
    }
}
