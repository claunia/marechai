using System;

namespace Marechai.Pages.Admin;

public sealed class SoftwareReleaseDialogResult
{
    public int?      VariantId    { get; set; }
    public int?      SubvariantId { get; set; }
    public int?      PlatformId   { get; set; }
    public int?      RegionId     { get; set; }
    public int?      PublisherId  { get; set; }
    public DateTime? ReleaseDate  { get; set; }
}
