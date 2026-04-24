using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record BulkOperationResult
{
    [JsonPropertyName("succeededCount")]
    public int SucceededCount { get; set; }

    [JsonPropertyName("failedCount")]
    public int FailedCount { get; set; }

    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }
}
