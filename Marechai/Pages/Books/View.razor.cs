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
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Marechai.Pages.Books;

public partial class View
{
    BookDto                      _book;
    List<CompanyByBookDto>       _companies;
    long                         _lastId;
    bool                         _isCollected;
    bool                         _loaded;
    List<BookByMachineFamilyDto> _machineFamilies;
    List<BookByMachineDto>       _machines;
    List<PersonByBookDto>        _people;
    BookDto                      _previousBook;
    BookDto                      _sourceBook;
    DocumentSynopsisDto          _synopsis;
    bool                         _togglingCollection;

    [CascadingParameter]
    Task<AuthenticationState> AuthState { get; set; }

    [Parameter]
    public long Id { get; set; }

    protected override void OnParametersSet()
    {
        if(Id == _lastId) return;

        _lastId = Id;
        _loaded = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        if(Id <= 0)
        {
            _loaded = true;

            return;
        }

        // Single consolidated fetch: head + previous/source titles + synopsis +
        // all four child collections (people, companies, machines, machine
        // families) in one HTTP round-trip. Replaces 6 sequential service
        // calls plus the two conditional Previous/Source book fetches that
        // used to dominate the page-load critical path.
        BookFullDto full = await Service.GetBookFullAsync(Id);

        if(full?.Book is null)
        {
            _loaded = true;
            StateHasChanged();

            return;
        }

        _book            = full.Book;
        _synopsis        = full.Synopsis;
        _people          = full.People          ?? [];
        _companies       = full.Companies       ?? [];
        _machines        = full.Machines        ?? [];
        _machineFamilies = full.MachineFamilies ?? [];

        // Materialize lightweight stand-ins for the previous/source book chips.
        // The markup only renders the link target (Id) and the button label
        // (Title), so projecting just those two fields server-side is enough.
        if(_book.PreviousId.HasValue && full.PreviousBookTitle is not null)
            _previousBook = new BookDto { Id = _book.PreviousId.Value, Title = full.PreviousBookTitle };

        if(_book.SourceId.HasValue && full.SourceBookTitle is not null)
            _sourceBook = new BookDto { Id = _book.SourceId.Value, Title = full.SourceBookTitle };

        AuthenticationState authState = await AuthState;

        if(authState.User.Identity?.IsAuthenticated == true)
            _isCollected = await CollectionSvc.IsBookCollectedAsync(Id);

        _loaded = true;
        StateHasChanged();
    }

    async Task ToggleCollectionAsync()
    {
        _togglingCollection = true;

        if(_isCollected)
        {
            (bool success, _) = await CollectionSvc.RemoveBookFromCollectionAsync(Id);

            if(success) _isCollected = false;
        }
        else
        {
            (bool success, _) = await CollectionSvc.AddBookToCollectionAsync(Id);

            if(success) _isCollected = true;
        }

        _togglingCollection = false;
    }
}
