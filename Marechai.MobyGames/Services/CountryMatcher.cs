using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data.Helpers;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class CountryMatcher
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    List<UnM49>                                 _countries;
    readonly Dictionary<string, UnM49>          _cache = new(StringComparer.OrdinalIgnoreCase);

    static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["United States"]         = "United States of America",
        ["Russia"]                = "Russian Federation",
        ["South Korea"]           = "Republic of Korea",
        ["North Korea"]           = "Democratic People's Republic of Korea",
        ["The Netherlands"]       = "Netherlands",
        ["Czechia"]               = "Czechia",
        ["Taiwan"]                = "Taiwan",
        ["Hong Kong"]             = "China, Hong Kong Special Administrative Region",
        ["Macau"]                 = "China, Macao Special Administrative Region",
        ["Vietnam"]               = "Viet Nam",
        ["Brunei"]                = "Brunei Darussalam",
        ["Bolivia"]               = "Bolivia (Plurinational State of)",
        ["Iran"]                  = "Iran (Islamic Republic of)",
        ["Syria"]                 = "Syrian Arab Republic",
        ["Venezuela"]             = "Venezuela (Bolivarian Republic of)",
        ["Tanzania"]              = "United Republic of Tanzania",
        ["Moldova"]               = "Republic of Moldova",
        ["Ivory Coast"]           = "Côte d'Ivoire",
        ["Laos"]                  = "Lao People's Democratic Republic",
        ["North Macedonia"]       = "North Macedonia"
    };

    public CountryMatcher(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task LoadAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        _countries = await context.UnM49
                                  .Where(u => u.Type == Data.UnM49Type.Country)
                                  .Select(u => new UnM49 { Id = u.Id, Name = u.Name })
                                  .ToListAsync();
    }

    public UnM49 Match(string countryName)
    {
        if(string.IsNullOrWhiteSpace(countryName))
            return null;

        string normalized = countryName.Replace("\u00a0", " ").Trim();

        // Strip continent prefix like "Africa » South Africa"
        int arrowIndex = normalized.IndexOf('»');

        if(arrowIndex >= 0)
            normalized = normalized[(arrowIndex + 1)..].Trim();

        if(_cache.TryGetValue(normalized, out var cached))
            return cached;

        // Try alias
        if(Aliases.TryGetValue(normalized, out string aliased))
            normalized = aliased;

        // Exact match
        var exact = _countries.FirstOrDefault(c =>
            string.Equals(c.Name, normalized, StringComparison.OrdinalIgnoreCase));

        if(exact != null)
        {
            _cache[countryName] = exact;

            return exact;
        }

        // Soundex match
        string soundex = SoundexHelper.Generate(normalized);
        var    matches  = _countries.Where(c => SoundexHelper.Generate(c.Name) == soundex).ToList();

        if(matches.Count == 1)
        {
            _cache[countryName] = matches[0];

            return matches[0];
        }

        return null;
    }
}
