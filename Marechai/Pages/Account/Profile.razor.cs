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
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;
using MudBlazor;

namespace Marechai.Pages.Account;

public partial class Profile
{
    // ── Account Settings state ──
    string?  _editEmail;
    string?  _editPhoneNumber;
    string?  _editUserName;
    string?  _errorMessage;
    bool     _isEditing;
    bool     _isLoading = true;
    bool     _isSaving;
    UserDto? _profile;
    string?  _successMessage;

    // ── Public Profile state ──
    PublicProfileDto? _publicProfile;
    string?           _editDisplayName;
    string?           _editBio;
    string?           _editWebsite;
    string?           _editLocation;
    bool              _editUseGravatar = true;
    string?           _editTwitter;
    string?           _editGitHub;
    string?           _editMastodon;
    string?           _editFacebook;
    string?           _editLinkedIn;
    bool              _isSavingPublic;
    string?           _publicSuccessMessage;
    string?           _publicErrorMessage;

    // ── Avatar state ──
    bool      _isUploadingAvatar;
    string?   _avatarMessage;
    Severity  _avatarMessageSeverity = Severity.Info;

    protected override async Task OnInitializedAsync()
    {
        _profile       = await AuthService.GetProfileAsync();
        _publicProfile = await AuthService.GetPublicProfileAsync();

        if(_publicProfile is not null)
            PopulatePublicProfileFields();

        _isLoading = false;
    }

    void PopulatePublicProfileFields()
    {
        _editDisplayName = _publicProfile?.DisplayName;
        _editBio         = _publicProfile?.Bio;
        _editWebsite     = _publicProfile?.Website;
        _editLocation    = _publicProfile?.Location;
        _editUseGravatar = _publicProfile?.UseGravatar ?? true;
        _editTwitter     = _publicProfile?.Twitter;
        _editGitHub      = _publicProfile?.GitHub;
        _editMastodon    = _publicProfile?.Mastodon;
        _editFacebook    = _publicProfile?.Facebook;
        _editLinkedIn    = _publicProfile?.LinkedIn;
    }

    // ── Account Settings methods ──

    void StartEdit()
    {
        _editUserName    = _profile?.UserName;
        _editEmail       = _profile?.Email;
        _editPhoneNumber = _profile?.PhoneNumber;
        _isEditing       = true;
        _errorMessage    = null;
        _successMessage  = null;
    }

    void CancelEdit()
    {
        _isEditing = false;
    }

    async Task SaveProfileAsync()
    {
        if(string.IsNullOrWhiteSpace(_editUserName))
        {
            _errorMessage = "Username is required.";

            return;
        }

        if(string.IsNullOrWhiteSpace(_editEmail))
        {
            _errorMessage = "Email is required.";

            return;
        }

        _isSaving     = true;
        _errorMessage = null;

        (bool succeeded, string? errorMessage) =
            await AuthService.UpdateProfileAsync(_editUserName, _editEmail, _editPhoneNumber);

        _isSaving = false;

        if(succeeded)
        {
            _profile        = await AuthService.GetProfileAsync();
            _isEditing      = false;
            _successMessage = "Profile updated successfully.";
        }
        else
        {
            _errorMessage = errorMessage ?? "Failed to update profile.";
        }
    }

    // ── Public Profile methods ──

    async Task SavePublicProfileAsync()
    {
        _isSavingPublic      = true;
        _publicErrorMessage  = null;

        var request = new UpdatePublicProfileRequest
        {
            DisplayName = string.IsNullOrWhiteSpace(_editDisplayName) ? null : _editDisplayName.Trim(),
            Bio         = string.IsNullOrWhiteSpace(_editBio) ? null : _editBio.Trim(),
            Website     = string.IsNullOrWhiteSpace(_editWebsite) ? null : _editWebsite.Trim(),
            Location    = string.IsNullOrWhiteSpace(_editLocation) ? null : _editLocation.Trim(),
            UseGravatar = _editUseGravatar,
            Twitter     = string.IsNullOrWhiteSpace(_editTwitter) ? null : _editTwitter.Trim(),
            GitHub      = string.IsNullOrWhiteSpace(_editGitHub) ? null : _editGitHub.Trim(),
            Mastodon    = string.IsNullOrWhiteSpace(_editMastodon) ? null : _editMastodon.Trim(),
            Facebook    = string.IsNullOrWhiteSpace(_editFacebook) ? null : _editFacebook.Trim(),
            LinkedIn    = string.IsNullOrWhiteSpace(_editLinkedIn) ? null : _editLinkedIn.Trim()
        };

        (bool succeeded, string? errorMessage) = await AuthService.UpdatePublicProfileAsync(request);

        _isSavingPublic = false;

        if(succeeded)
        {
            _publicProfile        = await AuthService.GetPublicProfileAsync();
            _publicSuccessMessage = "Public profile updated successfully.";

            if(_publicProfile is not null)
                PopulatePublicProfileFields();
        }
        else
        {
            _publicErrorMessage = errorMessage ?? "Failed to update public profile.";
        }
    }

    // ── Avatar methods ──

    async Task OnAvatarFileSelected(IBrowserFile file)
    {
        _isUploadingAvatar = true;
        _avatarMessage     = null;
        StateHasChanged();

        try
        {
            await using var stream = file.OpenReadStream(50 * 1024 * 1024);
            using var       ms     = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            var multipartBody = new MultipartBody();
            multipartBody.AddOrReplacePart("file", "application/octet-stream", ms);

            PublicProfileDto? result = await AuthService.UploadAvatarAsync(multipartBody);

            if(result is not null)
            {
                _publicProfile             = result;
                _avatarMessage             = "Avatar uploaded successfully.";
                _avatarMessageSeverity     = Severity.Success;
                PopulatePublicProfileFields();
            }
            else
            {
                _avatarMessage         = "Failed to upload avatar.";
                _avatarMessageSeverity = Severity.Error;
            }
        }
        catch(Exception ex)
        {
            _avatarMessage         = ex.Message;
            _avatarMessageSeverity = Severity.Error;
        }
        finally
        {
            _isUploadingAvatar = false;
            StateHasChanged();
        }
    }

    async Task DeleteAvatar()
    {
        _isUploadingAvatar = true;
        _avatarMessage     = null;
        StateHasChanged();

        (bool succeeded, string? error) = await AuthService.DeleteAvatarAsync();

        if(succeeded)
        {
            _publicProfile         = await AuthService.GetPublicProfileAsync();
            _avatarMessage         = "Avatar deleted.";
            _avatarMessageSeverity = Severity.Success;

            if(_publicProfile is not null)
                PopulatePublicProfileFields();
        }
        else
        {
            _avatarMessage         = error ?? "Failed to delete avatar.";
            _avatarMessageSeverity = Severity.Error;
        }

        _isUploadingAvatar = false;
        StateHasChanged();
    }
}
