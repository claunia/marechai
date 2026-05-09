using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

/// <summary>
///     Public, anonymous registration request. Requires a single-use invitation code in addition to the usual
///     account fields. The invitation code format is <c>XXXX-YYYY</c> (8 alphanumeric characters from a 32-char
///     unambiguous alphabet, separated by a dash) so client-side validation can pre-check length 9.
/// </summary>
public sealed record RegisterRequest
{
    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = null!;

    [Required]
    [StringLength(9, MinimumLength = 9)]
    [JsonPropertyName("invitationCode")]
    public string InvitationCode { get; set; } = null!;
}
