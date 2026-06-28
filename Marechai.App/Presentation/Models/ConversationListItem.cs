#nullable enable

namespace Marechai.App.Presentation.Models;

public sealed class ConversationListItem
{
    public long Id { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string ParticipantsText { get; init; } = string.Empty;
    public string PreviewText { get; init; } = string.Empty;
    public string TimestampText { get; init; } = string.Empty;
    public int UnreadCount { get; init; }
    public bool IsUnread { get; init; }
    public bool IsSystemThread { get; init; }
}
