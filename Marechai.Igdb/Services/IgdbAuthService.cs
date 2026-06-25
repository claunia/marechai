using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Marechai.Igdb.Services;

public class IgdbAuthService
{
    readonly HttpClient _httpClient;
    readonly string     _clientId;
    readonly string     _clientSecret;
    readonly string     _tokenCachePath;

    string   _accessToken;
    DateTime _expiresAt;

    public IgdbAuthService(HttpClient httpClient, string clientId, string clientSecret, string tokenCachePath)
    {
        _httpClient     = httpClient;
        _clientId       = clientId;
        _clientSecret   = clientSecret;
        _tokenCachePath = tokenCachePath;
    }

    public string ClientId => _clientId;

    public async Task<string> GetAccessTokenAsync()
    {
        if(!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _expiresAt)
            return _accessToken;

        if(TryLoadCachedToken())
            return _accessToken;

        await AuthenticateAsync();

        return _accessToken;
    }

    bool TryLoadCachedToken()
    {
        if(!File.Exists(_tokenCachePath))
            return false;

        try
        {
            var cached = JsonSerializer.Deserialize<CachedToken>(File.ReadAllText(_tokenCachePath));

            if(cached == null || DateTime.UtcNow >= cached.ExpiresAt)
                return false;

            _accessToken = cached.AccessToken;
            _expiresAt   = cached.ExpiresAt;

            return true;
        }
        catch(Exception)
        {
            return false;
        }
    }

    async Task AuthenticateAsync()
    {
        var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        ]);

        using HttpResponseMessage response =
            await _httpClient.PostAsync("https://id.twitch.tv/oauth2/token", content);

        response.EnsureSuccessStatusCode();

        string body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        _accessToken = doc.RootElement.GetProperty("access_token").GetString();
        int expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();

        // Refresh a day early so a long-running batch never gets caught mid-call with a stale token.
        _expiresAt = DateTime.UtcNow.AddSeconds(expiresIn).AddDays(-1);

        string directory = Path.GetDirectoryName(_tokenCachePath);

        if(!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(_tokenCachePath,
                           JsonSerializer.Serialize(new CachedToken
                           {
                               AccessToken = _accessToken,
                               ExpiresAt   = _expiresAt
                           }));
    }

    sealed class CachedToken
    {
        public string   AccessToken { get; set; }
        public DateTime ExpiresAt   { get; set; }
    }
}
