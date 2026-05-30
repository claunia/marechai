using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models;

/// <summary>
///     Maps the raw OS string scraped from old-dos.ru (e.g. "MS-DOS", "Windows 3.x") to a Marechai
///     <see cref="SoftwarePlatform" />. The key is the raw OS string so the seeder + admin overrides can
///     work on the same row. <see cref="IgnoreOnPromote" /> lets admins blacklist noisy OS strings that
///     should never map to a platform (a per-version override still works in the review dialog).
/// </summary>
public class OldDosOsPlatformMap
{
    [Key]
    [StringLength(256)]
    public string OsName { get; set; }

    public ulong? SoftwarePlatformId { get; set; }
    public virtual SoftwarePlatform SoftwarePlatform { get; set; }

    public bool IgnoreOnPromote { get; set; }
}
