using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Asks the server to dispatch a password-reset email to the supplied address. The server replies
///     <c>204 No Content</c> regardless of whether the address corresponds to a real account, so a
///     bad actor cannot enumerate registered emails through this endpoint.
/// </summary>
public sealed record ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;
}
