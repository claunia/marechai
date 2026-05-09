using System;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Reports whether the calling user has an in-flight self-service deletion request. While
///     <see cref="IsPending" /> is true the API rejects most endpoints (the
///     <c>DeletionPendingFilter</c> only allows status, cancel, export and logout); the user must call
///     <c>POST /auth/me/delete/cancel</c> to restore normal access. <see cref="DeletionScheduledFor" />
///     is computed server-side as <c>DeletionRequestedAt + 30 days</c> so the client can render an
///     accurate countdown without knowing the grace-window length.
/// </summary>
public sealed record AccountDeletionStatusDto
{
    [JsonPropertyName("isPending")]
    public bool IsPending { get; set; }

    [JsonPropertyName("deletionRequestedAt")]
    public DateTime? DeletionRequestedAt { get; set; }

    [JsonPropertyName("deletionScheduledFor")]
    public DateTime? DeletionScheduledFor { get; set; }
}
