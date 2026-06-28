#nullable enable

using System;

namespace Marechai.App.Presentation.Models;

public partial class AdminGpuVideoItem : ObservableObject
{
    [ObservableProperty]
    private long _id;

    [ObservableProperty]
    private int _gpuId;

    [ObservableProperty]
    private string _gpuName = string.Empty;

    [ObservableProperty]
    private string _provider = string.Empty;

    [ObservableProperty]
    private string _videoId = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    public string? ThumbnailUrl =>
        string.Equals(Provider, "YouTube", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(VideoId)
            ? $"https://img.youtube.com/vi/{VideoId}/hqdefault.jpg"
            : null;

    public Uri? LaunchUri
    {
        get
        {
            if(string.IsNullOrWhiteSpace(VideoId)) return null;

            if(string.Equals(Provider, "YouTube", StringComparison.OrdinalIgnoreCase))
                return new Uri($"https://www.youtube.com/watch?v={Uri.EscapeDataString(VideoId)}");

            if(Uri.TryCreate(VideoId, UriKind.Absolute, out Uri? uri)) return uri;

            return null;
        }
    }

    public bool HasThumbnail => !string.IsNullOrWhiteSpace(ThumbnailUrl);
    public bool CanOpenVideo => LaunchUri is not null;
    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? "Video" : Title;
}
