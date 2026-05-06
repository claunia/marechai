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
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/software/promo-art")]
[ApiController]
public class SoftwarePromoArtController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/software/{softwareId:ulong}/promo-art")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwarePromoArtDto>> GetBySoftwareAsync(ulong softwareId) =>
        context.SoftwarePromoArt
               .Where(p => p.SoftwareId == softwareId)
               .OrderBy(p => p.Group.Name)
               .Select(p => new SoftwarePromoArtDto
                {
                    Id                = p.Id,
                    SoftwareId        = p.SoftwareId,
                    GroupId           = p.GroupId,
                    GroupName         = p.Group.Name,
                    Caption           = p.Caption,
                    OriginalExtension = p.OriginalExtension
                })
               .ToListAsync();

    [HttpGet("{id:Guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoftwarePromoArtDto>> GetAsync(Guid id)
    {
        var promo = await context.SoftwarePromoArt
                                 .Where(p => p.Id == id)
                                 .Select(p => new SoftwarePromoArtDto
                                  {
                                      Id                = p.Id,
                                      SoftwareId        = p.SoftwareId,
                                      GroupId           = p.GroupId,
                                      GroupName         = p.Group.Name,
                                      Caption           = p.Caption,
                                      OriginalExtension = p.OriginalExtension
                                  })
                                 .FirstOrDefaultAsync();

        if(promo is null) return NotFound();

        return promo;
    }
}
