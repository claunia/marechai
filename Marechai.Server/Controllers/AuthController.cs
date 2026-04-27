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
using Marechai.Data.Models;
using Marechai.Database.Models;
using Marechai.Helpers;
using Marechai.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Marechai.Server.Controllers;

[ApiController]
[Route("auth")]
public class AuthController
    (UserManager<ApplicationUser> userManager, MarechaiContext context, TokenService tokenService,
     IConfiguration               configuration) : ControllerBase
{
    static readonly HashSet<string> _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"];

    static readonly HashSet<string> _allowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp"
    ];

    readonly string _assetRootPath = configuration["AssetRootPath"]!;
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(AuthResponse),
                          StatusCodes.Status200OK,
                          Description =
                              "Authenticates a user with email and password, returning an access token if successful.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        ApplicationUser managedUser = await userManager.FindByEmailAsync(request.Email);

        if(managedUser == null) return Unauthorized("Bad credentials");

        bool isPasswordValid = await userManager.CheckPasswordAsync(managedUser, request.Password);

        if(!isPasswordValid) return Unauthorized("Bad credentials");

        ApplicationUser userInDb = context.Users.FirstOrDefault(u => u.Email == request.Email);

        if(userInDb is null) return Unauthorized();

        string accessToken = tokenService.CreateToken(userInDb, await userManager.GetRolesAsync(managedUser));
        await context.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            Message   = "",
            Succeeded = true,
            Token     = accessToken
        });
    }

    [HttpGet]
    [Route("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK, Description = "Returns the current user's profile.")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        IList<string> roles = await userManager.GetRolesAsync(user);

        return Ok(new UserDto
        {
            Id                   = user.Id,
            UserName             = user.UserName!,
            Email                = user.Email!,
            EmailConfirmed       = user.EmailConfirmed,
            PhoneNumber          = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LockoutEnabled       = user.LockoutEnabled,
            LockoutEnd           = user.LockoutEnd?.ToString("O"),
            AccessFailedCount    = user.AccessFailedCount,
            Roles                = roles.ToList()
        });
    }

    [HttpPut]
    [Route("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK, Description = "Updates the current user's profile.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        user.UserName    = request.UserName;
        user.Email       = request.Email;
        user.PhoneNumber = request.PhoneNumber;

        IdentityResult result = await userManager.UpdateAsync(user);

        if(!result.Succeeded) return BadRequest(result.Errors);

        IList<string> roles = await userManager.GetRolesAsync(user);

        return Ok(new UserDto
        {
            Id                   = user.Id,
            UserName             = user.UserName,
            Email                = user.Email,
            EmailConfirmed       = user.EmailConfirmed,
            PhoneNumber          = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LockoutEnabled       = user.LockoutEnabled,
            LockoutEnd           = user.LockoutEnd?.ToString("O"),
            AccessFailedCount    = user.AccessFailedCount,
            Roles                = roles.ToList()
        });
    }

    [HttpPost]
    [Route("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Password changed successfully.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeOwnPasswordRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        IdentityResult result = await userManager.ChangePasswordAsync(user, request.CurrentPassword,
                                                                      request.NewPassword);

        if(!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpGet]
    [Route("me/public-profile")]
    [Authorize]
    [ProducesResponseType(typeof(PublicProfileDto), StatusCodes.Status200OK,
                          Description = "Returns the current user's public profile.")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    public async Task<ActionResult<PublicProfileDto>> GetPublicProfile()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        return Ok(ProfileController.MapToPublicProfile(user));
    }

    [HttpPut]
    [Route("me/public-profile")]
    [Authorize]
    [ProducesResponseType(typeof(PublicProfileDto), StatusCodes.Status200OK,
                          Description = "Updates the current user's public profile.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<PublicProfileDto>> UpdatePublicProfile(
        [FromBody] UpdatePublicProfileRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        user.DisplayName = request.DisplayName;
        user.Bio         = request.Bio;
        user.Website     = request.Website;
        user.Location    = request.Location;
        user.UseGravatar = request.UseGravatar;
        user.Twitter     = request.Twitter;
        user.GitHub      = request.GitHub;
        user.Mastodon    = request.Mastodon;
        user.Facebook    = request.Facebook;
        user.LinkedIn    = request.LinkedIn;

        IdentityResult result = await userManager.UpdateAsync(user);

        if(!result.Succeeded) return BadRequest(result.Errors);

        return Ok(ProfileController.MapToPublicProfile(user));
    }

    [HttpPost]
    [Route("me/avatar/upload")]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(PublicProfileDto), StatusCodes.Status200OK,
                          Description = "Uploads a custom avatar for the current user.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PublicProfileDto>> UploadAvatarAsync(IFormFile file)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

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

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        // If avatar already exists, delete old files
        if(user.AvatarGuid.HasValue)
            DeleteAvatarFiles(user.AvatarGuid.Value);

        Guid avatarGuid = Guid.NewGuid();

        Photos.EnsureCreated(_assetRootPath, false, "avatars");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "avatars", "originals");
        string originalPath = Path.Combine(originalsDir, $"{avatarGuid}{extension}");

        await using(var fs = new FileStream(originalPath, FileMode.CreateNew, FileAccess.Write))
        {
            await file.CopyToAsync(fs);
        }

        string sourceFormat = extension.TrimStart('.');

        _ = Task.Run(() =>
        {
            var photos = new Photos();
            photos.ConversionWorker(_assetRootPath, avatarGuid, originalPath, sourceFormat, false, "avatars");
        });

        user.AvatarGuid              = avatarGuid;
        user.OriginalAvatarExtension = sourceFormat;

        await userManager.UpdateAsync(user);

        return Ok(ProfileController.MapToPublicProfile(user));
    }

    [HttpDelete]
    [Route("me/avatar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Avatar deleted successfully.")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAvatarAsync()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null) return Unauthorized();

        if(user.AvatarGuid.HasValue)
            DeleteAvatarFiles(user.AvatarGuid.Value);

        user.AvatarGuid              = null;
        user.OriginalAvatarExtension = null;

        await userManager.UpdateAsync(user);

        return NoContent();
    }

    void DeleteAvatarFiles(Guid avatarGuid)
    {
        string photosRoot = Path.Combine(_assetRootPath, "photos", "avatars");

        // Delete original
        string originalsDir = Path.Combine(photosRoot, "originals");

        if(Directory.Exists(originalsDir))
        {
            foreach(string f in Directory.GetFiles(originalsDir, $"{avatarGuid}.*"))
                System.IO.File.Delete(f);
        }

        // Delete all generated variants
        string[] formats = ["jpeg", "jp2k", "webp", "heif", "avif"];
        string[] sizes   = ["hd", "1440p", "4k"];

        foreach(string format in formats)
        {
            foreach(string size in sizes)
            {
                string fullPath  = Path.Combine(photosRoot, format, size, $"{avatarGuid}.*");
                string thumbPath = Path.Combine(photosRoot, "thumbs", format, size, $"{avatarGuid}.*");

                foreach(string f in Directory.GetFiles(Path.GetDirectoryName(fullPath)!,
                                                       Path.GetFileName(fullPath)))
                    System.IO.File.Delete(f);

                string thumbDir = Path.GetDirectoryName(thumbPath)!;

                if(Directory.Exists(thumbDir))
                {
                    foreach(string f in Directory.GetFiles(thumbDir, Path.GetFileName(thumbPath)))
                        System.IO.File.Delete(f);
                }
            }
        }
    }
}