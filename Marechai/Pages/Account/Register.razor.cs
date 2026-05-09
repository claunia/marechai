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

namespace Marechai.Pages.Account;

public partial class Register
{
    string _invitationCode;
    string _email;
    string _userName;
    string _displayName;
    string _password;
    string _confirmPassword;
    string _errorMessage;
    bool   _isLoading;
    bool   _submitted;

    async Task SubmitAsync()
    {
        if(string.IsNullOrWhiteSpace(_invitationCode))
        {
            _errorMessage = L["Invitation code is required."];

            return;
        }

        if(string.IsNullOrWhiteSpace(_email))
        {
            _errorMessage = L["Email is required."];

            return;
        }

        if(string.IsNullOrWhiteSpace(_userName))
        {
            _errorMessage = L["Username is required."];

            return;
        }

        if(string.IsNullOrWhiteSpace(_displayName))
        {
            _errorMessage = L["Display name is required."];

            return;
        }

        if(string.IsNullOrWhiteSpace(_password))
        {
            _errorMessage = L["Password is required."];

            return;
        }

        if(_password != _confirmPassword)
        {
            _errorMessage = L["Passwords do not match."];

            return;
        }

        _isLoading    = true;
        _errorMessage = null;

        (bool ok, string err) = await AuthService.RegisterAsync(_email.Trim(),
                                                                 _userName.Trim(),
                                                                 _password,
                                                                 _displayName.Trim(),
                                                                 _invitationCode.Trim().ToUpperInvariant());

        _isLoading = false;

        if(ok)
        {
            _submitted = true;

            return;
        }

        _errorMessage = err ?? L["Registration failed."];
    }
}
