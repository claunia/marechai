using System;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class IgdbGame : BaseModel<long>
{
    [Required]
    public long IgdbId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    /// <summary>IGDB's URL-safe <c>slug</c> field, used to build a working public link
    /// (<c>https://www.igdb.com/games/{slug}</c>); the numeric <see cref="IgdbId" /> alone cannot be
    /// resolved to a working IGDB URL.</summary>
    [StringLength(255)]
    public string Slug { get; set; }

    /// <summary>
    ///     FK to <see cref="IgdbGameType" />, IGDB's <c>game_type</c> field. This is itself a lookup entity on
    ///     IGDB's side (<c>/game_types</c>), not a fixed enum. <c>category</c> is deprecated by IGDB and must not
    ///     be used.
    /// </summary>
    public int? GameTypeId { get; set; }

    public long? ParentGameId { get; set; }

    public long? VersionParentId { get; set; }

    /// <summary>Raw JSON array of IGDB platform ids, used only as a matching/disambiguation signal.</summary>
    [StringLength(4096)]
    public string PlatformIdsJson { get; set; }

    [Required]
    public IgdbMatchStatus MatchStatus { get; set; }

    [StringLength(64)]
    public string MatchType { get; set; }

    public double? MatchScore { get; set; }

    public double? PlatformOverlapScore { get; set; }

    [StringLength(4096)]
    public string CandidatesJson { get; set; }

    public DateTime? MatchedOn { get; set; }

    [StringLength(1024)]
    public string ErrorMessage { get; set; }

    public int BatchNumber { get; set; }

    public ulong? SoftwareId { get; set; }
}
