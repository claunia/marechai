#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
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
            }
            else
            {
                // Check if there's an error message in credentials
                if(credentials.TryGetValue("error", out string? error))
                    ErrorMessage = error;
                else
                    ErrorMessage = _stringLocalizer["LoginPage.Error.LoginFailed"];
            }
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
    private void ClearError() => ErrorMessage = null;
}