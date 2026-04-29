using System.Collections.Generic;

namespace Marechai.MobyGames.Models;

public class ParsedCoverGroup
{
    public string             Platform      { get; set; }
    public List<string>       Countries     { get; set; } = [];
    public string             Packaging     { get; set; }
    public string             VideoStandard { get; set; }
    public string             GroupUrl       { get; set; }
    public string             GroupId       { get; set; }
    public List<ParsedCoverImage> Covers    { get; set; } = [];
}

public class ParsedCoverImage
{
    public string Type          { get; set; } // "Front Cover", "Back Cover", etc.
    public string DetailPageUrl { get; set; }
    public string CoverId       { get; set; }
    public string ThumbnailUrl  { get; set; } // /images/covers/s/{id}-{slug}.{ext}
}
