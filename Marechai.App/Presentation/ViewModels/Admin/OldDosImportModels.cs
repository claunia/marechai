#nullable enable

using System;
using Marechai.ApiClient.Models;
using Marechai.Data;

namespace Marechai.App.Presentation.ViewModels.Admin;

public sealed partial class OldDosQueueItemViewModel : ObservableObject
{
    public OldDosPendingListItemDto Item { get; init; } = null!;

    public long Id => Item.Id ?? 0;
    public string Name => Item.Name ?? string.Empty;
    public string DeveloperName => string.IsNullOrWhiteSpace(Item.DeveloperName) ? "—" : Item.DeveloperName;
    public string OsName => string.IsNullOrWhiteSpace(Item.OsName) ? "—" : Item.OsName;
    public string RussianCategoryPath => Item.RussianCategoryPath ?? string.Empty;
    public string LastError => Item.LastError ?? string.Empty;
    public int VersionCount => Item.VersionCount ?? 0;
    public DateTimeOffset CrawledOn => Item.CrawledOn ?? DateTimeOffset.MinValue;
    public string SourceUrl => Item.SourceUrl ?? string.Empty;
    public string StatusLabel { get; init; } = string.Empty;
    public OldDosSoftwareStatus Status => (OldDosSoftwareStatus)(Item.Status ?? 0);
    public bool CanReview => Status is OldDosSoftwareStatus.ReadyForReview or OldDosSoftwareStatus.Skipped;
    public bool CanReject => Status is not OldDosSoftwareStatus.Discarded and not OldDosSoftwareStatus.Accepted;
}

public sealed class OldDosEnumOption
{
    public string Label { get; init; } = string.Empty;
    public int? Value { get; init; }
}

public sealed class OldDosKindOption
{
    public string Label { get; init; } = string.Empty;
    public int Value { get; init; }
}

public sealed partial class OldDosNameMatchCandidateItem : ObservableObject
{
    public OldDosNameMatchCandidateDto Candidate { get; init; } = null!;
    public string Name => Candidate.Name ?? string.Empty;
    public int SoftwareId => Candidate.SoftwareId ?? 0;
    public string KindLabel { get; init; } = string.Empty;
    public string YearLabel => Candidate.EarliestReleaseYear?.ToString() ?? "—";
    public string ScoreLabel => Candidate.JaroWinklerScore?.ToString("0.000") ?? string.Empty;
    public int ReleaseCount => Candidate.ReleaseCount ?? 0;
    public bool IsExactMatch => Candidate.MatchKind == (int)OldDosNameMatchKind.ExactNormalized;
}

public sealed partial class OldDosVersionDecisionItem : ObservableObject
{
    public long Id { get; init; }
    public string OriginalVersionString { get; init; } = string.Empty;
    public string OsHint { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public int ReleaseDatePrecision { get; init; }

    [ObservableProperty] private bool _include;
    [ObservableProperty] private string _versionStringOverride = string.Empty;
    [ObservableProperty] private DateTimeOffset? _releaseDateOverride;
    [ObservableProperty] private OldDosEnumOption? _releasePrecisionOption;
    [ObservableProperty] private SoftwarePlatformDto? _platform;

    public string DisplayLabel => string.IsNullOrWhiteSpace(VersionStringOverride) ? OriginalVersionString : VersionStringOverride;
}
