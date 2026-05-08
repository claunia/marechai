using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Confirms an authenticator-app setup by verifying a code generated from the shared key returned by
///     <c>POST /auth/me/two-factor/authenticator/setup</c>. On success the authenticator method is enabled.
/// </summary>
public sealed record AuthenticatorEnableRequest
{
    [Required]
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;
}
