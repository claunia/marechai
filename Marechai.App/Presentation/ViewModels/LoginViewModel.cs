#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services.Authentication;
using Uno.Extensions.Authentication;

namespace Marechai.App.Presentation.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly IRegionManager         _regionManager;
    private readonly IStringLocalizer       _stringLocalizer;

    [ObservableProperty]
    private string _email = string.Empty;
    [ObservableProperty]
    private string? _errorMessage;
    [ObservableProperty]
    private bool _isLoggingIn;
    [ObservableProperty]
    private string _password = string.Empty;

    // Two-factor flow state
    [ObservableProperty]
    private bool _requiresTwoFactor;
    [ObservableProperty]
    private string? _twoFactorToken;
    [ObservableProperty]
    private ObservableCollection<string> _availableMethods = new();
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSendEmailButton))]
    private string _selectedProvider = "authenticator";
    [ObservableProperty]
    private string _code = string.Empty;
    [ObservableProperty]
    private bool _recoveryMode;
    [ObservableProperty]
    private string _recoveryCode = string.Empty;

    /// <summary>
    ///     <see langword="true" /> when the last login attempt's password was correct but the account's
    ///     email address has not yet been confirmed. Surfaces the "Resend confirmation email" link in the
    ///     UI.
    /// </summary>
    [ObservableProperty]
    private bool _emailNotConfirmed;

    public bool HasMultipleMethods   => AvailableMethods.Count > 1;
    public bool ShowSendEmailButton => !RecoveryMode && SelectedProvider == "email";

    public LoginViewModel(IRegionManager regionManager, IAuthenticationService authService,
                          IStringLocalizer stringLocalizer)
    {
        _regionManager   = regionManager;
        _authService     = authService;
        _stringLocalizer = stringLocalizer;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        // Clear previous error
        ErrorMessage = null;

        // Validate inputs
        if(string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = _stringLocalizer["LoginPage.Error.EmailRequired"];

            return;
        }

        if(string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = _stringLocalizer["LoginPage.Error.PasswordRequired"];

            return;
        }

        IsLoggingIn = true;

        try
        {
            EmailNotConfirmed = false;

            var credentials = new Dictionary<string, string>
            {
                ["Email"]    = Email,
                ["Password"] = Password
            };

            bool success = await _authService.LoginAsync(null, credentials, null, CancellationToken.None);

            if(success)
            {
                // Navigate back to news page and refresh auth state
                _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));

                return;
            }

            // Did the server signal that email confirmation is required?
            if(credentials.TryGetValue("emailNotConfirmed", out string? notConfirmed) && notConfirmed == "true")
            {
                EmailNotConfirmed = true;

                if(credentials.TryGetValue("error", out string? unconfirmedError))
                    ErrorMessage = unconfirmedError;
                else
                    ErrorMessage = _stringLocalizer["LoginPage.Error.EmailNotConfirmed"];

                return;
            }

            // Did the server signal that a 2FA step is required?
            if(credentials.TryGetValue("requiresTwoFactor", out string? flag) && flag == "true" &&
               credentials.TryGetValue("twoFactorToken", out string? token))
            {
                TwoFactorToken    = token;
                AvailableMethods  = new ObservableCollection<string>(
                    (credentials.TryGetValue("availableMethods", out string? methods) && !string.IsNullOrEmpty(methods)
                         ? methods.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         : ["authenticator"]));
                SelectedProvider  = AvailableMethods.FirstOrDefault() ?? "authenticator";
                RequiresTwoFactor = true;
                OnPropertyChanged(nameof(HasMultipleMethods));
                OnPropertyChanged(nameof(ShowSendEmailButton));

                return;
            }

            // Check if there's an error message in credentials
            if(credentials.TryGetValue("error", out string? error))
                ErrorMessage = error;
            else
                ErrorMessage = _stringLocalizer["LoginPage.Error.LoginFailed"];
        }
        catch(Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    [RelayCommand]
    private async Task VerifyCodeAsync()
    {
        ErrorMessage = null;

        if(string.IsNullOrWhiteSpace(Code))
        {
            ErrorMessage = _stringLocalizer["LoginPage.Error.CodeRequired"];

            return;
        }

        if(_authService is not AuthService authService) return;

        IsLoggingIn = true;

        try
        {
            (bool ok, string err) = await authService.VerifyTwoFactorAsync(TwoFactorToken!, SelectedProvider,
                                                                            Code.Trim());

            if(ok)
                _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));
            else
                ErrorMessage = err ?? _stringLocalizer["Auth.InvalidCredentials"];
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    [RelayCommand]
    private async Task VerifyRecoveryAsync()
    {
        ErrorMessage = null;

        if(string.IsNullOrWhiteSpace(RecoveryCode))
        {
            ErrorMessage = _stringLocalizer["LoginPage.Error.CodeRequired"];

            return;
        }

        if(_authService is not AuthService authService) return;

        IsLoggingIn = true;

        try
        {
            (bool ok, string err) = await authService.VerifyRecoveryAsync(TwoFactorToken!, RecoveryCode.Trim());

            if(ok)
                _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));
            else
                ErrorMessage = err ?? _stringLocalizer["Auth.InvalidCredentials"];
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    [RelayCommand]
    private async Task SendEmailCodeAsync()
    {
        if(_authService is not AuthService authService) return;
        if(string.IsNullOrWhiteSpace(TwoFactorToken)) return;

        IsLoggingIn = true;

        try
        {
            (bool ok, _, string err) = await authService.SendLoginEmailCodeAsync(TwoFactorToken);

            if(!ok) ErrorMessage = err;
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    [RelayCommand]
    private void ToggleRecoveryMode()
    {
        RecoveryMode = !RecoveryMode;
        ErrorMessage = null;
        OnPropertyChanged(nameof(ShowSendEmailButton));
    }

    [RelayCommand]
    private void ForgotPassword() =>
        _regionManager.RequestNavigate(RegionNames.Content, nameof(ForgotPasswordPage));

    [RelayCommand]
    private void SignUp() =>
        _regionManager.RequestNavigate(RegionNames.Content, nameof(RegisterPage));

    [RelayCommand]
    private void ResendConfirmation() =>
        _regionManager.RequestNavigate(RegionNames.Content, nameof(ResendConfirmationPage));

    [RelayCommand]
    private void ClearError() => ErrorMessage = null;
}