using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Returned when 2FA is first enabled or recovery codes are regenerated. <see cref="RecoveryCodes" /> is the
///     full set of newly-generated single-use codes &mdash; the server does NOT keep them in plaintext after this
///     response, so the user must save them somewhere safe.
/// </summary>
public sealed record RecoveryCodesResponse
{
    [JsonPropertyName("recoveryCodes")]
    public List<string> RecoveryCodes { get; set; } = [];
}
