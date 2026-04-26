using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Marechai.Data;

namespace Marechai.Data.Dtos;

public sealed record UnM49Dto
{
    [JsonPropertyName("id")]
    [Required]
    public short Id { get; set; }

    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }

    [JsonPropertyName("parent_id")]
    public short? ParentId { get; set; }

    [JsonPropertyName("parent_name")]
    public string? ParentName { get; set; }

    [JsonPropertyName("type")]
    [Required]
    public UnM49Type Type { get; set; }
}
