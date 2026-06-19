using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class UpdateMachinePromoArtRequest
{
    [JsonPropertyName("group_name")]
    public string? GroupName { get; set; }

    [JsonPropertyName("caption")]
    public string? Caption { get; set; }
}
