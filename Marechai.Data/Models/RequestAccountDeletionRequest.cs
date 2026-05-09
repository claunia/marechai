using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Initiates a GDPR-compliant self-service account-deletion flow. Requires the user's current password
///     and, if 2FA is enabled, a fresh second-factor code. On success the server emails a confirmation
///     link the user must click within the token's lifetime to actually move the account into the 30-day
///     grace window.
/// </summary>
public sealed record RequestAccountDeletionRequest
{
    [Required]
    [DataType(DataType.Password)]
    [JsonPropertyName("currentPassword")]
    public string CurrentPassword { get; set; } = null!;

    /// <summary>
    ///     Required when the user has 2FA enabled; one of <c>"authenticator"</c>, <c>"email"</c> or
    ///     <c>"recovery"</c>. Ignored otherwise.
    /// </summary>
    [JsonPropertyName("twoFactorProvider")]
    public string? TwoFactorProvider { get; set; }

    /// <summary>
    ///     Required when the user has 2FA enabled. The TOTP digits, the email code, or a recovery code
    ///     depending on <see cref="TwoFactorProvider" />.
    /// </summary>
    [JsonPropertyName("twoFactorCode")]
    public string? TwoFactorCode { get; set; }
}
