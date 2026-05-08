using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Uno.Extensions;
using Uno.Extensions.Authentication;

namespace Marechai.App.Services.Authentication;

public sealed class AuthService
    (Client client, ITokenService tokenService, IStringLocalizer stringLocalizer) : IAuthenticationService
{
    /// <inheritdoc />
    public async ValueTask<bool> LoginAsync(IDispatcher dispatcher, IDictionary<string, string> credentials = null,
                                            string      provider = null, CancellationToken? cancellationToken = null)
    {
        if(credentials is null) return false;

        string email =
            (credentials.FirstOrDefault(x => x.Key.Equals("Email", StringComparison.OrdinalIgnoreCase)).Value ??
             credentials.FirstOrDefault(x => x.Key.Equals("email",    StringComparison.OrdinalIgnoreCase)).Value ??
             credentials.FirstOrDefault(x => x.Key.Equals("Username", StringComparison.OrdinalIgnoreCase)).Value)
          ?.Trim();

        string password =
            (credentials.FirstOrDefault(x => x.Key.Equals("Password", StringComparison.OrdinalIgnoreCase)).Value ??
             credentials.FirstOrDefault(x => x.Key.Equals("password", StringComparison.OrdinalIgnoreCase)).Value)
          ?.Trim();

        if(string.IsNullOrWhiteSpace(email))
        {
            credentials["error"] = stringLocalizer["Auth.EmailIsRequired"];

            return false;
        }

        if(string.IsNullOrWhiteSpace(password))
        {
            credentials["error"] = stringLocalizer["Auth.PasswordIsRequired"];

            return false;
        }

        var loginModel = new AuthRequest
        {
            Email    = email,
            Password = password
        };

        AuthResponse authResponse;

        try
        {
            tokenService.RemoveToken();
            authResponse = await client.Auth.Login.PostAsync(loginModel);
        }
        catch(ProblemDetails ex)
        {
            if(ex.Status == 400)
                credentials["error"] = ex.Detail ?? ex.Title ?? stringLocalizer["Http.BadRequest"];
            else if(ex.Status == 401)
                credentials["error"] = stringLocalizer["Auth.InvalidCredentials"];
            else
                credentials["error"] = ex.Detail ?? ex.Title ?? stringLocalizer["Http.BadRequest"];

            return false;
        }
        catch(ApiException ex)
        {
            if(ex.ResponseStatusCode == 401)
                credentials["error"] = stringLocalizer["Auth.InvalidCredentials"];
            else if(ex.ResponseStatusCode == 400)
                credentials["error"] = stringLocalizer["Http.BadRequest"];
            else
                credentials["error"] = ex.Message ?? stringLocalizer["Http.BadRequest"];

            return false;
        }
        catch(Exception ex)
        {
#pragma warning disable EPC12
            credentials["error"] = ex.Message;
#pragma warning restore EPC12

            return false;
        }

        if(string.IsNullOrWhiteSpace(authResponse?.Token))
        {
            // Two-factor required: bubble up the pending token + available methods so the LoginViewModel can
            // swap to the 2FA prompt. We deliberately return false (the ITokenService still has no usable token)
            // and use sentinel keys in the credentials dictionary as the only available transport.
            if(authResponse?.RequiresTwoFactor == true && !string.IsNullOrWhiteSpace(authResponse.TwoFactorToken))
            {
                credentials["requiresTwoFactor"] = "true";
                credentials["twoFactorToken"]    = authResponse.TwoFactorToken;
                credentials["availableMethods"]  = string.Join(",", authResponse.AvailableMethods ?? new List<string>());

                return false;
            }

            return false;
        }

        tokenService.SetToken(authResponse.Token);

        LoggedIn?.Invoke(this, EventArgs.Empty);

        return true;
    }

    /// <summary>
    ///     Completes a two-factor login by submitting an authenticator-app or email code together with the pending
    ///     token returned from <see cref="LoginAsync" />. On success the JWT is applied via <c>tokenService</c> and
    ///     the <see cref="LoggedIn" /> event fires.
    /// </summary>
    public async Task<(bool Succeeded, string ErrorMessage)> VerifyTwoFactorAsync(string twoFactorToken,
                                                                                   string provider, string code)
    {
        try
        {
            tokenService.RemoveToken();

            AuthResponse response = await client.Auth.Login.TwoFactor.PostAsync(new TwoFactorVerifyRequest
            {
                TwoFactorToken = twoFactorToken,
                Provider       = provider,
                Code           = code
            });

            if(response is null || response.Succeeded != true || string.IsNullOrWhiteSpace(response.Token))
                return (false, response?.Message ?? stringLocalizer["Auth.InvalidCredentials"]);

            tokenService.SetToken(response.Token);
            LoggedIn?.Invoke(this, EventArgs.Empty);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            return (false, ex.Detail ?? ex.Title ?? stringLocalizer["Auth.InvalidCredentials"]);
        }
        catch(ApiException ex)
        {
            return (false, ex.Message ?? stringLocalizer["Auth.InvalidCredentials"]);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> VerifyRecoveryAsync(string twoFactorToken,
                                                                                  string recoveryCode)
    {
        try
        {
            tokenService.RemoveToken();

            AuthResponse response = await client.Auth.Login.Recovery.PostAsync(new TwoFactorRecoveryRequest
            {
                TwoFactorToken = twoFactorToken,
                RecoveryCode   = recoveryCode
            });

            if(response is null || response.Succeeded != true || string.IsNullOrWhiteSpace(response.Token))
                return (false, response?.Message ?? stringLocalizer["Auth.InvalidCredentials"]);

            tokenService.SetToken(response.Token);
            LoggedIn?.Invoke(this, EventArgs.Empty);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            return (false, ex.Detail ?? ex.Title ?? stringLocalizer["Auth.InvalidCredentials"]);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, string EmailMasked, string ErrorMessage)> SendLoginEmailCodeAsync(
        string twoFactorToken)
    {
        try
        {
            TwoFactorEmailSendResponse response = await client.Auth.Login.TwoFactor.Email.Send.PostAsync(
                                                          new TwoFactorEmailSendRequest
                                                          {
                                                              TwoFactorToken = twoFactorToken
                                                          });

            return (true, response?.EmailMasked, null);
        }
        catch(ProblemDetails ex)
        {
            return (false, null, ex.Detail ?? ex.Title);
        }
        catch(Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    /// <inheritdoc />
    public ValueTask<bool> RefreshAsync(CancellationToken? cancellationToken = null) =>
        IsAuthenticated(cancellationToken);

    /// <inheritdoc />
    public async ValueTask<bool> LogoutAsync(IDispatcher dispatcher, CancellationToken? cancellationToken = null)
    {
        tokenService.RemoveToken();
        LoggedOut?.Invoke(this, EventArgs.Empty);

        return true;
    }

    /// <inheritdoc />
    public async ValueTask<bool> IsAuthenticated(CancellationToken? cancellationToken = null)
    {
        string token = tokenService.GetToken();

        // TODO: Check token validity
        return !string.IsNullOrWhiteSpace(token);
    }

    /// <inheritdoc />
    public string[] Providers { get; } = [];
    /// <inheritdoc />
    public event EventHandler LoggedOut;
    public event EventHandler LoggedIn;
}