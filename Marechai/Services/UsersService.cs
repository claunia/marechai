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
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Extensions.Logging;

namespace Marechai.Services;

public sealed class UsersService(Marechai.ApiClient.Client client, ILogger<UsersService> logger)
{
    public async Task<List<UserDto>> GetAllAsync()
    {
        try
        {
            List<UserDto> users = await client.Users.GetAsync();

            return users ?? [];
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading users");

            return [];
        }
    }

    /// <summary>
    ///     Returns the total user count, optionally constrained by the same
    ///     <c>"{Column}||{Operator}||{Value}"</c> filter triples accepted by
    ///     <see cref="GetPagedAsync" />. Used by MudDataGrid server pagination.
    /// </summary>
    public async Task<int> GetUsersCountAsync(IReadOnlyList<string> filters = null,
                                              CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Users.Count.GetAsync(config =>
            {
                if(filters is { Count: > 0 }) config.QueryParameters.Filters = filters.ToArray();
            }, cancellationToken);

            return count ?? 0;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error fetching user count");

            return 0;
        }
    }

    /// <summary>
    ///     Fetches a page of users with server-side sorting + filtering. Filters
    ///     are <c>"{Column}||{Operator}||{Value}"</c> triples mirroring MudBlazor's
    ///     <c>FilterDefinition</c> shape; <c>UsersController.ApplyFilters</c>
    ///     translates them into LINQ predicates.
    /// </summary>
    public async Task<List<UserDto>> GetPagedAsync(int skip, int take, string sortBy, bool sortDescending,
                                                   IReadOnlyList<string> filters,
                                                   CancellationToken cancellationToken = default)
    {
        try
        {
            List<UserDto> users = await client.Users.GetAsync(config =>
            {
                config.QueryParameters.Skip = skip;
                config.QueryParameters.Take = take;
                if(!string.IsNullOrWhiteSpace(sortBy)) config.QueryParameters.SortBy = sortBy;
                if(sortDescending) config.QueryParameters.SortDescending = true;
                if(filters is { Count: > 0 }) config.QueryParameters.Filters = filters.ToArray();
            }, cancellationToken);

            return users ?? [];
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading users page");

            return [];
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> CreateAsync(string email, string userName,
                                                                          string password, string phoneNumber)
    {
        try
        {
            var request = new CreateUserRequest
            {
                Email       = email,
                UserName    = userName,
                Password    = password,
                PhoneNumber = phoneNumber
            };

            await client.Users.PostAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Create user failed");

            return (false, ex.Detail ?? ex.Title ?? "Failed to create user.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Create user failed");

            return (false, "An error occurred while creating user.");
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> UpdateAsync(string id, string email, string userName,
                                                                          string phoneNumber)
    {
        try
        {
            var request = new UpdateUserRequest
            {
                Email       = email,
                UserName    = userName,
                PhoneNumber = phoneNumber
            };

            await client.Users[id].PutAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Update user failed");

            return (false, ex.Detail ?? ex.Title ?? "Failed to update user.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Update user failed");

            return (false, "An error occurred while updating user.");
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> DeleteAsync(string id)
    {
        try
        {
            await client.Users[id].DeleteAsync();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Delete user failed");

            return (false, ex.Detail ?? ex.Title ?? "Failed to delete user.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Delete user failed");

            return (false, "An error occurred while deleting user.");
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> ChangePasswordAsync(string id, string newPassword)
    {
        try
        {
            var request = new ChangePasswordRequest
            {
                NewPassword = newPassword
            };

            await client.Users[id].Password.PostAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Change password failed");

            return (false, ex.Detail ?? ex.Title ?? "Failed to change password.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Change password failed");

            return (false, "An error occurred while changing password.");
        }
    }

    public async Task<List<string>> GetRolesAsync()
    {
        try
        {
            List<string> roles = await client.Users.Roles.GetAsync();

            return roles ?? [];
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading roles");

            return [];
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> AddRoleAsync(string id, string roleName)
    {
        try
        {
            var request = new UserRoleRequest
            {
                RoleName = roleName
            };

            await client.Users[id].Roles.PostAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Add role failed");

            return (false, ex.Detail ?? ex.Title ?? "Failed to add role.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Add role failed");

            return (false, "An error occurred while adding role.");
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> RemoveRoleAsync(string id, string roleName)
    {
        try
        {
            await client.Users[id].Roles[roleName].DeleteAsync();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Remove role failed");

            return (false, ex.Detail ?? ex.Title ?? "Failed to remove role.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Remove role failed");

            return (false, "An error occurred while removing role.");
        }
    }

    public async Task<(BulkOperationResult Result, string ErrorMessage)> BulkDeleteAsync(List<string> userIds)
    {
        try
        {
            var request = new BulkUserIdsRequest
            {
                UserIds = userIds
            };

            BulkOperationResult result = await client.Users.BulkDelete.PostAsync(request);

            return (result, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Bulk delete failed");

            return (null, ex.Detail ?? ex.Title ?? "Failed to bulk delete users.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Bulk delete failed");

            return (null, "An error occurred while bulk deleting users.");
        }
    }

    public async Task<(BulkOperationResult Result, string ErrorMessage)> BulkAddRoleAsync(List<string> userIds,
        string roleName)
    {
        try
        {
            var request = new BulkRoleRequest
            {
                UserIds  = userIds,
                RoleName = roleName
            };

            BulkOperationResult result = await client.Users.BulkAddRole.PostAsync(request);

            return (result, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Bulk add role failed");

            return (null, ex.Detail ?? ex.Title ?? "Failed to bulk add role.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Bulk add role failed");

            return (null, "An error occurred while bulk adding role.");
        }
    }

    public async Task<(BulkOperationResult Result, string ErrorMessage)> BulkRemoveRoleAsync(List<string> userIds,
        string roleName)
    {
        try
        {
            var request = new BulkRoleRequest
            {
                UserIds  = userIds,
                RoleName = roleName
            };

            BulkOperationResult result = await client.Users.BulkRemoveRole.PostAsync(request);

            return (result, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Bulk remove role failed");

            return (null, ex.Detail ?? ex.Title ?? "Failed to bulk remove role.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Bulk remove role failed");

            return (null, "An error occurred while bulk removing role.");
        }
    }

    public async Task<(BulkOperationResult Result, string ErrorMessage)> BulkSetLockoutAsync(List<string> userIds,
        bool enable)
    {
        try
        {
            var request = new BulkLockoutRequest
            {
                UserIds = userIds,
                Enable  = enable
            };

            BulkOperationResult result = await client.Users.BulkLockout.PostAsync(request);

            return (result, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Bulk lockout update failed");

            return (null, ex.Detail ?? ex.Title ?? "Failed to bulk update lockout.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Bulk lockout update failed");

            return (null, "An error occurred while bulk updating lockout.");
        }
    }

    /// <summary>
    ///     Admin/UberAdmin action: forcibly disables ALL two-factor methods on the target account (clears the
    ///     authenticator key, recovery codes, and per-method flags). Used as a last-resort recovery path when a
    ///     user has lost access to their second factor.
    /// </summary>
    public async Task<(bool Succeeded, string ErrorMessage)> DisableTwoFactorAsync(string userId)
    {
        try
        {
            await client.Users[userId].TwoFactor.Disable.PostAsync();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Admin disable 2FA failed");

            return (false, ex.Detail ?? ex.Title ?? "Failed to disable 2FA.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Admin disable 2FA failed");

            return (false, "An error occurred while disabling 2FA.");
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> GrantInvitationCodesAsync(string userId, int count)
    {
        try
        {
            var request = new GrantInvitationCodesRequest { Count = count };
            await client.Users[userId].InvitationCodes.PostAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Grant invitation codes failed");

            return (false, ex.Detail ?? ex.Title ?? "Failed to grant invitation codes.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Grant invitation codes failed");

            return (false, "An error occurred while granting invitation codes.");
        }
    }
}
