using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class OldDosSoftware : BaseModel<long>
{
    [Required]
    public int SourceId { get; set; }

    [Required]
    [StringLength(1024)]
    public string SourceUrl { get; set; }

    [Required]
    public OldDosSoftwareStatus Status { get; set; }

    [Required]
    [StringLength(512)]
    public string Name { get; set; }

    [StringLength(512)]
    public string DeveloperName { get; set; }

    [StringLength(256)]
    public string OsName { get; set; }

    [MaxLength(262144)]
    public string RussianDescription { get; set; }

    [StringLength(2048)]
    public string RussianCategoryPath { get; set; }

    public int? OldDosCategoryId { get; set; }
    public virtual OldDosCategory OldDosCategory { get; set; }

    public DateTime CrawledOn { get; set; }

    [StringLength(2048)]
    public string LastError { get; set; }

    [MaxLength(262144)]
    public string EnglishDescriptionLiteral { get; set; }

    [MaxLength(262144)]
    public string EnglishDescriptionMuseum { get; set; }

    public int MuseumDescriptionPromptVersion { get; set; }

    [MaxLength(4096)]
    public string SuggestedGenreIdsJson { get; set; }

    [StringLength(256)]
    public string ReviewedBy { get; set; }

    public DateTime? ReviewedOn { get; set; }

    public ulong? PromotedSoftwareId { get; set; }

    public virtual ICollection<OldDosVersion> Versions { get; set; }
}
