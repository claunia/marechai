using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class Software : BaseModel<ulong>
{
    [Required]
    public string Name { get;                                                set; }
    public         ulong?                           FamilyId          { get; set; }
    public virtual SoftwareFamily                   Family            { get; set; }
    public         ulong?                           PredecessorId     { get; set; }
    public virtual Software                         Predecessor       { get; set; }
    public virtual ICollection<Software>            Successors        { get; set; }
    public         SoftwareKind                     Kind              { get; set; }
    public         ulong?                           BaseSoftwareId    { get; set; }
    public virtual Software                         BaseSoftware      { get; set; }
    public virtual ICollection<Software>            Addons            { get; set; }
    public virtual ICollection<SoftwareVersion>     Versions          { get; set; }
    public virtual ICollection<SoftwareCompanyRole> CompanyRoles      { get; set; }
    public virtual ICollection<SoftwareScreenshot>  Screenshots       { get; set; }
    public virtual ICollection<SoftwareRelease>             DirectReleases      { get; set; }
    public virtual ICollection<SoftwareBySoftwareRelease>   CompilationReleases { get; set; }
    public virtual ICollection<SoftwareDescription>         Descriptions        { get; set; }
    public virtual ICollection<GenreBySoftware>             Genres              { get; set; }
    public virtual ICollection<PeopleBySoftware>            Credits             { get; set; }
    public virtual ICollection<SoftwareCriticReview>        CriticReviews       { get; set; }
    public virtual ICollection<SoftwareUserRating>          UserRatings         { get; set; }
    public virtual ICollection<SoftwareUserReview>          UserReviews         { get; set; }
    public virtual ICollection<SoftwarePromoArt>            PromoArt            { get; set; }
    public virtual ICollection<SoftwareVideo>               Videos              { get; set; }
}