using System;
using System.Collections.Generic;

namespace Marechai.Pages.Admin;

public sealed class SoftwareReleaseDialogResult
{
    public string       Title                { get; set; }
    public bool         IsCompilation        { get; set; }
    public int?         SoftwareId           { get; set; }
    public int?         SoftwareVersionId    { get; set; }
    public int?         PlatformId           { get; set; }
    public int?         PublisherId          { get; set; }
    public DateTime?    ReleaseDate          { get; set; }
    public int          ReleaseDatePrecision { get; set; }
    public List<int>    RegionIds            { get; set; } = [];
    public List<string> LanguageCodes        { get; set; } = [];
}
