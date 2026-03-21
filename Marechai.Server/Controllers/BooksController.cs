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
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Marechai.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Marechai.Server.Controllers;

[Route("/books")]
[ApiController]
public class BooksController(MarechaiContext context, IConfiguration configuration) : ControllerBase
{
    static readonly HashSet<string> _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"];

    static readonly HashSet<string> _allowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp"
    ];

    readonly string _assetRootPath = configuration["AssetRootPath"]!;
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<BookDto>> GetAsync() => context.Books.OrderBy(b => b.NativeTitle)
                                                    .ThenBy(b => b.Published)
                                                    .ThenBy(b => b.Title)
                                                    .Select(b => new BookDto
                                                     {
                                                         Id                     = b.Id,
                                                         Title                  = b.Title,
                                                         NativeTitle            = b.NativeTitle,
                                                         Published              = b.Published,
                                                         Isbn                   = b.Isbn,
                                                         CountryId              = b.CountryId,
                                                         Pages                  = b.Pages,
                                                         Edition                = b.Edition,
                                                         PreviousId             = b.PreviousId,
                                                         SourceId               = b.SourceId,
                                                         Country                = b.Country.Name,
                                                         CoverGuid              = b.CoverGuid,
                                                         OriginalCoverExtension = b.OriginalCoverExtension
                                                     })
                                                    .ToListAsync();

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<BookDto> GetAsync(long id) => context.Books.Where(b => b.Id == id)
                                                     .Select(b => new BookDto
                                                      {
                                                          Id                     = b.Id,
                                                          Title                  = b.Title,
                                                          NativeTitle            = b.NativeTitle,
                                                          Published              = b.Published,
                                                          Isbn                   = b.Isbn,
                                                          CountryId              = b.CountryId,
                                                          Pages                  = b.Pages,
                                                          Edition                = b.Edition,
                                                          PreviousId             = b.PreviousId,
                                                          SourceId               = b.SourceId,
                                                          Country                = b.Country.Name,
                                                          CoverGuid              = b.CoverGuid,
                                                          OriginalCoverExtension = b.OriginalCoverExtension
                                                      })
                                                     .FirstOrDefaultAsync();

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] BookDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        Book model = await context.Books.FindAsync(id);

        if(model is null) return NotFound();

        model.Title       = dto.Title;
        model.NativeTitle = dto.NativeTitle;
        model.Published   = dto.Published;
        model.CountryId   = dto.CountryId;
        model.Isbn        = dto.Isbn;
        model.Pages       = dto.Pages;
        model.Edition     = dto.Edition;
        model.PreviousId  = dto.PreviousId;
        model.SourceId    = dto.SourceId;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] BookDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Book
        {
            Title       = dto.Title,
            NativeTitle = dto.NativeTitle,
            Published   = dto.Published,
            CountryId   = dto.CountryId,
            Isbn        = dto.Isbn,
            Pages       = dto.Pages,
            Edition     = dto.Edition,
            PreviousId  = dto.PreviousId,
            SourceId    = dto.SourceId
        };

        await context.Books.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpGet("{id:long}/synopses")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<DocumentSynopsisDto>> GetSynopsesAsync(long id) => context.BookSynopses
       .Where(s => s.BookId == id)
       .Select(s => new DocumentSynopsisDto
        {
            Id           = s.Id,
            Text         = s.Text,
            LanguageCode = s.LanguageCode,
            Language     = s.Language.ReferenceName
        })
       .ToListAsync();

    [HttpGet("{id:long}/synopsis")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<DocumentSynopsisDto> GetSynopsisAsync(long id, [FromQuery] string lang = "eng")
    {
        DocumentSynopsisDto synopsis = await context.BookSynopses
                                                    .Where(s => s.BookId == id && s.LanguageCode == lang)
                                                    .Select(s => new DocumentSynopsisDto
                                                     {
                                                         Id           = s.Id,
                                                         Text         = s.Text,
                                                         LanguageCode = s.LanguageCode,
                                                         Language     = s.Language.ReferenceName
                                                     })
                                                    .FirstOrDefaultAsync();

        if(synopsis is null && lang != "eng")
            synopsis = await context.BookSynopses
                                    .Where(s => s.BookId == id && s.LanguageCode == "eng")
                                    .Select(s => new DocumentSynopsisDto
                                     {
                                         Id           = s.Id,
                                         Text         = s.Text,
                                         LanguageCode = s.LanguageCode,
                                         Language     = s.Language.ReferenceName
                                     })
                                    .FirstOrDefaultAsync();

        return synopsis;
    }

    [HttpPost("{id:long}/synopsis")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateOrUpdateSynopsisAsync(
        long id, [FromBody] DocumentSynopsisDto synopsis)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        BookSynopsis current = await context.BookSynopses
                                            .FirstOrDefaultAsync(s => s.BookId       == id &&
                                                                      s.LanguageCode == synopsis.LanguageCode);

        if(current is null)
        {
            current = new BookSynopsis
            {
                BookId       = id,
                LanguageCode = synopsis.LanguageCode,
                Text         = synopsis.Text
            };

            await context.BookSynopses.AddAsync(current);
        }
        else
        {
            current.Text = synopsis.Text;
        }

        await context.SaveChangesWithUserAsync(userId);

        return current.Id;
    }

    [HttpDelete("{id:long}/synopsis/{languageCode}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteSynopsisAsync(long id, string languageCode)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        BookSynopsis synopsis = await context.BookSynopses
                                             .FirstOrDefaultAsync(s => s.BookId       == id &&
                                                                       s.LanguageCode == languageCode);

        if(synopsis is null) return NotFound();

        context.BookSynopses.Remove(synopsis);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        Book item = await context.Books.FindAsync(id);

        if(item is null) return NotFound();

        context.Books.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost("{id:long}/cover/upload")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BookDto>> UploadCoverAsync(long id, IFormFile file)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(file is null || file.Length == 0)
            return BadRequest("No file provided.");

        if(file.Length > 50 * 1024 * 1024)
            return BadRequest("File exceeds 50 MB limit.");

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        if(!_allowedExtensions.Contains(extension))
            return BadRequest("Unsupported file format. Accepted: JPEG, PNG, WebP, TIFF, BMP.");

        if(!string.IsNullOrEmpty(file.ContentType) &&
           !_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Unsupported content type.");

        Book book = await context.Books.FindAsync(id);

        if(book is null) return NotFound();

        // If cover already exists, delete old files
        if(book.CoverGuid.HasValue)
            DeleteCoverFiles(book.CoverGuid.Value);

        Guid coverGuid = Guid.NewGuid();

        // Save original file to disk
        Photos.EnsureCreated(_assetRootPath, false, "book-covers");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "book-covers", "originals");
        string originalPath = Path.Combine(originalsDir, $"{coverGuid}{extension}");

        await using(var fs = new FileStream(originalPath, FileMode.CreateNew, FileAccess.Write))
        {
            await file.CopyToAsync(fs);
        }

        // Fire conversion worker (generates all format/resolution variants)
        string sourceFormat = extension.TrimStart('.');

        _ = Task.Run(() =>
        {
            var photos = new Photos();
            photos.ConversionWorker(_assetRootPath, coverGuid, originalPath, sourceFormat, false, "book-covers");
        });

        // Update book record
        book.CoverGuid              = coverGuid;
        book.OriginalCoverExtension = extension.TrimStart('.');
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new BookDto
        {
            Id                     = book.Id,
            Title                  = book.Title,
            NativeTitle            = book.NativeTitle,
            Published              = book.Published,
            Isbn                   = book.Isbn,
            CountryId              = book.CountryId,
            Pages                  = book.Pages,
            Edition                = book.Edition,
            PreviousId             = book.PreviousId,
            SourceId               = book.SourceId,
            CoverGuid              = book.CoverGuid,
            OriginalCoverExtension = book.OriginalCoverExtension
        });
    }

    [HttpDelete("{id:long}/cover")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteCoverAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        Book book = await context.Books.FindAsync(id);

        if(book is null) return NotFound();

        if(!book.CoverGuid.HasValue) return NoContent();

        DeleteCoverFiles(book.CoverGuid.Value);

        book.CoverGuid              = null;
        book.OriginalCoverExtension = null;
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }

    void DeleteCoverFiles(Guid coverGuid)
    {
        string photosRoot = Path.Combine(_assetRootPath, "photos", "book-covers");
        string guidStr    = coverGuid.ToString();

        // Delete original
        DeleteFilesByPattern(Path.Combine(photosRoot, "originals"), $"{guidStr}.*");

        // Delete all format/resolution variants (full + thumbnails)
        string[] formats     = ["jpeg", "jp2k", "webp", "heif", "avif"];
        string[] resolutions = ["hd", "1440p", "4k"];

        foreach(string format in formats)
        {
            string ext = format switch
            {
                "jpeg" => ".jpg",
                "jp2k" => ".jp2",
                "webp" => ".webp",
                "heif" => ".heic",
                "avif" => ".avif",
                _      => $".{format}"
            };

            foreach(string res in resolutions)
            {
                string fullPath  = Path.Combine(photosRoot, format, res, $"{guidStr}{ext}");
                string thumbPath = Path.Combine(photosRoot, "thumbs", format, res, $"{guidStr}{ext}");

                if(System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);

                if(System.IO.File.Exists(thumbPath))
                    System.IO.File.Delete(thumbPath);
            }
        }
    }

    static void DeleteFilesByPattern(string directory, string pattern)
    {
        if(!System.IO.Directory.Exists(directory)) return;

        foreach(string file in System.IO.Directory.GetFiles(directory, pattern))
            System.IO.File.Delete(file);
    }
}