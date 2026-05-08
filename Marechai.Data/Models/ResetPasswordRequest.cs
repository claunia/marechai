using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Completes a password-reset flow started via <c>POST /auth/password/forgot</c>. The
///     <see cref="Token" /> is the opaque value the server placed in the reset email link
///     (URL-encoded there) and consumed via <c>UserManager.ResetPasswordAsync</c>.
/// </summary>
public sealed record ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [Required]
    [JsonPropertyName("token")]
    public string Token { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [JsonPropertyName("newPassword")]
    public string NewPassword { get; set; } = null!;
}
