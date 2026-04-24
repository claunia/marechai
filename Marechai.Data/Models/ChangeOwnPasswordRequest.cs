using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record ChangeOwnPasswordRequest
{
    [Required]
    [DataType(DataType.Password)]
    [JsonPropertyName("currentPassword")]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [MinLength(6)]
    [DataType(DataType.Password)]
    [JsonPropertyName("newPassword")]
    public string NewPassword { get; set; } = null!;
}
