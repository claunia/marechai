using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareGenreDto : BaseDto<int>
{
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    [Required]
    public int Type { get; set; }

    [JsonPropertyName("type_name")]
    public string? TypeName { get; set; }
}
