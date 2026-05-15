using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     User-controlled email notification preferences. Returned by <c>GET /auth/me/notification-preferences</c>
///     and accepted by <c>PUT /auth/me/notification-preferences</c>. Both fields are <see cref="RequiredAttribute" />
///     non-nullable so the OpenAPI schema emits them as concrete <c>boolean</c> properties (avoids Kiota's
///     composed-type-wrapper trap that fires on nullable refs).
/// </summary>
public sealed record NotificationPreferencesDto
{
    /// <summary>
    ///     <see langword="true" /> when the user wants to receive an email each time a new conversation
    ///     message is delivered to their inbox. Defaults to <see langword="true" /> for new accounts and
    ///     existing accounts at migration time.
    /// </summary>
    [Required]
    [JsonPropertyName("notifyOnNewMessage")]
    public bool NotifyOnNewMessage { get; set; }
}
