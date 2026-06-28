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
            // Email not yet confirmed: bubble up so the LoginViewModel can show a "resend confirmation"
            // affordance. Same sentinel-key transport convention as the two-factor signal below.
            if(authResponse?.EmailNotConfirmed == true)
            {
                credentials["emailNotConfirmed"] = "true";
                credentials["error"]             = stringLocalizer["LoginPage.Error.EmailNotConfirmed"];

                return false;
            }

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

    /// <summary>
    ///     Asks the server to send a password-reset email. Always returns success: the server already
    ///     responds 204 regardless of whether the address matches a real account (anti-enumeration), and we
    ///     mirror that on the client so the UI can show a single generic confirmation message.
    /// </summary>
    public async Task<(bool Succeeded, string ErrorMessage)> ForgotPasswordAsync(string email)
    {
        try
        {
            await client.Auth.Password.Forgot.PostAsync(new ForgotPasswordRequest
            {
                Email = email
            });

            return (true, null);
        }
        catch(ProblemDetails)
        {
            // Surface generic confirmation; do not leak server-side distinction between unknown and
            // known emails.
            return (true, null);
        }
        catch(Exception)
        {
            return (true, null);
        }
    }

    /// <summary>
    ///     Submits a public registration request. The user is created in an unconfirmed state and a
    ///     confirmation email is dispatched on success. On any failure the server returns a
    ///     <see cref="ProblemDetails" /> we surface to the caller.
    /// </summary>
    public async Task<(bool Succeeded, string ErrorMessage)> RegisterAsync(string email,        string userName,
                                                                            string password,    string displayName,
                                                                            string invitationCode)
    {
        try
        {
            await client.Auth.Register.PostAsync(new RegisterRequest
            {
                Email          = email,
                UserName       = userName,
                Password       = password,
                DisplayName    = displayName,
                InvitationCode = invitationCode
            });

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            return (false, ex.Detail ?? ex.Title ?? "Registration failed.");
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    ///     Submits the email-confirmation token captured from the link in the confirmation email. Any error
    ///     (unknown user, already confirmed, bad/expired token) is surfaced as a single generic message to
    ///     defeat probing.
    /// </summary>
    public async Task<(bool Succeeded, string ErrorMessage)> ConfirmEmailAsync(string email, string token)
    {
        try
        {
            await client.Auth.Email.Confirm.PostAsync(new ConfirmEmailRequest
            {
                Email = email,
                Token = token
            });

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            return (false, ex.Detail ?? ex.Title ?? "Invalid or expired confirmation link.");
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    ///     Asks the server to resend the email-confirmation message. Always returns success: the server already
    ///     responds 204 regardless of whether the address matches a real (or already-confirmed) account, and we
    ///     mirror that on the client so the UI can show a single generic confirmation message.
    /// </summary>
    public async Task<(bool Succeeded, string ErrorMessage)> ResendConfirmationAsync(string email)
    {
        try
        {
            await client.Auth.Email.ResendConfirmation.PostAsync(new ResendConfirmationRequest
            {
                Email = email
            });

            return (true, null);
        }
        catch(ProblemDetails)
        {
            return (true, null);
        }
        catch(Exception)
        {
            return (true, null);
        }
    }

    /// <summary>
    ///     Asks the server to start the GDPR self-service deletion flow. Re-verifies the user's password
    ///     and 2FA server-side and dispatches a confirmation email.
    /// </summary>
    public async Task<(bool Succeeded, string ErrorMessage)> RequestAccountDeletionAsync(
        string currentPassword, string twoFactorProvider, string twoFactorCode)
    {
        try
        {
            await client.Auth.Me.DeletePath.Request.PostAsync(new RequestAccountDeletionRequest
            {
                CurrentPassword   = currentPassword,
                TwoFactorProvider = twoFactorProvider,
                TwoFactorCode     = twoFactorCode
            });

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            return (false, ex.Detail ?? ex.Title ?? "Could not start account deletion.");
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> ConfirmAccountDeletionAsync(string token)
    {
        try
        {
            await client.Auth.Me.DeletePath.Confirm.PostAsync(new ConfirmAccountDeletionRequest
            {
                Token = token
            });

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            return (false, ex.Detail ?? ex.Title ?? "Invalid or expired deletion link.");
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<bool> CancelAccountDeletionAsync()
    {
        try
        {
            await client.Auth.Me.DeletePath.Cancel.PostAsync();

            return true;
        }
        catch(Exception)
        {
            return false;
        }
    }

    public async Task<AccountDeletionStatusDto> GetAccountDeletionStatusAsync()
    {
        try
        {
            return await client.Auth.Me.DeletePath.Status.GetAsync();
        }
        catch(Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     Downloads the GDPR data-portability JSON dump for the current user. Returns the raw bytes;
    ///     the caller hands them to a <c>FileSavePicker</c> to write to disk.
    /// </summary>
    public async Task<byte[]> ExportDataAsync()
    {
        try
        {
            using System.IO.Stream s = await client.Auth.Me.Export.GetAsync();

            if(s is null) return null;

            using var ms = new System.IO.MemoryStream();
            await s.CopyToAsync(ms);

            return ms.ToArray();
        }
        catch(Exception)
        {
            return null;
        }
    }

    public async Task<NotificationPreferencesDto?> GetNotificationPreferencesAsync()
    {
        try
        {
            return await client.Auth.Me.NotificationPreferences.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateNotificationPreferencesAsync(
        UpdateNotificationPreferencesRequest request)
    {
        try
        {
            await client.Auth.Me.NotificationPreferences.PutAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            return (false, ex.Detail ?? ex.Title ?? "Update failed.");
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
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
