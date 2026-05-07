using System;
using Marechai.Data;

namespace Marechai.App.Models;

public class SoftwareListItem
{
    public int          Id            { get; set; }
    public string       Name          { get; set; } = string.Empty;
    public string       Family        { get; set; }
    public int?         Year          { get; set; }
    public SoftwareKind Kind          { get; set; }
    public bool         IsCompilation { get; set; }
    public Guid?        FrontCoverId  { get; set; }
    public string       CoverImageUrl { get; set; }
    public bool         HasCover      => FrontCoverId.HasValue;
}
