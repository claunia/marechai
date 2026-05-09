using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Confirms a self-service account-deletion request via the token previously emailed to the user.
///     The authenticated caller must match the token's subject; on success the account enters the 30-day
///     grace window during which the user can still call <c>POST /auth/me/delete/cancel</c> to abort.
/// </summary>
public sealed record ConfirmAccountDeletionRequest
{
    [Required]
    [JsonPropertyName("token")]
    public string Token { get; set; } = null!;
}
