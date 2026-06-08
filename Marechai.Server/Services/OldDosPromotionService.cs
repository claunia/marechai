/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     Promotes a <see cref="OldDosSoftware" /> staging row to real <see cref="Software" /> +
///     descendants. Two modes: CreateNew (default) and MergeIntoExisting (attach to a Software the
///     admin picked via the name-collision warning, without overwriting any of its existing data).
///     Also exposes <see cref="FindNameMatchesAsync" /> used by the review dialog to surface
///     existing softwares with the same (normalized) or similar (Jaro-Winkler ≥ 0.85) name.
/// </summary>
public sealed class OldDosPromotionService
{
    const string                 DEV_ROLE_ID            = "dev";
    const double                 JARO_WINKLER_THRESHOLD = 0.85;
    static readonly string[]     ENGLISH_LANG_CODE      = { "eng" };

    readonly MarechaiContext     _db;
    readonly FuzzySearchService  _fuzzy;
    readonly ILogger<OldDosPromotionService> _log;

    public OldDosPromotionService(MarechaiContext db,
                                  FuzzySearchService fuzzy,
                                  ILogger<OldDosPromotionService> log)
    {
        _db    = db;
        _fuzzy = fuzzy;
        _log   = log;
    }

    public async Task<OldDosNameMatchCandidatesDto> FindNameMatchesAsync(string candidateName)
    {
        var result = new OldDosNameMatchCandidatesDto();
        if(string.IsNullOrWhiteSpace(candidateName)) return result;

        // 1. Exact normalized matches via the existing MariaDB function.
        List<NameMatchProjection> exact = await _db.Softwares
                                                   .Where(s => MarechaiContext.NormalizeForDuplicate(s.Name) ==
                                                               MarechaiContext.NormalizeForDuplicate(candidateName))
                                                   .Select(s => new NameMatchProjection
                                                   {
                                                       Id   = s.Id,
                                                       Name = s.Name,
                                                       Kind = s.Kind,
                                                       Year = s.DirectReleases
                                                                .Where(r => r.ReleaseDate.HasValue)
                                                                .Min(r => (int?)r.ReleaseDate.Value.Year),
                                                       ReleaseCount = s.DirectReleases.Count
                                                   })
                                                   .Take(20)
                                                   .ToListAsync();

        var exactIds = new HashSet<ulong>(exact.Select(e => e.Id));
        foreach(NameMatchProjection e in exact)
            result.Matches.Add(new OldDosNameMatchCandidateDto
            {
                SoftwareId          = e.Id,
                Name                = e.Name,
                Kind                = e.Kind,
                EarliestReleaseYear = e.Year,
                ReleaseCount        = e.ReleaseCount,
                JaroWinklerScore    = 1.0,
                MatchKind           = OldDosNameMatchKind.ExactNormalized
            });

        // 2. Fuzzy candidates: pre-filter via substring on whole name OR any token.
        var tokens = candidateName.Split(' ', '-', '_', '/', ':')
                                  .Where(t => t.Length >= 3)
                                  .Distinct(StringComparer.OrdinalIgnoreCase)
                                  .ToArray();

        IQueryable<Software> q = _db.Softwares.Where(s => s.Name.Contains(candidateName));
        foreach(string tok in tokens) q = q.Union(_db.Softwares.Where(s => s.Name.Contains(tok)));

        List<NameMatchProjection> fuzzyCandidates = await q.Where(s => !exactIds.Contains(s.Id))
                                                           .Select(s => new NameMatchProjection
                                                           {
                                                               Id   = s.Id,
                                                               Name = s.Name,
                                                               Kind = s.Kind,
                                                               Year = s.DirectReleases
                                                                        .Where(r => r.ReleaseDate.HasValue)
                                                                        .Min(r => (int?)r.ReleaseDate.Value.Year),
                                                               ReleaseCount = s.DirectReleases.Count
                                                           })
                                                           .Take(30)
                                                           .ToListAsync();

        foreach(NameMatchProjection c in fuzzyCandidates)
        {
            double score = _fuzzy.JaroWinkler(candidateName.ToUpperInvariant(), c.Name.ToUpperInvariant());
            if(score < JARO_WINKLER_THRESHOLD) continue;
            result.Matches.Add(new OldDosNameMatchCandidateDto
            {
                SoftwareId          = c.Id,
                Name                = c.Name,
                Kind                = c.Kind,
                EarliestReleaseYear = c.Year,
                ReleaseCount        = c.ReleaseCount,
                JaroWinklerScore    = score,
                MatchKind           = OldDosNameMatchKind.Fuzzy
            });
        }

        result.Matches = result.Matches.OrderByDescending(m => m.MatchKind == OldDosNameMatchKind.ExactNormalized)
                               .ThenByDescending(m => m.JaroWinklerScore)
                               .ToList();
        return result;
    }

