using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareReleaseLookupDto
{
    [JsonPropertyName("id")]
    public ulong Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("platform_name")]
    public string? PlatformName { get; set; }

    [JsonPropertyName("software_name")]
    public string? SoftwareName { get; set; }
}
