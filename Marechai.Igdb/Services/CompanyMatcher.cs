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

public class CompanyMatcher
{
    const double SoundexSingleAcceptThreshold = 0.90;
    const double SoundexBestAcceptThreshold   = 0.92;
    const double SoundexMargin                = 0.05;
    const double FullSweepSingleThreshold     = 0.90;
    const double FullSweepReviewThreshold      = 0.90;
    const double MinReviewThreshold            = 0.90;

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

    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public CompanyMatcher(IDbContextFactory<MarechaiContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<(int matched, int needsReview, int noMatch)> RunAsync(bool dryRun)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        List<Company> companies = await context.Companies
                                                 .Select(c => new Company
                                                 {
                                                     Id        = c.Id,
                                                     Name      = c.Name,
                                                     LegalName = c.LegalName
                                                 })
                                                 .ToListAsync();

        Dictionary<string, List<Company>> soundexIndex = SoundexHelper.BuildSoundexIndex(companies, c => c.Name);
        Dictionary<string, List<Company>> strippedSoundexIndex =
            SoundexHelper.BuildSoundexIndex(companies, c => StripSuffix(c.Name));
        Dictionary<string, List<Company>> legalSoundexIndex =
            SoundexHelper.BuildSoundexIndex(companies, c => c.LegalName);
        Dictionary<string, List<Company>> strippedLegalSoundexIndex =
            SoundexHelper.BuildSoundexIndex(companies, c => StripSuffix(c.LegalName));

        List<IgdbCompany> pending = await context.IgdbCompanies
                                                  .Where(c => c.MatchStatus == IgdbMatchStatus.Pending)
                                                  .ToListAsync();

        int matched     = 0;
        int needsReview = 0;
        int noMatch      = 0;

        foreach(IgdbCompany igdbCompany in pending)
        {
            (Company company, string matchType, double? score, List<(Company item, double score)> candidates) result =
                Match(igdbCompany.Name, companies, soundexIndex, strippedSoundexIndex, legalSoundexIndex,
                      strippedLegalSoundexIndex);

            if(!dryRun)
            {
                if(result.company != null)
                {
                    igdbCompany.CompanyId   = result.company.Id;
                    igdbCompany.MatchStatus = IgdbMatchStatus.Matched;
                    igdbCompany.MatchType   = result.matchType;
                    igdbCompany.MatchScore  = result.score;
                    igdbCompany.MatchedOn   = DateTime.UtcNow;
                }
                else if(result.candidates is { Count: > 0 })
                {
                    igdbCompany.MatchStatus    = IgdbMatchStatus.NeedsReview;
                    igdbCompany.CandidatesJson =
                        JsonSerializer.Serialize(result.candidates.Select(c => new { c.item.Id, c.item.Name, c.score }));
                }
                else
                {
                    igdbCompany.MatchStatus = IgdbMatchStatus.NoMatch;
                }
            }

            if(result.company != null)
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

    static (Company company, string matchType, double? score, List<(Company item, double score)> candidates) Match(
        string name, List<Company> companies, Dictionary<string, List<Company>> soundexIndex,
        Dictionary<string, List<Company>> strippedSoundexIndex, Dictionary<string, List<Company>> legalSoundexIndex,
        Dictionary<string, List<Company>> strippedLegalSoundexIndex)
    {
        if(string.IsNullOrWhiteSpace(name))
            return (null, null, null, null);

        string normalizedName = name.Replace(" ", " ").Trim();

        Company exact = companies.FirstOrDefault(c =>
            string.Equals(c.Name, normalizedName, StringComparison.OrdinalIgnoreCase));

        if(exact != null)
            return (exact, "exact", 1.0, null);

        Company legalExact = companies.FirstOrDefault(c =>
            !string.IsNullOrWhiteSpace(c.LegalName) &&
            string.Equals(c.LegalName, normalizedName, StringComparison.OrdinalIgnoreCase));

        if(legalExact != null)
            return (legalExact, "exact-legal", 1.0, null);

        string strippedInput = StripSuffix(normalizedName);

        Company strippedExact = companies.FirstOrDefault(c =>
            string.Equals(StripSuffix(c.Name), strippedInput, StringComparison.OrdinalIgnoreCase));

        if(strippedExact != null)
            return (strippedExact, "exact-stripped", 1.0, null);

        Company strippedLegalExact = companies.FirstOrDefault(c =>
            !string.IsNullOrWhiteSpace(c.LegalName) &&
            string.Equals(StripSuffix(c.LegalName), strippedInput, StringComparison.OrdinalIgnoreCase));

        if(strippedLegalExact != null)
            return (strippedLegalExact, "exact-stripped-legal", 1.0, null);

        string soundex         = SoundexHelper.Generate(normalizedName);
        string strippedSoundex = SoundexHelper.Generate(strippedInput);

        var soundexCandidates = new List<Company>();
        var seenIds           = new HashSet<int>();

        void AddCandidates(IEnumerable<Company> cs)
        {
            foreach(Company c in cs)
                if(seenIds.Add(c.Id))
                    soundexCandidates.Add(c);
        }

        if(soundexIndex.TryGetValue(soundex, out var fullCandidates))
            AddCandidates(fullCandidates);

        if(legalSoundexIndex.TryGetValue(soundex, out var legalCandidates))
            AddCandidates(legalCandidates);

        if(strippedSoundexIndex.TryGetValue(strippedSoundex, out var strippedCandidates))
            AddCandidates(strippedCandidates);

        if(strippedLegalSoundexIndex.TryGetValue(strippedSoundex, out var strippedLegalCandidates))
            AddCandidates(strippedLegalCandidates);

        if(soundexCandidates.Count > 0)
        {
            // A shared Soundex code alone is weak evidence — short/common codes collide constantly between
            // textually unrelated names. Only surface a candidate (Matched or NeedsReview) when its actual
            // Jaro-Winkler score clears MinReviewThreshold; otherwise the Soundex hit carries no real signal
            // and we fall through to the full sweep below instead of manufacturing a NeedsReview entry.
            List<(Company item, double score)> scored = soundexCandidates
                                                        .Select(c => (item: c, score: JaroWinkler.Similarity(normalizedName, c.Name)))
                                                        .Where(c => c.score >= MinReviewThreshold)
                                                        .OrderByDescending(c => c.score)
                                                        .ToList();

            if(scored.Count == 1)
            {
                if(scored[0].score >= SoundexSingleAcceptThreshold)
                    return (scored[0].item, "soundex", scored[0].score, null);

                return (null, null, null, scored);
            }

            if(scored.Count > 1)
            {
                if(scored[0].score >= SoundexBestAcceptThreshold &&
                   scored[0].score - scored[1].score >= SoundexMargin)
                    return (scored[0].item, "soundex-jw-best", scored[0].score, null);

                return (null, null, null, scored);
            }
        }

        List<(Company item, double score)> sweep =
            JaroWinkler.FindMatches(normalizedName, companies, c => c.Name, FullSweepReviewThreshold);

        if(sweep.Count == 1 && sweep[0].score >= FullSweepSingleThreshold)
            return (sweep[0].item, "jaro-winkler", sweep[0].score, null);

        if(sweep.Count > 0)
            return (null, null, null, sweep);

        return (null, null, null, null);
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

                break;
            }
        }

        return result;
    }
}
