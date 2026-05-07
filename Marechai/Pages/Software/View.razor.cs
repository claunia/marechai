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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Marechai.Pages.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Marechai.Pages.Software;

public partial class View
{
    List<SoftwareCompanyRoleDto>                 _companies = [];
    List<SoftwareReleaseDto>                     _compilations = [];
    List<PersonBySoftwareDto>                    _credits = [];
    Dictionary<string, List<PersonBySoftwareDto>> _creditsByRole = new();
    string                                      _description;
    List<SoftwareGenreDto>                       _genres = [];
    Dictionary<string, List<SoftwareGenreDto>>   _genresByType = new();
    List<SoftwareAttributeDto>                   _attributes = [];
    List<SoftwareAttributeDto>                   _specs = [];
    List<SoftwareAttributeDto>                   _ratings = [];
    Dictionary<string, List<SoftwareAttributeDto>> _specsByPlatform = new();
    Dictionary<string, List<SoftwareAttributeDto>> _ratingsByPlatform = new();
    int                                         _lastId;
    bool                                        _loaded;
    List<SoftwareReleaseDto>                     _releases = [];
    List<SoftwareScreenshotDto>                  _screenshots = [];
    Dictionary<string, List<SoftwareScreenshotDto>> _screenshotsByPlatform = new();
    SoftwareScreenshotDto                       _fullscreenScreenshot;
    List<SoftwareCoverDto>                       _covers = [];
    Dictionary<string, List<SoftwareCoverDto>>    _coversByRelease = new();
    SoftwareCoverDto                            _fullscreenCover;
    SoftwareCoverDto                            _heroCover;
    List<SoftwarePromoArtDto>                   _promoArt = [];
    Dictionary<string, List<SoftwarePromoArtDto>> _promoArtByGroup = new();
    SoftwarePromoArtDto                         _fullscreenPromo;
    List<SoftwareVideoDto>                      _videos = [];
    SoftwareDto                                 _software;
    List<SoftwareDto>                           _addons = [];
    List<SoftwareVersionDto>                    _versions = [];
    List<SoftwareCriticReviewDto>               _criticReviews = [];
    CriticReviewSummaryDto                      _reviewSummary;

    // User reviews & ratings
    List<SoftwareUserReviewDto>                 _userReviews = [];
    UserReviewSummaryDto                        _userReviewSummary;
    MarechaiScoreDto                            _marechaiScore;
    SoftwareUserReviewDto                       _myReview;
    float                                       _myRatingFloat;
    bool                                        _isAuthenticated;
    bool                                        _isAdmin;
    string                                      _currentUserId;

    // Tab state — backs the responsive sticky MudTabs in View.razor
    static readonly string[] _tabNames = ["overview", "specifications", "releases", "media", "reviews"];

    string _activeTab = "overview";

    int _activeTabIndex => Math.Max(0, Array.IndexOf(_tabNames, _activeTab));

    [Parameter]
    public int Id { get; set; }

    [SupplyParameterFromQuery(Name = "tab")]
    public string TabParam { get; set; }

    [Inject]
    NavigationManager NavManager { get; set; }

    protected override void OnParametersSet()
    {
        string tab = (TabParam ?? "overview").ToLowerInvariant();
        _activeTab = Array.IndexOf(_tabNames, tab) >= 0 ? tab : "overview";

        if(Id == _lastId) return;

        _lastId = Id;
        _loaded = false;
    }

