using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record AuthRequest
{
    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;
    [Required]
    [DataType(DataType.Password)]
    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;
}