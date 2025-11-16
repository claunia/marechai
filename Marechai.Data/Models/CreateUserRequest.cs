using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record CreateUserRequest
{
    [Required]
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;
    
    [Required]
    [MinLength(6)]
    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;
    
    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }
}
