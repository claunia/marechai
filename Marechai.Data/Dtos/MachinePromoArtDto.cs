using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class MachinePromoArtDto : BaseDto<Guid>
{
    [JsonPropertyName("machine_id")]
    [Required]
    public int MachineId { get; set; }

    [JsonPropertyName("group_id")]
    [Required]
    public int GroupId { get; set; }

    [JsonPropertyName("group_name")]
    public string? GroupName { get; set; }

    [JsonPropertyName("caption")]
    public string? Caption { get; set; }

    [JsonPropertyName("original_extension")]
    [Required]
    public string OriginalExtension { get; set; }
}
