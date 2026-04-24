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

using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.Pages.Account;

public partial class Profile
{
    string? _editEmail;
    string? _editPhoneNumber;
    string? _editUserName;
    string? _errorMessage;
    bool    _isEditing;
    bool    _isLoading = true;
    bool    _isSaving;
    UserDto? _profile;
    string? _successMessage;

    protected override async Task OnInitializedAsync()
    {
        _profile   = await AuthService.GetProfileAsync();
        _isLoading = false;
    }

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
}
