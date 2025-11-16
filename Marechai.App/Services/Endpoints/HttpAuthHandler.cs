using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Services.Endpoints;

public sealed class HttpAuthHandler(ITokenService tokenService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                 CancellationToken  cancellationToken)
    {
        string token = tokenService.GetToken();

        if(!string.IsNullOrEmpty(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        return response;
    }
}