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
using Microsoft.Extensions.Localization;

namespace Marechai.Server.Controllers;

[Route("/documents")]
[ApiController]
public class DocumentsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<DocumentDto>> GetAsync() => await context.Documents.OrderBy(b => b.NativeTitle)
                                                                          .ThenBy(b => b.Published)
                                                                          .ThenBy(b => b.Title)
                                                                          .Select(b => new DocumentDto
                                                                           {
                                                                               Id          = b.Id,
                                                                               Title       = b.Title,
                                                                               NativeTitle = b.NativeTitle,
                                                                               Published   = b.Published,
                                                                               Synopsis    = b.Synopsis,
                                                                               CountryId   = b.CountryId,
                                                                               Country     = b.Country.Name
                                                                           })
                                                                          .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<DocumentDto> GetAsync(long id) => await context.Documents.Where(b => b.Id == id)
                                                                            .Select(b => new DocumentDto
                                                                             {
                                                                                 Id          = b.Id,
                                                                                 Title       = b.Title,
                                                                                 NativeTitle = b.NativeTitle,
                                                                                 Published   = b.Published,
                                                                                 Synopsis    = b.Synopsis,
                                                                                 CountryId   = b.CountryId,
                                                                                 Country     = b.Country.Name
                                                                             })
                                                                            .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task UpdateAsync(DocumentDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        Document model = await context.Documents.FindAsync(dto.Id);

        if(model is null) return;

        model.Title       = dto.Title;
        model.NativeTitle = dto.NativeTitle;
        model.Published   = dto.Published;
        model.Synopsis    = dto.Synopsis;
        model.CountryId   = dto.CountryId;
        await context.SaveChangesWithUserAsync(userId);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<long> CreateAsync(DocumentDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return 0;
        var model = new Document
        {
            Title       = dto.Title,
            NativeTitle = dto.NativeTitle,
            Published   = dto.Published,
            Synopsis    = dto.Synopsis,
            CountryId   = dto.CountryId
        };

        await context.Documents.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetSynopsisTextAsync(int id) =>
        (await context.Documents.FirstOrDefaultAsync(d => d.Id == id))?.Synopsis;

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task DeleteAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        Document item = await context.Documents.FindAsync(id);

        if(item is null) return;

        context.Documents.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}