    void OnTabChanged(int index)
    {
        if(index < 0 || index >= _tabNames.Length) return;

        string newTab = _tabNames[index];

        if(newTab == _activeTab) return;

        _activeTab = newTab;

        // Default tab (overview) drops the query param to keep URLs clean.
        string newUri = NavManager.GetUriWithQueryParameter("tab", newTab == "overview" ? null : newTab);
        NavManager.NavigateTo(newUri, false, true);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        if(Id <= 0)
        {
            _loaded = true;

            return;
        }

        try
        {
            _software = await Service.GetSoftwareByIdAsync(Id);

            if(_software is null)
            {
                _loaded = true;
                StateHasChanged();

                return;
            }

            // ── Two-phase load ──
            // EVERY task is launched in parallel up front so they all benefit from
            // the connection pool concurrently. We then await them in two waves:
            //   Phase 1: data the header + Overview tab consume → unblocks LCP.
            //   Phase 2: data only the Specifications/Releases/Media/Reviews tabs
            //            need → re-renders once it arrives but doesn't block first paint.

            // Phase 1 (header + Overview)
            Task<List<SoftwareGenreDto>>            genresTask        = Service.GetGenresAsync(Id);
            Task<List<SoftwareDto>>                 addonsTask        = Service.GetAddonsAsync(Id);
            Task<string>                            descriptionTask   = Service.GetDescriptionTextAsync(Id);
            Task<List<SoftwareCoverDto>>            coversTask        = Service.GetCoversBySoftwareAsync(Id);
            Task<MarechaiScoreDto>                  marechaiScoreTask = AuthService.GetMarechaiScoreAsync(Id);
            Task<UserReviewSummaryDto>              userSummaryTask   = AuthService.GetUserReviewSummaryAsync(Id);
            Task<AuthenticationState>               authStateTask     = AuthStateProvider.GetAuthenticationStateAsync();
            Task<SoftwareUserRatingDto>             myRatingTask      = AuthService.GetMyRatingAsync(Id);

            // Phase 2 (deferred tabs) — kicked off NOW so they overlap Phase 1 awaits
            Task<List<SoftwareCompanyRoleDto>>      companiesTask     = Service.GetCompaniesAsync(Id);
            Task<List<SoftwareVersionDto>>          versionsTask      = Service.GetVersionsAsync(Id);
            Task<List<PersonBySoftwareDto>>         creditsTask       = Service.GetCreditsBySoftwareAsync(Id);
            Task<List<SoftwareAttributeDto>>        attributesTask    = Service.GetAttributesAsync(Id);
            Task<List<SoftwareReleaseDto>>          releasesTask      = Service.GetReleasesBySoftwareAsync(Id);
            Task<List<SoftwarePromoArtDto>>         promoArtTask      = Service.GetPromoArtBySoftwareAsync(Id);
            Task<List<SoftwareVideoDto>>            videosTask        = Service.GetVideosBySoftwareAsync(Id);
            Task<List<SoftwareReleaseDto>>          compilationsTask  = Service.GetCompilationsForSoftwareAsync(Id);
            Task<List<SoftwareCriticReviewDto>>     criticReviewsTask = Service.GetCriticReviewsAsync(Id);
            Task<CriticReviewSummaryDto>            criticSummaryTask = Service.GetCriticReviewSummaryAsync(Id);
            Task<List<SoftwareScreenshotDto>>       screenshotsTask   = Service.GetScreenshotsBySoftwareAsync(Id);
            Task<List<SoftwareUserReviewDto>>       userReviewsTask   = AuthService.GetUserReviewsAsync(Id);

            // ── Phase 1 await ──
            await Task.WhenAll(genresTask, addonsTask, descriptionTask, coversTask,
                               marechaiScoreTask, userSummaryTask, authStateTask, myRatingTask);

            _genres            = genresTask.Result;
            _addons            = addonsTask.Result;
            _description       = descriptionTask.Result;
            _covers            = coversTask.Result;
            _marechaiScore     = marechaiScoreTask.Result;
            _userReviewSummary = userSummaryTask.Result;

            AuthenticationState authState = authStateTask.Result;
            _isAuthenticated = authState.User.Identity?.IsAuthenticated == true;
            _currentUserId   = authState.User.FindFirst(ClaimTypes.Sid)?.Value;
            _isAdmin         = authState.User.IsInRole("Admin") || authState.User.IsInRole("UberAdmin");

            _genresByType = _genres
                           .GroupBy(g => g.TypeName ?? "Genre")
                           .OrderBy(g => g.Key)
                           .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Name).ToList());

            // Pick a random front cover for the hero header (covers list is also reused
            // by the Media tab below).
            var frontCovers = _covers.Where(c => c.Type == 0).ToList();

            if(frontCovers.Count > 0)
                _heroCover = frontCovers[Random.Shared.Next(frontCovers.Count)];

            if(_isAuthenticated)
            {
                SoftwareUserRatingDto myRating = myRatingTask.Result;
                _myRatingFloat = myRating is not null ? myRating.Rating.GetValueOrDefault() : 0;
            }

