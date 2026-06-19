using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Server-side projection of an <c>InvitationCode</c> row, returned by the self-service
///     <c>GET /invitation-codes/mine</c> endpoint. Deliberately omits <c>UsedByUserName</c> and
///     <c>UsedOn</c> so the owner does not learn who redeemed their code.
/// </summary>
public sealed record MyInvitationCodeDto
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;

    [JsonPropertyName("isUsed")]
    public bool IsUsed { get; set; }
}
