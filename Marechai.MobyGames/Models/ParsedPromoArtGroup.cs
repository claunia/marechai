using System.Collections.Generic;

namespace Marechai.MobyGames.Models;

public class ParsedPromoArtGroup
{
    public string                   GroupName { get; set; }
    public string                   GroupId   { get; set; }
    public List<ParsedPromoArtImage> Images   { get; set; } = [];
}

public class ParsedPromoArtImage
{
    public string Caption      { get; set; }
    public string ImageId      { get; set; }
    public string DetailPageUrl { get; set; }
    public string ThumbnailUrl  { get; set; }
}
