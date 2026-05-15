using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

/// <summary>
///     Wire body for POST /software/genres-by-software linking a Software to a SoftwareGenre.
///     Both fields are <c>[Required]</c> non-nullable so the OpenAPI schema emits direct properties
///     and Kiota does not produce composed-type wrappers (per kiota composed-type-wrapper trap).
/// </summary>
public sealed class SoftwareGenreLinkDto
{
    [JsonPropertyName("software_id")]
    [Required]
    public ulong SoftwareId { get; set; }

    [JsonPropertyName("genre_id")]
    [Required]
    public int GenreId { get; set; }
}
