using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.App.Services.Authentication;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

/// <summary>
///     Wraps the authenticated <c>/auth/me/two-factor/*</c> endpoints used by the Settings &gt; Security UI.
///     Separate from <see cref="AuthService" /> so the implementation isn't constrained by Uno.Extensions'
///     <c>IAuthenticationService</c> contract.
/// </summary>
public sealed class TwoFactorService(Client client)
{
    public async Task<TwoFactorStatusDto?> GetStatusAsync()
    {
        try
        {
            return await client.Auth.Me.TwoFactor.Status.GetAsync();
        }
        catch(Exception)
        {
            return null;
        }
    }

    public async Task<AuthenticatorSetupResponse?> SetupAuthenticatorAsync()
    {
        try
        {
            return await client.Auth.Me.TwoFactor.Authenticator.Setup.PostAsync();
        }
        catch(Exception)
        {
            return null;
        }
    }

    public async Task<(bool Succeeded, IList<string> RecoveryCodes, string? ErrorMessage)> EnableAuthenticatorAsync(
        string code)
    {
        try
        {
            RecoveryCodesResponse response = await client.Auth.Me.TwoFactor.Authenticator.Enable.PostAsync(
                                                     new AuthenticatorEnableRequest { Code = code });

            return (true, response?.RecoveryCodes ?? new List<string>(), null);
        }
        catch(ProblemDetails ex)
        {
            return (false, new List<string>(), ex.Detail ?? ex.Title);
        }
        catch(Exception ex)
        {
            return (false, new List<string>(), ex.Message);
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> StartEmailEnableAsync()
    {
        try
        {
            await client.Auth.Me.TwoFactor.Email.Start.PostAsync();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            return (false, ex.Detail ?? ex.Title);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, IList<string> RecoveryCodes, string? ErrorMessage)> EnableEmailAsync(
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

            return (true, response?.RecoveryCodes ?? new List<string>(), null);
        }
        catch(ProblemDetails ex)
        {
            return (false, new List<string>(), ex.Detail ?? ex.Title);
        }
        catch(Exception ex)
        {
            return (false, new List<string>(), ex.Message);
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> DisableAuthenticatorAsync(string password, string code,
        string provider)
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
            return (false, ex.Detail ?? ex.Title);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> DisableEmailAsync(string password, string code,
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
            return (false, ex.Detail ?? ex.Title);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Succeeded, IList<string> RecoveryCodes, string? ErrorMessage)>
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

            return (true, response?.RecoveryCodes ?? new List<string>(), null);
        }
        catch(ProblemDetails ex)
        {
            return (false, new List<string>(), ex.Detail ?? ex.Title);
        }
        catch(Exception ex)
        {
            return (false, new List<string>(), ex.Message);
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> AdminDisableAsync(string userId)
    {
        try
        {
            await client.Users[userId].TwoFactor.Disable.PostAsync();

            return (true, null);
        }
        catch(ProblemDetails ex)
        {
            return (false, ex.Detail ?? ex.Title);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
