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
// Copyright (c) 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Marechai.Pages.Books;

public partial class Search : IAsyncDisposable
{
    const int                     _pageSize    = 100;
    readonly string               _sentinelId  = $"books-search-sentinel-{Guid.NewGuid():N}";
    readonly string               _scrollerId  = $"books-search-scroll-{Guid.NewGuid():N}";
    readonly List<BookDto>        _books       = [];
    char?                         _character;
    bool                          _initialized;
    bool                          _loadingMore;
    bool                          _observerRegistered;
    string                        _lastStartingCharacter;
    int?                          _lastYear;
    int                           _total;
    DotNetObjectReference<Search> _selfRef;

    [Inject]
    IJSRuntime JS { get; set; }

    [Parameter]
    public int? Year { get; set; }

    [Parameter]
    public string StartingCharacter { get; set; }

    bool HasMore => _books.Count < _total;

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
            _character = null;
            _total     = 0;
            _books.Clear();

            if(!string.IsNullOrWhiteSpace(StartingCharacter) && StartingCharacter.Length == 1)
            {
                _character = StartingCharacter[0];

                // ToUpper()
                if(_character >= 'a' && _character <= 'z') _character -= (char)32;

                // Check if not letter or number
                if(_character < '0' || _character > '9' && _character < 'A' || _character > 'Z') _character = null;
            }

            await UnobserveAsync();

            Task<int>           countTask     = GetCurrentCountAsync();
            Task<List<BookDto>> firstPageTask = FetchBatchAsync(0, _pageSize);

            await Task.WhenAll(countTask, firstPageTask);

            _total = countTask.Result;
            _books.AddRange(firstPageTask.Result);
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
                // Circuit gone.
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
            List<BookDto> next = await FetchBatchAsync(_books.Count, _pageSize);
            _books.AddRange(next);

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
        if(_character.HasValue) return Service.GetBooksByLetterCountAsync(_character.Value);

        if(Year.HasValue) return Service.GetBooksByYearCountAsync(Year.Value);

        return Service.GetBooksCountAsync();
    }

    Task<List<BookDto>> FetchBatchAsync(int skip, int take)
    {
        if(_character.HasValue) return Service.GetBooksByLetterAsync(_character.Value, skip, take);

        if(Year.HasValue) return Service.GetBooksByYearAsync(Year.Value, skip, take);

        return Service.GetBooksAsync(skip, take);
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
            // Circuit gone.
        }

        _observerRegistered = false;
    }

    public async ValueTask DisposeAsync()
    {
        await UnobserveAsync();
        _selfRef?.Dispose();
    }
}
