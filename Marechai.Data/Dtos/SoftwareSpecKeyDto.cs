using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareSpecKeyDto
{
    [JsonPropertyName("key")]
    [Required]
    public required string Key { get; set; }

    [JsonPropertyName("values")]
    [Required]
    public required List<string> Values { get; set; }
}
