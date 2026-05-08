using System;

namespace Marechai.MobyGames.Models;

public class ParsedCriticReview
{
    public string PublicationName { get; set; }
    public int?   PublicationSourceId { get; set; }
    public string PlatformName   { get; set; }
    public int?   NormalizedScore { get; set; }
    public float? OriginalScore   { get; set; }
    public float? OriginalScoreMaximum { get; set; }
    public string ReviewText { get; set; }
    public string ReviewDate { get; set; }
    public string ReviewUrl  { get; set; }
}
