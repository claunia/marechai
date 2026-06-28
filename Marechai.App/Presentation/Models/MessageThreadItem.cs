#nullable enable

namespace Marechai.App.Presentation.Models;

public sealed class MessageThreadItem
{
    public long Id { get; init; }
    public string SenderDisplayName { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string TimestampText { get; init; } = string.Empty;
    public bool IsMine { get; init; }
    public bool IsSystemAuthored { get; init; }
    public bool IsRead { get; init; }
    public bool CanReport { get; init; }
}
