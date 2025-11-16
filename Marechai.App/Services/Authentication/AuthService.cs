using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using Uno.Extensions;
using Uno.Extensions.Authentication;

namespace Marechai.App.Services.Authentication;

public sealed class AuthService
    (ApiClient client, ITokenService tokenService, IStringLocalizer stringLocalizer) : IAuthenticationService
{
    /// <inheritdoc />
    public async ValueTask<bool> LoginAsync(IDispatcher? dispatcher, IDictionary<string, string>? credentials = null,
                                            string?      provider = null, CancellationToken? cancellationToken = null)
    {
        if(credentials is null) return false;

        string? email = credentials.FirstOrDefault(x => x.Key == "Email").Value;

        string? password = credentials.FirstOrDefault(x => x.Key == "Password").Value;

        if(email is null)
        {
            credentials["error"] = stringLocalizer["Auth.EmailIsRequired"];

            return false;
        }

        var loginModel = new AuthRequest
        {
            Email    = email,
            Password = password
        };

        AuthResponse? authResponse;

        try
        {
            tokenService.RemoveToken();
            authResponse = await client.Auth.Login.PostAsync(loginModel);
        }
        catch(ValidationApiException ex)
        {
            switch(ex.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    if(ex.Content is {} problemDetails)
                    {
                        if(problemDetails.Errors.Count > 0)
                        {
                            credentials["error"] = problemDetails.Errors.FirstOrDefault().Value?.FirstOrDefault() ??
                                                   stringLocalizer["Http.BadRequest"];

                            return false;
                        }

                        credentials["error"] = stringLocalizer["Http.BadRequest"];

                        return false;
                    }

                    break;
            }

            credentials["error"] = stringLocalizer["Http.BadRequest"];

            return false;
        }
        catch(ApiException ex)
        {
            switch(ex.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    credentials["error"] = stringLocalizer["Auth.InvalidCredentials"];

                    return false;
            }

            credentials["error"] = stringLocalizer["Http.BadRequest"];

            return false;
        }
        catch(Exception ex)
        {
#pragma warning disable EPC12
            credentials["error"] = ex.Message;
#pragma warning restore EPC12

            return false;
        }

        if(string.IsNullOrWhiteSpace(authResponse?.Token)) return false;

        tokenService.SetToken(authResponse.Token);

        return true;
    }

    /// <inheritdoc />
    public ValueTask<bool> RefreshAsync(CancellationToken? cancellationToken = null) =>
        IsAuthenticated(cancellationToken);

    /// <inheritdoc />
    public async ValueTask<bool> LogoutAsync(IDispatcher? dispatcher, CancellationToken? cancellationToken = null)
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
    public event EventHandler? LoggedOut;
}