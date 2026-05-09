using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Requests another email-confirmation message be sent. Server always returns 204 regardless of whether the
///     email maps to a real account, to avoid account-existence enumeration.
/// </summary>
public sealed record ResendConfirmationRequest
{
    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;
}
