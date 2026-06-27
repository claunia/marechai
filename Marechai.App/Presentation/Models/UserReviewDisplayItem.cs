#nullable enable

namespace Marechai.App.Presentation.Models;

public sealed class UserReviewDisplayItem
{
    public long ReviewId { get; set; }
    public string? UserId { get; set; }
    public string? DisplayName { get; set; }
    public string? UserName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsAnonymous { get; set; }
    public float? Rating { get; set; }
    public string? TheGood { get; set; }
    public string? TheBad { get; set; }
    public string? TheUgly { get; set; }
    public int ThumbsUp { get; set; }
    public int ThumbsDown { get; set; }
    public bool? CurrentUserVote { get; set; }
    public bool CanVote { get; set; }
    public string FormattedDate { get; set; } = string.Empty;

    public bool HasAvatar => !IsAnonymous && !string.IsNullOrWhiteSpace(AvatarUrl);
    public bool ShowReviewerName => !IsAnonymous && !string.IsNullOrWhiteSpace(DisplayName ?? UserName);
    public string InitialLetter => !string.IsNullOrWhiteSpace(DisplayName) ? DisplayName[..1].ToUpperInvariant()
                                  : !string.IsNullOrWhiteSpace(UserName) ? UserName[..1].ToUpperInvariant()
                                  : "?";
    public bool HasRating => Rating.HasValue;
    public string RatingText => HasRating ? $"{Rating:F1}/5" : string.Empty;
    public bool HasTheGood => !string.IsNullOrWhiteSpace(TheGood);
    public bool HasTheBad => !string.IsNullOrWhiteSpace(TheBad);
    public bool HasTheUgly => !string.IsNullOrWhiteSpace(TheUgly);
    public bool HasFormattedDate => !string.IsNullOrWhiteSpace(FormattedDate);
    public bool IsUpvoted => CurrentUserVote == true;
    public bool IsDownvoted => CurrentUserVote == false;
    public double UpvoteOpacity => IsUpvoted ? 1d : 0.6d;
    public double DownvoteOpacity => IsDownvoted ? 1d : 0.6d;
}
