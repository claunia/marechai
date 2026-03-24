namespace Marechai;

public sealed class ApiAssetUrlProvider(string baseUrl)
{
    public string BaseUrl { get; } = baseUrl.TrimEnd('/');

    public string Asset(string path) => $"{BaseUrl}/{path.TrimStart('/')}";
}
