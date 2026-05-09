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
using Microsoft.AspNetCore.Components.Web;

namespace Marechai.Pages.Account;

public partial class ResendConfirmation
{
    string _email;
    string _errorMessage;
    bool   _isLoading;
    bool   _submitted;

    async Task SubmitAsync()
    {
        if(string.IsNullOrWhiteSpace(_email))
        {
            _errorMessage = L["Email is required."];

            return;
        }

        _isLoading    = true;
        _errorMessage = null;

        // Server always responds with 204 regardless of whether the email matches a real (unconfirmed)
        // account, and AuthService mirrors that on the client. We always show the same generic
        // confirmation, again to defeat enumeration of registered addresses.
        await AuthService.ResendConfirmationAsync(_email.Trim());

        _isLoading = false;
        _submitted = true;
    }

    Task OnEmailKeyDown(KeyboardEventArgs e) => e.Key == "Enter" ? SubmitAsync() : Task.CompletedTask;
}
