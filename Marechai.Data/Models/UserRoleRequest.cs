using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record UserRoleRequest
{
    [Required]
    [JsonPropertyName("roleName")]
    public string RoleName { get; set; } = null!;
}
