using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SplitSoftwareAttributeResultDto
{
    [JsonPropertyName("attribute_id")]
    public long AttributeId { get; set; }

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("software_release_id")]
    public ulong SoftwareReleaseId { get; set; }

    [JsonPropertyName("fragments")]
    public List<SplitFragmentResultDto> Fragments { get; set; } = new();
}
