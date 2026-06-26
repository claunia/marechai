#nullable enable

using System;
using System.Collections.ObjectModel;
using Marechai.ApiClient.Models;
using Marechai.Data;

namespace Marechai.App.Presentation.ViewModels.Admin;

public sealed partial class WwpcQueueItemViewModel : ObservableObject
{
    public WwpcPendingListItemDto Item { get; init; } = null!;

    public long Id => Item.Id ?? 0;
    public string Name => Item.Name ?? string.Empty;
    public string VendorName => string.IsNullOrWhiteSpace(Item.VendorName) ? "—" : Item.VendorName;
    public string RawCategoriesCsv => Item.RawCategoriesCsv ?? string.Empty;
    public string PlatformsCsv => Item.PlatformsCsv ?? string.Empty;
    public string LastError => Item.LastError ?? string.Empty;
    public int VersionCount => Item.VersionCount ?? 0;
    public int ScreenshotCount => Item.ScreenshotCount ?? 0;
    public DateTimeOffset CrawledOn => Item.CrawledOn ?? DateTimeOffset.MinValue;
    public string SourceUrl => Item.SourceUrl ?? string.Empty;
    public WwpcSoftwareStatus Status => (WwpcSoftwareStatus)(Item.Status ?? 0);
    public WwpcProductType ProductType => (WwpcProductType)(Item.ProductType ?? 0);
    public string StatusLabel => Status.ToString();
    public string ProductTypeLabel => ProductType.ToString();
    public bool CanReview => Status is WwpcSoftwareStatus.ReadyForReview or WwpcSoftwareStatus.Skipped;
    public bool CanReject => Status is not WwpcSoftwareStatus.Discarded and not WwpcSoftwareStatus.Accepted;
}

public sealed class WwpcEnumOption
{
    public string Label { get; init; } = string.Empty;
    public int? Value { get; init; }
}

public sealed partial class WwpcNameMatchCandidateItem : ObservableObject
{
    public WwpcNameMatchCandidateDto Candidate { get; init; } = null!;
    public string Name => Candidate.Name ?? string.Empty;
    public ulong SoftwareId => (ulong)(Candidate.SoftwareId ?? 0);
    public string KindLabel { get; init; } = string.Empty;
    public string YearLabel => Candidate.EarliestReleaseYear?.ToString() ?? "—";
    public string ScoreLabel => Candidate.JaroWinklerScore?.ToString("0.000") ?? string.Empty;
}

public sealed partial class WwpcCompanyMatchCandidateItem : ObservableObject
{
    public WwpcCompanyMatchCandidateDto Candidate { get; init; } = null!;
    public string Label => $"{Candidate.Name} ({Candidate.JaroWinklerScore?.ToString("0.00") ?? "0.00"})";
}

public sealed partial class WwpcVersionDecisionItem : ObservableObject
{
    public long Id { get; init; }
    public string MajorRelease { get; init; } = string.Empty;
    public string OriginalVersionString { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string Architecture { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string SizeText { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;

    [ObservableProperty] private bool _include;
    [ObservableProperty] private string _versionStringOverride = string.Empty;
    [ObservableProperty] private SoftwareVersionDto? _linkedExistingVersion;

    public string DisplayLabel => string.IsNullOrWhiteSpace(VersionStringOverride)
        ? OriginalVersionString
        : VersionStringOverride;
}

public sealed partial class WwpcScreenshotDecisionItem : ObservableObject
{
    public long Id { get; init; }
    public string MajorRelease { get; init; } = string.Empty;
    public string SourceUrl { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string OriginalCaption { get; init; } = string.Empty;

    [ObservableProperty] private bool _include;
    [ObservableProperty] private string _captionOverride = string.Empty;
    [ObservableProperty] private SoftwarePlatformDto? _platform;
    [ObservableProperty] private WwpcVersionDecisionItem? _linkedVersion;
}

public sealed partial class WwpcGenreChipItem : ObservableObject
{
    public SoftwareGenreDto Genre { get; init; } = null!;
    public int Id => Genre.Id ?? 0;
    public string Name => Genre.Name ?? string.Empty;
}

public sealed class WwpcOperationState
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
}
