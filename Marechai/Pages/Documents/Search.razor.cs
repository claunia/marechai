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
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Marechai.Pages.Documents;

public partial class Search : IAsyncDisposable
{
    const int                       _pageSize           = 100;
    readonly string                 _sentinelId         = $"documents-search-sentinel-{Guid.NewGuid():N}";
    readonly string                 _scrollerId         = $"documents-search-scroll-{Guid.NewGuid():N}";
    readonly List<DocumentDto>      _documents          = [];
    char?                           _character;
    bool                            _initialized;
    bool                            _loadingMore;
    bool                            _observerRegistered;
    string                          _lastStartingCharacter;
    int?                            _lastYear;
    int                             _total;
    DotNetObjectReference<Search>   _selfRef;

    [Inject]
    IJSRuntime JS { get; set; }

    [Parameter]
    public int? Year { get; set; }

    [Parameter]
    public string StartingCharacter { get; set; }

    bool HasMore => _documents.Count < _total;

    protected override void OnParametersSet()
    {
        if(Year == _lastYear && StartingCharacter == _lastStartingCharacter) return;

        _lastYear              = Year;
        _lastStartingCharacter = StartingCharacter;
        _initialized           = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(!_initialized)
        {
            await UnobserveAsync();

            _character = null;
            _total     = 0;
            _documents.Clear();

            if(!string.IsNullOrWhiteSpace(StartingCharacter) && StartingCharacter.Length == 1)
            {
                _character = StartingCharacter[0];

                // ToUpper()
                if(_character >= 'a' && _character <= 'z') _character -= (char)32;

                // Check if not letter or number
                if(_character < '0' || _character > '9' && _character < 'A' || _character > 'Z') _character = null;
            }

            Task<int>               countTask     = GetCurrentCountAsync();
            Task<List<DocumentDto>> firstPageTask = FetchBatchAsync(0, _pageSize);

            await Task.WhenAll(countTask, firstPageTask);

            _total = countTask.Result;
            _documents.AddRange(firstPageTask.Result);
            _initialized = true;

            StateHasChanged();
            return;
        }

        if(!_observerRegistered && HasMore)
        {
            _selfRef ??= DotNetObjectReference.Create(this);

            try
            {
                await JS.InvokeVoidAsync("marechaiInfiniteScroll.observe", _sentinelId, _selfRef, $"#{_scrollerId}",
                                         "200px");
                _observerRegistered = true;
            }
            catch(JSDisconnectedException)
            {
                // Circuit gone — nothing to do.
            }
        }
    }

    [JSInvokable]
    public async Task OnSentinelVisibleAsync()
    {
        if(_loadingMore || !HasMore) return;

        _loadingMore = true;
        StateHasChanged();

        try
        {
            List<DocumentDto> next = await FetchBatchAsync(_documents.Count, _pageSize);
            _documents.AddRange(next);

            if(!HasMore) await UnobserveAsync();
        }
        finally
        {
            _loadingMore = false;
            StateHasChanged();
        }
    }

    Task<int> GetCurrentCountAsync()
    {
        if(_character.HasValue) return Service.GetDocumentsByLetterCountAsync(_character.Value);

        if(Year.HasValue) return Service.GetDocumentsByYearCountAsync(Year.Value);

        return Service.GetDocumentsCountAsync();
    }

    Task<List<DocumentDto>> FetchBatchAsync(int skip, int take)
    {
        if(_character.HasValue) return Service.GetDocumentsByLetterAsync(_character.Value, skip, take);

        if(Year.HasValue) return Service.GetDocumentsByYearAsync(Year.Value, skip, take);

        return Service.GetDocumentsAsync(skip, take);
    }

    async Task UnobserveAsync()
    {
        if(!_observerRegistered) return;

        try
        {
            await JS.InvokeVoidAsync("marechaiInfiniteScroll.unobserve", _sentinelId);
        }
        catch(JSDisconnectedException)
        {
            // Circuit gone; nothing to do.
        }

        _observerRegistered = false;
    }

    public async ValueTask DisposeAsync()
    {
        await UnobserveAsync();
        _selfRef?.Dispose();
    }
}
