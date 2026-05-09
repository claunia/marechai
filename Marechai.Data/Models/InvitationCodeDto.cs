using System;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Server-side projection of an <c>InvitationCode</c> row, returned by the admin
///     <c>GET /invitation-codes</c> endpoint. <see cref="IsUsed" /> is computed from <see cref="UsedOn" /> and is
///     surfaced separately so the client doesn't need to perform date-null checks.
/// </summary>
public sealed record InvitationCodeDto
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;

    [JsonPropertyName("createdOn")]
    public DateTime CreatedOn { get; set; }

    [JsonPropertyName("createdByUserName")]
    public string? CreatedByUserName { get; set; }

    [JsonPropertyName("usedOn")]
    public DateTime? UsedOn { get; set; }

    [JsonPropertyName("usedByUserName")]
    public string? UsedByUserName { get; set; }

    [JsonPropertyName("isUsed")]
    public bool IsUsed { get; set; }
}
