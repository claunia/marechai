using System;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class MobyGamesImportState : BaseModel<long>
{
    [Required]
    [StringLength(64)]
    public string MobyGameId { get; set; }

    [Required]
    public MobyGamesImportStatus Status { get; set; }

    [StringLength(1024)]
    public string ErrorMessage { get; set; }

    public DateTime? ProcessedOn { get; set; }

    public int BatchNumber { get; set; }

    public ulong? SoftwareId { get; set; }

    public         ulong?               SoftwareCompilationId { get; set; }
    public virtual SoftwareCompilation  SoftwareCompilation   { get; set; }

    public int? MobyNumericId { get; set; }

    /// <summary>
    ///     UTC timestamp of the last successful <c>update-year</c> refresh pass over this game
    ///     (live re-download of all MobyGames sub-pages plus diff-import of releases, description
    ///     and media). NULL means never refreshed since the original import. The refresh command
    ///     skips stamped rows unless <c>--force</c> or <c>--since</c> is passed.
    /// </summary>
    public DateTime? LastRefreshedAt { get; set; }

    /// <summary>
    ///     UTC timestamp at which <c>import-dlc-relations</c> examined this DLC and definitively found no base game
    ///     (no parent link on its page, or only an "Included in" compilation whose base game could not be derived).
    ///     NULL means not yet examined or examined with a transient failure. Stamped rows are skipped unless
    ///     <c>--recheck</c> is passed.
    /// </summary>
    public DateTime? DlcRelationCheckedAt { get; set; }
}
