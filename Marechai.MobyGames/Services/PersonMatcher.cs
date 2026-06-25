using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data.Helpers;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class PersonMatcher
{
    readonly IDbContextFactory<MarechaiContext>     _contextFactory;
    List<Person>                                    _people;
    Dictionary<string, List<Person>>                _soundexIndex;
    readonly Dictionary<string, Person>             _cache = new(StringComparer.OrdinalIgnoreCase);

    public PersonMatcher(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task LoadAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        _people = await context.People
                               .Select(p => new Person
                               {
                                   Id          = p.Id,
                                   Name        = p.Name,
                                   Surname     = p.Surname,
                                   DisplayName = p.DisplayName,
                                   Alias       = p.Alias
                               })
                               .ToListAsync();

        _soundexIndex = SoundexHelper.BuildSoundexIndex(_people, p => p.FullName);
    }

    public async Task<Person> MatchOrCreateAsync(string fullName)
    {
        if(string.IsNullOrWhiteSpace(fullName))
            return null;

        string normalized = fullName.Replace("\u00a0", " ").Trim();

        if(_cache.TryGetValue(normalized, out var cached))
            return cached;

        // Exact match on FullName, DisplayName, or Alias
        var exact = _people.FirstOrDefault(p =>
            string.Equals(p.FullName, normalized, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p.DisplayName, normalized, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p.Alias, normalized, StringComparison.OrdinalIgnoreCase));

        if(exact != null)
        {
            _cache[normalized] = exact;

            return exact;
        }

        // Soundex match
        string soundex = SoundexHelper.Generate(normalized);

        if(_soundexIndex.TryGetValue(soundex, out var candidates) && candidates.Count == 1)
        {
            _cache[normalized] = candidates[0];

            return candidates[0];
        }

        // Split into name/surname for creation
        string name, surname;
        int    lastSpace = normalized.LastIndexOf(' ');

        if(lastSpace > 0)
        {
            name    = normalized[..lastSpace].Trim();
            surname = normalized[(lastSpace + 1)..].Trim();
        }
        else
        {
            name    = normalized;
            surname = "";
        }

        await using var context = await _contextFactory.CreateDbContextAsync();

        var newPerson = new Person
        {
            Name    = name,
            Surname = surname
        };

        context.People.Add(newPerson);
        await context.SaveChangesAsync();

        _people.Add(newPerson);

        if(!_soundexIndex.TryGetValue(soundex, out var list))
        {
            list                   = [];
            _soundexIndex[soundex] = list;
        }

        list.Add(newPerson);
        _cache[normalized] = newPerson;

        return newPerson;
    }
}
