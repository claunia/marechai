#nullable enable

namespace Marechai.App.Presentation.Models;

public sealed class ReviewReportListItem
{
    public long Id { get; init; }
    public string ReporterName { get; init; } = string.Empty;
    public string SoftwareName { get; init; } = string.Empty;
    public string ReviewerName { get; init; } = string.Empty;
    public string ReasonText { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public string CreatedOnText { get; init; } = string.Empty;
    public bool IsResolved { get; init; }
}
