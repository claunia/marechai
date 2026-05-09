/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

/// <summary>
///     Outcome of <see cref="AuthService.LoginAsync" />. Either the JWT was issued and the user is fully signed
///     in (<see cref="Succeeded" /> + JWT applied), or the server requires a second factor and the caller must
///     prompt for a code using <see cref="TwoFactorToken" /> + <see cref="AvailableMethods" />, or the email
///     hasn't been confirmed yet (<see cref="EmailNotConfirmed" />) and the UI should offer a "resend" link.
/// </summary>
public sealed record LoginResult(bool          Succeeded,         string       ErrorMessage,
                                 bool          RequiresTwoFactor, string       TwoFactorToken,
                                 IList<string> AvailableMethods,
                                 bool          EmailNotConfirmed = false);

public sealed class AuthService(Marechai.ApiClient.Client             client,
                                TokenProvider                          tokenProvider,
                                JwtAuthenticationStateProvider         authStateProvider,
                                ILogger<AuthService>                   logger)
{
    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            var request = new AuthRequest
            {
                Email    = email,
                Password = password
            };

            AuthResponse response = await client.Auth.Login.PostAsync(request);

            if(response is null)
                return new LoginResult(false, "No response from server.", false, null, []);

            if(response.EmailNotConfirmed == true)
                return new LoginResult(false,
                                       "Please confirm your email address before signing in.",
                                       false,
                                       null,
                                       [],
                                       EmailNotConfirmed: true);

            if(response.Succeeded != true)
                return new LoginResult(false, response.Message ?? "Login failed.", false, null, []);

            if(response.RequiresTwoFactor == true)
            {
                List<string> methods = response.AvailableMethods?.ToList() ?? [];

                return new LoginResult(false, null, true, response.TwoFactorToken, methods);
            }

            if(string.IsNullOrWhiteSpace(response.Token))
                return new LoginResult(false, "No token received.", false, null, []);

            await tokenProvider.SetTokenAsync(response.Token);
            authStateProvider.NotifyUserAuthentication();

            return new LoginResult(true, null, false, null, []);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Login failed with ProblemDetails");

            return new LoginResult(false, ex.Detail ?? ex.Title ?? "Login failed.", false, null, []);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Login failed");

            return new LoginResult(false, "An error occurred during login.", false, null, []);
        }
    }

    /// <summary>
    ///     Posts the second-factor code captured from the login UI and, on success, applies the issued JWT.
    /// </summary>
    public async Task<(bool Succeeded, string ErrorMessage)> VerifyTwoFactorAsync(string twoFactorToken,
                                                                                   string provider, string code)
    {
        try
        {
            AuthResponse response = await client.Auth.Login.TwoFactor.PostAsync(new TwoFactorVerifyRequest
            {
                TwoFactorToken = twoFactorToken,
                Provider       = provider,
                Code           = code
            });

            if(response is null || response.Succeeded != true || string.IsNullOrWhiteSpace(response.Token))
                return (false, response?.Message ?? "Invalid verification code.");

            await tokenProvider.SetTokenAsync(response.Token);
            authStateProvider.NotifyUserAuthentication();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Two-factor verify failed with ProblemDetails");

            return (false, ex.Detail ?? ex.Title ?? "Invalid verification code.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Two-factor verify failed");

            return (false, "Invalid verification code.");
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> VerifyRecoveryAsync(string twoFactorToken,
                                                                                  string recoveryCode)
    {
        try
        {
            AuthResponse response = await client.Auth.Login.Recovery.PostAsync(new TwoFactorRecoveryRequest
            {
                TwoFactorToken = twoFactorToken,
                RecoveryCode   = recoveryCode
            });

            if(response is null || response.Succeeded != true || string.IsNullOrWhiteSpace(response.Token))
                return (false, response?.Message ?? "Invalid recovery code.");

            await tokenProvider.SetTokenAsync(response.Token);
            authStateProvider.NotifyUserAuthentication();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Recovery code verify failed with ProblemDetails");

            return (false, ex.Detail ?? ex.Title ?? "Invalid recovery code.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Recovery code verify failed");

            return (false, "Invalid recovery code.");
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
            logger.LogWarning(ex, "Send login email code failed with ProblemDetails");

            return (false, null, ex.Detail ?? ex.Title ?? "Could not send code.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Send login email code failed");

            return (false, null, "Could not send code.");
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
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Forgot password call returned a ProblemDetails");

            // Surface generic confirmation to defeat enumeration; the server already swallows the
            // distinction between unknown and known emails.
            return (true, null);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Forgot password call failed");

            return (true, null);
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> ResetPasswordAsync(string email, string token,
                                                                                 string newPassword)
    {
        try
        {
            await client.Auth.Password.Reset.PostAsync(new ResetPasswordRequest
            {
                Email       = email,
                Token       = token,
                NewPassword = newPassword
            });

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Reset password call returned a ProblemDetails");

            return (false, ex.Detail ?? ex.Title ?? "Invalid or expired reset link.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Reset password call failed");

            return (false, "An error occurred while resetting your password.");
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
            logger.LogWarning(ex, "Register call returned a ProblemDetails");

            return (false, ex.Detail ?? ex.Title ?? "Registration failed.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Register call failed");

            return (false, "An error occurred during registration.");
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
            logger.LogWarning(ex, "Confirm email call returned a ProblemDetails");

            return (false, ex.Detail ?? ex.Title ?? "Invalid or expired confirmation link.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Confirm email call failed");

            return (false, "An error occurred while confirming your email.");
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
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Resend confirmation call returned a ProblemDetails");

            return (true, null);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Resend confirmation call failed");

            return (true, null);
        }
    }

    /// <summary>
    ///     Asks the server to start the GDPR self-service deletion flow. Re-verifies the user's password
    ///     (and 2FA if enabled) server-side and dispatches a confirmation email; returns success once the
    ///     email has been queued. The actual move into the 30-day grace window happens when the user
    ///     clicks the link in that email and the client calls <see cref="ConfirmAccountDeletionAsync" />.
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
            logger.LogWarning(ex, "Account-deletion request returned a ProblemDetails");

            return (false, ex.Detail ?? ex.Title ?? "Could not start account deletion.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Account-deletion request failed");

            return (false, "An error occurred while requesting account deletion.");
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
            logger.LogWarning(ex, "Account-deletion confirm returned a ProblemDetails");

            return (false, ex.Detail ?? ex.Title ?? "Invalid or expired deletion link.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Account-deletion confirm failed");

            return (false, "An error occurred while confirming the deletion link.");
        }
    }

    public async Task<bool> CancelAccountDeletionAsync()
    {
        try
        {
            await client.Auth.Me.DeletePath.Cancel.PostAsync();

            return true;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Account-deletion cancel failed");

            return false;
        }
    }

    public async Task<AccountDeletionStatusDto> GetAccountDeletionStatusAsync()
    {
        try
        {
            return await client.Auth.Me.DeletePath.Status.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Account-deletion status query failed");

            return null;
        }
    }

    /// <summary>
    ///     Downloads the GDPR data-portability JSON dump for the current user. Returns the raw bytes
    ///     so the caller can hand them to the browser via JS interop (<c>download-blob.js</c>) for a
    ///     "Save as..." prompt.
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
        catch(Exception ex)
        {
            logger.LogError(ex, "Data export download failed");

            return null;
        }
    }

    public async Task<TwoFactorStatusDto> GetTwoFactorStatusAsync()
    {
        try
        {
            return await client.Auth.Me.TwoFactor.Status.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading two-factor status");

            return null;
        }
    }

    public async Task<AuthenticatorSetupResponse> SetupAuthenticatorAsync()
    {
        try
        {
            return await client.Auth.Me.TwoFactor.Authenticator.Setup.PostAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Authenticator setup failed");

            return null;
        }
    }

    public async Task<(bool Succeeded, IList<string> RecoveryCodes, string ErrorMessage)> EnableAuthenticatorAsync(
        string code)
    {
        try
        {
            RecoveryCodesResponse response = await client.Auth.Me.TwoFactor.Authenticator.Enable.PostAsync(
                                                     new AuthenticatorEnableRequest
                                                     {
                                                         Code = code
                                                     });

            return (true, response?.RecoveryCodes ?? [], null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Enable authenticator failed");

            return (false, [], ex.Detail ?? ex.Title ?? "Invalid verification code.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Enable authenticator failed");

            return (false, [], "Invalid verification code.");
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> StartEmailEnableAsync()
    {
        try
        {
            await client.Auth.Me.TwoFactor.Email.Start.PostAsync();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Email start failed");

            return (false, ex.Detail ?? ex.Title ?? "Could not send code.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Email start failed");

            return (false, "Could not send code.");
        }
    }

    public async Task<(bool Succeeded, IList<string> RecoveryCodes, string ErrorMessage)> EnableEmailAsync(
        string password, string code)
    {
        try
        {
            RecoveryCodesResponse response = await client.Auth.Me.TwoFactor.Email.Enable.PostAsync(
                                                     new EmailTwoFactorEnableRequest
                                                     {
                                                         Password = password,
                                                         Code     = code
                                                     });

            return (true, response?.RecoveryCodes ?? [], null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Enable email failed");

            return (false, [], ex.Detail ?? ex.Title ?? "Invalid verification code.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Enable email failed");

            return (false, [], "Invalid verification code.");
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> DisableAuthenticatorAsync(
        string password, string code, string provider)
    {
        try
        {
            await client.Auth.Me.TwoFactor.Authenticator.Disable.PostAsync(new DisableTwoFactorRequest
            {
                Password = password,
                Code     = code,
                Provider = provider
            });

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Disable authenticator failed");

            return (false, ex.Detail ?? ex.Title ?? "Invalid verification code.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Disable authenticator failed");

            return (false, "Invalid verification code.");
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> DisableEmailAsync(string password, string code,
                                                                                string provider)
    {
        try
        {
            await client.Auth.Me.TwoFactor.Email.Disable.PostAsync(new DisableTwoFactorRequest
            {
                Password = password,
                Code     = code,
                Provider = provider
            });

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Disable email failed");

            return (false, ex.Detail ?? ex.Title ?? "Invalid verification code.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Disable email failed");

            return (false, "Invalid verification code.");
        }
    }

    public async Task<(bool Succeeded, IList<string> RecoveryCodes, string ErrorMessage)>
        RegenerateRecoveryCodesAsync(string password, string code, string provider)
    {
        try
        {
            RecoveryCodesResponse response = await client.Auth.Me.TwoFactor.RecoveryCodes.Regenerate.PostAsync(
                                                     new DisableTwoFactorRequest
                                                     {
                                                         Password = password,
                                                         Code     = code,
                                                         Provider = provider
                                                     });

            return (true, response?.RecoveryCodes ?? [], null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Regenerate recovery codes failed");

            return (false, [], ex.Detail ?? ex.Title ?? "Invalid verification code.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Regenerate recovery codes failed");

            return (false, [], "Invalid verification code.");
        }
    }

    public async Task LogoutAsync()
    {
        await tokenProvider.RemoveTokenAsync();
        authStateProvider.NotifyUserLogout();
    }

    public async Task<UserDto> GetProfileAsync()
    {
        try
        {
            return await client.Auth.Me.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading profile");

            return null;
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> UpdateProfileAsync(string userName, string email,
                                                                                  string phoneNumber)
    {
        try
        {
            var request = new UpdateProfileRequest
            {
                UserName    = userName,
                Email       = email,
                PhoneNumber = phoneNumber
            };

            await client.Auth.Me.PutAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Profile update failed");

            return (false, ex.Detail ?? ex.Title ?? "Update failed.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Profile update failed");

            return (false, "An error occurred while updating profile.");
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> ChangePasswordAsync(string currentPassword,
                                                                                   string newPassword)
    {
        try
        {
            var request = new ChangeOwnPasswordRequest
            {
                CurrentPassword = currentPassword,
                NewPassword     = newPassword
            };

            await client.Auth.ChangePassword.PostAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Password change failed");

            return (false, ex.Detail ?? ex.Title ?? "Password change failed.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Password change failed");

            return (false, "An error occurred while changing password.");
        }
    }

    /// <summary>
    ///     Persist the user's preferred UI theme. Pass <see langword="null" /> to revert to the catalog default.
    ///     Returns <c>(true, null)</c> on success, or <c>(false, errorMessage)</c> with a user-facing message.
    /// </summary>
    public async Task<(bool Succeeded, string ErrorMessage)> SetThemeAsync(string themeId)
    {
        try
        {
            var request = new UpdateUserThemeRequest
            {
                ThemeId = themeId
            };

            await client.Auth.Me.Theme.PutAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Theme update failed");

            return (false, ex.Detail ?? ex.Title ?? "Theme update failed.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Theme update failed");

            return (false, "An error occurred while saving the theme.");
        }
    }

    public async Task<PublicProfileDto> GetPublicProfileAsync()
    {
        try
        {
            return await client.Auth.Me.PublicProfile.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading public profile");

            return null;
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> UpdatePublicProfileAsync(
        UpdatePublicProfileRequest request)
    {
        try
        {
            await client.Auth.Me.PublicProfile.PutAsync(request);

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Public profile update failed");

            return (false, ex.Detail ?? ex.Title ?? "Update failed.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Public profile update failed");

            return (false, "An error occurred while updating public profile.");
        }
    }

    public async Task<PublicProfileDto> UploadAvatarAsync(MultipartBody body)
    {
        try
        {
            return await client.Auth.Me.Avatar.Upload.PostAsync(body);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Avatar upload failed");

            return null;
        }
    }

    public async Task<(bool Succeeded, string ErrorMessage)> DeleteAvatarAsync()
    {
        try
        {
            await client.Auth.Me.Avatar.DeleteAsync();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            logger.LogWarning(ex, "Avatar delete failed");

            return (false, ex.Detail ?? ex.Title ?? "Delete failed.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Avatar delete failed");

            return (false, "An error occurred while deleting avatar.");
        }
    }
}
