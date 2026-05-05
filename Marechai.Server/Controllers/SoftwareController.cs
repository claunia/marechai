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
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/software")]
[ApiController]
public class SoftwareController(MarechaiContext context) : ControllerBase
{
    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetSoftwareCountAsync() => context.Softwares.CountAsync();

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetMinimumYearAsync() => context.SoftwareReleases
                                                     .Where(r => r.ReleaseDate.HasValue &&
                                                                 r.ReleaseDate.Value.Year > 1000)
                                                     .MinAsync(r => r.ReleaseDate.Value.Year);

    [HttpGet("maximum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetMaximumYearAsync() => context.SoftwareReleases
                                                     .Where(r => r.ReleaseDate.HasValue &&
                                                                 r.ReleaseDate.Value.Year > 1000)
                                                     .MaxAsync(r => r.ReleaseDate.Value.Year);

    [HttpGet("by-letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetSoftwareByLetterAsync(char c) => context.Softwares
       .Where(s => EF.Functions.Like(s.Name, $"{c}%"))
       .OrderBy(s => s.Name)
       .Select(s => new SoftwareDto
        {
            Id                = s.Id,
            Name              = s.Name,
            FamilyId          = s.FamilyId,
            Family            = s.Family.Name,
            IsOperatingSystem = s.IsOperatingSystem,
            IsGame            = s.IsGame,
            FrontCoverId = context.SoftwareCovers
                                  .Where(c2 => (c2.Release.SoftwareId == s.Id ||
                                                 c2.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                c2.Type == SoftwareCoverType.Front)
                                  .Select(c2 => (Guid?)c2.Id)
                                  .FirstOrDefault()
        })
       .ToListAsync();

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetSoftwareByYearAsync(int year) => context.Softwares
       .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.ReleaseDate != null &&
                                                           r.ReleaseDate.Value.Year == year))
                || s.DirectReleases.Any(r => r.ReleaseDate != null &&
                                             r.ReleaseDate.Value.Year == year))
       .OrderBy(s => s.Name)
       .Select(s => new SoftwareDto
        {
            Id                = s.Id,
            Name              = s.Name,
            FamilyId          = s.FamilyId,
            Family            = s.Family.Name,
            IsOperatingSystem = s.IsOperatingSystem,
            IsGame            = s.IsGame,
            FrontCoverId = context.SoftwareCovers
                                  .Where(c => (c.Release.SoftwareId == s.Id ||
                                                c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                               c.Type == SoftwareCoverType.Front)
                                  .Select(c => (Guid?)c.Id)
                                  .FirstOrDefault()
        })
       .ToListAsync();

    [HttpGet("by-platform/{platformId:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetSoftwareByPlatformAsync(ulong platformId) => context.Softwares
       .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.PlatformId == platformId))
                || s.DirectReleases.Any(r => r.PlatformId == platformId))
       .OrderBy(s => s.Name)
       .Select(s => new SoftwareDto
        {
            Id                = s.Id,
            Name              = s.Name,
            FamilyId          = s.FamilyId,
            Family            = s.Family.Name,
            IsOperatingSystem = s.IsOperatingSystem,
            IsGame            = s.IsGame,
            FrontCoverId = context.SoftwareCovers
                                  .Where(c => (c.Release.SoftwareId == s.Id ||
                                                c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                               c.Type == SoftwareCoverType.Front)
                                  .Select(c => (Guid?)c.Id)
                                  .FirstOrDefault()
        })
       .ToListAsync();

    [HttpGet("companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetSoftwareCompaniesAsync() =>
        context.SoftwareCompanyRoles
               .Select(cr => cr.Company)
               .Union(context.SoftwareReleases.Select(sr => sr.Publisher))
               .Distinct()
               .Include(c => c.Logos)
               .OrderBy(c => c.Name)
               .Select(c => new CompanyDto
                {
                    Id       = c.Id,
                    LastLogo = c.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid,
                    Name     = c.Name
                })
               .ToListAsync();

    [HttpGet("companies/letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetSoftwareCompaniesByLetterAsync(char c) =>
        context.SoftwareCompanyRoles
               .Select(cr => cr.Company)
               .Union(context.SoftwareReleases.Select(sr => sr.Publisher))
               .Distinct()
               .Include(c => c.Logos)
               .Where(co => EF.Functions.Like(co.Name, $"{c}%"))
               .OrderBy(co => co.Name)
               .Select(co => new CompanyDto
                {
                    Id       = co.Id,
                    LastLogo = co.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid,
                    Name     = co.Name
                })
               .ToListAsync();

    [HttpGet("/software/{softwareId:ulong}/companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareCompanyRoleDto>> GetCompaniesAsync(ulong softwareId) => context.SoftwareCompanyRoles
       .Where(cr => cr.SoftwareId == softwareId)
       .Select(cr => new SoftwareCompanyRoleDto
        {
            SoftwareId = cr.SoftwareId,
            Software   = cr.Software.Name,
            CompanyId  = cr.CompanyId,
            Company    = cr.Company.Name,
            RoleId     = cr.RoleId,
            Role       = cr.Role.Name
        })
       .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetAsync() => context.Softwares.OrderBy(s => s.Name)
                                                        .Select(s => new SoftwareDto
                                                         {
                                                             Id                = s.Id,
                                                             Name              = s.Name,
                                                             FamilyId          = s.FamilyId,
                                                             Family            = s.Family.Name,
                                                             IsOperatingSystem = s.IsOperatingSystem,
                                                             IsGame            = s.IsGame,
                                                             FrontCoverId = context.SoftwareCovers
                                                                                   .Where(c => (c.Release.SoftwareId == s.Id ||
                                                                                                 c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                                                                c.Type == SoftwareCoverType.Front)
                                                                                   .Select(c => (Guid?)c.Id)
                                                                                   .FirstOrDefault()
                                                         })
                                                        .ToListAsync();

    [HttpGet("{id:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoftwareDto> GetAsync(ulong id) => context.Softwares.Where(s => s.Id == id)
                                                          .Select(s => new SoftwareDto
                                                           {
                                                               Id                = s.Id,
                                                               Name              = s.Name,
                                                               FamilyId          = s.FamilyId,
                                                               Family            = s.Family.Name,
                                                               IsOperatingSystem = s.IsOperatingSystem,
                                                               IsGame            = s.IsGame
                                                           })
                                                          .FirstOrDefaultAsync();

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] SoftwareDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Software model = await context.Softwares.FindAsync(id);

        if(model is null) return NotFound();

        model.Name              = dto.Name;
        model.FamilyId          = dto.FamilyId;
        model.IsOperatingSystem = dto.IsOperatingSystem;
        model.IsGame            = dto.IsGame;

        await context.News.AddAsync(new News
        {
            AddedId = (long)model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.UpdatedSoftwareInDb,
            Name    = dto.Name
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] SoftwareDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Software
        {
            Name              = dto.Name,
            FamilyId          = dto.FamilyId,
            IsOperatingSystem = dto.IsOperatingSystem,
            IsGame            = dto.IsGame
        };

        await context.Softwares.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        await context.News.AddAsync(new News
        {
            AddedId = (long)model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.NewSoftwareInDb,
            Name    = dto.Name
        });

        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Software item = await context.Softwares.FindAsync(id);

        if(item is null) return NotFound();

        context.Softwares.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpGet("{id:ulong}/descriptions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDescriptionDto>> GetDescriptionsAsync(ulong id) => context.SoftwareDescriptions
       .Where(d => d.SoftwareId == id)
       .Select(d => new SoftwareDescriptionDto
        {
            Id           = d.Id,
            SoftwareId   = d.SoftwareId,
            Html         = d.Html,
            Markdown     = d.Text,
            LanguageCode = d.LanguageCode,
            Language     = d.Language.ReferenceName
        })
       .ToListAsync();

    [HttpGet("{id:ulong}/description")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<SoftwareDescriptionDto> GetDescriptionAsync(ulong id, [FromQuery] string lang = "eng")
    {
        SoftwareDescriptionDto description = await context.SoftwareDescriptions
                                                          .Where(d => d.SoftwareId == id && d.LanguageCode == lang)
                                                          .Select(d => new SoftwareDescriptionDto
                                                           {
                                                               Id           = d.Id,
                                                               SoftwareId   = d.SoftwareId,
                                                               Html         = d.Html,
                                                               Markdown     = d.Text,
                                                               LanguageCode = d.LanguageCode,
                                                               Language     = d.Language.ReferenceName
                                                           })
                                                          .FirstOrDefaultAsync();

        // Fallback to English if requested language not found
        if(description is null && lang != "eng")
            description = await context.SoftwareDescriptions
                                       .Where(d => d.SoftwareId == id && d.LanguageCode == "eng")
                                       .Select(d => new SoftwareDescriptionDto
                                        {
                                            Id           = d.Id,
                                            SoftwareId   = d.SoftwareId,
                                            Html         = d.Html,
                                            Markdown     = d.Text,
                                            LanguageCode = d.LanguageCode,
                                            Language     = d.Language.ReferenceName
                                        })
                                       .FirstOrDefaultAsync();

        return description;
    }

    [HttpGet("{id:ulong}/description/text")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetDescriptionTextAsync(ulong id, [FromQuery] string lang = "eng")
    {
        SoftwareDescription description =
            await context.SoftwareDescriptions.FirstOrDefaultAsync(d => d.SoftwareId   == id &&
                                                                        d.LanguageCode == lang);

        // Fallback to English if requested language not found
        if(description is null && lang != "eng")
            description = await context.SoftwareDescriptions.FirstOrDefaultAsync(d => d.SoftwareId   == id &&
                              d.LanguageCode == "eng");

        return description?.Html ?? description?.Text;
    }

    [HttpPost("{id:ulong}/description")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateOrUpdateDescriptionAsync(
        ulong id, [FromBody] SoftwareDescriptionDto description)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareDescription current = await context.SoftwareDescriptions
                                                   .FirstOrDefaultAsync(d => d.SoftwareId   == id &&
                                                                             d.LanguageCode == description.LanguageCode);

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        string             html     = Markdown.ToHtml(description.Markdown, pipeline);

        if(current is null)
        {
            current = new SoftwareDescription
            {
                SoftwareId   = id,
                LanguageCode = description.LanguageCode,
                Html         = html,
                Text         = description.Markdown
            };

            await context.SoftwareDescriptions.AddAsync(current);
        }
        else
        {
            current.Html = html;
            current.Text = description.Markdown;
        }

        await context.SaveChangesWithUserAsync(userId);

        return current.Id;
    }

    [HttpDelete("{id:ulong}/description/{languageCode}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteDescriptionAsync(ulong id, string languageCode)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareDescription description = await context.SoftwareDescriptions
                                                       .FirstOrDefaultAsync(d => d.SoftwareId   == id &&
                                                                                 d.LanguageCode == languageCode);

        if(description is null) return NotFound();

        context.SoftwareDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpGet("genres")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareGenreDto>> GetAllGenresAsync() => context.SoftwareGenres
       .Where(g => g.Softwares.Any())
       .OrderBy(g => g.Type)
       .ThenBy(g => g.Name)
       .Select(g => new SoftwareGenreDto
        {
            Id       = g.Id,
            Name     = g.Name,
            Type     = (int)g.Type,
            TypeName = g.Type.ToString()
        })
       .ToListAsync();

    [HttpGet("by-genre/{genreId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetSoftwareByGenreAsync(int genreId) => context.Softwares
       .Where(s => s.Genres.Any(g => g.GenreId == genreId))
       .OrderBy(s => s.Name)
       .Select(s => new SoftwareDto
        {
            Id                = s.Id,
            Name              = s.Name,
            FamilyId          = s.FamilyId,
            Family            = s.Family.Name,
            IsOperatingSystem = s.IsOperatingSystem,
            IsGame            = s.IsGame,
            FrontCoverId = context.SoftwareCovers
                                  .Where(c => (c.Release.SoftwareId == s.Id ||
                                                c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                               c.Type == SoftwareCoverType.Front)
                                  .Select(c => (Guid?)c.Id)
                                  .FirstOrDefault()
        })
       .ToListAsync();

    [HttpGet("specifications")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareSpecKeyDto>> GetSpecificationsAsync()
    {
        var raw = await context.SoftwareAttributes
                               .Where(a => a.Category == "Spec" && a.Key != "Notes")
                               .Select(a => new { a.Key, a.Value })
                               .Distinct()
                               .ToListAsync();

        return raw.GroupBy(a => a.Key)
                  .OrderBy(g => g.Key)
                  .Select(g => new SoftwareSpecKeyDto
                   {
                       Key    = g.Key,
                       Values = g.Select(a => a.Value).OrderBy(v => v).ToList()
                   })
                  .ToList();
    }

    [HttpGet("by-spec")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetSoftwareBySpecAsync([FromQuery] string key, [FromQuery] string value)
    {
        if(key == "Notes") return [];

        return await context.Softwares
               .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.Attributes
                                                                     .Any(a => a.Category == "Spec" &&
                                                                              a.Key   == key         &&
                                                                              a.Value == value)))
                         || s.DirectReleases.Any(r => r.Attributes
                                                       .Any(a => a.Category == "Spec" &&
                                                                  a.Key   == key       &&
                                                                  a.Value == value)))
               .OrderBy(s => s.Name)
               .Select(s => new SoftwareDto
                {
                    Id                = s.Id,
                    Name              = s.Name,
                    FamilyId          = s.FamilyId,
                    Family            = s.Family.Name,
                    IsOperatingSystem = s.IsOperatingSystem,
                    IsGame            = s.IsGame,
                    FrontCoverId = context.SoftwareCovers
                                          .Where(c => (c.Release.SoftwareId == s.Id ||
                                                        c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                       c.Type == SoftwareCoverType.Front)
                                          .Select(c => (Guid?)c.Id)
                                          .FirstOrDefault()
                })
               .ToListAsync();
    }

    [HttpGet("/software/{softwareId:ulong}/genres")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareGenreDto>> GetGenresAsync(ulong softwareId) => context.GenresBySoftware
       .Where(gs => gs.SoftwareId == softwareId)
       .Select(gs => new SoftwareGenreDto
        {
            Id       = gs.Genre.Id,
            Name     = gs.Genre.Name,
            Type     = (int)gs.Genre.Type,
            TypeName = gs.Genre.Type.ToString()
        })
       .OrderBy(g => g.Type)
       .ThenBy(g => g.Name)
       .ToListAsync();

    [HttpGet("/software/{softwareId:ulong}/attributes")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareAttributeDto>> GetAttributesAsync(ulong softwareId) => context.SoftwareAttributes
       .Where(a => a.SoftwareRelease.SoftwareId == softwareId)
       .Select(a => new SoftwareAttributeDto
        {
            Id                = a.Id,
            SoftwareReleaseId = a.SoftwareReleaseId,
            Category          = a.Category,
            Key               = a.Key,
            Value             = a.Value,
            PlatformName      = a.SoftwareRelease.Platform.Name,
            RegionNames       = string.Join(", ", a.SoftwareRelease.Regions.Select(r => r.UnM49.Name))
        })
       .OrderBy(a => a.PlatformName)
       .ThenBy(a => a.Category)
       .ThenBy(a => a.Key)
       .ToListAsync();

    [HttpGet("/software/{softwareId:ulong}/credits")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<PersonBySoftwareDto>> GetCreditsAsync(ulong softwareId) =>
        (await context.PeopleBySoftware
                      .Where(p => p.SoftwareId == softwareId)
                      .Select(p => new PersonBySoftwareDto
                       {
                           Id           = p.Id,
                           PersonId     = p.PersonId,
                           SoftwareId   = p.SoftwareId,
                           Role         = p.DocumentRole != null ? p.DocumentRole.Name : p.Role,
                           SoftwareName = p.Software.Name,
                           Name         = p.Person.Name,
                           Surname      = p.Person.Surname,
                           Alias        = p.Person.Alias,
                           DisplayName  = p.Person.DisplayName
                       })
                      .ToListAsync()).OrderBy(p => p.Role)
           .ThenBy(p => p.FullName)
           .ToList();
}
