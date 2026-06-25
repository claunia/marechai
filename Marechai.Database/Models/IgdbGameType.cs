using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

/// <summary>
///     Mirror of IGDB's <c>/game_types</c> lookup table (main_game, dlc_addon, expansion, bundle,
///     standalone_expansion, mod, episode, season, remake, remaster, expanded_game, port, fork, pack, update, ...).
///     Small and rarely changing; used by <see cref="IgdbGame.GameTypeId" /> to detect edition/bundle/remaster
///     rows that must inherit their parent's <c>SoftwareId</c> instead of being matched independently.
/// </summary>
public class IgdbGameType : BaseModel<int>
{
    [Required]
    [StringLength(64)]
    public string Type { get; set; }
}
