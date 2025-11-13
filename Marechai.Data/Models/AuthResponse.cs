using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record AuthResponse
{
    [JsonPropertyName("succeeded")]
    public bool Succeeded { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;
    [JsonPropertyName("token")]
    public string Token { get; set; } = null!;
}