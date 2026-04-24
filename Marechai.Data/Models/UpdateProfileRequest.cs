using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record UpdateProfileRequest
{
    [Required]
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }
}
