using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public sealed record UnM49BySoftwareReleaseDto
{
    [JsonPropertyName("software_release_id")]
    public ulong SoftwareReleaseId { get; set; }

    [JsonPropertyName("un_m49_id")]
    public short UnM49Id { get; set; }

    [JsonPropertyName("region_name")]
    public string? RegionName { get; set; }
}
