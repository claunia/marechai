using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Submits a TOTP authenticator-app or email second-factor code together with the short-lived pending-2FA
///     JWT obtained from the password step. On success the server returns a final API JWT.
/// </summary>
public sealed record TwoFactorVerifyRequest
{
    /// <summary>The pending-2FA token returned by <c>POST /auth/login</c>.</summary>
    [Required]
    [JsonPropertyName("twoFactorToken")]
    public string TwoFactorToken { get; set; } = null!;

    /// <summary>One of <c>"authenticator"</c> or <c>"email"</c>.</summary>
    [Required]
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = null!;

    /// <summary>The 6-digit code from the authenticator app or the email message.</summary>
    [Required]
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;
}
