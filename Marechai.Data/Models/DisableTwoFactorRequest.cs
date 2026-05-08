using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Disables a single 2FA method (or regenerates recovery codes) on the caller's own account. Defense in
///     depth: requires the current password AND a valid second-factor code from any active provider (or a
///     recovery code).
/// </summary>
public sealed record DisableTwoFactorRequest
{
    [Required]
    [DataType(DataType.Password)]
    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;

    /// <summary>
    ///     Valid TOTP code, email code, or recovery code. The server tries the providers indicated by
    ///     <see cref="Provider" /> first, and falls back to recovery-code redemption.
    /// </summary>
    [Required]
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;

    /// <summary>
    ///     One of <c>"authenticator"</c>, <c>"email"</c>, or <c>"recovery"</c> &mdash; identifies how
    ///     <see cref="Code" /> should be interpreted.
    /// </summary>
    [Required]
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = null!;
}