            // First render: Overview tab + header are fully populated.
            _loaded = true;
            StateHasChanged();

            // ── Phase 2 await ──
            await Task.WhenAll(companiesTask, versionsTask, creditsTask, attributesTask, releasesTask,
                               promoArtTask, videosTask, compilationsTask, criticReviewsTask, criticSummaryTask,
                               screenshotsTask, userReviewsTask);

            _companies     = companiesTask.Result;
            _versions      = versionsTask.Result;
            _credits       = creditsTask.Result;
            _attributes    = attributesTask.Result;
            _releases      = releasesTask.Result;
            _promoArt      = promoArtTask.Result;
            _videos        = videosTask.Result;
            _compilations  = compilationsTask.Result;
            _criticReviews = criticReviewsTask.Result;
            _reviewSummary = criticSummaryTask.Result;
            _screenshots   = screenshotsTask.Result;
            _userReviews   = userReviewsTask.Result;

            _creditsByRole = _credits
                            .GroupBy(c => c.Role ?? "Other")
                            .OrderBy(g => g.Key)
                            .ToDictionary(g => g.Key, g => g.ToList());

            _specs   = _attributes.Where(a => a.Category == "Spec").ToList();
            _ratings = _attributes.Where(a => a.Category == "Rating").ToList();

            _specsByPlatform = _specs
                              .GroupBy(s => s.PlatformName ?? "Unknown")
                              .OrderBy(g => g.Key)
                              .ToDictionary(g => g.Key,
                                            g => g.DistinctBy(s => (s.Key, s.Value)).ToList());

            _ratingsByPlatform = _ratings
                                .GroupBy(r => r.PlatformName ?? "Unknown")
                                .OrderBy(g => g.Key)
                                .ToDictionary(g => g.Key,
                                              g => g.DistinctBy(r => (r.Key, r.Value)).ToList());

            _versions.Sort((a, b) => NaturalStringComparer.Instance.Compare(a.VersionString, b.VersionString));

