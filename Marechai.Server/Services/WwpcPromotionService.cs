/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Marechai.Helpers;
using Marechai.Server.Helpers;
using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     Promotes a <see cref="WwpcSoftware" /> staging row to real <see cref="Software" /> +
///     descendants. Two modes:
///     <list type="bullet">
///         <item><b>CreateNew</b> — insert a brand-new <see cref="Software" /> with the museum
///               description (translated to all locales), the selected category genres, every
///               enabled <see cref="WwpcVersion" /> as a <see cref="SoftwareVersion" />, and every
///               enabled <see cref="WwpcScreenshot" /> downloaded into the screenshot assets tree.</item>
///         <item><b>MergeIntoExisting</b> — attach to an existing <see cref="Software" />:
///               versions may be linked to a pre-existing <see cref="SoftwareVersion" /> instead of
///               creating new ones; descriptions, genres and screenshots are added without
///               overwriting anything already present.</item>
///     </list>
///     Also exposes name-match / company-match helpers used by the review dialog to surface
///     existing rows by exact normalized + Jaro-Winkler ≥ 0.85.
/// </summary>
public sealed class WwpcPromotionService
{
    const string DEV_ROLE_ID            = "dev";
    const double JARO_WINKLER_THRESHOLD = 0.85;

    readonly MarechaiContext               _db;
    readonly FuzzySearchService            _fuzzy;
    readonly IConfiguration                _config;
    readonly ILogger<WwpcPromotionService> _log;

    public WwpcPromotionService(MarechaiContext db, FuzzySearchService fuzzy,
                                IConfiguration config, ILogger<WwpcPromotionService> log)
    {
        _db     = db;
        _fuzzy  = fuzzy;
        _config = config;
        _log    = log;
    }

    // ---------- Name match (Software) ----------

    public async Task<WwpcNameMatchCandidatesDto> FindNameMatchesAsync(string candidateName)
    {
        var result = new WwpcNameMatchCandidatesDto();
        if(string.IsNullOrWhiteSpace(candidateName)) return result;

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
            result.Matches.Add(new WwpcNameMatchCandidateDto
            {
                SoftwareId          = e.Id,
                Name                = e.Name,
                Kind                = (byte)e.Kind,
                EarliestReleaseYear = e.Year,
                ReleaseCount        = e.ReleaseCount,
                JaroWinklerScore    = 1.0,
                MatchKind           = WwpcNameMatchKind.ExactNormalized
            });

        string[] tokens = candidateName.Split(' ', '-', '_', '/', ':')
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
            result.Matches.Add(new WwpcNameMatchCandidateDto
            {
                SoftwareId          = c.Id,
                Name                = c.Name,
                Kind                = (byte)c.Kind,
                EarliestReleaseYear = c.Year,
                ReleaseCount        = c.ReleaseCount,
                JaroWinklerScore    = score,
                MatchKind           = WwpcNameMatchKind.Fuzzy
            });
        }

