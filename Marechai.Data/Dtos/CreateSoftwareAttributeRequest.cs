using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class CreateSoftwareAttributeRequest
{
    [JsonPropertyName("software_release_id")]
    [Required]
    public ulong SoftwareReleaseId { get; set; }

    [JsonPropertyName("category")]
    [Required]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    [Required]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    [Required]
    public string Value { get; set; } = string.Empty;
}
