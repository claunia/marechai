#nullable enable

using System;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels;

/// <summary>
///     Backs <see cref="ForgotPasswordPage" />. The actual reset step (clicking the link in the email,
///     entering a new password) is web-only — the desktop/mobile app only collects the email and asks
///     the server to dispatch a reset link, then directs the user to open the link in a web browser to
///     complete the flow.
/// </summary>
public partial class ForgotPasswordViewModel : ObservableObject
{
    private readonly AuthService      _authService;
    private readonly IRegionManager   _regionManager;
    private readonly IStringLocalizer _stringLocalizer;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isSubmitting;

    [ObservableProperty]
    private bool _submitted;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public ForgotPasswordViewModel(IRegionManager regionManager, AuthService authService,
                                   IStringLocalizer stringLocalizer)
    {
        _regionManager   = regionManager;
        _authService     = authService;
        _stringLocalizer = stringLocalizer;
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        ErrorMessage = null;

        if(string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = _stringLocalizer["ForgotPasswordPage_EmailRequired"];

            return;
        }

        IsSubmitting = true;

        try
        {
            // The server always returns 204 regardless of whether the email matches a real account
            // (anti-enumeration). AuthService swallows transport-level failures and likewise reports
            // success, so the UI shows a single generic confirmation regardless.
            await _authService.ForgotPasswordAsync(Email.Trim());
            Submitted = true;
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private void BackToLogin() => _regionManager.RequestNavigate(RegionNames.Content, nameof(LoginPage));
}
