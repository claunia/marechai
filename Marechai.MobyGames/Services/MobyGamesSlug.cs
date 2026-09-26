using System.Collections.Generic;

namespace Marechai.MobyGames.Services;

/// <summary>Slug helpers shared by the cache-reparsing/backfill services.</summary>
public static class MobyGamesSlug
{
    /// <summary>
    ///     Candidate <c>mobygames_raw.id</c> spellings for a slug, in lookup order. Some cached
    ///     rows were stored with a leading hyphen, so a plain slug lookup can miss a page that is
    ///     in fact cached.
    /// </summary>
    public static IEnumerable<string> Variants(string slug)
    {
        if(string.IsNullOrWhiteSpace(slug)) yield break;

        string trimmed = slug.TrimStart('-');
        var seen       = new HashSet<string>();

        foreach(string candidate in new[] { slug, $"-{slug}", trimmed, $"-{trimmed}" })
            if(seen.Add(candidate))
                yield return candidate;
    }
}
