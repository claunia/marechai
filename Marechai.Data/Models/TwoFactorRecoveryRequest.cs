using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Substitutes a single-use recovery code for the second factor when the user has lost access to their
///     authenticator. The code is consumed (single-use) on success.
/// </summary>
public sealed record TwoFactorRecoveryRequest
{
    /// <summary>The pending-2FA token returned by <c>POST /auth/login</c>.</summary>
    [Required]
    [JsonPropertyName("twoFactorToken")]
    public string TwoFactorToken { get; set; } = null!;

    /// <summary>A recovery code generated when 2FA was enabled or last regenerated.</summary>
    [Required]
    [JsonPropertyName("recoveryCode")]
    public string RecoveryCode { get; set; } = null!;
}
