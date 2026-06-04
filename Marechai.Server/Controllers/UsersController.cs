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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Data.Models;
using Marechai.Database.Models;
using Marechai.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[ApiController]
[Route("users")]
[Authorize(Roles = ApplicationRole.RoleUberAdmin)]
public class UsersController(UserManager<ApplicationUser> userManager,
                             UserAccountDeletionService   userAccountDeletionService,
                             MarechaiContext              context) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK,
                          Description = "Returns users, optionally paged / filtered / sorted.")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Produces("application/json")]
    public async Task<ActionResult<List<UserDto>>> GetAll([FromQuery] int?  skip           = null,
                                                          [FromQuery] int?  take           = null,
                                                          [FromQuery] string sortBy         = null,
                                                          [FromQuery] bool   sortDescending = false,
                                                          [FromQuery(Name = "filters")] string[] filters = null,
                                                          CancellationToken cancellationToken = default)
    {
        IQueryable<ApplicationUser> query = ApplyFilters(context.Users.AsNoTracking(), filters, context);

        IOrderedQueryable<ApplicationUser> ordered = sortBy switch
        {
            "Email" => sortDescending
                           ? query.OrderByDescending(u => u.Email)
                           : query.OrderBy(u => u.Email),
            "UserName" => sortDescending
                              ? query.OrderByDescending(u => u.UserName)
                              : query.OrderBy(u => u.UserName),
            "PhoneNumber" => sortDescending
                                 ? query.OrderByDescending(u => u.PhoneNumber)
                                 : query.OrderBy(u => u.PhoneNumber),
            _ => query.OrderBy(u => u.Email)
        };

        IQueryable<ApplicationUser> paged = ordered;
        if(skip.HasValue) paged = paged.Skip(skip.Value);
        if(take.HasValue) paged = paged.Take(take.Value);

        // Materialize the page's user rows first, then batch-load every role assignment in a single round-trip.
        // Avoids the previous N+1 (one `GetRolesAsync` call per user) when listing all users.
        List<ApplicationUser> users = await paged.ToListAsync(cancellationToken);

        List<string> userIds = users.Select(u => u.Id).ToList();

        Dictionary<string, List<string>> roleMap = userIds.Count == 0
            ? []
            : await (from ur in context.UserRoles.AsNoTracking()
                     join r in context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                     where userIds.Contains(ur.UserId)
                     select new { ur.UserId, r.Name }).GroupBy(x => x.UserId)
                                                     .ToDictionaryAsync(g => g.Key,
                                                                        g => g.Select(x => x.Name).ToList(),
                                                                        cancellationToken);

        var userDtos = new List<UserDto>(users.Count);

        foreach(ApplicationUser user in users)
        {
            userDtos.Add(new UserDto
            {
                Id                    = user.Id,
                UserName              = user.UserName!,
                Email                 = user.Email!,
                EmailConfirmed        = user.EmailConfirmed,
                PhoneNumber           = user.PhoneNumber,
                PhoneNumberConfirmed  = user.PhoneNumberConfirmed,
                LockoutEnabled        = user.LockoutEnabled,
                LockoutEnd            = user.LockoutEnd?.ToString("O"),
                AccessFailedCount     = user.AccessFailedCount,
                TwoFactorEnabled      = user.TwoFactorEnabled,
                AuthenticatorEnabled  = user.TwoFactorViaAuthenticator,
                EmailTwoFactorEnabled = user.TwoFactorViaEmail,
                Roles                 = roleMap.TryGetValue(user.Id, out List<string> r) ? r : []
            });
        }

        return Ok(userDtos);
    }

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK, Description = "Returns the total user count matching the optional filters.")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Produces("application/json")]
    public Task<int> GetCountAsync([FromQuery(Name = "filters")] string[] filters = null,
                                   CancellationToken cancellationToken = default) =>
        ApplyFilters(context.Users.AsNoTracking(), filters, context).CountAsync(cancellationToken);

    /// <summary>
    /// Translates MudDataGrid <c>FilterDefinition</c>s wired over the network as
    /// <c>"{Column}||{Operator}||{Value}"</c> triples into LINQ predicates against
    /// <see cref="ApplicationUser"/>. Recognized columns mirror the
    /// <c>PropertyColumn</c>/<c>TemplateColumn PropertyName</c> values emitted
    /// by the admin grid: <c>Email</c>, <c>UserName</c>, <c>PhoneNumber</c>,
    /// <c>Roles</c>. The <c>Roles</c> column is multi-valued; its predicates
    /// match against ANY role assignment via a join into
    /// <c>UserRoles</c>+<c>Roles</c>. Unknown columns/operators are silently
    /// ignored so MudBlazor never 400s the listing call.
    /// </summary>
    static IQueryable<ApplicationUser> ApplyFilters(IQueryable<ApplicationUser> query, string[] filters,
                                                    MarechaiContext ctx)
    {
        if(filters is null || filters.Length == 0) return query;

        foreach(string raw in filters)
        {
            if(string.IsNullOrWhiteSpace(raw)) continue;

            string[] parts = raw.Split("||", 3, StringSplitOptions.None);
            if(parts.Length < 2) continue;

            string column   = parts[0];
            string op       = parts[1];
            string value    = parts.Length >= 3 ? parts[2] : string.Empty;
            bool   isEmpty  = op == "is empty";
            bool   isNotEmp = op == "is not empty";

            // Skip non-empty-check operators with no value supplied so a stray
            // open-but-unfilled filter UI doesn't accidentally hide every row.
            if(!isEmpty && !isNotEmp && string.IsNullOrEmpty(value)) continue;

            switch(column)
            {
                case "Email":
                    query = op switch
                    {
                        "contains"     => query.Where(u => u.Email.Contains(value)),
                        "not contains" => query.Where(u => !u.Email.Contains(value)),
                        "equals"       => query.Where(u => u.Email == value),
                        "not equals"   => query.Where(u => u.Email != value),
                        "starts with"  => query.Where(u => u.Email.StartsWith(value)),
                        "ends with"    => query.Where(u => u.Email.EndsWith(value)),
                        "is empty"     => query.Where(u => u.Email == null || u.Email == string.Empty),
                        "is not empty" => query.Where(u => u.Email != null && u.Email != string.Empty),
                        _              => query
                    };
                    break;

                case "UserName":
                    query = op switch
                    {
                        "contains"     => query.Where(u => u.UserName.Contains(value)),
                        "not contains" => query.Where(u => !u.UserName.Contains(value)),
                        "equals"       => query.Where(u => u.UserName == value),
                        "not equals"   => query.Where(u => u.UserName != value),
                        "starts with"  => query.Where(u => u.UserName.StartsWith(value)),
                        "ends with"    => query.Where(u => u.UserName.EndsWith(value)),
                        "is empty"     => query.Where(u => u.UserName == null || u.UserName == string.Empty),
                        "is not empty" => query.Where(u => u.UserName != null && u.UserName != string.Empty),
                        _              => query
                    };
                    break;

                case "PhoneNumber":
                    query = op switch
                    {
                        "contains"     => query.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(value)),
                        "not contains" => query.Where(u => u.PhoneNumber == null || !u.PhoneNumber.Contains(value)),
                        "equals"       => query.Where(u => u.PhoneNumber == value),
                        "not equals"   => query.Where(u => u.PhoneNumber != value),
                        "starts with"  => query.Where(u => u.PhoneNumber != null && u.PhoneNumber.StartsWith(value)),
                        "ends with"    => query.Where(u => u.PhoneNumber != null && u.PhoneNumber.EndsWith(value)),
                        "is empty"     => query.Where(u => u.PhoneNumber == null || u.PhoneNumber == string.Empty),
                        "is not empty" => query.Where(u => u.PhoneNumber != null && u.PhoneNumber != string.Empty),
                        _              => query
                    };
                    break;

                case "Roles":
                    // Predicates over the multi-valued role assignment: "contains foo" means at least one of
                    // the user's role names contains "foo"; "not contains" means none of them does; "equals"
                    // means at least one role name equals exactly. "is empty" / "is not empty" check for the
                    // presence of any role at all (membership) rather than empty role-name strings, which
                    // ApplicationRole's seeded set never produces.
                    query = op switch
                    {
                        "contains" => query.Where(u =>
                            ctx.UserRoles.Any(ur => ur.UserId == u.Id &&
                                                    ctx.Roles.Any(r => r.Id == ur.RoleId &&
                                                                       r.Name.Contains(value)))),
                        "not contains" => query.Where(u =>
                            !ctx.UserRoles.Any(ur => ur.UserId == u.Id &&
                                                     ctx.Roles.Any(r => r.Id == ur.RoleId &&
                                                                        r.Name.Contains(value)))),
                        "equals" => query.Where(u =>
                            ctx.UserRoles.Any(ur => ur.UserId == u.Id &&
                                                    ctx.Roles.Any(r => r.Id == ur.RoleId &&
                                                                       r.Name == value))),
                        "not equals" => query.Where(u =>
                            !ctx.UserRoles.Any(ur => ur.UserId == u.Id &&
                                                     ctx.Roles.Any(r => r.Id == ur.RoleId &&
                                                                        r.Name == value))),
                        "starts with" => query.Where(u =>
                            ctx.UserRoles.Any(ur => ur.UserId == u.Id &&
                                                    ctx.Roles.Any(r => r.Id == ur.RoleId &&
                                                                       r.Name.StartsWith(value)))),
                        "ends with" => query.Where(u =>
                            ctx.UserRoles.Any(ur => ur.UserId == u.Id &&
                                                    ctx.Roles.Any(r => r.Id == ur.RoleId &&
                                                                       r.Name.EndsWith(value)))),
                        "is empty"     => query.Where(u => !ctx.UserRoles.Any(ur => ur.UserId == u.Id)),
                        "is not empty" => query.Where(u => ctx.UserRoles.Any(ur => ur.UserId == u.Id)),
                        _              => query
                    };
                    break;
            }
        }

        return query;
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

        if(user == null) return Problem(detail: "User not found", statusCode: StatusCodes.Status404NotFound);

        IList<string> roles = await userManager.GetRolesAsync(user);

        return Ok(new UserDto
        {
            Id                    = user.Id,
            UserName              = user.UserName!,
            Email                 = user.Email!,
            EmailConfirmed        = user.EmailConfirmed,
            PhoneNumber           = user.PhoneNumber,
            PhoneNumberConfirmed  = user.PhoneNumberConfirmed,
            LockoutEnabled        = user.LockoutEnabled,
            LockoutEnd            = user.LockoutEnd?.ToString("O"),
            AccessFailedCount     = user.AccessFailedCount,
            TwoFactorEnabled      = user.TwoFactorEnabled,
            AuthenticatorEnabled  = user.TwoFactorViaAuthenticator,
            EmailTwoFactorEnabled = user.TwoFactorViaEmail,
            Roles                 = roles.ToList()
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
                                   Id                    = user.Id,
                                   UserName              = user.UserName,
                                   Email                 = user.Email,
                                   EmailConfirmed        = user.EmailConfirmed,
                                   PhoneNumber           = user.PhoneNumber,
                                   PhoneNumberConfirmed  = user.PhoneNumberConfirmed,
                                   LockoutEnabled        = user.LockoutEnabled,
                                   LockoutEnd            = user.LockoutEnd?.ToString("O"),
                                   AccessFailedCount     = user.AccessFailedCount,
                                   TwoFactorEnabled      = user.TwoFactorEnabled,
                                   AuthenticatorEnabled  = user.TwoFactorViaAuthenticator,
                                   EmailTwoFactorEnabled = user.TwoFactorViaEmail,
                                   Roles                 = roles.ToList()
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

        if(user == null) return Problem(detail: "User not found", statusCode: StatusCodes.Status404NotFound);

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
            Id                    = user.Id,
            UserName              = user.UserName,
            Email                 = user.Email,
            EmailConfirmed        = user.EmailConfirmed,
            PhoneNumber           = user.PhoneNumber,
            PhoneNumberConfirmed  = user.PhoneNumberConfirmed,
            LockoutEnabled        = user.LockoutEnabled,
            LockoutEnd            = user.LockoutEnd?.ToString("O"),
            AccessFailedCount     = user.AccessFailedCount,
            TwoFactorEnabled      = user.TwoFactorEnabled,
            AuthenticatorEnabled  = user.TwoFactorViaAuthenticator,
            EmailTwoFactorEnabled = user.TwoFactorViaEmail,
            Roles                 = roles.ToList()
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

        if(user == null) return Problem(detail: "User not found", statusCode: StatusCodes.Status404NotFound);

        // Delegate to the shared deletion service so the admin path applies the same anonymise-content +
        // hard-delete-state + avatar-file-cleanup as the GDPR self-service path. The admin path is
        // immediate (no grace window).
        string actorId = User.FindFirstValue(System.Security.Claims.ClaimTypes.Sid);

        bool ok = await userAccountDeletionService.PurgeAsync(id, actorUserIdForLog: actorId ?? "admin");

        if(!ok) return Problem(detail: "Failed to purge user account.", statusCode: StatusCodes.Status400BadRequest);

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

        if(user == null) return Problem(detail: "User not found", statusCode: StatusCodes.Status404NotFound);

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

        if(user == null) return Problem(detail: "User not found", statusCode: StatusCodes.Status404NotFound);

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

        if(user == null) return Problem(detail: "User not found", statusCode: StatusCodes.Status404NotFound);

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

    [HttpPost("bulk-delete")]
    [ProducesResponseType(typeof(BulkOperationResult), StatusCodes.Status200OK,
                          Description = "Deletes multiple users.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<BulkOperationResult>> BulkDelete([FromBody] BulkUserIdsRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        var result = new BulkOperationResult
        {
            Errors = []
        };

        foreach(string userId in request.UserIds)
        {
            ApplicationUser user = await userManager.FindByIdAsync(userId);

            if(user == null)
            {
                result.FailedCount++;
                result.Errors.Add($"User '{userId}' not found.");

                continue;
            }

            IdentityResult deleteResult = await userManager.DeleteAsync(user);

            if(deleteResult.Succeeded)
                result.SucceededCount++;
            else
            {
                result.FailedCount++;
                result.Errors.Add($"Failed to delete '{user.Email}': {string.Join(", ", deleteResult.Errors.Select(e => e.Description))}");
            }
        }

        return Ok(result);
    }

    [HttpPost("bulk-add-role")]
    [ProducesResponseType(typeof(BulkOperationResult), StatusCodes.Status200OK,
                          Description = "Adds a role to multiple users.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<BulkOperationResult>> BulkAddRole([FromBody] BulkRoleRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        var result = new BulkOperationResult
        {
            Errors = []
        };

        foreach(string userId in request.UserIds)
        {
            ApplicationUser user = await userManager.FindByIdAsync(userId);

            if(user == null)
            {
                result.FailedCount++;
                result.Errors.Add($"User '{userId}' not found.");

                continue;
            }

            IdentityResult roleResult = await userManager.AddToRoleAsync(user, request.RoleName);

            if(roleResult.Succeeded)
                result.SucceededCount++;
            else
            {
                result.FailedCount++;
                result.Errors.Add($"Failed to add role to '{user.Email}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        return Ok(result);
    }

    [HttpPost("bulk-remove-role")]
    [ProducesResponseType(typeof(BulkOperationResult), StatusCodes.Status200OK,
                          Description = "Removes a role from multiple users.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<BulkOperationResult>> BulkRemoveRole([FromBody] BulkRoleRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        var result = new BulkOperationResult
        {
            Errors = []
        };

        foreach(string userId in request.UserIds)
        {
            ApplicationUser user = await userManager.FindByIdAsync(userId);

            if(user == null)
            {
                result.FailedCount++;
                result.Errors.Add($"User '{userId}' not found.");

                continue;
            }

            IdentityResult roleResult = await userManager.RemoveFromRoleAsync(user, request.RoleName);

            if(roleResult.Succeeded)
                result.SucceededCount++;
            else
            {
                result.FailedCount++;
                result.Errors.Add($"Failed to remove role from '{user.Email}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        return Ok(result);
    }

    [HttpPost("bulk-lockout")]
    [ProducesResponseType(typeof(BulkOperationResult), StatusCodes.Status200OK,
                          Description = "Enables or disables lockout for multiple users.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<BulkOperationResult>> BulkLockout([FromBody] BulkLockoutRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        var result = new BulkOperationResult
        {
            Errors = []
        };

        foreach(string userId in request.UserIds)
        {
            ApplicationUser user = await userManager.FindByIdAsync(userId);

            if(user == null)
            {
                result.FailedCount++;
                result.Errors.Add($"User '{userId}' not found.");

                continue;
            }

            user.LockoutEnabled = request.Enable;
            IdentityResult updateResult = await userManager.UpdateAsync(user);

            if(updateResult.Succeeded)
                result.SucceededCount++;
            else
            {
                result.FailedCount++;
                result.Errors.Add($"Failed to update lockout for '{user.Email}': {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
            }
        }

        return Ok(result);
    }

    /// <summary>
    ///     Disables ALL two-factor methods for the target user. Available to <c>Admin</c> and <c>UberAdmin</c>
    ///     (overrides the class-level <c>UberAdmin</c>-only restriction). Clears the global flag, the per-method
    ///     flags, the authenticator key and any outstanding recovery codes, and resets the access-failed counter.
    /// </summary>
    [HttpPost("{id}/two-factor/disable")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableTwoFactor(string id)
    {
        ApplicationUser user = await userManager.FindByIdAsync(id);

        if(user == null) return Problem(detail: "User not found", statusCode: StatusCodes.Status404NotFound);

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);
        await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 0);

        user.TwoFactorViaAuthenticator = false;
        user.TwoFactorViaEmail         = false;

        await userManager.UpdateAsync(user);
        await userManager.ResetAccessFailedCountAsync(user);

        return NoContent();
    }
}