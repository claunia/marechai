#nullable enable

namespace Marechai.App.Presentation.Models;

public sealed class SuggestionListItem
{
    public long Id { get; init; }
    public string EntityTypeText { get; init; } = string.Empty;
    public string EntityDisplayName { get; init; } = string.Empty;
    public long? EntityId { get; init; }
    public bool IsNew { get; init; }
    public string SubkeyText { get; init; } = string.Empty;
    public string CreatedByText { get; init; } = string.Empty;
    public string CreatedOnText { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public bool IsPending { get; init; }
}

public sealed class SuggestionFieldDiffItem
{
    public string FieldName { get; init; } = string.Empty;
    public string CurrentLabel { get; set; } = string.Empty;
    public string SuggestedLabel { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
}
