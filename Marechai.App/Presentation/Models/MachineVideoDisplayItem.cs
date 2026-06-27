#nullable enable

using System;

namespace Marechai.App.Presentation.Models;

public sealed class MachineVideoDisplayItem
{
    public string Title { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string VideoId { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public Uri? LaunchUri { get; set; }
    public bool HasThumbnail => !string.IsNullOrWhiteSpace(ThumbnailUrl);
    public bool HasTitle => !string.IsNullOrWhiteSpace(Title);
    public string DisplayTitle => HasTitle ? Title : "Video";
}
