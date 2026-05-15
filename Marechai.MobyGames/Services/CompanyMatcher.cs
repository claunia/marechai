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

    /// <summary>
    ///     When true, <see cref="MatchOrCreateAsync" /> throws
    ///     <see cref="NeedsInteractionException" /> instead of calling
    ///     <see cref="PromptMultiple" /> when more than one Soundex candidate is found and no
    ///     exact match resolves the name. The batch loop's pre-flight uses
    ///     <see cref="WouldPromptForMatch" /> to detect this condition before any DB mutation
    ///     so the throw is only a defensive backstop.
    /// </summary>
    public bool Unattended { get; set; }

    /// <summary>
    ///     When true (in addition to <see cref="Unattended" />), <see cref="MatchOrCreateAsync" />
    ///     falls through to the "create new company" branch on multi-candidate Soundex matches
    ///     instead of throwing. Equivalent to an operator who answers <c>[0] Create new
    ///     company</c> at the <see cref="PromptMultiple" /> prompt.
    /// </summary>
    public bool YesToAll { get; set; }

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

    /// <summary>
    ///     Read-only pre-flight check: returns <c>true</c> if calling
    ///     <see cref="MatchOrCreateAsync" /> with the same name would block on
    ///     <see cref="PromptMultiple" /> (more than one Soundex candidate and no earlier
    ///     exact / cache resolution). Mirrors <see cref="MatchOrCreateAsync" /> exactly up
    ///     to that point and does not modify <see cref="_cache" /> or any Soundex index.
    /// </summary>
    public bool WouldPromptForMatch(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            return false;

        string normalizedName = name.Replace("\u00a0", " ").Trim();

        if(_cache.ContainsKey(normalizedName))
            return false;

        bool exactExists = _companies.Any(c =>
            string.Equals(c.Name, normalizedName, StringComparison.OrdinalIgnoreCase));

        if(exactExists) return false;

        bool legalExactExists = _companies.Any(c =>
            !string.IsNullOrWhiteSpace(c.LegalName) &&
            string.Equals(c.LegalName, normalizedName, StringComparison.OrdinalIgnoreCase));

        if(legalExactExists) return false;

        string strippedInput = StripSuffix(normalizedName);

        bool strippedExactExists = _companies.Any(c =>
            string.Equals(StripSuffix(c.Name), strippedInput, StringComparison.OrdinalIgnoreCase));

        if(strippedExactExists) return false;

        bool strippedLegalExactExists = _companies.Any(c =>
            !string.IsNullOrWhiteSpace(c.LegalName) &&
            string.Equals(StripSuffix(c.LegalName), strippedInput, StringComparison.OrdinalIgnoreCase));

        if(strippedLegalExactExists) return false;

        string soundex         = SoundexHelper.Generate(normalizedName);
        string strippedSoundex = SoundexHelper.Generate(strippedInput);

        var seenIds = new HashSet<int>();
        int count   = 0;

        void Count(IEnumerable<Company> cs)
        {
            foreach(Company c in cs)
                if(seenIds.Add(c.Id))
                    count++;
        }

        if(_soundexIndex.TryGetValue(soundex, out var fullCandidates))
            Count(fullCandidates);

        if(_legalNameSoundexIndex.TryGetValue(soundex, out var legalCandidates))
            Count(legalCandidates);

        if(_strippedSoundexIndex.TryGetValue(strippedSoundex, out var strippedCandidates))
            Count(strippedCandidates);

        if(_strippedLegalNameSoundexIndex.TryGetValue(strippedSoundex, out var strippedLegalCandidates))
            Count(strippedLegalCandidates);

        return count > 1;
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

        // Soundex-based matching: gather candidates from all four indexes (full name, legal name,
        // stripped name, stripped legal name) into a single deduplicated list, then prompt the user
        // once. Previously each index was checked sequentially and a later pass with a SINGLE
        // candidate would silently override the user's "create new" choice from an earlier pass.
        string soundex         = SoundexHelper.Generate(normalizedName);
        string strippedSoundex = SoundexHelper.Generate(strippedInput);

        var soundexCandidates = new List<Company>();
        var seenIds           = new HashSet<int>();

        void AddCandidates(IEnumerable<Company> cs)
        {
            foreach(Company c in cs)
            {
                if(seenIds.Add(c.Id))
                    soundexCandidates.Add(c);
            }
        }

        if(_soundexIndex.TryGetValue(soundex, out var fullCandidates))
            AddCandidates(fullCandidates);

        if(_legalNameSoundexIndex.TryGetValue(soundex, out var legalCandidates))
            AddCandidates(legalCandidates);

        if(_strippedSoundexIndex.TryGetValue(strippedSoundex, out var strippedCandidates))
            AddCandidates(strippedCandidates);

        if(_strippedLegalNameSoundexIndex.TryGetValue(strippedSoundex, out var strippedLegalCandidates))
            AddCandidates(strippedLegalCandidates);

        if(soundexCandidates.Count == 1)
        {
            _cache[normalizedName] = soundexCandidates[0];

            return (soundexCandidates[0], "soundex");
        }

        if(soundexCandidates.Count > 1)
        {
            // Yes-to-all mode: fall through to the "create new company" branch below,
            // mirroring an operator who answers [0] at PromptMultiple.
            if(YesToAll)
            {
                Console.WriteLine(
                    $"  Multiple Soundex matches for \"{normalizedName}\" — auto-creating new company (yes-to-all).");
            }
            else
            {
                // Defensive backstop: in unattended mode the importer's pre-flight should have
                // already skipped the game. Throw rather than block on Console.ReadLine.
                if(Unattended)
                    throw new NeedsInteractionException(
                        $"multiple Soundex matches for company \"{normalizedName}\"");

                Company prompted = PromptMultiple(normalizedName, soundexCandidates);

                if(prompted != null)
                {
                    _cache[normalizedName] = prompted;

                    return (prompted, "soundex-selected");
                }
                // User chose 0 → fall through to create new company.
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
