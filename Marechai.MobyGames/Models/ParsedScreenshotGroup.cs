using System.Collections.Generic;

namespace Marechai.MobyGames.Models;

public class ParsedScreenshotGroup
{
    public string                      PlatformName    { get; set; }
    public string                      MobyPlatformId  { get; set; }
    public List<ParsedScreenshotImage> Images          { get; set; } = [];
}

public class ParsedScreenshotImage
{
    public string Caption       { get; set; }
    public string ScreenshotId  { get; set; }
    public string DetailPageUrl { get; set; }
    public string ThumbnailUrl  { get; set; }
}
