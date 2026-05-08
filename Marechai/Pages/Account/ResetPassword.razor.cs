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
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Marechai.Pages.Account;

public partial class ResetPassword
{
    string _confirmPassword;
    string _errorMessage;
    bool   _isLoading;
    string _newPassword;
    bool   _succeeded;

    /// <summary>
    ///     Email address the reset link was generated for. Comes from the URL the server placed in the
    ///     reset email; we round-trip it back so the server can find the correct user when redeeming the
    ///     token.
    /// </summary>
    [SupplyParameterFromQuery(Name = "email")]
    public string Email { get; set; }

    /// <summary>The opaque <c>UserManager.GeneratePasswordResetTokenAsync</c> output, URL-decoded.</summary>
    [SupplyParameterFromQuery(Name = "token")]
    public string Token { get; set; }

    async Task SubmitAsync()
    {
        if(string.IsNullOrWhiteSpace(_newPassword))
        {
            _errorMessage = L["New password is required."];

            return;
        }

        if(_newPassword != _confirmPassword)
        {
            _errorMessage = L["Passwords do not match."];

            return;
        }

        _isLoading    = true;
        _errorMessage = null;

        (bool ok, string err) = await AuthService.ResetPasswordAsync(Email, Token, _newPassword);

        _isLoading = false;

        if(ok)
        {
            _succeeded = true;

            return;
        }

        _errorMessage = err ?? L["Could not reset password."];
    }

    Task OnConfirmKeyDown(KeyboardEventArgs e) => e.Key == "Enter" ? SubmitAsync() : Task.CompletedTask;
}
