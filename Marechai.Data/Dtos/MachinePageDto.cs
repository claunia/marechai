using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class MachinePageDto
{
    [JsonPropertyName("items")]
    public List<MachineDto> Items { get; set; } = new();

    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
}
