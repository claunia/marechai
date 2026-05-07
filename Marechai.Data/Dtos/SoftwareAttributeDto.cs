using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareAttributeDto : BaseDto<long>
{
    [JsonPropertyName("software_release_id")]
    [Required]
    public ulong SoftwareReleaseId { get; set; }

    [JsonPropertyName("category")]
    [Required]
    public required string Category { get; set; }

    [JsonPropertyName("key")]
    [Required]
    public required string Key { get; set; }

    [JsonPropertyName("value")]
    [Required]
    public required string Value { get; set; }

    [JsonPropertyName("platform_name")]
    public string? PlatformName { get; set; }

    [JsonPropertyName("region_names")]
    public string? RegionNames { get; set; }

    [JsonPropertyName("software_name")]
    public string? SoftwareName { get; set; }

    [JsonPropertyName("software_release_title")]
    public string? SoftwareReleaseTitle { get; set; }
}
