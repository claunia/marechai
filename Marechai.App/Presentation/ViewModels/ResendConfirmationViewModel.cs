#nullable enable

using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels;

/// <summary>
///     Backs <see cref="ResendConfirmationPage" />. Asks the server to resend the email-confirmation link
///     for an unconfirmed account. Always reports success regardless of whether the email matches a real
///     account, mirroring the server's anti-enumeration response.
/// </summary>
public partial class ResendConfirmationViewModel : ObservableObject
{
    private readonly AuthService      _authService;
    private readonly IRegionManager   _regionManager;
    private readonly IStringLocalizer _stringLocalizer;

    [ObservableProperty] private string _email = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty] private bool _isSubmitting;
    [ObservableProperty] private bool _submitted;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public ResendConfirmationViewModel(IRegionManager regionManager, AuthService authService,
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
            ErrorMessage = _stringLocalizer["ResendConfirmationPage.Error.EmailRequired"];

            return;
        }

        IsSubmitting = true;

        try
        {
            await _authService.ResendConfirmationAsync(Email.Trim());
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
