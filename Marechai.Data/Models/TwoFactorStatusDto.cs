using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Snapshot of the caller's 2FA state, returned by <c>GET /auth/me/two-factor/status</c>. Drives the
///     "Security" UI on the profile page.
/// </summary>
public sealed record TwoFactorStatusDto
{
    /// <summary>True when at least one method is enabled (mirrors <c>IdentityUser.TwoFactorEnabled</c>).</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("authenticatorEnabled")]
    public bool AuthenticatorEnabled { get; set; }

    [JsonPropertyName("emailEnabled")]
    public bool EmailEnabled { get; set; }

    /// <summary>Number of unused recovery codes the user still has (0&ndash;10 typically).</summary>
    [JsonPropertyName("recoveryCodesRemaining")]
    public int RecoveryCodesRemaining { get; set; }
}
