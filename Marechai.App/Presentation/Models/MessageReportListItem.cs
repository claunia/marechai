#nullable enable

namespace Marechai.App.Presentation.Models;

public sealed class MessageReportListItem
{
    public long Id { get; init; }
    public long? ConversationId { get; init; }
    public string Reporter { get; init; } = string.Empty;
    public string ReasonText { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
    public string CreatedOnText { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public bool IsResolved { get; init; }
    public bool HasConversation { get; init; }
}
