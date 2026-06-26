using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareCompilationDto : BaseDto<ulong>
{
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }
    [JsonPropertyName("software_id")]
    public ulong? SoftwareId { get; set; }
    [JsonPropertyName("software")]
    public string? Software { get; set; }
    [JsonPropertyName("machine_id")]
    public int? MachineId { get; set; }
    [JsonPropertyName("machine")]
    public string? Machine { get; set; }
    [JsonPropertyName("predecessor_id")]
    public ulong? PredecessorId { get; set; }
    [JsonPropertyName("predecessor")]
    public string? Predecessor { get; set; }
    [JsonPropertyName("relationship_type")]
    public SoftwareRelationshipType RelationshipType { get; set; }
    [JsonPropertyName("successors")]
    public List<SoftwareCompilationSuccessorDto> Successors { get; set; } = [];
    [JsonPropertyName("front_cover_id")]
    public Guid? FrontCoverId { get; set; }
}
