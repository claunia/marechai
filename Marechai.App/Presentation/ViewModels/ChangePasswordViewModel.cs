#nullable enable

using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels;

/// <summary>
///     Backs <see cref="ChangePasswordPage" />. Collects the current and new password and calls the
///     self-service change-password endpoint, mirroring the Blazor app's <c>/account/change-password</c> page.
/// </summary>
public partial class ChangePasswordViewModel : ObservableObject
{
    private readonly AuthService      _authService;
    private readonly IRegionManager   _regionManager;
    private readonly IStringLocalizer _stringLocalizer;

    [ObservableProperty] private string _currentPassword = string.Empty;
    [ObservableProperty] private string _newPassword     = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSuccess))]
    private string? _successMessage;

    [ObservableProperty] private bool _isSubmitting;

    public bool HasError   => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);

    public ChangePasswordViewModel(IRegionManager regionManager, AuthService authService,
                                    IStringLocalizer stringLocalizer)
    {
        _regionManager   = regionManager;
        _authService     = authService;
        _stringLocalizer = stringLocalizer;
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        ErrorMessage   = null;
        SuccessMessage = null;

        if(string.IsNullOrWhiteSpace(CurrentPassword))
        {
            ErrorMessage = _stringLocalizer["ChangePasswordPage.Error.CurrentPasswordRequired"];

            return;
        }

        if(string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = _stringLocalizer["ChangePasswordPage.Error.NewPasswordRequired"];

            return;
        }

        if(NewPassword.Length < 6)
        {
            ErrorMessage = _stringLocalizer["ChangePasswordPage.Error.TooShort"];

            return;
        }

        if(NewPassword != ConfirmPassword)
        {
            ErrorMessage = _stringLocalizer["ChangePasswordPage.Error.Mismatch"];

            return;
        }

        IsSubmitting = true;

        try
        {
            (bool ok, string? err) = await _authService.ChangePasswordAsync(CurrentPassword, NewPassword);

            if(ok)
            {
                SuccessMessage   = _stringLocalizer["ChangePasswordPage.Success"];
                CurrentPassword  = string.Empty;
                NewPassword      = string.Empty;
                ConfirmPassword  = string.Empty;

                return;
            }

            ErrorMessage = err ?? _stringLocalizer["ChangePasswordPage.Error.Generic"];
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private void BackToSettings() => _regionManager.RequestNavigate(RegionNames.Content, nameof(SettingsPage));
}
