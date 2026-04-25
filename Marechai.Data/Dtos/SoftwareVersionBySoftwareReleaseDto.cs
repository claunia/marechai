using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareVersionBySoftwareReleaseDto
{
    [JsonPropertyName("release_id")]
    public ulong ReleaseId { get; set; }
    [JsonPropertyName("software_version_id")]
    public ulong SoftwareVersionId { get; set; }
    [JsonPropertyName("software_version")]
    public string? SoftwareVersion { get; set; }
    [JsonPropertyName("software_name")]
    public string? SoftwareName { get; set; }
}