        result.Matches = result.Matches.OrderByDescending(m => m.MatchKind == WwpcNameMatchKind.ExactNormalized)
                               .ThenByDescending(m => m.JaroWinklerScore)
                               .ToList();
        return result;
    }

    // ---------- Company match (Vendor) ----------

    public async Task<WwpcCompanyMatchCandidatesDto> FindCompanyMatchesAsync(string vendorName)
    {
        var result = new WwpcCompanyMatchCandidatesDto();
        if(string.IsNullOrWhiteSpace(vendorName)) return result;

        // 1. Exact normalized via MariaDB function (same one Software uses).
        List<(int Id, string Name)> exact = await _db.Companies
                                                     .Where(c => MarechaiContext.NormalizeForDuplicate(c.Name) ==
                                                                 MarechaiContext.NormalizeForDuplicate(vendorName))
                                                     .Select(c => new ValueTuple<int, string>(c.Id, c.Name))
                                                     .Take(20)
                                                     .ToListAsync();
        var exactIds = new HashSet<int>(exact.Select(e => e.Id));
        foreach((int id, string name) in exact)
            result.Matches.Add(new WwpcCompanyMatchCandidateDto
            {
                CompanyId        = id,
                Name             = name,
                JaroWinklerScore = 1.0,
                MatchKind        = WwpcCompanyMatchKind.ExactNormalized
            });

        // 2. Fuzzy candidates via substring on whole name OR any token.
        string[] tokens = vendorName.Split(' ', '-', '_', '/', ':', '.', ',')
                                    .Where(t => t.Length >= 3)
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .ToArray();
        IQueryable<Company> q = _db.Companies.Where(c => c.Name != null && c.Name.Contains(vendorName));
        foreach(string tok in tokens) q = q.Union(_db.Companies.Where(c => c.Name != null && c.Name.Contains(tok)));

        List<(int Id, string Name)> fuzzy = await q.Where(c => !exactIds.Contains(c.Id))
                                                   .Select(c => new ValueTuple<int, string>(c.Id, c.Name))
                                                   .Take(30)
                                                   .ToListAsync();

        foreach((int id, string name) in fuzzy)
        {
            double score = _fuzzy.JaroWinkler(vendorName.ToUpperInvariant(), name.ToUpperInvariant());
            if(score < JARO_WINKLER_THRESHOLD) continue;
            result.Matches.Add(new WwpcCompanyMatchCandidateDto
            {
                CompanyId        = id,
                Name             = name,
                JaroWinklerScore = score,
                MatchKind        = WwpcCompanyMatchKind.Fuzzy
            });
        }

        result.Matches = result.Matches.OrderByDescending(m => m.MatchKind == WwpcCompanyMatchKind.ExactNormalized)
                               .ThenByDescending(m => m.JaroWinklerScore)
                               .ToList();
        return result;
    }

    // ---------- Accept ----------

    public async Task<AcceptWwpcImportResultDto> AcceptAsync(long wwpcId, AcceptWwpcImportDto dto, string adminUserId)
    {
        WwpcSoftware staging = await _db.WwpcSoftwares
                                        .Include(s => s.Versions)
                                        .Include(s => s.Screenshots)
                                        .FirstOrDefaultAsync(s => s.Id == wwpcId);
        if(staging == null)
            return Fail("Staging row not found.");
        if(staging.Status != WwpcSoftwareStatus.ReadyForReview &&
           staging.Status != WwpcSoftwareStatus.Skipped)
            return Fail($"Cannot accept row in status {staging.Status}.");

        int? vendorCompanyId = dto.VendorCompanyId ?? staging.SuggestedVendorCompanyId;
        Company developer    = vendorCompanyId.HasValue
                                   ? await _db.Companies.FindAsync(vendorCompanyId.Value)
                                   : null;
        // Vendor is OPTIONAL — Software requires no publisher (unlike OldDos which builds Releases).

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            int insertedVersions     = 0;
            int insertedScreenshots  = 0;
            int insertedDescriptions = 0;
            int insertedGenres       = 0;

            ulong targetSoftwareId;
            if(dto.Mode == WwpcAcceptMode.MergeIntoExisting)
            {
                if(dto.TargetSoftwareId is not { } tid)
                    return Fail("TargetSoftwareId required for merge mode.");
                Software target = await _db.Softwares.FindAsync(tid);
                if(target == null) return Fail("Target software not found.");
                targetSoftwareId = target.Id;
            }
            else
            {
                var fresh = new Software
                {
                    Name = string.IsNullOrWhiteSpace(dto.NameOverride) ? staging.Name : dto.NameOverride.Trim(),
                    Kind = dto.KindOverride.HasValue ? (SoftwareKind)dto.KindOverride.Value : SoftwareKind.Application
                };
                _db.Softwares.Add(fresh);
                await _db.SaveChangesAsync();
                targetSoftwareId = fresh.Id;
            }

            // ---- Company → Software link ----
            if(developer != null)
            {
                bool alreadyLinked = await _db.SoftwareCompanyRoles
                                              .AnyAsync(r => r.SoftwareId == targetSoftwareId &&
                                                             r.CompanyId  == developer.Id &&
                                                             r.RoleId     == DEV_ROLE_ID);
                if(!alreadyLinked)
                    _db.SoftwareCompanyRoles.Add(new SoftwareCompanyRole
                    {
                        SoftwareId = targetSoftwareId,
                        CompanyId  = developer.Id,
                        RoleId     = DEV_ROLE_ID
                    });
            }

            // ---- Versions ----
            // Map staging WwpcVersion.Id → real SoftwareVersion.Id so screenshots can be linked.
            var versionMap = new Dictionary<long, ulong>();
            foreach(AcceptWwpcVersionDecisionDto vd in dto.Versions)
            {
                if(!vd.Include) continue;
                WwpcVersion staged = staging.Versions.FirstOrDefault(v => v.Id == vd.WwpcVersionId);
                if(staged == null) continue;

                ulong versionId;
                if(vd.LinkToExistingVersionId.HasValue)
                {
                    bool exists = await _db.SoftwareVersions.AnyAsync(v => v.Id == vd.LinkToExistingVersionId.Value &&
                                                                           v.SoftwareId == targetSoftwareId);
                    if(!exists) continue;
                    versionId                       = vd.LinkToExistingVersionId.Value;
                    staged.PromotedSoftwareVersionId = versionId;
                }
                else
                {
                    string finalVersionString = !string.IsNullOrWhiteSpace(vd.VersionStringOverride)
                                                    ? vd.VersionStringOverride.Trim()
                                                    : string.IsNullOrWhiteSpace(staged.MajorRelease)
                                                          ? staged.VersionString
                                                          : $"{staged.MajorRelease} / {staged.VersionString}";

                    var version = new SoftwareVersion
                    {
                        SoftwareId    = targetSoftwareId,
                        VersionString = finalVersionString
                    };
                    _db.SoftwareVersions.Add(version);
                    await _db.SaveChangesAsync();
                    versionId                        = version.Id;
                    staged.PromotedSoftwareVersionId = versionId;
                    insertedVersions++;

                    if(developer != null)
                        _db.CompaniesBySoftwareVersions.Add(new CompanyBySoftwareVersion
                        {
                            CompanyId         = developer.Id,
                            SoftwareVersionId = versionId,
                            RoleId            = DEV_ROLE_ID
                        });
                }
                versionMap[staged.Id] = versionId;
            }
            await _db.SaveChangesAsync();

            // ---- Screenshots ----
            string assetRootPath = _config["AssetRootPath"];
            if(!string.IsNullOrEmpty(assetRootPath))
                Photos.EnsureCreated(assetRootPath, false, "software-screenshots");
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            http.DefaultRequestHeaders.Add("User-Agent", "Marechai-WinWorld-Importer/1.0");

            foreach(AcceptWwpcScreenshotDecisionDto sd in dto.Screenshots)
            {
                if(!sd.Include) continue;
                WwpcScreenshot staged = staging.Screenshots.FirstOrDefault(x => x.Id == sd.WwpcScreenshotId);
                if(staged == null || string.IsNullOrEmpty(staged.ImageUrl)) continue;
                if(string.IsNullOrEmpty(assetRootPath))
                {
                    _log.LogWarning("WwpcAccept #{Id}: AssetRootPath not configured; skipping screenshot {ShotId}.",
                                    wwpcId, staged.Id);
                    continue;
                }

                try
                {
                    string url       = staged.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                           ? staged.ImageUrl
                                           : "https://winworldpc.com" + (staged.ImageUrl.StartsWith("/")
                                                                            ? staged.ImageUrl
                                                                            : "/" + staged.ImageUrl);
                    byte[] bytes     = await http.GetByteArrayAsync(url);
                    string extension = (Path.GetExtension(staged.ImageUrl) ?? ".png").ToLowerInvariant();
                    if(string.IsNullOrEmpty(extension) || extension == ".") extension = ".png";

                    var shot = new SoftwareScreenshot
                    {
                        Id                 = Guid.NewGuid(),
                        SoftwareId         = targetSoftwareId,
                        SoftwarePlatformId = sd.SoftwarePlatformId ?? staged.SuggestedSoftwarePlatformId,
                        SoftwareVersionId  = sd.SoftwareVersionId
                                          ?? (sd.WwpcVersionId.HasValue && versionMap.TryGetValue(sd.WwpcVersionId.Value, out ulong vId) ? vId : (ulong?)null),
                        Caption            = string.IsNullOrWhiteSpace(sd.CaptionOverride) ? staged.Caption : sd.CaptionOverride,
                        OriginalExtension  = extension.TrimStart('.')
                    };

                    string originalsDir = Path.Combine(assetRootPath, "photos", "software-screenshots", "originals");
                    string originalPath = Path.Combine(originalsDir, $"{shot.Id}{extension}");
                    await File.WriteAllBytesAsync(originalPath, bytes);

                    _db.SoftwareScreenshots.Add(shot);
                    staged.PromotedSoftwareScreenshotId = shot.Id;

                    string sourceFormat = extension.TrimStart('.');
                    Guid   shotId       = shot.Id;
                    _ = Task.Run(() =>
                    {
                        var photos = new Photos();
                        photos.ConversionWorker(assetRootPath, shotId, originalPath, sourceFormat, false,
                                                "software-screenshots");
                    });
                    insertedScreenshots++;
                }
                catch(Exception ex)
                {
                    _log.LogWarning(ex, "WwpcAccept #{Id}: screenshot {ShotId} download failed.", wwpcId, staged.Id);
                }
            }
            await _db.SaveChangesAsync();

            // ---- Descriptions ----
            // Only the English museum-grade source row is inserted here. The 5-locale fan-out is
            // owned by DescriptionTranslationWorker (SoftwareDescriptionSource) which picks up any
            // SoftwareDescription that's missing siblings in other languages and translates them
            // in the background. Inline NLLB calls used to make AcceptAsync hit Apache's
            // ProxyTimeout and surface as a 502 → masked 404 in the dialog.
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

            // ---- Genres ----
            HashSet<int> existingGenres = new(await _db.GenresBySoftware
                                                       .Where(g => g.SoftwareId == targetSoftwareId)
                                                       .Select(g => g.GenreId)
                                                       .ToListAsync());
            foreach(int gid in (dto.GenreIds ?? new List<int>()).Distinct())
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

            staging.Status             = WwpcSoftwareStatus.Accepted;
            staging.PromotedSoftwareId = targetSoftwareId;
            staging.ReviewedBy         = adminUserId;
            staging.ReviewedOn         = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return new AcceptWwpcImportResultDto
            {
                Success                  = true,
                PromotedSoftwareId       = targetSoftwareId,
                InsertedVersionCount     = insertedVersions,
                InsertedScreenshotCount  = insertedScreenshots,
                InsertedDescriptionCount = insertedDescriptions,
                InsertedGenreCount       = insertedGenres
            };
        }
        catch(Exception ex)
        {
            await tx.RollbackAsync();
            _log.LogError(ex, "Wwpc accept #{Id} failed.", wwpcId);
            return Fail(ex.Message);
        }
    }

    public async Task<bool> SkipAsync(long id, string adminUserId)
    {
        WwpcSoftware row = await _db.WwpcSoftwares.FindAsync(id);
        if(row == null) return false;
        if(row.Status is WwpcSoftwareStatus.Accepted or WwpcSoftwareStatus.Discarded) return false;
        row.Status     = WwpcSoftwareStatus.Skipped;
        row.ReviewedBy = adminUserId;
        row.ReviewedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DiscardAsync(long id, string adminUserId)
    {
        WwpcSoftware row = await _db.WwpcSoftwares.FindAsync(id);
        if(row == null) return false;
        if(row.Status == WwpcSoftwareStatus.Accepted) return false;
        row.Status     = WwpcSoftwareStatus.Discarded;
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

    static AcceptWwpcImportResultDto Fail(string error) =>
        new() { Success = false, Error = error };

    sealed class NameMatchProjection
    {
        public ulong        Id           { get; set; }
        public string       Name         { get; set; }
        public SoftwareKind Kind         { get; set; }
        public int?         Year         { get; set; }
        public int          ReleaseCount { get; set; }
    }
}
