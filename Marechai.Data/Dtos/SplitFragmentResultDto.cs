using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SplitFragmentResultDto
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("created_id")]
    public long? CreatedId { get; set; }

    [JsonPropertyName("already_existed")]
    public bool AlreadyExisted { get; set; }
}
