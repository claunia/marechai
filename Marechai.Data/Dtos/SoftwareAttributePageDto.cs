using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareAttributePageDto
{
    [JsonPropertyName("items")]
    public List<SoftwareAttributeDto> Items { get; set; } = new();

    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
}
