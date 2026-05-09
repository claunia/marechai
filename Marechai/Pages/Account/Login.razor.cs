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

using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.Services;
using Microsoft.AspNetCore.Components.Web;

namespace Marechai.Pages.Account;

public partial class Login
{
    string       _email;
    string       _errorMessage;
    string       _infoMessage;
    bool         _isLoading;
    string       _password;
    bool         _emailNotConfirmed;

    // Two-factor state
    bool         _requiresTwoFactor;
    string       _twoFactorToken;
    List<string> _availableMethods = [];
    string       _selectedProvider = "authenticator";
    string       _code;
    bool         _recoveryMode;
    string       _recoveryCode;

    async Task LoginAsync()
    {
        if(string.IsNullOrWhiteSpace(_email))
        {
            _errorMessage = "Email is required.";

            return;
        }

        if(string.IsNullOrWhiteSpace(_password))
        {
            _errorMessage = "Password is required.";

            return;
        }

        _isLoading    = true;
        _errorMessage = null;
        _infoMessage  = null;
        _emailNotConfirmed = false;

        LoginResult result = await AuthService.LoginAsync(_email, _password);

        _isLoading = false;

        if(result.Succeeded)
        {
            Navigation.NavigateTo("/");

            return;
        }

        if(result.RequiresTwoFactor)
        {
            _requiresTwoFactor = true;
            _twoFactorToken    = result.TwoFactorToken;
            _availableMethods  = result.AvailableMethods is { Count: > 0 } ? new List<string>(result.AvailableMethods) : ["authenticator"];
            _selectedProvider  = _availableMethods[0];

            return;
        }

        if(result.EmailNotConfirmed)
        {
            _emailNotConfirmed = true;
            _errorMessage      = result.ErrorMessage ?? "Please confirm your email address before signing in.";

            return;
        }

        _errorMessage = result.ErrorMessage ?? "Login failed.";
    }

    async Task SendEmailCodeAsync()
    {
        _isLoading    = true;
        _errorMessage = null;
        _infoMessage  = null;

        (bool ok, string masked, string err) = await AuthService.SendLoginEmailCodeAsync(_twoFactorToken);

        _isLoading = false;

        if(ok)
            _infoMessage = string.IsNullOrEmpty(masked) ? "Code sent to your email." : $"Code sent to {masked}.";
        else
            _errorMessage = err ?? "Could not send code.";
    }

    async Task VerifyCodeAsync()
    {
        if(string.IsNullOrWhiteSpace(_code))
        {
            _errorMessage = "Verification code is required.";

            return;
        }

        _isLoading    = true;
        _errorMessage = null;

        (bool ok, string err) = await AuthService.VerifyTwoFactorAsync(_twoFactorToken, _selectedProvider, _code.Trim());

        _isLoading = false;

        if(ok)
            Navigation.NavigateTo("/");
        else
            _errorMessage = err ?? "Invalid verification code.";
    }

    async Task VerifyRecoveryAsync()
    {
        if(string.IsNullOrWhiteSpace(_recoveryCode))
        {
            _errorMessage = "Recovery code is required.";

            return;
        }

        _isLoading    = true;
        _errorMessage = null;

        (bool ok, string err) = await AuthService.VerifyRecoveryAsync(_twoFactorToken, _recoveryCode.Trim());

        _isLoading = false;

        if(ok)
            Navigation.NavigateTo("/");
        else
            _errorMessage = err ?? "Invalid recovery code.";
    }

    void OnEmailKeyDown(KeyboardEventArgs e)
    {
        if(e.Key == "Enter") StateHasChanged();
    }

    void OnPasswordKeyDown(KeyboardEventArgs e)
    {
        if(e.Key == "Enter") _ = LoginAsync();
    }

    void OnCodeKeyDown(KeyboardEventArgs e)
    {
        if(e.Key == "Enter") _ = VerifyCodeAsync();
    }

    void OnRecoveryKeyDown(KeyboardEventArgs e)
    {
        if(e.Key == "Enter") _ = VerifyRecoveryAsync();
    }
}
