using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareBySoftwareReleaseDto
{
    [JsonPropertyName("release_id")]
    public ulong ReleaseId { get; set; }
    [JsonPropertyName("software_id")]
    public ulong SoftwareId { get; set; }
    [JsonPropertyName("software_name")]
    public string? SoftwareName { get; set; }
}
