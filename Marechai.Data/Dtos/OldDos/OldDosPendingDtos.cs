using System;
using System.Collections.Generic;
using Marechai.Data;

namespace Marechai.Data.Dtos;

public class OldDosPendingListItemDto
{
    public long                 Id                  { get; set; }
    public int                  SourceId            { get; set; }
    public string               SourceUrl           { get; set; }
    public OldDosSoftwareStatus Status              { get; set; }
    public string               Name                { get; set; }
    public string               DeveloperName       { get; set; }
    public string               OsName              { get; set; }
    public string               RussianCategoryPath { get; set; }
    public DateTime             CrawledOn           { get; set; }
    public int                  VersionCount        { get; set; }
    public List<int>            SuggestedGenreIds   { get; set; } = new();
    public ulong?               PromotedSoftwareId  { get; set; }
    public string               LastError           { get; set; }
}

public class OldDosPendingDetailDto
{
    public long                 Id                         { get; set; }
    public int                  SourceId                   { get; set; }
    public string               SourceUrl                  { get; set; }
    public OldDosSoftwareStatus Status                     { get; set; }
    public string               Name                       { get; set; }
    public string               DeveloperName              { get; set; }
    public string               OsName                     { get; set; }
    public string               RussianCategoryPath        { get; set; }
    public string               RussianDescription         { get; set; }
    public string               EnglishDescriptionLiteral  { get; set; }
    public string               EnglishDescriptionMuseum   { get; set; }
    public List<int>            SuggestedGenreIds          { get; set; } = new();
    public List<OldDosVersionDto> Versions                 { get; set; } = new();
}

public class OldDosVersionDto
{
    public long          Id                   { get; set; }
    public string        VersionString        { get; set; }
    public DateTime?     ReleaseDate          { get; set; }
    public DatePrecision ReleaseDatePrecision { get; set; }
    public string        OsHint               { get; set; }
    public string        DownloadUrl          { get; set; }
    public string        FileName             { get; set; }
    public string        Notes                { get; set; }
    public bool          IsEnabledByDefault   { get; set; }
}

public class OldDosNameMatchCandidatesDto
{
    public List<OldDosNameMatchCandidateDto> Matches { get; set; } = new();
}

public class OldDosNameMatchCandidateDto
{
    public ulong               SoftwareId        { get; set; }
    public string              Name              { get; set; }
    public SoftwareKind        Kind              { get; set; }
    public int?                EarliestReleaseYear { get; set; }
    public int                 ReleaseCount      { get; set; }
    public double              JaroWinklerScore  { get; set; }
    public OldDosNameMatchKind MatchKind         { get; set; }
}

public class AcceptOldDosImportDto
{
    public OldDosAcceptMode Mode                    { get; set; }
    public ulong?           TargetSoftwareId        { get; set; }
    public string           NameOverride            { get; set; }
    /// <summary>Underlying value of <see cref="SoftwareKind" />; primitive to keep Kiota from generating a wrapper class.</summary>
    public byte?            KindOverride            { get; set; }
    public string           MuseumDescriptionEdited { get; set; }
    public int              DeveloperCompanyId      { get; set; }
    public List<int>        GenreIds                { get; set; } = new();
    public List<AcceptOldDosVersionDecisionDto> Versions { get; set; } = new();
}

public class AcceptOldDosVersionDecisionDto
{
    public long          OldDosVersionId      { get; set; }
    public bool          Include              { get; set; }
    public string        VersionStringOverride { get; set; }
    public DateTime?     ReleaseDateOverride  { get; set; }
    /// <summary>Underlying value of <see cref="DatePrecision" />; primitive to keep Kiota from generating a wrapper class.</summary>
    public byte?         ReleaseDatePrecisionOverride { get; set; }
    public ulong?        SoftwarePlatformIdOverride { get; set; }
}

public class AcceptOldDosImportResultDto
{
    public bool   Success            { get; set; }
    public string Error              { get; set; }
    public ulong? PromotedSoftwareId { get; set; }
    public int    InsertedVersionCount { get; set; }
    public int    InsertedReleaseCount { get; set; }
    public int    InsertedDescriptionCount { get; set; }
    public int    InsertedGenreCount { get; set; }
}
