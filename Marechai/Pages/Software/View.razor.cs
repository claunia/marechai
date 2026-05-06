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

    [Parameter]
    public int Id { get; set; }

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

        try
        {

        _software = await Service.GetSoftwareByIdAsync(Id);

        if(_software is null)
        {
            _loaded = true;
            StateHasChanged();

            return;
        }

        _companies = await Service.GetCompaniesAsync(Id);
        _versions  = await Service.GetVersionsAsync(Id);

        // Load credits
        _credits = await Service.GetCreditsBySoftwareAsync(Id);

        _creditsByRole = _credits
                        .GroupBy(c => c.Role ?? "Other")
                        .OrderBy(g => g.Key)
                        .ToDictionary(g => g.Key, g => g.ToList());

        // Load genres
        _genres = await Service.GetGenresAsync(Id);

        _genresByType = _genres
                       .GroupBy(g => g.TypeName ?? "Genre")
                       .OrderBy(g => g.Key)
                       .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Name).ToList());

        // Load attributes (specs + ratings)
        _attributes = await Service.GetAttributesAsync(Id);

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

        // Load description with language fallback to English
        _description = await Service.GetDescriptionTextAsync(Id);
        _versions.Sort((a, b) => NaturalStringComparer.Instance.Compare(a.VersionString, b.VersionString));

        // Load all non-compilation releases for this software (flat list)
        _releases = await Service.GetReleasesBySoftwareAsync(Id);

        // Load covers from all releases
        _covers = await Service.GetCoversBySoftwareAsync(Id);

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

        // Pick a random front cover for the hero header
        var frontCovers = _covers.Where(c => c.Type == 0).ToList();

        if(frontCovers.Count > 0)
            _heroCover = frontCovers[Random.Shared.Next(frontCovers.Count)];

        // Load promo art
        _promoArt = await Service.GetPromoArtBySoftwareAsync(Id);

        _promoArtByGroup = _promoArt
                          .GroupBy(p => p.GroupName ?? "Other")
                          .OrderBy(g => g.Key)
                          .ToDictionary(g => g.Key, g => g.ToList());

        // Load videos
        _videos = await Service.GetVideosBySoftwareAsync(Id);

        // Load compilations that include this software
        _compilations = await Service.GetCompilationsForSoftwareAsync(Id);

        // Load critic reviews
        _criticReviews = await Service.GetCriticReviewsAsync(Id);
        _reviewSummary = await Service.GetCriticReviewSummaryAsync(Id);

        // Load auth state
        AuthenticationState authState = await AuthStateProvider.GetAuthenticationStateAsync();
        _isAuthenticated = authState.User.Identity?.IsAuthenticated == true;
        _currentUserId   = authState.User.FindFirst(ClaimTypes.Sid)?.Value;
        _isAdmin         = authState.User.IsInRole("Admin") || authState.User.IsInRole("UberAdmin");

        // Load user reviews, ratings, score
        _userReviews       = await AuthService.GetUserReviewsAsync(Id);
        _userReviewSummary = await AuthService.GetUserReviewSummaryAsync(Id);
        _marechaiScore     = await AuthService.GetMarechaiScoreAsync(Id);

        if(_isAuthenticated)
        {
            SoftwareUserRatingDto myRating = await AuthService.GetMyRatingAsync(Id);
            _myRatingFloat = myRating is not null ? myRating.Rating.GetValueOrDefault() : 0;
            _myReview    = _userReviews.FirstOrDefault(r => r.UserId == _currentUserId);
        }

        // Load screenshots
        List<Guid?> screenshotIds = await Service.GetScreenshotIdsAsync(Id);

        _screenshots = [];

        foreach(Guid? id in screenshotIds)
        {
            if(id.HasValue && id.Value != Guid.Empty)
            {
                SoftwareScreenshotDto detail = await Service.GetScreenshotDetailsAsync(id.Value);

                if(detail != null) _screenshots.Add(detail);
            }
        }

        // Group screenshots by platform
        _screenshotsByPlatform = _screenshots
                                .GroupBy(s => s.PlatformName ?? "Unknown")
                                .OrderBy(g => g.Key)
                                .ToDictionary(g => g.Key, g => g.ToList());

        _loaded = true;
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
