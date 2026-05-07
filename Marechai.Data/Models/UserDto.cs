using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record UserDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
    
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = null!;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;
    
    [JsonPropertyName("emailConfirmed")]
    public bool EmailConfirmed { get; set; }
    
    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }
    
    [JsonPropertyName("phoneNumberConfirmed")]
    public bool PhoneNumberConfirmed { get; set; }
    
    [JsonPropertyName("lockoutEnabled")]
    public bool LockoutEnabled { get; set; }
    
    [JsonPropertyName("lockoutEnd")]
    public string? LockoutEnd { get; set; }
    
    [JsonPropertyName("accessFailedCount")]
    public int AccessFailedCount { get; set; }
    
    [JsonPropertyName("preferredThemeId")]
    public string? PreferredThemeId { get; set; }
    
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = [];
}
