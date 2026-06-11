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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[ApiController]
public class CollectionController(UserManager<ApplicationUser> userManager, MarechaiContext context) : ControllerBase
{
    // ── Public endpoints: view any user's collection ──

    [HttpGet("profile/{username}/collection")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserCollectionSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<ActionResult<UserCollectionSummaryDto>> GetCollectionSummaryAsync(string username)
    {
        ApplicationUser user = await userManager.FindByNameAsync(username);

        if(user is null) return NotFound();

        // Single round-trip instead of 4 sequential CountAsync calls. Each Count
        // becomes a scalar subquery the DB executes in parallel as part of one
        // composite SELECT.
        string userId = user.Id;

        var counts = await context.Users
                                  .Where(u => u.Id == userId)
                                  .Select(_ => new
                                   {
                                       Books    = context.CollectedBooks.Count(c => c.UserId == userId),
                                       Docs     = context.CollectedDocuments.Count(c => c.UserId == userId),
                                       Machines = context.OwnedMachines.Count(c => c.UserId == userId),
                                       Releases = context.CollectedSoftwareReleases.Count(c => c.UserId == userId),
                                       Issues   = context.CollectedMagazineIssues.Count(c => c.UserId == userId)
                                   })
                                  .FirstOrDefaultAsync();

        var summary = new UserCollectionSummaryDto
        {
            BookCount            = counts?.Books    ?? 0,
            DocumentCount        = counts?.Docs     ?? 0,
            MachineCount         = counts?.Machines ?? 0,
            SoftwareReleaseCount = counts?.Releases ?? 0,
            MagazineIssueCount   = counts?.Issues   ?? 0
        };

        return Ok(summary);
    }

    [HttpGet("profile/{username}/collection/books")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CollectedBookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<ActionResult<List<CollectedBookDto>>> GetCollectedBooksAsync(string username)
    {
        ApplicationUser user = await userManager.FindByNameAsync(username);

        if(user is null) return NotFound();

        List<CollectedBookDto> books = await context.CollectedBooks
                                                    .Where(c => c.UserId == user.Id)
                                                    .Include(c => c.Book)
                                                    .OrderByDescending(c => c.CreatedOn)
                                                    .Select(c => new CollectedBookDto
                                                    {
                                                        BookId      = c.BookId,
                                                        Title       = c.Book.Title,
                                                        CoverUrl    = c.Book.CoverGuid != null
                                                            ? $"photos/books/thumbs/jpeg/4k/{c.Book.CoverGuid}.jpg"
                                                            : null,
                                                        Published   = c.Book.Published,
                                                        CollectedOn = c.CreatedOn
                                                    })
                                                    .ToListAsync();

        return Ok(books);
    }

    [HttpGet("profile/{username}/collection/documents")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CollectedDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<ActionResult<List<CollectedDocumentDto>>> GetCollectedDocumentsAsync(string username)
    {
        ApplicationUser user = await userManager.FindByNameAsync(username);

        if(user is null) return NotFound();

        List<CollectedDocumentDto> documents = await context.CollectedDocuments
                                                            .Where(c => c.UserId == user.Id)
                                                            .Include(c => c.Document)
                                                            .OrderByDescending(c => c.CreatedOn)
                                                            .Select(c => new CollectedDocumentDto
                                                            {
                                                                DocumentId  = c.DocumentId,
                                                                Title       = c.Document.Title,
                                                                Published   = c.Document.Published,
                                                                CollectedOn = c.CreatedOn
                                                            })
                                                            .ToListAsync();

        return Ok(documents);
    }

    [HttpGet("profile/{username}/collection/machines")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CollectedMachineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<ActionResult<List<CollectedMachineDto>>> GetCollectedMachinesAsync(string username)
    {
        ApplicationUser user = await userManager.FindByNameAsync(username);

        if(user is null) return NotFound();

        List<CollectedMachineDto> machines = await context.OwnedMachines
                                                          .Where(c => c.UserId == user.Id)
                                                          .Include(c => c.Machine)
                                                          .ThenInclude(m => m.Company)
                                                          .OrderByDescending(c => c.CreatedOn)
                                                          .Select(c => new CollectedMachineDto
                                                          {
                                                              OwnedMachineId          = c.Id,
                                                              MachineId               = c.MachineId,
                                                              Name                    = c.Machine.Name,
                                                              CompanyName             = c.Machine.Company.Name,
                                                              Type                    = (int)c.Machine.Type,
                                                              CollectedOn             = c.CreatedOn,
                                                              AcquisitionDate         = c.AcquisitionDate,
                                                              AcquisitionDatePrecision = (int)c.AcquisitionDatePrecision,
                                                              Status                  = (int)c.Status,
                                                              Trade                   = c.Trade,
                                                              Boxed                   = c.Boxed,
                                                              Manuals                 = c.Manuals,
                                                              SerialNumber            = c.SerialNumberVisible
                                                                  ? c.SerialNumber
                                                                  : null,
                                                              SerialNumberVisible     = c.SerialNumberVisible
                                                          })
                                                          .ToListAsync();

        return Ok(machines);
    }

    [HttpGet("profile/{username}/collection/software-releases")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CollectedSoftwareReleaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<ActionResult<List<CollectedSoftwareReleaseDto>>> GetCollectedSoftwareReleasesAsync(
        string username)
    {
        ApplicationUser user = await userManager.FindByNameAsync(username);

        if(user is null) return NotFound();

        List<CollectedSoftwareReleaseDto> releases =
            await context.CollectedSoftwareReleases
                         .Where(c => c.UserId == user.Id)
                         .Include(c => c.SoftwareRelease)
                         .ThenInclude(r => r.SoftwareVersion)
                         .Include(c => c.SoftwareRelease)
                         .ThenInclude(r => r.Software)
                         .Include(c => c.SoftwareRelease)
                         .ThenInclude(r => r.Platform)
                         .OrderByDescending(c => c.CreatedOn)
                         .Select(c => new CollectedSoftwareReleaseDto
                         {
                             SoftwareReleaseId = c.SoftwareReleaseId,
                             SoftwareName = c.SoftwareRelease.SoftwareVersion != null
                                 ? c.SoftwareRelease.SoftwareVersion.Software.Name
                                 : c.SoftwareRelease.Software != null
                                     ? c.SoftwareRelease.Software.Name
                                     : c.SoftwareRelease.Title ?? "Unknown",
                             Version = c.SoftwareRelease.SoftwareVersion != null
                                 ? c.SoftwareRelease.SoftwareVersion.VersionString
                                 : null,
                             Title    = c.SoftwareRelease.Title,
                             Platform = c.SoftwareRelease.Platform != null
                                 ? c.SoftwareRelease.Platform.Name
                                 : null,
                             CollectedOn = c.CreatedOn
                         })
                         .ToListAsync();

        return Ok(releases);
    }

    [HttpGet("profile/{username}/collection/magazine-issues")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CollectedMagazineIssueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<ActionResult<List<CollectedMagazineIssueDto>>> GetCollectedMagazineIssuesAsync(string username)
    {
        ApplicationUser user = await userManager.FindByNameAsync(username);

        if(user is null) return NotFound();

        List<CollectedMagazineIssueDto> issues = await context.CollectedMagazineIssues
                                                              .Where(c => c.UserId == user.Id)
                                                              .Include(c => c.MagazineIssue)
                                                              .ThenInclude(i => i.Magazine)
                                                              .OrderByDescending(c => c.CreatedOn)
                                                              .Select(c => new CollectedMagazineIssueDto
                                                              {
                                                                  MagazineIssueId    = c.MagazineIssueId,
                                                                  MagazineId         = c.MagazineIssue.MagazineId,
                                                                  MagazineTitle      = c.MagazineIssue.Magazine.Title,
                                                                  Caption            = c.MagazineIssue.Caption,
                                                                  IssueNumber        = c.MagazineIssue.IssueNumber,
                                                                  Published          = c.MagazineIssue.Published,
                                                                  PublishedPrecision = (int)c.MagazineIssue.PublishedPrecision,
                                                                  CoverGuid          = c.MagazineIssue.CoverGuid,
                                                                  CollectedOn        = c.CreatedOn
                                                              })
                                                              .ToListAsync();

        return Ok(issues);
    }

    // ── Authenticated endpoints: manage own collection ──

    // Books

    [HttpGet("auth/me/collection/books/{bookId:long}")]
    [Authorize]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    public async Task<IActionResult> IsBookCollectedAsync(long bookId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        bool exists = await context.CollectedBooks.AnyAsync(c => c.UserId == userId && c.BookId == bookId);

        return Ok(exists);
    }

    [HttpPost("auth/me/collection/books/{bookId:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddBookToCollectionAsync(long bookId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        bool bookExists = await context.Set<Book>().AnyAsync(b => b.Id == bookId);
        if(!bookExists) return NotFound();

        bool alreadyCollected = await context.CollectedBooks.AnyAsync(c => c.UserId == userId && c.BookId == bookId);
        if(alreadyCollected) return Conflict();

        context.CollectedBooks.Add(new CollectedBook { UserId = userId, BookId = bookId });
        await context.SaveChangesWithUserAsync(userId);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpDelete("auth/me/collection/books/{bookId:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveBookFromCollectionAsync(long bookId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        CollectedBook entry = await context.CollectedBooks
                                            .FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == bookId);

        if(entry is null) return NotFound();

        context.CollectedBooks.Remove(entry);
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }

    // Documents

    [HttpGet("auth/me/collection/documents/{documentId:long}")]
    [Authorize]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    public async Task<IActionResult> IsDocumentCollectedAsync(long documentId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        bool exists =
            await context.CollectedDocuments.AnyAsync(c => c.UserId == userId && c.DocumentId == documentId);

        return Ok(exists);
    }

    [HttpPost("auth/me/collection/documents/{documentId:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddDocumentToCollectionAsync(long documentId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        bool documentExists = await context.Documents.AnyAsync(d => d.Id == documentId);
        if(!documentExists) return NotFound();

        bool alreadyCollected =
            await context.CollectedDocuments.AnyAsync(c => c.UserId == userId && c.DocumentId == documentId);

        if(alreadyCollected) return Conflict();

        context.CollectedDocuments.Add(new CollectedDocument { UserId = userId, DocumentId = documentId });
        await context.SaveChangesWithUserAsync(userId);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpDelete("auth/me/collection/documents/{documentId:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveDocumentFromCollectionAsync(long documentId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        CollectedDocument entry = await context.CollectedDocuments
                                                .FirstOrDefaultAsync(c => c.UserId == userId &&
                                                                          c.DocumentId == documentId);

        if(entry is null) return NotFound();

        context.CollectedDocuments.Remove(entry);
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }

    // Machines (uses OwnedMachine)

    [HttpGet("auth/me/collection/machines/{machineId:int}")]
    [Authorize]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    public async Task<IActionResult> IsMachineCollectedAsync(int machineId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        bool exists = await context.OwnedMachines.AnyAsync(c => c.UserId == userId && c.MachineId == machineId);

        return Ok(exists);
    }

    [HttpPost("auth/me/collection/machines/{machineId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMachineToCollectionAsync(int machineId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        bool machineExists = await context.Machines.AnyAsync(m => m.Id == machineId);
        if(!machineExists) return NotFound();

        bool alreadyCollected =
            await context.OwnedMachines.AnyAsync(c => c.UserId == userId && c.MachineId == machineId);

        if(alreadyCollected) return Conflict();

        context.OwnedMachines.Add(new OwnedMachine
        {
            UserId                  = userId,
            MachineId               = machineId,
            AcquisitionDate         = DateTime.UtcNow,
            AcquisitionDatePrecision = DatePrecision.Full,
            Status                  = StatusType.Unknown
        });

        await context.SaveChangesWithUserAsync(userId);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpDelete("auth/me/collection/machines/{machineId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMachineFromCollectionAsync(int machineId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        OwnedMachine entry = await context.OwnedMachines
                                           .FirstOrDefaultAsync(c => c.UserId == userId && c.MachineId == machineId);

        if(entry is null) return NotFound();

        context.OwnedMachines.Remove(entry);
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }

    // Software Releases

    [HttpGet("auth/me/collection/software-releases/{releaseId}")]
    [Authorize]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    public async Task<IActionResult> IsSoftwareReleaseCollectedAsync(ulong releaseId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        bool exists =
            await context.CollectedSoftwareReleases.AnyAsync(c => c.UserId     == userId &&
                                                                   c.SoftwareReleaseId == releaseId);

        return Ok(exists);
    }

    [HttpPost("auth/me/collection/software-releases/{releaseId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSoftwareReleaseToCollectionAsync(ulong releaseId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        bool releaseExists = await context.SoftwareReleases.AnyAsync(r => r.Id == releaseId);
        if(!releaseExists) return NotFound();

        bool alreadyCollected =
            await context.CollectedSoftwareReleases.AnyAsync(c => c.UserId     == userId &&
                                                                   c.SoftwareReleaseId == releaseId);

        if(alreadyCollected) return Conflict();

        context.CollectedSoftwareReleases.Add(new CollectedSoftwareRelease
        {
            UserId            = userId,
            SoftwareReleaseId = releaseId
        });

        await context.SaveChangesWithUserAsync(userId);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpDelete("auth/me/collection/software-releases/{releaseId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSoftwareReleaseFromCollectionAsync(ulong releaseId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        CollectedSoftwareRelease entry =
            await context.CollectedSoftwareReleases
                         .FirstOrDefaultAsync(c => c.UserId     == userId &&
                                                   c.SoftwareReleaseId == releaseId);

        if(entry is null) return NotFound();

        context.CollectedSoftwareReleases.Remove(entry);
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }

    // Magazine Issues

    [HttpGet("auth/me/collection/magazine-issues/{issueId:long}")]
    [Authorize]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    public async Task<IActionResult> IsMagazineIssueCollectedAsync(long issueId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        bool exists =
            await context.CollectedMagazineIssues.AnyAsync(c => c.UserId == userId && c.MagazineIssueId == issueId);

        return Ok(exists);
    }

    [HttpPost("auth/me/collection/magazine-issues/{issueId:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMagazineIssueToCollectionAsync(long issueId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        bool issueExists = await context.MagazineIssues.AnyAsync(i => i.Id == issueId);
        if(!issueExists) return NotFound();

        bool alreadyCollected =
            await context.CollectedMagazineIssues.AnyAsync(c => c.UserId == userId && c.MagazineIssueId == issueId);

        if(alreadyCollected) return Conflict();

        context.CollectedMagazineIssues.Add(new CollectedMagazineIssue
        {
            UserId          = userId,
            MagazineIssueId = issueId
        });

        await context.SaveChangesWithUserAsync(userId);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpDelete("auth/me/collection/magazine-issues/{issueId:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMagazineIssueFromCollectionAsync(long issueId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        CollectedMagazineIssue entry =
            await context.CollectedMagazineIssues
                         .FirstOrDefaultAsync(c => c.UserId == userId && c.MagazineIssueId == issueId);

        if(entry is null) return NotFound();

        context.CollectedMagazineIssues.Remove(entry);
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }
}
