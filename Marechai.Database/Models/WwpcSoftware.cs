using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class WwpcSoftware : BaseModel<long>
{
    [Required]
    [StringLength(1024)]
    public string SourceUrl { get; set; }

    [Required]
    [StringLength(256)]
    public string Slug { get; set; }

    [Required]
    public WwpcSoftwareStatus Status { get; set; }

    [Required]
    public WwpcProductType ProductType { get; set; }

    [Required]
    [StringLength(512)]
    public string Name { get; set; }

    [StringLength(512)]
    public string VendorName { get; set; }

    /// <summary>Vendor URL as published by WinWorldPC (typically the search-by-vendor link).</summary>
    [StringLength(1024)]
    public string VendorUrl { get; set; }

    /// <summary>Comma-separated raw chip categories (e.g. "Graphics,Publishing").</summary>
    [StringLength(1024)]
    public string RawCategoriesCsv { get; set; }

    /// <summary>Comma-separated raw platform chips (e.g. "DOS,Windows,MacOS").</summary>
    [StringLength(256)]
    public string PlatformsCsv { get; set; }

    /// <summary>"Release date" string straight from the product page (may be a partial year).</summary>
    [StringLength(64)]
    public string ReleaseDateText { get; set; }

    /// <summary>"User interface" string straight from the product page (GUI / CUI / TUI / etc).</summary>
    [StringLength(32)]
    public string UserInterface { get; set; }

    /// <summary>Raw English description text scraped from the product page.</summary>
    [MaxLength(262144)]
    public string RawDescription { get; set; }

    [MaxLength(262144)]
    public string EnglishDescriptionMuseum { get; set; }

    public int MuseumDescriptionPromptVersion { get; set; }

    /// <summary>JSON array of <c>SoftwareGenre.Id</c> (<c>Type=Category</c>) chosen by the categorize enricher.</summary>
    [MaxLength(4096)]
    public string SuggestedGenreIdsJson { get; set; }

    /// <summary>Pre-filled by the crawler when the vendor name is an exact normalized match against <c>Company.Name</c>.</summary>
    public int? SuggestedVendorCompanyId { get; set; }

    public int? WwpcCategoryId { get; set; }
    public virtual WwpcCategory WwpcCategory { get; set; }

    public DateTime CrawledOn { get; set; }

    [StringLength(2048)]
    public string LastError { get; set; }

    [StringLength(256)]
    public string ReviewedBy { get; set; }

    public DateTime? ReviewedOn { get; set; }

    public ulong? PromotedSoftwareId { get; set; }

    public virtual ICollection<WwpcVersion>    Versions    { get; set; }
    public virtual ICollection<WwpcScreenshot> Screenshots { get; set; }
}
