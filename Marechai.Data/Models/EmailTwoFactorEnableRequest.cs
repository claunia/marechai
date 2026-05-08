using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Confirms an email-code 2FA setup by combining the user's current password (defense-in-depth) and the
///     6-digit code that was emailed to them by <c>POST /auth/me/two-factor/email/start</c>.
/// </summary>
public sealed record EmailTwoFactorEnableRequest
{
    [Required]
    [DataType(DataType.Password)]
    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;

    [Required]
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;
}
