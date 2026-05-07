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
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace Marechai.Services;

public sealed class TokenProvider(ProtectedLocalStorage localStorage)
{
    const  string StorageKey = "auth_token";
    string _cachedToken;
    bool   _initialized;

    public async Task<string> GetTokenAsync()
    {
        if(_initialized)
            return _cachedToken;

        try
        {
            ProtectedBrowserStorageResult<string> result = await localStorage.GetAsync<string>(StorageKey);
            _cachedToken = result.Success ? result.Value : null;
            _initialized = true;
        }
        catch(InvalidOperationException)
        {
            // JS interop not available yet (prerendering). Leave uninitialized so the next call retries.
            _cachedToken = null;
        }
        catch(JSException)
        {
            // Circuit not ready or JS error. Leave uninitialized so the next call retries.
            _cachedToken = null;
        }

        return _cachedToken;
    }

    public async Task SetTokenAsync(string token)
    {
        _cachedToken = token;
        _initialized = true;
        await localStorage.SetAsync(StorageKey, token);
    }

    public async Task RemoveTokenAsync()
    {
        _cachedToken = null;
        _initialized = true;
        await localStorage.DeleteAsync(StorageKey);
    }
}
