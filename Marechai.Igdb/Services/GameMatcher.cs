using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Helpers;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Igdb.Services;

public class GameMatcher
{
    const double FullSweepAcceptThreshold = 0.90;
    const double FullSweepReviewThreshold = 0.85;

    /// <summary>
    ///     Slack allowed when comparing IGDB's single first-release year against a local release year: ports,
    ///     regional releases and re-issues legitimately slip a year either way.
    /// </summary>
    const int YearTolerance = 1;

    static readonly HashSet<string> StandaloneGameTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "main_game", "dlc_addon", "expansion", "standalone_expansion", "mod", "episode", "season"
    };

    static readonly Dictionary<char, int> RomanDigits = new()
    {
        ['I'] = 1, ['V'] = 5, ['X'] = 10, ['L'] = 50, ['C'] = 100, ['D'] = 500, ['M'] = 1000
    };

    /// <summary>Verdict of comparing IGDB's first release year against a local Software's release years.</summary>
    enum YearVerdict
    {
        /// <summary>Either side has no year, so the signal must not be used in any direction.</summary>
        Unknown,
        Agree,
        Disagree
    }

    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public GameMatcher(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<(int matched, int needsReview, int noMatch)> RunAsync(bool dryRun)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        List<Software> softwareList = await context.Softwares.Select(s => new Software
        {
            Id   = s.Id,
            Name = s.Name
        }).ToListAsync();

        // Local alternative titles, grouped by Software, so each side of the match can be
        // widened to "canonical name + every alternative title" instead of just the canonical
        // name. Mirrors IgdbAlternativeNames below.
        Dictionary<ulong, List<string>> alternativeTitlesBySoftwareId =
            (await context.SoftwareAlternativeTitles.Select(t => new { t.SoftwareId, t.Title }).ToListAsync())
           .GroupBy(t => t.SoftwareId)
           .ToDictionary(g => g.Key, g => g.Select(t => t.Title).ToList());

        // IGDB's own alternative names (mirrored from /alternative_names), grouped by the
        // referenced game's IGDB id.
        Dictionary<long, List<string>> igdbAlternativeNamesByGameId =
            (await context.IgdbAlternativeNames.Select(a => new { a.GameIgdbId, a.Name }).ToListAsync())
           .GroupBy(a => a.GameIgdbId)
           .ToDictionary(g => g.Key, g => g.Select(a => a.Name).ToList());

        // Precompute the full name-variant set (canonical + alternatives, deduplicated) for
        // every local Software row once, instead of recomputing it per IGDB game.
        Dictionary<ulong, List<string>> softwareNameVariantsById = softwareList.ToDictionary(s => s.Id,
            s => BuildNameVariants(s.Name,
                alternativeTitlesBySoftwareId.TryGetValue(s.Id, out List<string> alts) ? alts : null));

        // Series ordinals per Software, precomputed alongside the name variants for the same reason.
        Dictionary<ulong, HashSet<int>> softwareOrdinalsById =
            softwareNameVariantsById.ToDictionary(kv => kv.Key, kv => SeriesOrdinals(kv.Value));

        Dictionary<ulong, HashSet<ulong>> platformsBySoftwareId =
            (await context.SoftwareReleases.Where(r => r.SoftwareId != null && r.PlatformId != null)
                           .Select(r => new { SoftwareId = r.SoftwareId.Value, PlatformId = r.PlatformId.Value })
                           .ToListAsync())
           .GroupBy(r => r.SoftwareId)
           .ToDictionary(g => g.Key, g => g.Select(r => r.PlatformId).ToHashSet());

        // Release years per Software, the signal that separates same-series titles whose names alone score above
        // the auto-accept threshold. Only the year is compared, so ReleaseDatePrecision is irrelevant here.
        Dictionary<ulong, HashSet<int>> releaseYearsBySoftwareId =
            (await context.SoftwareReleases.Where(r => r.SoftwareId != null && r.ReleaseDate != null)
                           .Select(r => new { SoftwareId = r.SoftwareId.Value, Year = r.ReleaseDate.Value.Year })
                           .ToListAsync())
           .GroupBy(r => r.SoftwareId)
           .ToDictionary(g => g.Key, g => g.Select(r => r.Year).ToHashSet());

        // Developer/publisher corroboration. IgdbInvolvedCompanies is already mirrored and IgdbCompanies already
        // carries a local CompanyId from match-companies, so this needs no extra mirroring — it is simply a signal
        // GameMatcher has never consulted (only GameEnricherService did).
        Dictionary<long, int> localCompanyByIgdbCompanyId =
            await context.IgdbCompanies.Where(c => c.CompanyId != null)
                          .ToDictionaryAsync(c => c.IgdbId, c => c.CompanyId.Value);

        Dictionary<long, HashSet<int>> localCompanyIdsByIgdbGameId =
            (await context.IgdbInvolvedCompanies.Where(i => i.Developer || i.Publisher)
                           .Select(i => new { i.GameIgdbId, i.CompanyIgdbId })
                           .ToListAsync())
           .GroupBy(i => i.GameIgdbId)
           .ToDictionary(g => g.Key,
                         g => g.Select(i => localCompanyByIgdbCompanyId.TryGetValue(i.CompanyIgdbId, out int id)
                                                 ? id
                                                 : (int?)null)
                               .Where(id => id.HasValue)
                               .Select(id => id.Value)
                               .ToHashSet());

        Dictionary<ulong, HashSet<int>> companyIdsBySoftwareId =
            (await context.SoftwareCompanyRoles.Select(r => new { r.SoftwareId, r.CompanyId }).ToListAsync())
           .GroupBy(r => r.SoftwareId)
           .ToDictionary(g => g.Key, g => g.Select(r => r.CompanyId).ToHashSet());

        Dictionary<int, ulong> softwarePlatformByIgdbPlatformId =
            await context.IgdbPlatforms.Where(p => p.SoftwarePlatformId != null)
                          .ToDictionaryAsync(p => p.Id, p => p.SoftwarePlatformId.Value);

        Dictionary<int, string> gameTypeById = await context.IgdbGameTypes.ToDictionaryAsync(t => t.Id, t => t.Type);

        List<IgdbGame> allGames = await context.IgdbGames.ToListAsync();

        Dictionary<long, ulong> resolvedSoftwareIdByIgdbId = allGames
                                                             .Where(g => g.MatchStatus == IgdbMatchStatus.Matched &&
                                                                         g.SoftwareId.HasValue)
                                                             .ToDictionary(g => g.IgdbId, g => g.SoftwareId.Value);

        List<IgdbGame> pending = allGames.Where(g => g.MatchStatus == IgdbMatchStatus.Pending).ToList();

        // Process parentless / standalone games first so dependents can inherit within the same run.
        List<IgdbGame> ordered = pending
                                 .OrderBy(g => g.ParentGameId.HasValue || g.VersionParentId.HasValue ? 1 : 0)
                                 .ToList();

        long? igdbSiteId = dryRun
                                ? null
                                : (await context.ExternalSites.FirstOrDefaultAsync(s => s.Name == "IGDB"))?.Id;

        HashSet<(long siteId, string externalId)> existingExternalIds = dryRun || igdbSiteId is null
                                                                              ? []
                                                                              : (await context.SoftwareExternalIds
                                                                                  .Where(e => e.ExternalSiteId ==
                                                                                      igdbSiteId.Value)
                                                                                  .Select(e => e.ExternalId)
                                                                                  .ToListAsync()).Select(
                                                                                  id => (igdbSiteId.Value, id))
                                                                             .ToHashSet();

        int matched     = 0;
        int needsReview = 0;
        int noMatch     = 0;

        foreach(IgdbGame igdbGame in ordered)
        {
            (ulong? softwareId, string matchType, double? score, List<(Software item, double score)> candidates,
                double? platformOverlap) result = MatchOne(igdbGame, softwareList, softwareNameVariantsById,
                                                           igdbAlternativeNamesByGameId, softwareOrdinalsById,
                                                           platformsBySoftwareId,
                                                           releaseYearsBySoftwareId, companyIdsBySoftwareId,
                                                           localCompanyIdsByIgdbGameId,
                                                           softwarePlatformByIgdbPlatformId, gameTypeById,
                                                           resolvedSoftwareIdByIgdbId);

            if(result.softwareId.HasValue)
                resolvedSoftwareIdByIgdbId[igdbGame.IgdbId] = result.softwareId.Value;

            if(!dryRun)
            {
                if(result.softwareId.HasValue)
                {
                    igdbGame.SoftwareId           = result.softwareId.Value;
                    igdbGame.MatchStatus          = IgdbMatchStatus.Matched;
                    igdbGame.MatchType            = result.matchType;
                    igdbGame.MatchScore           = result.score;
                    igdbGame.PlatformOverlapScore = result.platformOverlap;
                    igdbGame.MatchedOn            = DateTime.UtcNow;

                    if(igdbSiteId.HasValue)
                    {
                        string externalId = string.IsNullOrEmpty(igdbGame.Slug)
                                                 ? igdbGame.IgdbId.ToString()
                                                 : igdbGame.Slug;

                        if(existingExternalIds.Add((igdbSiteId.Value, externalId)))
                            context.SoftwareExternalIds.Add(new SoftwareExternalId
                            {
                                SoftwareId     = result.softwareId.Value,
                                ExternalSiteId = igdbSiteId.Value,
                                ExternalId     = externalId
                            });
                    }
                }
                else if(result.candidates is { Count: > 0 })
                {
                    igdbGame.MatchStatus    = IgdbMatchStatus.NeedsReview;
                    igdbGame.CandidatesJson =
                        JsonSerializer.Serialize(result.candidates.Select(c => new { c.item.Id, c.item.Name, c.score }));
                }
                else
                {
                    igdbGame.MatchStatus = IgdbMatchStatus.NoMatch;
                }
            }

            if(result.softwareId.HasValue)
                matched++;
            else if(result.candidates is { Count: > 0 })
                needsReview++;
            else
                noMatch++;
        }

        if(!dryRun)
            await context.SaveChangesAsync();

        return (matched, needsReview, noMatch);
    }

    static (ulong? softwareId, string matchType, double? score, List<(Software item, double score)> candidates,
        double? platformOverlap) MatchOne(IgdbGame igdbGame, List<Software> softwareList,
                                          Dictionary<ulong, List<string>> softwareNameVariantsById,
                                          Dictionary<long, List<string>> igdbAlternativeNamesByGameId,
                                          Dictionary<ulong, HashSet<int>> softwareOrdinalsById,
                                          Dictionary<ulong, HashSet<ulong>> platformsBySoftwareId,
                                          Dictionary<ulong, HashSet<int>> releaseYearsBySoftwareId,
                                          Dictionary<ulong, HashSet<int>> companyIdsBySoftwareId,
                                          Dictionary<long, HashSet<int>> localCompanyIdsByIgdbGameId,
                                          Dictionary<int, ulong> softwarePlatformByIgdbPlatformId,
                                          Dictionary<int, string> gameTypeById,
                                          Dictionary<long, ulong> resolvedSoftwareIdByIgdbId)
    {
        // Step 1: edition/bundle inheritance.
        string gameType = igdbGame.GameTypeId.HasValue && gameTypeById.TryGetValue(igdbGame.GameTypeId.Value, out string t)
                               ? t
                               : null;

        bool isStandaloneType = gameType == null || StandaloneGameTypes.Contains(gameType);
        long? parentIgdbId    = igdbGame.ParentGameId ?? igdbGame.VersionParentId;

        if(!isStandaloneType && parentIgdbId.HasValue)
        {
            if(resolvedSoftwareIdByIgdbId.TryGetValue(parentIgdbId.Value, out ulong parentSoftwareId))
                return (parentSoftwareId, "inherited-from-parent", null, null, null);

            // Parent not resolved yet (possibly in a later mirror batch) — leave Pending for a later run.
            return (null, null, null, null, null);
        }

        // Steps 2-4: name + platform corroboration.
        HashSet<int> igdbPlatformIds = ParsePlatformIds(igdbGame.PlatformIdsJson);

        HashSet<ulong> targetSoftwarePlatformIds = igdbPlatformIds
                                                    .Where(softwarePlatformByIgdbPlatformId.ContainsKey)
                                                    .Select(id => softwarePlatformByIgdbPlatformId[id])
                                                    .ToHashSet();

        double PlatformOverlap(ulong softwareId)
        {
            if(targetSoftwarePlatformIds.Count == 0)
                return 0;

            if(!platformsBySoftwareId.TryGetValue(softwareId, out HashSet<ulong> releasePlatforms))
                return 0;

            int overlap = targetSoftwarePlatformIds.Count(releasePlatforms.Contains);

            return (double)overlap / targetSoftwarePlatformIds.Count;
        }

        // FirstReleaseDate is null when never fetched and 0 when fetched but absent upstream; both mean unknown.
        int? igdbYear = igdbGame.FirstReleaseDate is > 0
                            ? DateTimeOffset.FromUnixTimeSeconds(igdbGame.FirstReleaseDate.Value).Year
                            : null;

        YearVerdict YearAgreement(ulong softwareId)
        {
            if(igdbYear is null)
                return YearVerdict.Unknown;

            if(!releaseYearsBySoftwareId.TryGetValue(softwareId, out HashSet<int> years) || years.Count == 0)
                return YearVerdict.Unknown;

            return years.Any(y => Math.Abs(y - igdbYear.Value) <= YearTolerance)
                       ? YearVerdict.Agree
                       : YearVerdict.Disagree;
        }

        HashSet<int> igdbCompanyIds = localCompanyIdsByIgdbGameId.GetValueOrDefault(igdbGame.IgdbId) ?? [];

        // Corroboration only, never a veto: company coverage is partial on both sides, so absence of a shared
        // developer/publisher says nothing, while presence of one is strong evidence.
        bool CompaniesAgree(ulong softwareId) =>
            igdbCompanyIds.Count > 0 &&
            companyIdsBySoftwareId.TryGetValue(softwareId, out HashSet<int> localIds) &&
            igdbCompanyIds.Overlaps(localIds);

        // Name candidates on the IGDB side: the canonical name plus every mirrored
        // IGDB alternative_names row for this game (e.g. Japanese title, working title).
        List<string> igdbNames = BuildNameVariants(igdbGame.Name,
            igdbAlternativeNamesByGameId.TryGetValue(igdbGame.IgdbId, out List<string> igdbAlts) ? igdbAlts : null);

        // Exact match: any IGDB name variant against any local name variant (canonical name +
        // SoftwareAlternativeTitle rows) — so an alternate can match an alternate.
        List<Software> exactNameMatches = softwareList
                                          .Where(s => NameSetsOverlap(igdbNames, softwareNameVariantsById[s.Id]))
                                          .ToList();

        if(exactNameMatches.Count == 1)
        {
            string matchType = string.Equals(exactNameMatches[0].Name, igdbGame.Name, StringComparison.OrdinalIgnoreCase)
                                    ? "exact"
                                    : "exact-alt";

            return (exactNameMatches[0].Id, matchType, null, null, PlatformOverlap(exactNameMatches[0].Id));
        }

        if(exactNameMatches.Count > 1)
        {
            // Strongest signal first: a shared developer/publisher AND an agreeing year.
            List<Software> corroborated = exactNameMatches
                                         .Where(s => CompaniesAgree(s.Id) && YearAgreement(s.Id) != YearVerdict.Disagree)
                                         .ToList();

            if(corroborated.Count == 1)
            {
                string matchType = string.Equals(corroborated[0].Name, igdbGame.Name, StringComparison.OrdinalIgnoreCase)
                                        ? "exact-company-disambiguated"
                                        : "exact-company-disambiguated-alt";

                return (corroborated[0].Id, matchType, null, null, PlatformOverlap(corroborated[0].Id));
            }

            // Then prefer candidates whose release year corroborates IGDB's; only fall back to the platform filter
            // when the year says nothing useful, so a same-named remake does not win on platform overlap alone.
            List<Software> yearAgreeing = exactNameMatches.Where(s => YearAgreement(s.Id) == YearVerdict.Agree)
                                                          .ToList();

            if(yearAgreeing.Count == 1)
            {
                string matchType = string.Equals(yearAgreeing[0].Name, igdbGame.Name, StringComparison.OrdinalIgnoreCase)
                                        ? "exact-year-disambiguated"
                                        : "exact-year-disambiguated-alt";

                return (yearAgreeing[0].Id, matchType, null, null, PlatformOverlap(yearAgreeing[0].Id));
            }

            // Drop candidates the year positively contradicts before falling back to platform overlap.
            List<Software> yearPlausible = exactNameMatches
                                          .Where(s => YearAgreement(s.Id) != YearVerdict.Disagree)
                                          .ToList();

            if(yearPlausible.Count == 0)
                yearPlausible = exactNameMatches;

            List<(Software software, double overlap)> withOverlap = yearPlausible
               .Select(s => (software: s, overlap: PlatformOverlap(s.Id)))
               .Where(s => s.overlap > 0)
               .ToList();

            if(withOverlap.Count == 1)
            {
                string matchType =
                    string.Equals(withOverlap[0].software.Name, igdbGame.Name, StringComparison.OrdinalIgnoreCase)
                        ? "exact-platform-disambiguated"
                        : "exact-platform-disambiguated-alt";

                return (withOverlap[0].software.Id, matchType, null, null, withOverlap[0].overlap);
            }

            return (null, null, null, yearPlausible.Select(s => (item: s, score: 1.0)).ToList(), null);
        }

        // Fuzzy sweep: best Jaro-Winkler score across the full cross-product of IGDB name
        // variants and local name variants, so e.g. an IGDB alternative name can fuzzy-match a
        // local alternative title even when neither side's canonical name is close.
        // Vetoes, applied before any score is consulted: Jaro-Winkler rates a conflicting-ordinal pair such as
        // "Quake 4" vs "Quake" at 0.94, above the auto-accept threshold, so a score-based penalty cannot stop it.
        HashSet<int> igdbOrdinals = SeriesOrdinals(igdbNames);

        bool Vetoed(Software s) => OrdinalsConflict(igdbOrdinals, softwareOrdinalsById[s.Id]) ||
                                    YearAgreement(s.Id) == YearVerdict.Disagree;

        List<(Software item, double score, bool viaCanonicalPair)> sweep = softwareList
           .Where(s => !Vetoed(s))
           .Select(s =>
            {
                (double score, bool canonical) = BestNameScore(igdbGame.Name, igdbNames, s.Name,
                    softwareNameVariantsById[s.Id]);

                return (item: s, score, viaCanonicalPair: canonical);
            })
           .Where(t => t.score >= FullSweepReviewThreshold)
           .OrderByDescending(t => t.score)
           .ToList();

        if(sweep.Count == 1)
        {
            bool hasExistingReleases = platformsBySoftwareId.TryGetValue(sweep[0].item.Id, out HashSet<ulong> releases) &&
                                        releases.Count > 0;

            double overlap = PlatformOverlap(sweep[0].item.Id);

            if(sweep[0].score >= FullSweepAcceptThreshold && (!hasExistingReleases || overlap > 0))
            {
                string matchType = sweep[0].viaCanonicalPair
                                        ? "jw-platform-corroborated"
                                        : "jw-platform-corroborated-alt";

                return (sweep[0].item.Id, matchType, sweep[0].score, null, overlap);
            }

            return (null, null, null, sweep.Select(s => (s.item, s.score)).ToList(), null);
        }

        if(sweep.Count > 1)
            return (null, null, null, sweep.Select(s => (s.item, s.score)).ToList(), null);

        return (null, null, null, null, null);
    }

    /// <summary>
    ///     Builds the deduplicated (case-insensitive) list of name variants for one side of a
    ///     match: the canonical name first, followed by any alternative titles/names, skipping
    ///     blanks and duplicates of the canonical name.
    /// </summary>
    static List<string> BuildNameVariants(string canonicalName, List<string> alternatives)
    {
        var variants = new List<string>();

        if(!string.IsNullOrWhiteSpace(canonicalName))
            variants.Add(canonicalName);

        if(alternatives is not null)
            foreach(string alt in alternatives)
            {
                if(string.IsNullOrWhiteSpace(alt))
                    continue;

                if(variants.Any(v => string.Equals(v, alt, StringComparison.OrdinalIgnoreCase)))
                    continue;

                variants.Add(alt);
            }

        return variants;
    }

    /// <summary>Whether any name in <paramref name="a" /> case-insensitively equals any name in
    /// <paramref name="b" />.</summary>
    static bool NameSetsOverlap(List<string> a, List<string> b) =>
        a.Any(x => b.Any(y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    ///     Best Jaro-Winkler score across the cross-product of <paramref name="aNames" /> and
    ///     <paramref name="bNames" />. The returned <c>canonicalPair</c> flag tells the caller
    ///     whether the winning pair was canonical-vs-canonical (<paramref name="aCanonical" /> vs
    ///     <paramref name="bCanonical" />) or involved at least one alternative name/title, purely
    ///     for audit-trail labelling (match type suffix).
    /// </summary>
    static (double score, bool canonicalPair) BestNameScore(string aCanonical, List<string> aNames,
        string bCanonical, List<string> bNames)
    {
        double best          = 0.0;
        bool   bestCanonical = false;

        foreach(string a in aNames)
        foreach(string b in bNames)
        {
            double score = JaroWinkler.Similarity(a, b);

            if(score <= best)
                continue;

            best          = score;
            bestCanonical = string.Equals(a, aCanonical, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(b, bCanonical, StringComparison.OrdinalIgnoreCase);
        }

        return (best, bestCanonical);
    }

    /// <summary>
    ///     Extracts the series ordinal from a game title, e.g. 3 for <c>Doom 3</c>, 2 for
    ///     <c>Doom II: Hell on Earth</c>, 5 for <c>Metal Gear Solid V: The Phantom Pain</c>, 3 for
    ///     <c>Quake III Arena</c>. Returns 1 when no ordinal is present, since an unnumbered title is the first
    ///     entry in its series — that is what makes <c>Quake</c> distinguishable from <c>Quake 4</c>.
    /// </summary>
    /// <remarks>
    ///     Only values 2-30 count, which deliberately excludes years (<c>Microsoft Flight Simulator 2000</c>),
    ///     zero-padded numbers (<c>007: Quantum of Solace</c>) and the roman letters that are far more often words
    ///     than numerals (<c>C</c>, <c>D</c>, <c>L</c>, <c>M</c> all exceed 30; bare <c>I</c> is 1, below the floor).
    ///     Anything after a <c>:</c> subtitle separator is ignored, and the whole token must be the numeral, so
    ///     <c>10-Pin Bowling</c> yields no ordinal.
    /// </remarks>
    static int SeriesOrdinal(string title)
    {
        if(string.IsNullOrWhiteSpace(title))
            return 1;

        int colon = title.IndexOf(':');
        string main = colon >= 0 ? title[..colon] : title;

        foreach(string token in main.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries))
        {
            string trimmed = token.Trim('.', ',', '(', ')', '[', ']', '!', '?', '\'', '"');

            if(trimmed.Length == 0)
                continue;

            int value = ParseArabicOrdinal(trimmed) ?? ParseRomanOrdinal(trimmed) ?? 0;

            if(value is >= 2 and <= 30)
                return value;
        }

        return 1;
    }

    static int? ParseArabicOrdinal(string token)
    {
        // A leading zero means a zero-padded label rather than a series number (e.g. "007").
        if(token[0] == '0')
            return null;

        return token.All(char.IsAsciiDigit) && int.TryParse(token, out int value) ? value : null;
    }

    static int? ParseRomanOrdinal(string token)
    {
        string upper = token.ToUpperInvariant();

        if(!upper.All(RomanDigits.ContainsKey))
            return null;

        int total = 0;

        for(var i = 0; i < upper.Length; i++)
        {
            int value = RomanDigits[upper[i]];

            // Subtractive pairs: IV, IX, XL and so on.
            total += i + 1 < upper.Length && RomanDigits[upper[i + 1]] > value ? -value : value;
        }

        return total;
    }

    /// <summary>
    ///     The distinct series ordinals a side of the match can present, precomputed once per name-variant set
    ///     because the fuzzy sweep is a full cross-product and would otherwise re-tokenize every title for every
    ///     candidate pair.
    /// </summary>
    static HashSet<int> SeriesOrdinals(List<string> names) => names.Select(SeriesOrdinal).ToHashSet();

    /// <summary>
    ///     Whether two titles carry conflicting series ordinals, in which case they are different games however
    ///     similar they read. Jaro-Winkler's prefix boost scores sequels in a series <em>above</em> the auto-accept
    ///     threshold against each other (<c>Quake 4</c> vs <c>Quake</c> = 0.94), because they share a long prefix and
    ///     differ only in the trailing numeral the metric discounts — so this has to be a veto, not a penalty.
    /// </summary>
    /// <remarks>
    ///     Compared at the name-set level, and a conflict only stands when <em>no</em> variant pair agrees: an
    ///     alternative title that agrees on the ordinal is enough to let the pair through to scoring.
    /// </remarks>
    static bool OrdinalsConflict(HashSet<int> aOrdinals, HashSet<int> bOrdinals) => !aOrdinals.Overlaps(bOrdinals);

    static HashSet<int> ParsePlatformIds(string json)
    {
        if(string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<int>>(json)?.ToHashSet() ?? [];
        }
        catch(JsonException)
        {
            return [];
        }
    }
}
