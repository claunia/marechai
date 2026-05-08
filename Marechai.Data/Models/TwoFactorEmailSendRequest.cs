using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Asks the server to send a fresh email second-factor code to the user identified by the pending-2FA token.
/// </summary>
public sealed record TwoFactorEmailSendRequest
{
    /// <summary>The pending-2FA token returned by <c>POST /auth/login</c>.</summary>
    [Required]
    [JsonPropertyName("twoFactorToken")]
    public string TwoFactorToken { get; set; } = null!;
}
