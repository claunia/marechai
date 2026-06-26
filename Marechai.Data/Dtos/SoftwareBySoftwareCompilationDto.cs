using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareBySoftwareCompilationDto
{
    [JsonPropertyName("software_compilation_id")]
    public ulong SoftwareCompilationId { get; set; }
    [JsonPropertyName("software_id")]
    public ulong SoftwareId { get; set; }
    [JsonPropertyName("software_name")]
    public string? SoftwareName { get; set; }
}
