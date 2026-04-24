using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record BulkLockoutRequest
{
    [Required]
    [MinLength(1)]
    [JsonPropertyName("userIds")]
    public List<string> UserIds { get; set; } = [];

    [JsonPropertyName("enable")]
    public bool Enable { get; set; }
}
