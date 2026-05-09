#nullable enable

using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels;

/// <summary>
///     Backs <see cref="DeleteAccountPage" />. Collects the password (and optional 2FA code) and asks the
///     server to dispatch a deletion-confirmation email. The actual deletion + 30-day grace window is
///     started when the user clicks the link in that email and lands on the web app's
///     <c>/account/delete-confirm</c> page (Uno doesn't yet implement deep-link handlers).
/// </summary>
public partial class DeleteAccountViewModel : ObservableObject
{
    private readonly AuthService      _authService;
    private readonly IRegionManager   _regionManager;
    private readonly IStringLocalizer _stringLocalizer;

    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private bool   _has2fa;
    [ObservableProperty] private string _provider = "authenticator";
    [ObservableProperty] private string _code     = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty] private bool _isSubmitting;
    [ObservableProperty] private bool _submitted;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public DeleteAccountViewModel(IRegionManager regionManager, AuthService authService,
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

        if(string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = _stringLocalizer["DeleteAccountPage.Error.PasswordRequired"];

            return;
        }

        if(Has2fa && string.IsNullOrWhiteSpace(Code))
        {
            ErrorMessage = _stringLocalizer["DeleteAccountPage.Error.CodeRequired"];

            return;
        }

        IsSubmitting = true;

        try
        {
            (bool ok, string? err) = await _authService.RequestAccountDeletionAsync(Password,
                                                                                    Has2fa ? Provider : null!,
                                                                                    Has2fa ? Code.Trim() : null!);

            if(ok)
            {
                Submitted = true;

                return;
            }

            ErrorMessage = err ?? _stringLocalizer["DeleteAccountPage.Error.Generic"];
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private void BackToSettings() => _regionManager.RequestNavigate(RegionNames.Content, nameof(SettingsPage));
}
