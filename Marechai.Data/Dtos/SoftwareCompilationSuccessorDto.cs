using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareCompilationSuccessorDto
{
    [JsonPropertyName("id")]
    public ulong Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("relationship_type")]
    public SoftwareRelationshipType RelationshipType { get; set; }
}
