using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record UpdateUserRequest
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
    
    [JsonPropertyName("emailConfirmed")]
    public bool? EmailConfirmed { get; set; }
    
    [JsonPropertyName("lockoutEnabled")]
    public bool? LockoutEnabled { get; set; }
}
