using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Marechai.Igdb.Services;

public class IgdbHttpClient
{
    const string BaseUrl = "https://api.igdb.com/v4/";

    readonly HttpClient        _httpClient;
    readonly IgdbAuthService   _auth;
    readonly SemaphoreSlim     _concurrency;
    readonly TimeSpan          _minDelayBetweenRequests;
    DateTime                   _lastRequestUtc = DateTime.MinValue;
    readonly object            _delayLock = new();

    public IgdbHttpClient(HttpClient httpClient, IgdbAuthService auth, int requestsPerSecond, int maxConcurrentRequests)
    {
        _httpClient              = httpClient;
        _auth                    = auth;
        _concurrency             = new SemaphoreSlim(maxConcurrentRequests, maxConcurrentRequests);
        _minDelayBetweenRequests = TimeSpan.FromSeconds(1.0 / Math.Max(1, requestsPerSecond));
    }

    public static IgdbHttpClient Create(string clientId, string clientSecret, string tokenCachePath,
                                         int requestsPerSecond, int maxConcurrentRequests)
    {
        var httpClient = new HttpClient();
        var auth       = new IgdbAuthService(httpClient, clientId, clientSecret, tokenCachePath);

        return new IgdbHttpClient(httpClient, auth, requestsPerSecond, maxConcurrentRequests);
    }

    public async Task<JsonDocument> QueryAsync(string endpoint, string apicalypseBody)
    {
        await _concurrency.WaitAsync();

        try
        {
            await ThrottleAsync();

            for(int attempt = 0; attempt < 5; attempt++)
            {
                string token = await _auth.GetAccessTokenAsync();

                using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + endpoint)
                {
                    Content = new StringContent(apicalypseBody, Encoding.UTF8, "text/plain")
                };

                request.Headers.Add("Client-ID", _auth.ClientId);
                request.Headers.Add("Authorization", $"Bearer {token}");

                using HttpResponseMessage response = await _httpClient.SendAsync(request);

                if(response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TimeSpan wait = response.Headers.RetryAfter?.Delta ??
                                    TimeSpan.FromSeconds(Math.Pow(2, attempt + 1));

                    await Task.Delay(wait);

                    continue;
                }

                response.EnsureSuccessStatusCode();

                string body = await response.Content.ReadAsStringAsync();

                return JsonDocument.Parse(body);
            }

            throw new HttpRequestException($"IGDB request to {endpoint} kept being rate-limited after 5 attempts.");
        }
        finally
        {
            _concurrency.Release();
        }
    }

    async Task ThrottleAsync()
    {
        TimeSpan waitFor;

        lock(_delayLock)
        {
            DateTime now      = DateTime.UtcNow;
            DateTime nextSlot = _lastRequestUtc + _minDelayBetweenRequests;
            waitFor           = nextSlot > now ? nextSlot - now : TimeSpan.Zero;
            _lastRequestUtc   = waitFor > TimeSpan.Zero ? nextSlot : now;
        }

        if(waitFor > TimeSpan.Zero)
            await Task.Delay(waitFor);
    }
}
