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
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Marechai.Pages.Software;

public partial class Search : IAsyncDisposable
{
    const int                  _pageSize    = 100;
    readonly string            _sentinelId  = $"sw-search-sentinel-{Guid.NewGuid():N}";
    readonly string            _scrollerId  = $"sw-search-scroll-{Guid.NewGuid():N}";
    char?                      _character;
    int?                       _lastGenreId;
    string                     _lastKindString;
    int?                       _lastPlatformId;
    string                     _lastStartingCharacter;
    string                     _lastSpecKey;
    string                     _lastSpecValue;
    int?                       _lastYear;
    bool                       _initialized;
    bool                       _loadingMore;
    bool                       _observerRegistered;
    string                     _genreName;
    SoftwareKind?              _kind;
    string                     _platformName;
    int                        _total;
    DotNetObjectReference<Search> _selfRef;
    readonly List<SoftwareDto>     _software = [];

    [Inject]
    NavigationManager NavigationManager { get; set; }

    [Inject]
    IJSRuntime JS { get; set; }

    [Parameter]
    public int? Year { get; set; }

    [Parameter]
    public string StartingCharacter { get; set; }

    [Parameter]
    public int? PlatformId { get; set; }

    [Parameter]
    public int? GenreId { get; set; }

    [SupplyParameterFromQuery(Name = "key")]
    public string SpecKey { get; set; }

    [SupplyParameterFromQuery(Name = "value")]
    public string SpecValue { get; set; }

    [SupplyParameterFromQuery(Name = "kind")]
    public string KindString { get; set; }

    bool HasMore => _software.Count < _total;

    protected override void OnParametersSet()
    {
        if(Year == _lastYear                                                          &&
           StartingCharacter == _lastStartingCharacter                                 &&
           PlatformId        == _lastPlatformId                                       &&
           GenreId           == _lastGenreId                                          &&
           string.Equals(SpecKey,    _lastSpecKey,    StringComparison.Ordinal)       &&
           string.Equals(SpecValue,  _lastSpecValue,  StringComparison.Ordinal)       &&
           string.Equals(KindString, _lastKindString, StringComparison.OrdinalIgnoreCase)) return;

        _lastYear              = Year;
        _lastStartingCharacter = StartingCharacter;
        _lastPlatformId        = PlatformId;
        _lastGenreId           = GenreId;
        _lastSpecKey           = SpecKey;
        _lastSpecValue         = SpecValue;
        _lastKindString        = KindString;
        _initialized           = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(!_initialized)
        {
            _character    = null;
            _kind         = null;
            _platformName = null;
            _genreName    = null;
            _total        = 0;
            _software.Clear();

            if(!string.IsNullOrWhiteSpace(KindString)                                  &&
               Enum.TryParse<SoftwareKind>(KindString, true, out SoftwareKind parsed) &&
               Enum.IsDefined(typeof(SoftwareKind), parsed))
                _kind = parsed;

            if(!string.IsNullOrWhiteSpace(StartingCharacter) && StartingCharacter.Length == 1)
            {
                _character = StartingCharacter[0];

                // ToUpper()
                if(_character >= 'a' && _character <= 'z') _character -= (char)32;

                // Check if not letter or number
                if(_character < '0' || _character > '9' && _character < 'A' || _character > 'Z') _character = null;
            }

            // Fetch the total count + first page + (optionally) the platform/genre
            // display name in parallel so the page is fully painted in one round.
            Task<int>               countTask     = GetCurrentCountAsync();
            Task<List<SoftwareDto>> firstPageTask = FetchBatchAsync(0, _pageSize);
            Task<string>            platformTask  = PlatformId.HasValue ? GetPlatformNameAsync(PlatformId.Value) : Task.FromResult<string>(null);
            Task<string>            genreTask     = GenreId.HasValue    ? GetGenreNameAsync(GenreId.Value)       : Task.FromResult<string>(null);

            await Task.WhenAll(countTask, firstPageTask, platformTask, genreTask);

            _total        = countTask.Result;
            _software.AddRange(firstPageTask.Result);
            _platformName = platformTask.Result;
            _genreName    = genreTask.Result;
            _initialized  = true;

            StateHasChanged();
            return;
        }

        // Register the IntersectionObserver once the sentinel has been
        // rendered. Re-register defensively whenever a new batch of items
        // shifts the sentinel deeper into the scroller (the observer keeps
        // the same target reference, so this is a one-shot).
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
            List<SoftwareDto> next = await FetchBatchAsync(_software.Count, _pageSize);
            _software.AddRange(next);

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

    Task<int> GetCurrentCountAsync()
    {
        if(_character.HasValue) return Service.GetSoftwareByLetterCountAsync(_character.Value, _kind);

        if(Year.HasValue) return Service.GetSoftwareByYearCountAsync(Year.Value, _kind);

        if(PlatformId.HasValue) return Service.GetSoftwareByPlatformCountAsync(PlatformId.Value, _kind);

        if(GenreId.HasValue) return Service.GetSoftwareByGenreCountAsync(GenreId.Value, _kind);

        if(!string.IsNullOrEmpty(SpecKey) && !string.IsNullOrEmpty(SpecValue))
            return Service.GetSoftwareBySpecCountAsync(SpecKey, SpecValue, _kind);

        return Service.GetCountAsync(kind: _kind);
    }

    async Task<string> GetPlatformNameAsync(int platformId)
    {
        List<SoftwarePlatformDto> platforms = await Service.GetPlatformsAsync();

        return platforms.FirstOrDefault(p => p.Id == platformId)?.Name;
    }

    async Task<string> GetGenreNameAsync(int genreId)
    {
        List<SoftwareGenreDto> genres = await Service.GetAllGenresAsync();

        return genres.FirstOrDefault(g => g.Id == genreId)?.Name;
    }

    Task<List<SoftwareDto>> FetchBatchAsync(int skip, int take)
    {
        if(_character.HasValue) return Service.GetSoftwareByLetterAsync(_character.Value, _kind, skip, take);

        if(Year.HasValue) return Service.GetSoftwareByYearAsync(Year.Value, _kind, skip, take);

        if(PlatformId.HasValue) return Service.GetSoftwareByPlatformAsync(PlatformId.Value, _kind, skip, take);

        if(GenreId.HasValue) return Service.GetSoftwareByGenreAsync(GenreId.Value, _kind, skip, take);

        if(!string.IsNullOrEmpty(SpecKey) && !string.IsNullOrEmpty(SpecValue))
            return Service.GetSoftwareBySpecAsync(SpecKey, SpecValue, _kind, skip, take);

        return Service.GetAllSoftwareAsync(_kind, skip, take);
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

    void OnKindChanged(SoftwareKind? value)
    {
        if(value == _kind) return;

        // Rewrite the current URL preserving path + non-kind query params, replacing
        // (or removing) the kind value, and force-reload via NavigateTo.
        Uri                 uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        NameValueCollection q   = HttpUtility.ParseQueryString(uri.Query);
        q.Remove("kind");

        if(value.HasValue) q["kind"] = value.Value.ToString();

        string query  = q.Count == 0 ? string.Empty : "?" + q;
        string target = uri.GetLeftPart(UriPartial.Path) + query;
        NavigationManager.NavigateTo(target);
    }

    public async ValueTask DisposeAsync()
    {
        await UnobserveAsync();
        _selfRef?.Dispose();
    }
}
