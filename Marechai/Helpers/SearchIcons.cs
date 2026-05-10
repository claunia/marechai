/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using Marechai.ApiClient.Models;
using Marechai.Data;
using MudBlazor;

namespace Marechai.Helpers;

/// <summary>Maps a <see cref="SearchEntityType"/> to a representative MudBlazor icon string.</summary>
public static class SearchIcons
{
    public static string For(SearchEntityType type) => type switch
    {
        SearchEntityType.Company             => Icons.Material.Filled.Business,
        SearchEntityType.Computer            => Icons.Material.Filled.Computer,
        SearchEntityType.Console             => Icons.Material.Filled.SportsEsports,
        SearchEntityType.Smartphone          => Icons.Material.Filled.Smartphone,
        SearchEntityType.Book                => Icons.Material.Filled.MenuBook,
        SearchEntityType.Document            => Icons.Material.Filled.Description,
        SearchEntityType.Magazine            => Icons.Material.Filled.Newspaper,
        SearchEntityType.Gpu                 => Icons.Material.Filled.Memory,
        SearchEntityType.Processor           => Icons.Material.Filled.DeveloperBoard,
        SearchEntityType.SoundSynth          => Icons.Material.Filled.MusicNote,
        SearchEntityType.Person              => Icons.Material.Filled.Person,
        SearchEntityType.Software            => Icons.Material.Filled.Apps,
        SearchEntityType.SoftwareCompilation => Icons.Material.Filled.Folder,
        _                                    => Icons.Material.Filled.HelpOutline
    };
}

/// <summary>Builds the public-page URL for a search result, mapping each entity type to its View page.</summary>
public static class SearchUrlBuilder
{
    public static string For(SearchResultDto item)
    {
        SearchEntityType type = (SearchEntityType)item.EntityType.GetValueOrDefault();
        long             id   = item.EntityId.GetValueOrDefault();

        return type switch
        {
            SearchEntityType.Company             => $"/company/{id}",
            SearchEntityType.Computer            => $"/machine/{id}",
            SearchEntityType.Console             => $"/machine/{id}",
            SearchEntityType.Smartphone          => $"/machine/{id}",
            SearchEntityType.Book                => $"/book/{id}",
            SearchEntityType.Document            => $"/document/{id}",
            SearchEntityType.Magazine            => $"/magazine/{id}",
            SearchEntityType.Gpu                 => $"/gpu/{id}",
            SearchEntityType.Processor           => $"/processor/{id}",
            SearchEntityType.SoundSynth          => $"/soundsynth/{id}",
            SearchEntityType.Person              => $"/person/{id}",
            SearchEntityType.Software            => $"/software/{id}",
            SearchEntityType.SoftwareCompilation => $"/software/release/{id}",
            _                                    => "/"
        };
    }
}
