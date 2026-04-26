using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public sealed record Iso639Dto
{
    [JsonPropertyName("id")]
    [Required]
    [StringLength(3)]
    public required string Id { get; set; }

    [JsonPropertyName("reference_name")]
    [Required]
    public required string ReferenceName { get; set; }

    [JsonPropertyName("part1")]
    public string? Part1 { get; set; }
}
