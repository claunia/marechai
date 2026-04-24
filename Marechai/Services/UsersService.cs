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
            List<UserDto>? users = await client.Users.GetAsync();

            return users ?? [];
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading users");

            return [];
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> CreateAsync(string email, string userName,
                                                                          string password, string? phoneNumber)
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

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateAsync(string id, string email, string userName,
                                                                          string? phoneNumber)
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

    public async Task<(bool Succeeded, string? ErrorMessage)> DeleteAsync(string id)
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

    public async Task<(bool Succeeded, string? ErrorMessage)> ChangePasswordAsync(string id, string newPassword)
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
            List<string>? roles = await client.Users.Roles.GetAsync();

            return roles ?? [];
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading roles");

            return [];
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> AddRoleAsync(string id, string roleName)
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

    public async Task<(bool Succeeded, string? ErrorMessage)> RemoveRoleAsync(string id, string roleName)
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
}
