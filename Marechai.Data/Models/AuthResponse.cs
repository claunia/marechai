using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record AuthResponse
{
    [JsonPropertyName("succeeded")]
    public bool Succeeded { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;

    /// <summary>
    ///     The full API JWT issued after successful authentication. Empty when
    ///     <see cref="RequiresTwoFactor" /> is <see langword="true" />, in which case the client must call one of the
    ///     <c>POST /auth/login/two-factor</c> family of endpoints with <see cref="TwoFactorToken" /> to obtain it.
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = null!;

    /// <summary>
    ///     <see langword="true" /> when the user has at least one 2FA method enabled and the password step
    ///     succeeded but the second factor has not yet been verified. The client must collect a code or recovery
    ///     code and call the appropriate verification endpoint.
    /// </summary>
    [JsonPropertyName("requiresTwoFactor")]
    public bool RequiresTwoFactor { get; set; }

    /// <summary>
    ///     Short-lived JWT scoped to the "two-factor pending" audience. Only valid against the
    ///     <c>POST /auth/login/two-factor</c>, <c>POST /auth/login/recovery</c> and
    ///     <c>POST /auth/login/two-factor/email/send</c> endpoints; rejected by every other API route.
    /// </summary>
    [JsonPropertyName("twoFactorToken")]
    public string? TwoFactorToken { get; set; }

    /// <summary>
    ///     Identifiers of the 2FA methods the user has enabled. Each entry is one of
    ///     <c>"authenticator"</c> or <c>"email"</c>. Recovery codes are always implicitly available when 2FA is
    ///     required and are not listed here; the client surfaces a separate "Use a recovery code" affordance.
    /// </summary>
    [JsonPropertyName("availableMethods")]
    public List<string> AvailableMethods { get; set; } = [];
}
