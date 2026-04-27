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
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

public sealed class AuthService(Marechai.ApiClient.Client             client,
                                TokenProvider                          tokenProvider,
                                JwtAuthenticationStateProvider         authStateProvider,
                                ILogger<AuthService>                   logger)
{
    public async Task<(bool Succeeded, string? ErrorMessage)> LoginAsync(string email, string password)
    {
        try
        {
            var request = new AuthRequest
            {
                Email    = email,
                Password = password
            };

            AuthResponse? response = await client.Auth.Login.PostAsync(request);

            if(response is null)
                return (false, "No response from server.");

            if(response.Succeeded != true)
                return (false, response.Message ?? "Login failed.");

            if(string.IsNullOrWhiteSpace(response.Token))
                return (false, "No token received.");

            tokenProvider.SetToken(response.Token);
            authStateProvider.NotifyUserAuthentication();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Login failed with ProblemDetails");

            return (false, ex.Detail ?? ex.Title ?? "Login failed.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Login failed");

            return (false, "An error occurred during login.");
        }
    }

    public void Logout()
    {
        tokenProvider.RemoveToken();
        authStateProvider.NotifyUserLogout();
    }

    public async Task<UserDto?> GetProfileAsync()
    {
        try
        {
            return await client.Auth.Me.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading profile");

            return null;
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateProfileAsync(string userName, string email,
                                                                                  string? phoneNumber)
    {
        try
        {
            var request = new UpdateProfileRequest
            {
                UserName    = userName,
                Email       = email,
                PhoneNumber = phoneNumber
            };

            await client.Auth.Me.PutAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Profile update failed");

            return (false, ex.Detail ?? ex.Title ?? "Update failed.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Profile update failed");

            return (false, "An error occurred while updating profile.");
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> ChangePasswordAsync(string currentPassword,
                                                                                   string newPassword)
    {
        try
        {
            var request = new ChangeOwnPasswordRequest
            {
                CurrentPassword = currentPassword,
                NewPassword     = newPassword
            };

            await client.Auth.ChangePassword.PostAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Password change failed");

            return (false, ex.Detail ?? ex.Title ?? "Password change failed.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Password change failed");

            return (false, "An error occurred while changing password.");
        }
    }

    public async Task<PublicProfileDto?> GetPublicProfileAsync()
    {
        try
        {
            return await client.Auth.Me.PublicProfile.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading public profile");

            return null;
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdatePublicProfileAsync(
        UpdatePublicProfileRequest request)
    {
        try
        {
            await client.Auth.Me.PublicProfile.PutAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Public profile update failed");

            return (false, ex.Detail ?? ex.Title ?? "Update failed.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Public profile update failed");

            return (false, "An error occurred while updating public profile.");
        }
    }

    public async Task<PublicProfileDto?> UploadAvatarAsync(MultipartBody body)
    {
        try
        {
            return await client.Auth.Me.Avatar.Upload.PostAsync(body);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Avatar upload failed");

            return null;
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> DeleteAvatarAsync()
    {
        try
        {
            await client.Auth.Me.Avatar.DeleteAsync();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Avatar delete failed");

            return (false, ex.Detail ?? ex.Title ?? "Delete failed.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Avatar delete failed");

            return (false, "An error occurred while deleting avatar.");
        }
    }
}
