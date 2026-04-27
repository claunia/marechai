using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record UpdatePublicProfileRequest
{
    [MaxLength(100)]
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [MaxLength(2000)]
    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    [MaxLength(500)]
    [Url]
    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [MaxLength(200)]
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("useGravatar")]
    public bool UseGravatar { get; set; }

    [MaxLength(100)]
    [JsonPropertyName("twitter")]
    public string? Twitter { get; set; }

    [MaxLength(100)]
    [JsonPropertyName("gitHub")]
    public string? GitHub { get; set; }

    [MaxLength(200)]
    [JsonPropertyName("mastodon")]
    public string? Mastodon { get; set; }

    [MaxLength(200)]
    [JsonPropertyName("facebook")]
    public string? Facebook { get; set; }

    [MaxLength(200)]
    [JsonPropertyName("linkedIn")]
    public string? LinkedIn { get; set; }
}
