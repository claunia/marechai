using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareVersionBySoftwareCompilationDto
{
    [JsonPropertyName("software_compilation_id")]
    public ulong SoftwareCompilationId { get; set; }
    [JsonPropertyName("software_version_id")]
    public ulong SoftwareVersionId { get; set; }
    [JsonPropertyName("software_version")]
    public string? SoftwareVersion { get; set; }
    [JsonPropertyName("software_name")]
    public string? SoftwareName { get; set; }
}
