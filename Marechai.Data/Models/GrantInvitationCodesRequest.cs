using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Request to grant N invitation codes to a user (admin only).
/// </summary>
public sealed record GrantInvitationCodesRequest
{
    [JsonPropertyName("count")]
    [Range(1, 50, ErrorMessage = "Count must be between 1 and 50.")]
    public int Count { get; set; }
}
