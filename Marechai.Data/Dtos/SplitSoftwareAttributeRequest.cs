using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SplitSoftwareAttributeRequest
{
    [JsonPropertyName("separator")]
    public string? Separator { get; set; }
}
