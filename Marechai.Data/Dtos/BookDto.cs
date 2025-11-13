using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class BookDto : DocumentBaseDto
{
    [JsonPropertyName("isbn")]
    public string? Isbn { get; set; }
    [JsonPropertyName("pages")]
    public short? Pages { get; set; }
    [JsonPropertyName("edition")]
    public int? Edition { get; set; }
    [JsonPropertyName("previous_id")]
    public long? PreviousId { get; set; }
    [JsonPropertyName("source_id")]
    public long? SourceId { get; set; }
}