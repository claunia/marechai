using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class MagazineMatcher
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    List<(long Id, string Title)>               _magazines;
    readonly Dictionary<string, long>           _cache = new(StringComparer.OrdinalIgnoreCase);

    public MagazineMatcher(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task LoadAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        _magazines = await context.Magazines
                                  .Select(m => new { m.Id, m.Title })
                                  .ToListAsync()
                                  .ContinueWith(t => t.Result
                                      .Select(m => (m.Id, m.Title))
                                      .ToList());
    }

    public async Task<(long magazineId, string matchType)> MatchOrCreateAsync(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            return (0, "empty");

        string normalizedName = name.Replace("\u00a0", " ").Trim();

        // Check cache
        if(_cache.TryGetValue(normalizedName, out long cachedId))
            return (cachedId, "cached");

        // Exact match on Title
        var exact = _magazines.FirstOrDefault(m =>
            string.Equals(m.Title, normalizedName, StringComparison.OrdinalIgnoreCase));

        if(exact != default)
        {
            _cache[normalizedName] = exact.Id;

            return (exact.Id, "exact");
        }

        // No match — create new magazine
        await using var context = await _contextFactory.CreateDbContextAsync();

        var newMagazine = new Magazine
        {
            Title = normalizedName
        };

        context.Magazines.Add(newMagazine);
        await context.SaveChangesAsync();

        _magazines.Add((newMagazine.Id, newMagazine.Title));
        _cache[normalizedName] = newMagazine.Id;

        Console.WriteLine($"  Created new magazine: \"{normalizedName}\" (ID: {newMagazine.Id})");

        return (newMagazine.Id, "created");
    }
}
