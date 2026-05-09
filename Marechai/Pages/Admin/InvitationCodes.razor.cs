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
using Marechai.ApiClient.Models;
using Microsoft.JSInterop;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class InvitationCodes
{
    List<InvitationCodeDto> _codes = [];
    bool                    _isLoading = true;
    bool                    _isGenerating;
    bool                    _unusedOnly = true;
    string                  _errorMessage;
    string                  _lastGeneratedCode;

    protected override Task OnInitializedAsync() => LoadAsync();

    async Task LoadAsync()
    {
        _isLoading = true;
        _codes     = await Service.GetAllAsync(_unusedOnly ? true : null);
        _isLoading = false;
    }

    Task OnFilterChanged(bool _) => LoadAsync();

    async Task GenerateCodeAsync()
    {
        _isGenerating  = true;
        _errorMessage  = null;
        _lastGeneratedCode = null;

        (InvitationCodeDto created, string err) = await Service.CreateAsync();

        _isGenerating = false;

        if(created is null)
        {
            _errorMessage = err ?? "Failed to generate invitation code.";

            return;
        }

        _lastGeneratedCode = created.Code;

        // Best-effort clipboard copy. If the browser denies clipboard access we still display the code in
        // the success banner so the admin can copy it manually.
        try
        {
            await Js.InvokeVoidAsync("navigator.clipboard.writeText", created.Code);
        }
        catch
        {
            // Ignore: most browsers refuse clipboard.writeText outside a user-gesture chain in some
            // contexts. The code is still visible in the success banner.
        }

        await LoadAsync();
    }

    async Task CopyToClipboardAsync(string code)
    {
        try
        {
            await Js.InvokeVoidAsync("navigator.clipboard.writeText", code);
        }
        catch
        {
            // Silently swallow; the user can select the code text manually.
        }
    }

    async Task ConfirmRevokeAsync(InvitationCodeDto entry)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText, $"Revoke invitation code '{entry.Code}'? This cannot be undone."
            }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DeleteConfirmDialog>("Revoke invitation code",
                                                                                     parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth = MaxWidth.ExtraSmall,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is not { Canceled: false }) return;

        (bool ok, string err) = await Service.RevokeAsync(entry.Code);

        if(ok)
            await LoadAsync();
        else
            _errorMessage = err ?? "Failed to revoke invitation code.";
    }
}
