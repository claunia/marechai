#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Presentation.Dialogs;
using Marechai.App.Presentation.Models;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.Data;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels.Admin;

[Bindable]
public partial class AdminReviewReportsViewModel : ObservableObject, IRegionAware
{
    readonly IJwtService _jwtService;
    readonly IStringLocalizer _localizer;
    readonly ILogger<AdminReviewReportsViewModel> _logger;
    readonly ReviewReportsService _reviewReportsService;
    readonly ITokenService _tokenService;

    [ObservableProperty] bool _isAdmin;
    [ObservableProperty] bool _showResolved;
    [ObservableProperty] bool _isLoading;
    [ObservableProperty] bool _hasError;
    [ObservableProperty] bool _isDataLoaded;
    [ObservableProperty] string _errorMessage = string.Empty;

    public ObservableCollection<ReviewReportListItem> Reports { get; } = [];

    public AdminReviewReportsViewModel(ReviewReportsService reviewReportsService,
        ILogger<AdminReviewReportsViewModel> logger, ITokenService tokenService, IJwtService jwtService,
        IStringLocalizer localizer)
    {
        _reviewReportsService = reviewReportsService;
        _logger               = logger;
        _tokenService          = tokenService;
        _jwtService            = jwtService;
        _localizer             = localizer;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        UpdateAdminStatus();
        if(IsAdmin) _ = LoadAsync();
    }

    [RelayCommand]
    Task Refresh() => LoadAsync();

    [RelayCommand]
    Task ToggleShowResolved() => LoadAsync();

    [RelayCommand]
    async Task ResolveReportAsync(ReviewReportListItem? item)
    {
        if(item is null) return;

        (bool succeeded, string? error) = await _reviewReportsService.ResolveAsync(item.Id);

        if(!succeeded)
        {
            HasError     = true;
            ErrorMessage = error ?? _localizer["ReviewReportsFailedToResolve"];

            return;
        }

        await LoadAsync();
    }

    [RelayCommand]
    async Task DeleteReportAsync(ReviewReportListItem? item)
    {
        if(item is null) return;

        if(!await ConfirmationDialogHelper.ConfirmDeleteAsync(_localizer, item.SoftwareName))
            return;

        (bool succeeded, string? error) = await _reviewReportsService.DeleteAsync(item.Id);

        if(!succeeded)
        {
            HasError     = true;
            ErrorMessage = error ?? _localizer["ReviewReportsFailedToDelete"];

            return;
        }

        await LoadAsync();
    }

    async Task LoadAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            IsDataLoaded = false;
            Reports.Clear();

            (var reports, string? error) = await _reviewReportsService.GetReportsAsync(ShowResolved ? null : false);

            if(error is not null)
            {
                HasError     = true;
                ErrorMessage = error;
            }

            foreach(ReviewReportDto report in reports)
                Reports.Add(new ReviewReportListItem
                {
                    Id            = report.Id ?? 0,
                    ReporterName  = report.ReporterName ?? "(deleted)",
                    SoftwareName  = report.SoftwareName ?? string.Empty,
                    ReviewerName  = report.ReviewerName ?? string.Empty,
                    ReasonText    = FormatReason(report.Reason),
                    StatusText = report.IsResolved == true
                        ? _localizer["ReviewReportsStatusResolved"]
                        : _localizer["ReviewReportsStatusPending"],
                    CreatedOnText = report.CreatedOn?.LocalDateTime.ToString("yyyy-MM-dd HH:mm") ?? string.Empty,
                    IsResolved    = report.IsResolved == true
                });

            IsDataLoaded = !HasError;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading review reports");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    void UpdateAdminStatus()
    {
        string token = _tokenService.GetToken();
        IsAdmin = _jwtService.GetRoles(token).Any(r =>
            string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, "UberAdmin", StringComparison.OrdinalIgnoreCase));
    }

    string FormatReason(int? code) => code switch
    {
        (int)ReviewReportReason.Spam       => _localizer["ReviewReportsReasonSpam"],
        (int)ReviewReportReason.Offensive  => _localizer["ReviewReportsReasonOffensive"],
        (int)ReviewReportReason.Misleading => _localizer["ReviewReportsReasonMisleading"],
        (int)ReviewReportReason.OffTopic   => _localizer["ReviewReportsReasonOffTopic"],
        _                                  => _localizer["ReviewReportsReasonOther"]
    };
}
