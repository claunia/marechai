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
using Marechai.Services;

namespace Marechai.Pages.Account;

public partial class ChangePassword
{
    string _confirmPassword;
    string _currentPassword;
    string _errorMessage;
    bool    _isSaving;
    string _newPassword;
    string _successMessage;

    async Task ChangePasswordAsync()
    {
        _errorMessage   = null;
        _successMessage = null;

        if(string.IsNullOrWhiteSpace(_currentPassword))
        {
            _errorMessage = L["Current password is required."];

            return;
        }

        if(string.IsNullOrWhiteSpace(_newPassword))
        {
            _errorMessage = L["New password is required."];

            return;
        }

        if(_newPassword.Length < 6)
        {
            _errorMessage = L["New password must be at least 6 characters."];

            return;
        }

        if(_newPassword != _confirmPassword)
        {
            _errorMessage = L["Passwords do not match."];

            return;
        }

        _isSaving = true;

        (bool succeeded, string errorMessage) = await AuthService.ChangePasswordAsync(_currentPassword, _newPassword);

        _isSaving = false;

        if(succeeded)
        {
            _successMessage  = L["Password changed successfully."];
            _currentPassword = null;
            _newPassword     = null;
            _confirmPassword = null;
        }
        else
        {
            _errorMessage = errorMessage ?? L["Failed to change password."];
        }
    }
}
