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

namespace Marechai.Pages.Gpus;

public partial class Index : IAsyncDisposable
{
    const int                    _pageSize           = 100;
    readonly string              _sentinelId         = $"gpus-sentinel-{Guid.NewGuid():N}";
    readonly string              _scrollerId         = $"gpus-scroll-{Guid.NewGuid():N}";
    readonly List<GpuDto>        _gpus               = [];
    bool                         _initialized;
    bool                         _loadingMore;
    bool                         _observerRegistered;
    int                          _total;
    DotNetObjectReference<Index> _selfRef;

    [Inject]
    IJSRuntime JS { get; set; }

    bool HasMore => _gpus.Count < _total;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(!_initialized)
        {
            // Stop the previous observer (if any) before swapping in a new
            // batch — the sentinel/scroller IDs are stable per page instance,
            // but defensive cleanup keeps callbacks from firing against a
            // stale state when the component re-initializes.
            await UnobserveAsync();

            // Fetch the total count + first page in parallel so the page is
            // fully painted in one round-trip pair.
            Task<int>          countTask     = Service.GetCountAsync();
            Task<List<GpuDto>> firstPageTask = Service.GetAllAsync(0, _pageSize);

            await Task.WhenAll(countTask, firstPageTask);

            _total = countTask.Result;
            _gpus.AddRange(firstPageTask.Result);
            _initialized = true;

            StateHasChanged();
            return;
        }

        // Register the IntersectionObserver once the sentinel has been
        // rendered. The observer keeps the same target reference, so this is
        // a one-shot until UnobserveAsync clears the flag.
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

    /// <summary>
    /// Invoked by the IntersectionObserver in <c>infinite-scroll.js</c> when the
    /// sentinel scrolls into view. Loads the next page, appends it to the
    /// in-memory list, and re-renders.
    /// </summary>
    [JSInvokable]
    public async Task OnSentinelVisibleAsync()
    {
        if(_loadingMore || !HasMore) return;

        _loadingMore = true;
        StateHasChanged();

        try
        {
            List<GpuDto> next = await Service.GetAllAsync(_gpus.Count, _pageSize);
            _gpus.AddRange(next);

            // If the very last page just arrived, stop the observer so it
            // doesn't keep firing as the user scrolls past the new tail.
            if(!HasMore) await UnobserveAsync();
        }
        finally
        {
            _loadingMore = false;
            StateHasChanged();
        }
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
