/******************************************************************************
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
using System.Threading.Tasks;
using Marechai.Data.Models;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Marechai.Server.Controllers;

[ApiController]
[Route("users")]
[Authorize(Roles = ApplicationRole.RoleUberAdmin)]
public class UsersController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK, Description = "Returns a list of all users.")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Produces("application/json")]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users    = userManager.Users.ToList();
        var userDtos = new List<UserDto>();

        foreach(ApplicationUser user in users)
        {
            IList<string> roles = await userManager.GetRolesAsync(user);

            userDtos.Add(new UserDto
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

        return Ok(userDtos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK, Description = "Returns a specific user by ID.")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<ActionResult<UserDto>> GetById(string id)
    {
        ApplicationUser user = await userManager.FindByIdAsync(id);

        if(user == null) return NotFound("User not found");

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

    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created, Description = "Creates a new user.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        var user = new ApplicationUser
        {
            UserName    = request.UserName,
            Email       = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        IdentityResult result = await userManager.CreateAsync(user, request.Password);

        if(!result.Succeeded) return BadRequest(result.Errors);

        IList<string> roles = await userManager.GetRolesAsync(user);

        return CreatedAtAction(nameof(GetById),
                               new
                               {
                                   id = user.Id
                               },
                               new UserDto
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

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK, Description = "Updates an existing user.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<UserDto>> Update(string id, [FromBody] UpdateUserRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        ApplicationUser user = await userManager.FindByIdAsync(id);

        if(user == null) return NotFound("User not found");

        user.UserName    = request.UserName;
        user.Email       = request.Email;
        user.PhoneNumber = request.PhoneNumber;

        if(request.EmailConfirmed.HasValue) user.EmailConfirmed = request.EmailConfirmed.Value;

        if(request.LockoutEnabled.HasValue) user.LockoutEnabled = request.LockoutEnabled.Value;

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

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Deletes a user.")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        ApplicationUser user = await userManager.FindByIdAsync(id);

        if(user == null) return NotFound("User not found");

        IdentityResult result = await userManager.DeleteAsync(user);

        if(!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{id}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Changes a user's password.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Consumes("application/json")]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        ApplicationUser user = await userManager.FindByIdAsync(id);

        if(user == null) return NotFound("User not found");

        // Remove old password and set new one
        IdentityResult removeResult = await userManager.RemovePasswordAsync(user);

        if(!removeResult.Succeeded) return BadRequest(removeResult.Errors);

        IdentityResult addResult = await userManager.AddPasswordAsync(user, request.NewPassword);

        if(!addResult.Succeeded) return BadRequest(addResult.Errors);

        return NoContent();
    }

    [HttpPost("{id}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Adds a role to a user.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Consumes("application/json")]
    public async Task<IActionResult> AddRole(string id, [FromBody] UserRoleRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        ApplicationUser user = await userManager.FindByIdAsync(id);

        if(user == null) return NotFound("User not found");

        IdentityResult result = await userManager.AddToRoleAsync(user, request.RoleName);

        if(!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpDelete("{id}/roles/{roleName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Removes a role from a user.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(string id, string roleName)
    {
        ApplicationUser user = await userManager.FindByIdAsync(id);

        if(user == null) return NotFound("User not found");

        IdentityResult result = await userManager.RemoveFromRoleAsync(user, roleName);

        if(!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpGet("roles")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK, Description = "Returns all available roles.")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Produces("application/json")]
    public IActionResult GetAllRoles()
    {
        var roles = new List<string>
        {
            ApplicationRole.RoleUberAdmin,
            ApplicationRole.RoleWriter,
            ApplicationRole.RoleProofreader,
            ApplicationRole.RoleTranslator,
            ApplicationRole.RoleSuperTranslator,
            ApplicationRole.RoleCollaborator,
            ApplicationRole.RoleCurator,
            ApplicationRole.RolePhysicalCurator,
            ApplicationRole.RoleTechnician,
            ApplicationRole.RoleSuperTechnician,
            ApplicationRole.RoleAdmin,
            ApplicationRole.RoleNone
        };

        return Ok(roles);
    }
}