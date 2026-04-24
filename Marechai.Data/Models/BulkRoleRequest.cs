using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record BulkRoleRequest
{
    [Required]
    [MinLength(1)]
    [JsonPropertyName("userIds")]
    public List<string> UserIds { get; set; } = [];

    [Required]
    [JsonPropertyName("roleName")]
    public string RoleName { get; set; } = null!;
}
