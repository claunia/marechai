using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record ChangePasswordRequest
{
    [Required]
    [MinLength(6)]
    [JsonPropertyName("newPassword")]
    public string NewPassword { get; set; } = null!;
}
