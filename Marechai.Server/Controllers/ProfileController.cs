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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Data.Models;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("profile")]
[ApiController]
public class ProfileController(UserManager<ApplicationUser> userManager, MarechaiContext context) : ControllerBase
{
    [HttpGet("{username}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicProfileDto), StatusCodes.Status200OK,
                          Description = "Returns the public profile for the specified user.")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<ActionResult<PublicProfileDto>> GetPublicProfileAsync(string username)
    {
        ApplicationUser user = await userManager.FindByNameAsync(username);

        if(user is null) return NotFound();

        return Ok(await MapToPublicProfileAsync(user, userManager));
    }

    internal static async Task<PublicProfileDto> MapToPublicProfileAsync(ApplicationUser user,
                                                                         UserManager<ApplicationUser> mgr)
    {
        PublicProfileDto dto = MapToPublicProfile(user);

        if(mgr is not null)
        {
            dto.IsAdmin = await mgr.IsInRoleAsync(user, "Admin")
                       || await mgr.IsInRoleAsync(user, "UberAdmin");
            dto.IsCollaborator = await mgr.IsInRoleAsync(user, "Collaborator");
        }

        return dto;
    }

    internal static PublicProfileDto MapToPublicProfile(ApplicationUser user) => new()
    {
        UserName                = user.UserName!,
        DisplayName             = user.DisplayName,
        Bio                     = user.Bio,
        Website                 = user.Website,
        Location                = user.Location,
        AvatarUrl               = GetAvatarUrl(user),
        UseGravatar             = user.UseGravatar,
        Twitter                 = user.Twitter,
        GitHub                  = user.GitHub,
        Mastodon                = user.Mastodon,
        Facebook                = user.Facebook,
        LinkedIn                = user.LinkedIn,
        AvatarGuid              = user.AvatarGuid,
        OriginalAvatarExtension = user.OriginalAvatarExtension
    };

    static string GetAvatarUrl(ApplicationUser user)
    {
        if(user.UseGravatar)
        {
            if(string.IsNullOrWhiteSpace(user.Email)) return null;

            string email    = user.Email.Trim().ToLowerInvariant();
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(email));

            var sb = new StringBuilder(hashBytes.Length * 2);

            foreach(byte b in hashBytes)
                sb.Append(b.ToString("x2"));

            return $"https://www.gravatar.com/avatar/{sb}?s=256&d=identicon";
        }

        if(user.AvatarGuid.HasValue)
            return $"photos/avatars/thumbs/jpeg/hd/{user.AvatarGuid}.jpg";

        return null;
    }

    [HttpGet("{username}/reviews")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SoftwareUserReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<SoftwareUserReviewDto>>> GetUserReviewsAsync(string username)
    {
        ApplicationUser user = await userManager.FindByNameAsync(username);

        if(user is null) return NotFound();

        string callerId = User.FindFirstValue(ClaimTypes.Sid);
        bool   isAdmin  = User.IsInRole("Admin") || User.IsInRole("UberAdmin");
        bool   isSelf   = callerId == user.Id;

        var query = context.SoftwareUserReviews.Where(r => r.UserId == user.Id);

        // Non-admins and non-self can only see non-anonymous reviews
        if(!isAdmin && !isSelf)
            query = query.Where(r => !r.IsAnonymous);

        var reviews = await query.OrderByDescending(r => r.CreatedOn)
                                 .Select(r => new
                                  {
                                      r.Id,
                                      r.UserId,
                                      r.SoftwareId,
                                      SoftwareName = r.Software.Name,
                                      r.TheGood,
                                      r.TheBad,
                                      r.TheUgly,
                                      r.IsAnonymous,
                                      r.CreatedOn,
                                      r.UpdatedOn,
                                      ThumbsUp   = r.Votes.Count(v => v.IsUpvote),
                                      ThumbsDown = r.Votes.Count(v => !v.IsUpvote)
                                  })
                                 .ToListAsync();

        // Load ratings for user
        var ratingsBySoftware = await context.SoftwareUserRatings
                                             .Where(r => r.UserId == user.Id)
                                             .ToDictionaryAsync(r => r.SoftwareId, r => r.Rating);

        string avatarUrl       = GetAvatarUrl(user);
        bool   isCollaborator  = await userManager.IsInRoleAsync(user, "Collaborator");

        return Ok(reviews.Select(r => new SoftwareUserReviewDto
        {
            Id             = r.Id,
            UserId         = r.UserId,
            UserName       = user.UserName,
            DisplayName    = user.DisplayName,
            AvatarUrl      = avatarUrl,
            SoftwareId     = r.SoftwareId,
            SoftwareName   = r.SoftwareName,
            TheGood        = r.TheGood,
            TheBad         = r.TheBad,
            TheUgly        = r.TheUgly,
            IsAnonymous    = r.IsAnonymous,
            Rating         = ratingsBySoftware.TryGetValue(r.SoftwareId, out float rating) ? rating : null,
            ThumbsUp       = r.ThumbsUp,
            ThumbsDown     = r.ThumbsDown,
            CreatedOn      = r.CreatedOn,
            UpdatedOn      = r.UpdatedOn,
            IsCollaborator = isCollaborator
        }).ToList());
    }
}
