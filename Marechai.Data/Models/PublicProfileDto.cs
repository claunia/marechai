using System;
using System.Text.Json.Serialization;

namespace Marechai.Data.Models;

public sealed record PublicProfileDto
{
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = null!;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("useGravatar")]
    public bool UseGravatar { get; set; }

    [JsonPropertyName("twitter")]
    public string? Twitter { get; set; }

    [JsonPropertyName("gitHub")]
    public string? GitHub { get; set; }

    [JsonPropertyName("mastodon")]
    public string? Mastodon { get; set; }

    [JsonPropertyName("facebook")]
    public string? Facebook { get; set; }

    [JsonPropertyName("linkedIn")]
    public string? LinkedIn { get; set; }

    [JsonPropertyName("avatarGuid")]
    public Guid? AvatarGuid { get; set; }

    [JsonPropertyName("originalAvatarExtension")]
    public string? OriginalAvatarExtension { get; set; }

    [JsonPropertyName("isAdmin")]
    public bool IsAdmin { get; set; }

    [JsonPropertyName("isCollaborator")]
    public bool IsCollaborator { get; set; }
}
