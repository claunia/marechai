using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public partial class CompanyMatcher
{
    readonly IDbContextFactory<MarechaiContext>          _contextFactory;
    List<Company>                                        _companies;
    Dictionary<string, List<Company>>                    _soundexIndex;
    Dictionary<string, List<Company>>                    _strippedSoundexIndex;
    Dictionary<string, List<Company>>                    _legalNameSoundexIndex;
    Dictionary<string, List<Company>>                    _strippedLegalNameSoundexIndex;
    readonly Dictionary<string, Company>                 _cache = new(StringComparer.OrdinalIgnoreCase);

    // Common company suffixes to strip for matching
    static readonly string[] CompanySuffixes =
    [
        ", Inc.", ", Inc", " Inc.", " Inc",
        ", Ltd.", ", Ltd", " Ltd.", " Ltd",
        ", LLC", " LLC",
        ", S.A.", " S.A.", ", SA", " SA",
        ", S.L.", " S.L.",
        ", GmbH", " GmbH",
        ", AG", " AG",
        ", Co.", " Co.",
        ", Corp.", " Corp.", " Corp",
        " Corporation",
        ", Limited", " Limited",
        " Interactive",
        " Entertainment",
        " Software",
        " Games",
        " Studios",
        " Studio",
        " Productions",
        " Publishing",
        " Digital",
        " Media",
        " Group",
        " Company"
    ];

    public CompanyMatcher(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task LoadAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        _companies = await context.Companies
                                  .Select(c => new Company { Id = c.Id, Name = c.Name, LegalName = c.LegalName })
                                  .ToListAsync();

        _soundexIndex                  = SoundexHelper.BuildSoundexIndex(_companies, c => c.Name);
        _strippedSoundexIndex          = SoundexHelper.BuildSoundexIndex(_companies, c => StripSuffix(c.Name));
        _legalNameSoundexIndex         = SoundexHelper.BuildSoundexIndex(_companies, c => c.LegalName);
        _strippedLegalNameSoundexIndex = SoundexHelper.BuildSoundexIndex(_companies, c => StripSuffix(c.LegalName));
    }

    public async Task<(Company company, string matchType)> MatchOrCreateAsync(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            return (null, "empty");

        string normalizedName = name.Replace("\u00a0", " ").Trim();

        // Check cache first
        if(_cache.TryGetValue(normalizedName, out var cached))
            return (cached, "cached");

        // Exact match
        var exact = _companies.FirstOrDefault(c =>
            string.Equals(c.Name, normalizedName, StringComparison.OrdinalIgnoreCase));

        if(exact != null)
        {
            _cache[normalizedName] = exact;

            return (exact, "exact");
        }

        // Exact match against legal name (e.g., MobyGames "Apple" matches Marechai legal name "Apple Computer, Inc.")
        var legalExact = _companies.FirstOrDefault(c =>
            !string.IsNullOrWhiteSpace(c.LegalName) &&
            string.Equals(c.LegalName, normalizedName, StringComparison.OrdinalIgnoreCase));

        if(legalExact != null)
        {
            _cache[normalizedName] = legalExact;

            return (legalExact, "exact-legal");
        }

        // Exact match on stripped names (e.g., "Apple Inc." matches "Apple")
        string strippedInput = StripSuffix(normalizedName);

        var strippedExact = _companies.FirstOrDefault(c =>
            string.Equals(StripSuffix(c.Name), strippedInput, StringComparison.OrdinalIgnoreCase));

        if(strippedExact != null)
        {
            _cache[normalizedName] = strippedExact;

            return (strippedExact, "exact-stripped");
        }

        // Exact match on stripped legal names
        var strippedLegalExact = _companies.FirstOrDefault(c =>
            !string.IsNullOrWhiteSpace(c.LegalName) &&
            string.Equals(StripSuffix(c.LegalName), strippedInput, StringComparison.OrdinalIgnoreCase));

        if(strippedLegalExact != null)
        {
            _cache[normalizedName] = strippedLegalExact;

            return (strippedLegalExact, "exact-stripped-legal");
        }

        // Soundex match on full name
        string soundex = SoundexHelper.Generate(normalizedName);

        if(_soundexIndex.TryGetValue(soundex, out var candidates) && candidates.Count > 0)
        {
            if(candidates.Count == 1)
            {
                _cache[normalizedName] = candidates[0];

                return (candidates[0], "soundex");
            }

            var prompted = PromptMultiple(normalizedName, candidates);

            if(prompted != null)
            {
                _cache[normalizedName] = prompted;

                return (prompted, "soundex-selected");
            }
        }

        // Soundex match on legal name
        if(_legalNameSoundexIndex.TryGetValue(soundex, out var legalCandidates) && legalCandidates.Count > 0)
        {
            if(legalCandidates.Count == 1)
            {
                _cache[normalizedName] = legalCandidates[0];

                return (legalCandidates[0], "soundex-legal");
            }

            var prompted = PromptMultiple(normalizedName, legalCandidates);

            if(prompted != null)
            {
                _cache[normalizedName] = prompted;

                return (prompted, "soundex-legal-selected");
            }
        }

        // Soundex match on stripped name
        string strippedSoundex = SoundexHelper.Generate(strippedInput);

        if(_strippedSoundexIndex.TryGetValue(strippedSoundex, out var strippedCandidates) &&
           strippedCandidates.Count > 0)
        {
            if(strippedCandidates.Count == 1)
            {
                _cache[normalizedName] = strippedCandidates[0];

                return (strippedCandidates[0], "soundex-stripped");
            }

            var prompted = PromptMultiple(normalizedName, strippedCandidates);

            if(prompted != null)
            {
                _cache[normalizedName] = prompted;

                return (prompted, "soundex-stripped-selected");
            }
        }

        // Soundex match on stripped legal name
        if(_strippedLegalNameSoundexIndex.TryGetValue(strippedSoundex, out var strippedLegalCandidates) &&
           strippedLegalCandidates.Count > 0)
        {
            if(strippedLegalCandidates.Count == 1)
            {
                _cache[normalizedName] = strippedLegalCandidates[0];

                return (strippedLegalCandidates[0], "soundex-stripped-legal");
            }

            var prompted = PromptMultiple(normalizedName, strippedLegalCandidates);

            if(prompted != null)
            {
                _cache[normalizedName] = prompted;

                return (prompted, "soundex-stripped-legal-selected");
            }
        }

        // Create new company
        await using var context = await _contextFactory.CreateDbContextAsync();

        var newCompany = new Company
        {
            Name   = normalizedName,
            Status = Data.CompanyStatus.Unknown
        };

        context.Companies.Add(newCompany);
        await context.SaveChangesAsync();

        _companies.Add(newCompany);

        // Update Soundex index
        if(!_soundexIndex.TryGetValue(soundex, out var list))
        {
            list                  = [];
            _soundexIndex[soundex] = list;
        }

        list.Add(newCompany);
        _cache[normalizedName] = newCompany;

        Console.WriteLine($"  Created new company: \"{normalizedName}\" (ID: {newCompany.Id})");

        return (newCompany, "created");
    }

    static Company PromptMultiple(string normalizedName, List<Company> candidates)
    {
        Console.WriteLine($"\n  Multiple Soundex matches for \"{normalizedName}\":");

        for(int i = 0; i < candidates.Count; i++)
            Console.WriteLine($"    [{i + 1}] {candidates[i].Name} (ID: {candidates[i].Id})");

        Console.WriteLine($"    [0] Create new company");
        Console.Write("  Select: ");

        string input = Console.ReadLine()?.Trim();

        if(int.TryParse(input, out int choice) && choice >= 1 && choice <= candidates.Count)
            return candidates[choice - 1];

        return null; // User chose 0 or invalid — fall through to create
    }

    static string StripSuffix(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            return name;

        string result = name;

        foreach(string suffix in CompanySuffixes)
        {
            if(result.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                result = result[..^suffix.Length].TrimEnd();

                break; // Only strip the first matching suffix
            }
        }

        return result;
    }
}
