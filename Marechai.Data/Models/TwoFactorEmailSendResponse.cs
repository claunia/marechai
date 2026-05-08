using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Acknowledges that an email-code 2FA message has been dispatched. The
///     <see cref="EmailMasked" /> field is suitable for direct UI display (e.g. <c>n***@e***.com</c>) so the user
///     knows which inbox to check without leaking the full address to anyone who only had a password.
/// </summary>
public sealed record TwoFactorEmailSendResponse
{
    [JsonPropertyName("emailMasked")]
    public string EmailMasked { get; set; } = null!;
}
