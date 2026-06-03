using System;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class WwpcScreenshot : BaseModel<long>
{
    [Required]
    public long WwpcSoftwareId { get; set; }
    public virtual WwpcSoftware WwpcSoftware { get; set; }

    /// <summary>Major release label this screenshot was scraped from (e.g. "1.x").</summary>
    [Required]
    [StringLength(64)]
    public string MajorRelease { get; set; }

    /// <summary>WinWorldPC screenshot detail page URL (<c>/screenshot/{releaseUuid}/{shotUuid}</c>).</summary>
    [Required]
    [StringLength(1024)]
    public string SourceUrl { get; set; }

    /// <summary>Direct CDN URL of the actual image binary (resolved from the screenshot detail page).</summary>
    [StringLength(1024)]
    public string ImageUrl { get; set; }

    [StringLength(1024)]
    public string Caption { get; set; }

    /// <summary>
    ///     Heuristically pre-filled at crawl time when the parent software has exactly one platform chip
    ///     that maps to exactly one <c>SoftwarePlatform</c>. Admin can override en-masse and per-row in the review UI.
    /// </summary>
    public ulong? SuggestedSoftwarePlatformId { get; set; }

    public bool IsEnabledByDefault { get; set; } = true;

    public DateTime CrawledOn { get; set; }

    public Guid? PromotedSoftwareScreenshotId { get; set; }
}
