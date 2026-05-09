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

namespace Marechai.Pages.Account;

public partial class ConfirmEmail
{
    [SupplyParameterFromQuery(Name = "email")]
    public string Email { get; set; }

    [SupplyParameterFromQuery(Name = "token")]
    public string Token { get; set; }

    bool   _isLoading = true;
    bool   _succeeded;
    string _errorMessage;

    // The confirmation POST is dispatched from OnAfterRenderAsync(firstRender) instead of
    // OnInitializedAsync because Blazor Server runs OnInitializedAsync twice: once during
    // server-side prerender and again after the SignalR circuit connects. The first call
    // would consume the token successfully (and send the welcome email), and the second
    // call would then see EmailConfirmed=true on the server and return the generic
    // "Invalid or expired confirmation link." error &mdash; which the user would see
    // even though their account is in fact confirmed.
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(!firstRender) return;

        if(string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Token))
        {
            _isLoading    = false;
            _succeeded    = false;
            _errorMessage = L["The confirmation link is missing required fields."];
            StateHasChanged();

            return;
        }

        (bool ok, string err) = await AuthService.ConfirmEmailAsync(Email, Token);

        _isLoading    = false;
        _succeeded    = ok;
        _errorMessage = ok ? null : err;
        StateHasChanged();
    }
}
