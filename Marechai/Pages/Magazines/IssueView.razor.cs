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
using Marechai.Pages.Suggestions;
using Marechai.Services;
using Marechai.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Marechai.Pages.Magazines;

public partial class IssueView
{
    MagazineIssueDto                 _issue;
    bool                             _isCollected;
    long                             _lastId;
    PhotoLightbox                    _lightbox;
    bool                             _loaded;
    List<MagazineByMachineFamilyDto> _machineFamilies = [];
    List<MagazineByMachineDto>       _machines        = [];
    string                           _magazineTitle;
    List<MagazineBySoftwareDto>      _software        = [];
    bool                             _togglingCollection;

    [Inject]
    NavigationManager Nav { get; set; }

    [CascadingParameter]
    Task<AuthenticationState> AuthState { get; set; }

    [Parameter]
    public long Id { get; set; }

    protected override void OnParametersSet()
    {
        if(Id == _lastId) return;

        _lastId      = Id;
        _loaded      = false;
        _isCollected = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        if(Id <= 0)
        {
            _loaded = true;

            return;
        }

        // Single round-trip via /magazines/issues/{id}/full — head + magazine title +
        // 3 junction collections fan out server-side over independent DbContexts.
        MagazineIssueFullDto full = await Service.GetIssueFullAsync(Id);

        if(full?.Issue is null)
        {
            _loaded = true;
            StateHasChanged();

            return;
        }

        _issue           = full.Issue;
        _magazineTitle   = full.MagazineTitle ?? full.Issue.MagazineTitle;
        _machines        = full.Machines        ?? [];
        _machineFamilies = full.MachineFamilies ?? [];
        _software        = full.Software        ?? [];

        // Skip the 401-bound collection check for anonymous viewers.
        AuthenticationState authState = await AuthState;

        if(authState.User.Identity?.IsAuthenticated == true)
            _isCollected = await CollectionSvc.IsMagazineIssueCollectedAsync(Id);

        _loaded = true;
        StateHasChanged();
    }

    async Task ToggleCollectionAsync()
    {
        _togglingCollection = true;

        if(_isCollected)
        {
            (bool success, _) = await CollectionSvc.RemoveMagazineIssueFromCollectionAsync(Id);

            if(success) _isCollected = false;
        }
        else
        {
            (bool success, _) = await CollectionSvc.AddMagazineIssueToCollectionAsync(Id);

            if(success) _isCollected = true;
        }

        _togglingCollection = false;
    }

    /// <summary>
    /// Format a publication date according to its precision: year-only, month-and-year,
    /// or short date — never <see cref="DateTime.ToLongDateString"/>, since the wire value
    /// is sometimes incomplete (e.g. only the year is meaningful when precision == 2).
    /// </summary>
    static string FormatDate(DateTimeOffset published, int? precision) => (precision ?? 0) switch
    {
        2 => published.Year.ToString(),
        1 => published.ToString("MMMM yyyy"),
        _ => published.DateTime.ToShortDateString()
    };

    /// <summary>
    /// Open the collaborative suggestion dialog scoped to this magazine issue. Captures
    /// the route parameter <see cref="Id"/> directly (NOT <c>_issue.Id</c> — async race
    /// lesson from the Book/Machine ports: the OwningComponentBase + async DTO load can
    /// momentarily leave <c>_issue</c> null, and reading <c>.Value</c> off a nullable
    /// would 0-cast and trigger the addition-suggestion path on the server (400).
    /// </summary>
    async Task OpenIssueSuggestionDialogAsync()
    {
        if(_issue is null) return;
        long issueId = Id;
        var parameters = new DialogParameters
        {
            ["EntityId"]   = issueId,
            ["CurrentDto"] = _issue
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog  = await DialogService.ShowAsync<MagazineIssueSuggestionDialog>(L["Suggest changes"], parameters, options);
        await dialog.Result;
    }
}
