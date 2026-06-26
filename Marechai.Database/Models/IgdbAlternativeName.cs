using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

/// <summary>
///     Mirror of one row from IGDB's <c>/alternative_names</c> endpoint — a regional, script, or
///     otherwise alternative title IGDB records for a game (mirrors the same concept as
///     <see cref="SoftwareAlternativeTitle" /> on our side). <see cref="GameIgdbId" /> is the
///     referenced game's IGDB id, not a local FK, since the game may not be mirrored yet when
///     alternative names are pulled.
/// </summary>
public class IgdbAlternativeName : BaseModel<long>
{
    [Required]
    public long IgdbId { get; set; }

    [Required]
    public long GameIgdbId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Comment { get; set; }
}
