using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public sealed record LanguageBySoftwareReleaseDto
{
    [JsonPropertyName("software_release_id")]
    public ulong SoftwareReleaseId { get; set; }

    [JsonPropertyName("language_code")]
    [Required]
    [StringLength(3)]
    public required string LanguageCode { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }
}
