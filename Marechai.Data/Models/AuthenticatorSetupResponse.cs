using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Returned by <c>POST /auth/me/two-factor/authenticator/setup</c>. Contains the freshly-generated TOTP
///     shared key (base32) suitable for manual entry, and the matching <c>otpauth://</c> URI that clients render
///     as a QR code. The authenticator is NOT yet enabled &mdash; the client must verify a generated code via
///     <c>POST /auth/me/two-factor/authenticator/enable</c> to commit.
/// </summary>
public sealed record AuthenticatorSetupResponse
{
    /// <summary>Base32-encoded TOTP shared key, formatted in 4-character groups for easy manual entry.</summary>
    [JsonPropertyName("sharedKey")]
    public string SharedKey { get; set; } = null!;

    /// <summary>The <c>otpauth://totp/...</c> URI to render as a QR code.</summary>
    [JsonPropertyName("authenticatorUri")]
    public string AuthenticatorUri { get; set; } = null!;
}