            _coversByRelease = _covers
                              .GroupBy(c =>
                               {
                                   string label = c.PlatformName ?? "Unknown";

                                   if(!string.IsNullOrEmpty(c.RegionNames))
                                       label += " — " + c.RegionNames;

                                   return label;
                               })
                              .OrderBy(g => g.Key)
                              .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Type).ToList());

            _promoArtByGroup = _promoArt
                              .GroupBy(p => p.GroupName ?? "Other")
                              .OrderBy(g => g.Key)
                              .ToDictionary(g => g.Key, g => g.ToList());

            _screenshotsByPlatform = _screenshots
                                    .GroupBy(s => s.PlatformName ?? "Unknown")
                                    .OrderBy(g => g.Key)
                                    .ToDictionary(g => g.Key, g => g.ToList());

            if(_isAuthenticated)
                _myReview = _userReviews.FirstOrDefault(r => r.UserId == _currentUserId);

            StateHasChanged();
        }
        catch(ObjectDisposedException)
        {
            // Component was disposed during async loading — ignore
        }
    }

    static string FormatReviewDate(SoftwareCriticReviewDto review)
    {
        if(!review.ReviewDate.HasValue) return "";

        return review.ReviewDatePrecision switch
        {
            (int)DatePrecision.Full      => review.ReviewDate.Value.ToString("yyyy-MM-dd"),
            (int)DatePrecision.MonthYear => review.ReviewDate.Value.ToString("yyyy-MM"),
            (int)DatePrecision.YearOnly  => review.ReviewDate.Value.ToString("yyyy"),
            _                            => review.ReviewDate.Value.ToString("yyyy-MM-dd")
        };
    }

    async Task OnMyRatingChanged(float value)
    {
        _myRatingFloat = value;

        if(value > 0)
        {
            (bool succeeded, string error) = await AuthService.SetMyRatingAsync(Id, value);

            if(!succeeded)
            {
                Snackbar.Add(error ?? "Failed to save rating", Severity.Error);

                return;
            }
        }
        else
        {
            (bool succeeded, string error) = await AuthService.DeleteMyRatingAsync(Id);

            if(!succeeded)
            {
                Snackbar.Add(error ?? "Failed to remove rating", Severity.Error);

                return;
            }
        }

        _userReviewSummary = await AuthService.GetUserReviewSummaryAsync(Id);
        _marechaiScore     = await AuthService.GetMarechaiScoreAsync(Id);
        StateHasChanged();
    }

    async Task OpenReviewDialog()
    {
        var parameters = new DialogParameters<UserReviewDialog>
        {
            { x => x.SoftwareId, Id },
            { x => x.ExistingReview, _myReview },
            { x => x.CurrentRating, _myRatingFloat }
        };

        IDialogReference dialog = await DialogService.ShowAsync<UserReviewDialog>(
            _myReview is not null ? L["Edit Review"] : L["Write Review"], parameters,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            _userReviews       = await AuthService.GetUserReviewsAsync(Id);
            _userReviewSummary = await AuthService.GetUserReviewSummaryAsync(Id);
            _marechaiScore     = await AuthService.GetMarechaiScoreAsync(Id);
            _myReview          = _userReviews.FirstOrDefault(r => r.UserId == _currentUserId);

            SoftwareUserRatingDto myRating = await AuthService.GetMyRatingAsync(Id);
            _myRatingFloat = myRating is not null ? myRating.Rating.GetValueOrDefault() : 0;

            StateHasChanged();
        }
    }

    async Task OpenEditReviewDialog(SoftwareUserReviewDto review)
    {
        var parameters = new DialogParameters<UserReviewDialog>
        {
            { x => x.SoftwareId, Id },
            { x => x.ExistingReview, review },
            { x => x.CurrentRating, review.Rating ?? 0f }
        };

        IDialogReference dialog = await DialogService.ShowAsync<UserReviewDialog>(
            L["Edit Review"], parameters,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            _userReviews       = await AuthService.GetUserReviewsAsync(Id);
            _userReviewSummary = await AuthService.GetUserReviewSummaryAsync(Id);
            _marechaiScore     = await AuthService.GetMarechaiScoreAsync(Id);
            _myReview          = _userReviews.FirstOrDefault(r => r.UserId == _currentUserId);
            StateHasChanged();
        }
    }

    async Task DeleteReviewAsync(SoftwareUserReviewDto review)
    {
        var parameters = new DialogParameters<DeleteConfirmDialog>
        {
            { x => x.ContentText, L["Are you sure you want to delete this review?"].Value }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DeleteConfirmDialog>(
            L["Delete"], parameters, new DialogOptions { MaxWidth = MaxWidth.Small });

        DialogResult dialogResult = await dialog.Result;
        bool? confirmed = dialogResult is { Canceled: false };

        if(confirmed != true) return;

        (bool succeeded, string error) = await AuthService.DeleteUserReviewAsync(Id, review.Id.GetValueOrDefault());

        if(succeeded)
        {
            Snackbar.Add(L["Review deleted."], Severity.Success);
            _userReviews       = await AuthService.GetUserReviewsAsync(Id);
            _userReviewSummary = await AuthService.GetUserReviewSummaryAsync(Id);
            _marechaiScore     = await AuthService.GetMarechaiScoreAsync(Id);
            _myReview          = _userReviews.FirstOrDefault(r => r.UserId == _currentUserId);
            StateHasChanged();
        }
        else
        {
            Snackbar.Add(error ?? "Error", Severity.Error);
        }
    }

    async Task VoteAsync(SoftwareUserReviewDto review, bool isUpvote)
    {
        // Toggle: if already voted the same way, remove vote
        if(review.CurrentUserVote == isUpvote)
        {
            await AuthService.RemoveReviewVoteAsync(Id, review.Id.GetValueOrDefault());
        }
        else
        {
            await AuthService.VoteReviewAsync(Id, review.Id.GetValueOrDefault(), isUpvote);
        }

        _userReviews = await AuthService.GetUserReviewsAsync(Id);
        StateHasChanged();
    }

    async Task OpenReportDialog(SoftwareUserReviewDto review)
    {
        var parameters = new DialogParameters<ReportReviewDialog>
        {
            { x => x.SoftwareId, Id },
            { x => x.ReviewId, review.Id.GetValueOrDefault() }
        };

        IDialogReference dialog = await DialogService.ShowAsync<ReportReviewDialog>(
            L["Report Review"], parameters,
            new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            Snackbar.Add(L["Report submitted. Thank you."], Severity.Success);
        }
    }
}
