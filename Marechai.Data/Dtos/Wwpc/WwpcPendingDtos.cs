using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Data.Dtos;

public class WwpcPendingListItemDto
{
    public long               Id                       { get; set; }
    public string             SourceUrl                { get; set; }
    public string             Slug                     { get; set; }
    public WwpcSoftwareStatus Status                   { get; set; }
    public WwpcProductType    ProductType              { get; set; }
    public string             Name                     { get; set; }
    public string             VendorName               { get; set; }
    public string             RawCategoriesCsv         { get; set; }
    public string             PlatformsCsv             { get; set; }
    public DateTime           CrawledOn                { get; set; }
    public int                VersionCount             { get; set; }
    public int                ScreenshotCount          { get; set; }
    public int?               SuggestedVendorCompanyId { get; set; }
    public List<int>          SuggestedGenreIds        { get; set; } = new();
    public ulong?             PromotedSoftwareId       { get; set; }
    public string             LastError                { get; set; }
}

public class WwpcPendingDetailDto
{
    public long                       Id                             { get; set; }
    public string                     SourceUrl                      { get; set; }
    public string                     Slug                           { get; set; }
    public WwpcSoftwareStatus         Status                         { get; set; }
    public WwpcProductType            ProductType                    { get; set; }
    public string                     Name                           { get; set; }
    public string                     VendorName                     { get; set; }
    public string                     VendorUrl                      { get; set; }
    public string                     RawCategoriesCsv               { get; set; }
    public string                     PlatformsCsv                   { get; set; }
    public string                     ReleaseDateText                { get; set; }
    public string                     UserInterface                  { get; set; }
    public string                     RawDescription                 { get; set; }
    public string                     EnglishDescriptionMuseum       { get; set; }
    public int                        MuseumDescriptionPromptVersion { get; set; }
    public int?                       SuggestedVendorCompanyId       { get; set; }
    public List<int>                  SuggestedGenreIds              { get; set; } = new();
    public DateTime                   CrawledOn                      { get; set; }
    public ulong?                     PromotedSoftwareId             { get; set; }
    public List<WwpcVersionDto>       Versions                       { get; set; } = new();
    public List<WwpcScreenshotDto>    Screenshots                    { get; set; } = new();
}

public class WwpcVersionDto
{
    public long   Id                        { get; set; }
    public string MajorRelease              { get; set; }
    public string MajorReleaseUrl           { get; set; }
    public string VersionString             { get; set; }
    public string Language                  { get; set; }
    public string Architecture              { get; set; }
    public string MediaKind                 { get; set; }
    public string SizeText                  { get; set; }
    public string DownloadUrl               { get; set; }
    public bool   IsEnabledByDefault        { get; set; }
    public ulong? PromotedSoftwareVersionId { get; set; }
}

public class WwpcScreenshotDto
{
    public long   Id                           { get; set; }
    public string MajorRelease                 { get; set; }
    public string SourceUrl                    { get; set; }
    public string ImageUrl                     { get; set; }
    public string Caption                      { get; set; }
    public ulong? SuggestedSoftwarePlatformId  { get; set; }
    public bool   IsEnabledByDefault           { get; set; }
    public Guid?  PromotedSoftwareScreenshotId { get; set; }
}

public class WwpcNameMatchCandidatesDto
{
    public List<WwpcNameMatchCandidateDto> Matches { get; set; } = new();
}

public class WwpcNameMatchCandidateDto
{
    public ulong             SoftwareId          { get; set; }
    public string            Name                { get; set; }
    /// <summary>Underlying value of <see cref="SoftwareKind" />; primitive to keep Kiota from generating a wrapper class.</summary>
    public byte              Kind                { get; set; }
    public int?              EarliestReleaseYear { get; set; }
    public int               ReleaseCount        { get; set; }
    public double            JaroWinklerScore    { get; set; }
    public WwpcNameMatchKind MatchKind           { get; set; }
}

public class WwpcCompanyMatchCandidatesDto
{
    public List<WwpcCompanyMatchCandidateDto> Matches { get; set; } = new();
}

public class WwpcCompanyMatchCandidateDto
{
    public int                  CompanyId        { get; set; }
    public string               Name             { get; set; }
    public double               JaroWinklerScore { get; set; }
    public WwpcCompanyMatchKind MatchKind        { get; set; }
}

public class AcceptWwpcImportDto
{
    public WwpcAcceptMode Mode                    { get; set; }
    public ulong?         TargetSoftwareId        { get; set; }
    public string         NameOverride            { get; set; }
    /// <summary>Underlying value of <see cref="SoftwareKind" />; primitive to keep Kiota from generating a wrapper class.</summary>
    public byte?          KindOverride            { get; set; }
    public string         MuseumDescriptionEdited { get; set; }
    public int?           VendorCompanyId         { get; set; }
    public List<int>      GenreIds                { get; set; } = new();
    public List<AcceptWwpcVersionDecisionDto>    Versions    { get; set; } = new();
    public List<AcceptWwpcScreenshotDecisionDto> Screenshots { get; set; } = new();
}

public class AcceptWwpcVersionDecisionDto
{
    public long   WwpcVersionId             { get; set; }
    public bool   Include                   { get; set; }
    public string VersionStringOverride     { get; set; }
    /// <summary>When set in <c>MergeIntoExisting</c> mode, link this Wwpc row to the existing <c>SoftwareVersion</c> instead of creating a new one.</summary>
    public ulong? LinkToExistingVersionId   { get; set; }
}

public class AcceptWwpcScreenshotDecisionDto
{
    public long   WwpcScreenshotId        { get; set; }
    public bool   Include                 { get; set; }
    public string CaptionOverride         { get; set; }
    public ulong? SoftwarePlatformId      { get; set; }
    /// <summary>Optional: link the new screenshot to a specific <c>SoftwareVersion</c> (typically the one a matching <see cref="AcceptWwpcVersionDecisionDto" /> resolved to).</summary>
    public ulong? SoftwareVersionId       { get; set; }
}

public class AcceptWwpcImportResultDto
{
    public bool   Success                  { get; set; }
    public string Error                    { get; set; }
    public ulong? PromotedSoftwareId       { get; set; }
    public int    InsertedVersionCount     { get; set; }
    public int    InsertedScreenshotCount  { get; set; }
    public int    InsertedDescriptionCount { get; set; }
    public int    InsertedGenreCount       { get; set; }
}
