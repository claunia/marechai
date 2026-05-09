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
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Marechai.Pages.Account;

public partial class DeletionPending
{
    [Inject] IJSRuntime Js { get; set; }

    AccountDeletionStatusDto _status;
    bool                     _isCancelling;
    string                   _message;

    protected override async Task OnInitializedAsync()
    {
        _status = await AuthService.GetAccountDeletionStatusAsync() ?? new AccountDeletionStatusDto
        {
            IsPending = false
        };
    }

    async Task CancelAsync()
    {
        _isCancelling = true;
        bool ok = await AuthService.CancelAccountDeletionAsync();
        _isCancelling = false;

        if(ok)
        {
            _status  = await AuthService.GetAccountDeletionStatusAsync();
            _message = L["Deletion cancelled. You can continue using your account normally."];
        }
        else
        {
            _message = L["Could not cancel the deletion. Please try again or contact support."];
        }
    }

    async Task DownloadDataAsync()
    {
        byte[] bytes = await AuthService.ExportDataAsync();

        if(bytes is null || bytes.Length == 0)
        {
            _message = L["Could not generate your data export."];

            return;
        }

        string filename = $"marechai-user-data-{DateTime.UtcNow:yyyyMMdd}.json";
        string base64   = Convert.ToBase64String(bytes);

        try
        {
            await Js.InvokeVoidAsync("marechaiDownloadBlob.save", filename, "application/json", base64);
        }
        catch(Exception)
        {
            _message = L["Could not start the download. Please try a different browser."];
        }
    }
}
