#nullable enable

using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels;

/// <summary>
///     Backs <see cref="RegisterPage" />. Collects the registration fields and submits them to the server,
///     which creates the user in an unconfirmed state and dispatches a confirmation email. The user must
///     click the link in the email (which goes to the web app) before they can sign in.
/// </summary>
public partial class RegisterViewModel : ObservableObject
{
    private readonly AuthService      _authService;
    private readonly IRegionManager   _regionManager;
    private readonly IStringLocalizer _stringLocalizer;

    [ObservableProperty] private string _invitationCode = string.Empty;
    [ObservableProperty] private string _email          = string.Empty;
    [ObservableProperty] private string _userName       = string.Empty;
    [ObservableProperty] private string _displayName    = string.Empty;
    [ObservableProperty] private string _password       = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty] private bool _isSubmitting;
    [ObservableProperty] private bool _submitted;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public RegisterViewModel(IRegionManager regionManager, AuthService authService,
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

        if(string.IsNullOrWhiteSpace(InvitationCode))
        {
            ErrorMessage = _stringLocalizer["RegisterPage.Error.InvitationCodeRequired"];

            return;
        }

        if(string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = _stringLocalizer["RegisterPage.Error.EmailRequired"];

            return;
        }

        if(string.IsNullOrWhiteSpace(UserName))
        {
            ErrorMessage = _stringLocalizer["RegisterPage.Error.UserNameRequired"];

            return;
        }

        if(string.IsNullOrWhiteSpace(DisplayName))
        {
            ErrorMessage = _stringLocalizer["RegisterPage.Error.DisplayNameRequired"];

            return;
        }

        if(string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = _stringLocalizer["RegisterPage.Error.PasswordRequired"];

            return;
        }

        if(Password != ConfirmPassword)
        {
            ErrorMessage = _stringLocalizer["RegisterPage.Error.PasswordMismatch"];

            return;
        }

        IsSubmitting = true;

        try
        {
            (bool ok, string? err) = await _authService.RegisterAsync(Email.Trim(),
                                                                      UserName.Trim(),
                                                                      Password,
                                                                      DisplayName.Trim(),
                                                                      InvitationCode.Trim().ToUpperInvariant());

            if(ok)
            {
                Submitted = true;

                return;
            }

            ErrorMessage = err ?? _stringLocalizer["RegisterPage.Error.Generic"];
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private void BackToLogin() => _regionManager.RequestNavigate(RegionNames.Content, nameof(LoginPage));

    [RelayCommand]
    private void ResendConfirmation() =>
        _regionManager.RequestNavigate(RegionNames.Content, nameof(ResendConfirmationPage));
}