    public async Task<AcceptOldDosImportResultDto> AcceptAsync(long oldDosId, AcceptOldDosImportDto dto, string adminUserId)
    {
        OldDosSoftware staging = await _db.OldDosSoftwares
                                          .Include(s => s.Versions)
                                          .FirstOrDefaultAsync(s => s.Id == oldDosId);
        if(staging == null)
            return new AcceptOldDosImportResultDto { Success = false, Error = "Staging row not found." };
        if(staging.Status != OldDosSoftwareStatus.ReadyForReview &&
           staging.Status != OldDosSoftwareStatus.Skipped)
            return new AcceptOldDosImportResultDto { Success = false, Error = $"Cannot accept row in status {staging.Status}." };

        // Developer is required for the publisher fk; surface a clean error if missing.
        Company developer = await _db.Companies.FindAsync(dto.DeveloperCompanyId);
        if(developer == null)
            return new AcceptOldDosImportResultDto { Success = false, Error = "Developer company must be selected." };

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            ulong targetSoftwareId;
            string softwareName;
            int   insertedDescriptions = 0;
            int   insertedGenres       = 0;
            int   insertedVersions     = 0;
            int   insertedReleases     = 0;

            if(dto.Mode == OldDosAcceptMode.MergeIntoExisting)
            {
                if(dto.TargetSoftwareId is not { } tid)
                    return new AcceptOldDosImportResultDto { Success = false, Error = "TargetSoftwareId required for merge mode." };

                Software target = await _db.Softwares.FindAsync(tid);
                if(target == null)
                    return new AcceptOldDosImportResultDto { Success = false, Error = "Target software not found." };

                targetSoftwareId = target.Id;
                softwareName     = target.Name;
            }
            else
            {
                var fresh = new Software
                {
                    Name = string.IsNullOrWhiteSpace(dto.NameOverride) ? staging.Name : dto.NameOverride.Trim(),
                    Kind = dto.KindOverride.HasValue ? (SoftwareKind)dto.KindOverride.Value : SoftwareKind.Game
                };
                _db.Softwares.Add(fresh);
                await _db.SaveChangesAsync();
                targetSoftwareId = fresh.Id;
                softwareName     = fresh.Name;
            }

            // Per-version attach (Version + Release + DEV company role).
            foreach(AcceptOldDosVersionDecisionDto vd in dto.Versions)
            {
                if(!vd.Include) continue;
                OldDosVersion staged = staging.Versions.FirstOrDefault(v => v.Id == vd.OldDosVersionId);
                if(staged == null) continue;

                var version = new SoftwareVersion
                {
                    SoftwareId    = targetSoftwareId,
                    VersionString = string.IsNullOrWhiteSpace(vd.VersionStringOverride)
                                        ? staged.VersionString
                                        : vd.VersionStringOverride
                };
                _db.SoftwareVersions.Add(version);
                await _db.SaveChangesAsync();
                staged.PromotedSoftwareVersionId = version.Id;
                insertedVersions++;

                ulong? platformId = vd.SoftwarePlatformIdOverride;
                if(platformId is null && !string.IsNullOrWhiteSpace(staged.OsHint))
                {
                    OldDosOsPlatformMap map =
                        await _db.OldDosOsPlatformMaps.FirstOrDefaultAsync(m => m.OsName == staged.OsHint);
                    if(map is { IgnoreOnPromote: false }) platformId = map.SoftwarePlatformId;
                }

                // PlatformId on SoftwareRelease is nullable \u2014 always create the release even
                // when the admin removed the platform suggestion (and we couldn't resolve one
                // from the OS hint). A "platformless" release is still useful: it groups the
                // SoftwareVersion + Publisher + ReleaseDate metadata and can be edited later
                // to add a platform.
                DateTime? releaseDate          = vd.ReleaseDateOverride ?? staged.ReleaseDate;
                DatePrecision releasePrecision = vd.ReleaseDatePrecisionOverride.HasValue
                                                     ? (DatePrecision)vd.ReleaseDatePrecisionOverride.Value
                                                     : staged.ReleaseDatePrecision;
                _db.SoftwareReleases.Add(new SoftwareRelease
                {
                    Title                = string.IsNullOrWhiteSpace(dto.NameOverride) ? staging.Name : dto.NameOverride.Trim(),
                    SoftwareVersionId    = version.Id,
                    PlatformId           = platformId,
                    PublisherId          = developer.Id,
                    ReleaseDate          = releaseDate,
                    ReleaseDatePrecision = releasePrecision
                });
                insertedReleases++;

                _db.CompaniesBySoftwareVersions.Add(new CompanyBySoftwareVersion
                {
                    CompanyId         = developer.Id,
                    SoftwareVersionId = version.Id,
                    RoleId            = DEV_ROLE_ID
                });
            }
            await _db.SaveChangesAsync();

            // Descriptions: only the English museum-grade source row. The 5-locale fan-out is the
            // job of DescriptionTranslationWorker (see SoftwareDescriptionSource) which picks up
            // any SoftwareDescription that's missing siblings in other languages and translates
            // them in the background. Running NLLB inline here used to make AcceptAsync hit
            // Apache's ProxyTimeout and surface as a 502 → masked 404 in the dialog.
            string museum = string.IsNullOrWhiteSpace(dto.MuseumDescriptionEdited)
                                ? staging.EnglishDescriptionMuseum
                                : dto.MuseumDescriptionEdited.Trim();

            if(!string.IsNullOrWhiteSpace(museum))
            {
                bool engExists = await _db.SoftwareDescriptions
                                          .AnyAsync(d => d.SoftwareId == targetSoftwareId && d.LanguageCode == "eng");
                if(!engExists)
                {
                    _db.SoftwareDescriptions.Add(new SoftwareDescription
                    {
                        SoftwareId   = targetSoftwareId,
                        LanguageCode = "eng",
                        Text         = museum,
                        Html         = RenderMarkdown(museum)
                    });
                    insertedDescriptions++;
                }
            }

            // Genres (dedupe in merge mode is required, harmless in create mode).
            HashSet<int> existingGenres = new(await _db.GenresBySoftware
                                                       .Where(g => g.SoftwareId == targetSoftwareId)
                                                       .Select(g => g.GenreId)
                                                       .ToListAsync());
            foreach(int gid in dto.GenreIds.Distinct())
            {
                if(existingGenres.Contains(gid)) continue;
                _db.GenresBySoftware.Add(new GenreBySoftware
                {
                    SoftwareId = targetSoftwareId,
                    GenreId    = gid
                });
                existingGenres.Add(gid);
                insertedGenres++;
            }

            staging.Status             = OldDosSoftwareStatus.Accepted;
            staging.PromotedSoftwareId = targetSoftwareId;
            staging.ReviewedBy         = adminUserId;
            staging.ReviewedOn         = DateTime.UtcNow;

            _db.News.Add(new News
            {
                AddedId = (long)targetSoftwareId,
                Date    = DateTime.UtcNow,
                Type    = dto.Mode == OldDosAcceptMode.MergeIntoExisting
                              ? NewsType.UpdatedSoftwareInDb
                              : NewsType.NewSoftwareInDb,
                Name = softwareName
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return new AcceptOldDosImportResultDto
            {
                Success                  = true,
                PromotedSoftwareId       = targetSoftwareId,
                InsertedVersionCount     = insertedVersions,
                InsertedReleaseCount     = insertedReleases,
                InsertedDescriptionCount = insertedDescriptions,
                InsertedGenreCount       = insertedGenres
            };
        }
        catch(Exception ex)
        {
            await tx.RollbackAsync();
            _log.LogError(ex, "OldDos accept #{Id} failed.", oldDosId);
            return new AcceptOldDosImportResultDto { Success = false, Error = ex.Message };
        }
    }

    public async Task<bool> SkipAsync(long oldDosId, string adminUserId)
    {
        OldDosSoftware row = await _db.OldDosSoftwares.FindAsync(oldDosId);
        if(row == null) return false;
        if(row.Status is OldDosSoftwareStatus.Accepted or OldDosSoftwareStatus.Discarded) return false;
        row.Status     = OldDosSoftwareStatus.Skipped;
        row.ReviewedBy = adminUserId;
        row.ReviewedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DiscardAsync(long oldDosId, string adminUserId)
    {
        OldDosSoftware row = await _db.OldDosSoftwares.FindAsync(oldDosId);
        if(row == null) return false;
        if(row.Status == OldDosSoftwareStatus.Accepted) return false;
        row.Status     = OldDosSoftwareStatus.Discarded;
        row.ReviewedBy = adminUserId;
        row.ReviewedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    ///     Render the museum-grade markdown to HTML via Markdig with the same advanced-extensions
    ///     pipeline used by <see cref="SoundSynthDescriptionSuggestionApplier" /> and friends.
    ///     We populate both <c>SoftwareDescription.Text</c> (raw) and <c>SoftwareDescription.Html</c>
    ///     (pre-rendered) because the public <c>/software/{id}</c> view reads <c>Html</c> first and
    ///     falls back to dumping <c>Text</c> straight into a <c>MarkupString</c> — which collapses
    ///     paragraph breaks because the raw markdown has no <c>&lt;p&gt;</c> tags.
    /// </summary>
    static readonly MarkdownPipeline s_markdownPipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    static string RenderMarkdown(string markdown) =>
        string.IsNullOrWhiteSpace(markdown) ? string.Empty : Markdown.ToHtml(markdown, s_markdownPipeline);

    public static List<int> ParseSuggestedGenreIds(string json)
    {
        if(string.IsNullOrWhiteSpace(json)) return new List<int>();
        try
        {
            int[] ids = JsonSerializer.Deserialize<int[]>(json);
            return ids?.ToList() ?? new List<int>();
        }
        catch { return new List<int>(); }
    }

    sealed class NameMatchProjection
    {
        public ulong         Id           { get; set; }
        public string        Name         { get; set; }
        public SoftwareKind  Kind         { get; set; }
        public int?          Year         { get; set; }
        public int           ReleaseCount { get; set; }
    }
}
