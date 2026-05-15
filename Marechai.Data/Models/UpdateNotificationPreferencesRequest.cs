using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Request body for <c>PUT /auth/me/notification-preferences</c>. Carries the same fields as
///     <see cref="NotificationPreferencesDto" /> but as a separate type so future preference additions
///     (digest cadence, quiet hours, etc.) can default-allocate cleanly server-side without expanding the
///     read-side DTO.
/// </summary>
public sealed record UpdateNotificationPreferencesRequest
{
    [Required]
    [JsonPropertyName("notifyOnNewMessage")]
    public bool NotifyOnNewMessage { get; set; }
}
