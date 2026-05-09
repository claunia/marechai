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

public partial class DeleteAccount
{
    string _password;
    bool   _has2fa;
    string _provider = "authenticator";
    string _code;
    string _errorMessage;
    bool   _isLoading;
    bool   _submitted;

    async Task SubmitAsync()
    {
        if(string.IsNullOrWhiteSpace(_password))
        {
            _errorMessage = L["Current password is required."];

            return;
        }

        if(_has2fa && string.IsNullOrWhiteSpace(_code))
        {
            _errorMessage = L["Two-factor code is required."];

            return;
        }

        _isLoading    = true;
        _errorMessage = null;

        (bool ok, string err) = await AuthService.RequestAccountDeletionAsync(_password,
                                                                              _has2fa ? _provider : null,
                                                                              _has2fa ? _code?.Trim() : null);

        _isLoading = false;

        if(ok)
        {
            _submitted = true;

            return;
        }

        _errorMessage = err ?? L["Could not start account deletion."];
    }
}
