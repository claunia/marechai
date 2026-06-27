#nullable enable

using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Presentation.Models;

public sealed class CriticReviewDisplayItem
{
    public string MagazineTitle { get; set; } = string.Empty;
    public string? PlatformName { get; set; }
    public int? NormalizedScore { get; set; }
    public float? OriginalScore { get; set; }
    public float? OriginalScoreMaximum { get; set; }
    public string? ReviewText { get; set; }
    public string FormattedDate { get; set; } = string.Empty;
    public string? ReviewUrl { get; set; }

    public bool HasPlatformName => !string.IsNullOrWhiteSpace(PlatformName);
    public bool HasReviewText => !string.IsNullOrWhiteSpace(ReviewText);
    public bool HasReviewUrl => !string.IsNullOrWhiteSpace(ReviewUrl);
    public bool HasFormattedDate => !string.IsNullOrWhiteSpace(FormattedDate);
    public bool HasScore => NormalizedScore.HasValue;
    public string ScoreText => HasScore ? $"{NormalizedScore}%" : string.Empty;

    public bool HasOriginalScore => OriginalScore.HasValue && OriginalScoreMaximum.HasValue;
    public string OriginalScoreText => HasOriginalScore ? $"{OriginalScore} / {OriginalScoreMaximum}" : string.Empty;

    public SolidColorBrush ScoreBrush => NormalizedScore switch
    {
        >= 75 => new SolidColorBrush(Colors.SeaGreen),
        >= 50 => new SolidColorBrush(Colors.DarkOrange),
        not null => new SolidColorBrush(Colors.IndianRed),
        _ => new SolidColorBrush(Colors.Gray)
    };
}
