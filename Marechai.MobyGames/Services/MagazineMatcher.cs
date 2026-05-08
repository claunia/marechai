using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Parsers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class MagazineMatcher
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly CountryMatcher                     _countryMatcher;
    readonly MobyGamesHttpClient                _httpClient;
    List<(long Id, string Title)>               _magazines;
    readonly Dictionary<string, long>           _cache = new(StringComparer.OrdinalIgnoreCase);

    public MagazineMatcher(IDbContextFactory<MarechaiContext> contextFactory,
                           CountryMatcher                     countryMatcher = null,
                           MobyGamesHttpClient                httpClient     = null)
    {
        _contextFactory = contextFactory;
        _countryMatcher = countryMatcher;
        _httpClient     = httpClient;
    }

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

    public async Task<(long magazineId, string matchType)> MatchOrCreateAsync(string name, int? sourceId = null)
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

        // No match — create new magazine, optionally enriched from the MobyGames critic page.
        await using var context = await _contextFactory.CreateDbContextAsync();

        var newMagazine = new Magazine
        {
            Title = normalizedName
        };

        if(sourceId.HasValue && _httpClient != null)
        {
            try
            {
                string url        = $"/critic/{sourceId.Value}/";
                string criticHtml = await _httpClient.FetchPageAsync(url);

                if(!string.IsNullOrWhiteSpace(criticHtml))
                {
                    var critic = CriticPageParser.Parse(criticHtml);

                    if(critic != null)
                    {
                        if(critic.FirstPublication.HasValue)
                        {
                            newMagazine.FirstPublication          = critic.FirstPublication;
                            newMagazine.FirstPublicationPrecision = critic.FirstPublicationPrecision;
                        }

                        if(_countryMatcher != null && !string.IsNullOrWhiteSpace(critic.CountryName))
                        {
                            var country = _countryMatcher.Match(critic.CountryName);

                            if(country != null)
                                newMagazine.CountryId = country.Id;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"  \e[33mWarning: failed to fetch critic page for sourceId {sourceId}: {ex.Message}\e[0m");
            }
        }

        context.Magazines.Add(newMagazine);
        await context.SaveChangesAsync();

        _magazines.Add((newMagazine.Id, newMagazine.Title));
        _cache[normalizedName] = newMagazine.Id;

        string extra = "";

        if(newMagazine.CountryId.HasValue || newMagazine.FirstPublication.HasValue)
        {
            var bits = new List<string>();

            if(newMagazine.CountryId.HasValue)
                bits.Add($"country={newMagazine.CountryId.Value}");

            if(newMagazine.FirstPublication.HasValue)
            {
                var d = newMagazine.FirstPublication.Value;

                string label = newMagazine.FirstPublicationPrecision switch
                {
                    DatePrecision.Full      => d.ToString("yyyy-MM-dd"),
                    DatePrecision.MonthYear => d.ToString("yyyy-MM"),
                    _                       => d.Year.ToString()
                };

                bits.Add($"firstPublication={label}");
            }

            extra = $" [{string.Join(", ", bits)}]";
        }

        Console.WriteLine($"  Created new magazine: \"{normalizedName}\" (ID: {newMagazine.Id}){extra}");

        return (newMagazine.Id, "created");
    }
}
