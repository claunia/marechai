using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class PlatformMatcher
{
    readonly IDbContextFactory<MarechaiContext>          _contextFactory;
    List<SoftwarePlatform>                               _platforms;
    readonly Dictionary<string, SoftwarePlatform>        _cache = new(StringComparer.OrdinalIgnoreCase);

    public PlatformMatcher(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    /// <summary>True once <see cref="LoadAsync" /> has populated the in-memory index.</summary>
    public bool IsLoaded => _platforms is not null;

    public async Task LoadAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        _platforms = await context.SoftwarePlatforms
                                  .Select(p => new SoftwarePlatform { Id = p.Id, Name = p.Name })
                                  .ToListAsync();
    }

    /// <summary>
    ///     Lookup-only counterpart of <see cref="MatchOrCreateAsync" />: cache + exact
    ///     (case-insensitive) name match, never creates. Returns <c>null</c> when unknown.
    /// </summary>
    public SoftwarePlatform TryMatch(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            return null;

        string normalized = name.Replace("\u00a0", " ").Trim();

        if(normalized.Length > 255)
            normalized = normalized[..255];

        if(_cache.TryGetValue(normalized, out var cached))
            return cached;

        return _platforms.FirstOrDefault(p =>
            string.Equals(p.Name, normalized, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<SoftwarePlatform> MatchOrCreateAsync(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            return null;

        string normalized = name.Replace("\u00a0", " ").Trim();

        // DB column is varchar(255) — truncate to avoid DbUpdateException
        if(normalized.Length > 255)
        {
            Console.WriteLine($"\e[33m  Warning: Platform name too long ({normalized.Length} chars), truncating: \"{normalized[..80]}...\"\e[0m");
            normalized = normalized[..255];
        }

        if(_cache.TryGetValue(normalized, out var cached))
            return cached;

        var exact = _platforms.FirstOrDefault(p =>
            string.Equals(p.Name, normalized, StringComparison.OrdinalIgnoreCase));

        if(exact != null)
        {
            _cache[normalized] = exact;

            return exact;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();

        var newPlatform = new SoftwarePlatform { Name = normalized };

        context.SoftwarePlatforms.Add(newPlatform);
        await context.SaveChangesAsync();

        _platforms.Add(newPlatform);
        _cache[normalized] = newPlatform;

        Console.WriteLine($"  Created new platform: \"{normalized}\" (ID: {newPlatform.Id})");

        return newPlatform;
    }
}
