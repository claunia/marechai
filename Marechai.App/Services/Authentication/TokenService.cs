using Windows.Storage;

namespace Marechai.App.Services.Authentication;

public interface ITokenService
{
    string GetToken();

    void RemoveToken();

    void SetToken(string token);
}

public sealed class TokenService : ITokenService
{
    readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

    /// <inheritdoc />
    public string GetToken() => (string)_settings.Values["token"];

    /// <inheritdoc />
    public void RemoveToken()
    {
        _settings.Values.Remove("token");
    }

    /// <inheritdoc />
    public void SetToken(string token)
    {
        _settings.Values["token"] = token;
    }
}