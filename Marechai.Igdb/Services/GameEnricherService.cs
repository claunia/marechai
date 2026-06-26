using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Markdig;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Igdb.Services;

/// <summary>Fills in the richer per-game fields IGDB exposes (age ratings, franchise, involved companies,
/// screenshots, slug, summary, videos, release dates, similar games) for <see cref="Software" /> entries already
/// matched to an <see cref="IgdbGame" />. Resumable and idempotent like <see cref="CompanyEnricherService" /> /
/// <see cref="PlatformEnricherService" />: each game is marked <see cref="IgdbGame.EnrichmentApplied" /> once
/// processed and skipped on subsequent runs.</summary>
public class GameEnricherService
{
    const int GameBatchSize = 50;
    const int IgdbMaxPageSize = 500;

    static readonly Dictionary<string, string> InvolvedCompanyRoleMap = new()
    {
        ["developer"] = "dev",
        ["publisher"] = "pub",
        ["porting"]   = "por",
        ["supporting"] = "ctb"
    };

    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly IgdbHttpClient                     _client;
    readonly HttpClient                         _httpClient;
    readonly string                              _assetRootPath;

    public GameEnricherService(IDbContextFactory<MarechaiContext> contextFactory, IgdbHttpClient client,
                                 string assetRootPath)
    {
        _contextFactory = contextFactory;
        _client         = client;
        _assetRootPath  = assetRootPath;
        _httpClient     = new HttpClient();
    }

    public class Stats
    {
        public int GamesProcessed;
        public int GamesFailed;
        public int FamiliesCreated;
        public int FamiliesLinked;
        public int CompanyRolesAdded;
        public int DescriptionsAdded;
        public int VideosAdded;
        public int ReleasesAdded;
        public int PlatformsAutoCreated;
        public int RatingsAdded;
        public int ScreenshotsCached;
        public int SimilarLinksAdded;
    }

    public async Task<Stats> RunAsync(bool dryRun)
    {
        var stats = new Stats();

        List<long> pendingIgdbIds;

        await using(var probe = await _contextFactory.CreateDbContextAsync())
        {
            pendingIgdbIds = await probe.IgdbGames
                                         .Where(g => g.MatchStatus == IgdbMatchStatus.Matched &&
                                                     g.SoftwareId != null && !g.EnrichmentApplied)
                                         .OrderBy(g => g.IgdbId)
                                         .Select(g => g.IgdbId)
                                         .ToListAsync();
        }

        if(pendingIgdbIds.Count == 0)
        {
            Console.WriteLine("  No matched games pending enrichment.");

            return stats;
        }

        Dictionary<int, string> orgNames    = await FetchLookupAsync("age_rating_organizations", "name");
        Dictionary<int, string> ratingTexts = await FetchLookupAsync("age_rating_categories", "rating");

        for(int offset = 0; offset < pendingIgdbIds.Count; offset += GameBatchSize)
        {
            List<long> batchIds = pendingIgdbIds.Skip(offset).Take(GameBatchSize).ToList();
            await ProcessBatchAsync(batchIds, orgNames, ratingTexts, dryRun, stats);
        }

        return stats;
    }

    async Task ProcessBatchAsync(List<long> batchIds, Dictionary<int, string> orgNames,
                                   Dictionary<int, string> ratingTexts, bool dryRun, Stats stats)
    {
        List<JsonElement> games = await FetchByFieldAsync("games",
            "id,slug,summary,age_ratings,franchises,screenshots,videos,release_dates,similar_games", "id",
            batchIds);

        var gamesById = games.ToDictionary(g => g.GetProperty("id").GetInt64());

        var allScreenshotIds  = new List<long>();
        var allVideoIds       = new List<long>();
        var allReleaseIds     = new List<long>();
        var allAgeRatingIds   = new List<long>();
        var allFranchiseIds   = new List<long>();

        foreach(JsonElement g in games)
        {
            allScreenshotIds.AddRange(GetLongArray(g, "screenshots"));
            allVideoIds.AddRange(GetLongArray(g, "videos"));
            allReleaseIds.AddRange(GetLongArray(g, "release_dates"));
            allAgeRatingIds.AddRange(GetLongArray(g, "age_ratings"));
            allFranchiseIds.AddRange(GetLongArray(g, "franchises"));
        }

        List<JsonElement> involvedCompanies = await FetchByFieldAsync("involved_companies",
            "id,game,company,developer,publisher,porting,supporting", "game", batchIds);

        List<JsonElement> screenshots = await FetchByFieldAsync("screenshots", "id,image_id", "id",
            allScreenshotIds);

        List<JsonElement> videos = await FetchByFieldAsync("game_videos", "id,video_id,name", "id", allVideoIds);

        List<JsonElement> releaseDates = await FetchByFieldAsync("release_dates", "id,date,platform,date_format",
            "id", allReleaseIds);

        List<JsonElement> ageRatings = await FetchByFieldAsync("age_ratings", "id,organization,rating_category",
            "id", allAgeRatingIds);

        List<JsonElement> franchises = await FetchByFieldAsync("franchises", "id,name", "id", allFranchiseIds);

        var involvedByGame   = involvedCompanies.GroupBy(e => e.GetProperty("game").GetInt64())
                                                  .ToDictionary(gr => gr.Key, gr => gr.ToList());
        var screenshotsById  = screenshots.ToDictionary(e => e.GetProperty("id").GetInt64());
        var videosById       = videos.ToDictionary(e => e.GetProperty("id").GetInt64());
        var releasesById     = releaseDates.ToDictionary(e => e.GetProperty("id").GetInt64());
        var ageRatingsById   = ageRatings.ToDictionary(e => e.GetProperty("id").GetInt64());
        var franchisesById   = franchises.ToDictionary(e => e.GetProperty("id").GetInt64());

        foreach(long igdbId in batchIds)
        {
            if(!gamesById.TryGetValue(igdbId, out JsonElement game))
                continue;

            try
            {
                await ProcessGameAsync(igdbId, game, involvedByGame, screenshotsById, videosById, releasesById,
                                         ageRatingsById, franchisesById, orgNames, ratingTexts, dryRun, stats);
            }
            catch(Exception ex)
            {
                stats.GamesFailed++;
                Console.WriteLine($"  [{igdbId}] ERROR: {ex.Message}");

                if(!dryRun)
                {
                    await using var errContext = await _contextFactory.CreateDbContextAsync();
                    IgdbGame errGame = await errContext.IgdbGames.FirstOrDefaultAsync(x => x.IgdbId == igdbId);

                    if(errGame != null)
                    {
                        errGame.ErrorMessage = ex.Message.Length > 1024 ? ex.Message[..1024] : ex.Message;
                        await errContext.SaveChangesAsync();
                    }
                }
            }
        }
    }

    async Task ProcessGameAsync(long igdbId, JsonElement game, Dictionary<long, List<JsonElement>> involvedByGame,
                                  Dictionary<long, JsonElement> screenshotsById,
                                  Dictionary<long, JsonElement> videosById, Dictionary<long, JsonElement> releasesById,
                                  Dictionary<long, JsonElement> ageRatingsById,
                                  Dictionary<long, JsonElement> franchisesById, Dictionary<int, string> orgNames,
                                  Dictionary<int, string> ratingTexts, bool dryRun, Stats stats)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        IgdbGame igdbGame = await context.IgdbGames.FirstOrDefaultAsync(g => g.IgdbId == igdbId);

        if(igdbGame?.SoftwareId == null)
            return;

        Software software = await context.Softwares.Include(s => s.Descriptions).Include(s => s.Videos)
                                            .Include(s => s.CompanyRoles)
                                            .Include(s => s.DirectReleases).ThenInclude(r => r.Attributes)
                                            .FirstOrDefaultAsync(s => s.Id == igdbGame.SoftwareId);

        if(software == null)
        {
            Console.WriteLine($"  [{igdbId}] No local software {igdbGame.SoftwareId} found, skipping.");

            return;
        }

        stats.GamesProcessed++;

        // -- Franchise -> SoftwareFamily --------------------------------------------------------
        List<long> franchiseIds = GetLongArray(game, "franchises");

        if(franchiseIds.Count > 0 && software.FamilyId == null)
        {
            if(franchiseIds.Count > 1)
                Console.WriteLine(
                    $"  [{igdbId}] Game has {franchiseIds.Count} franchises, using only the first.");

            if(franchisesById.TryGetValue(franchiseIds[0], out JsonElement franchise) &&
               franchise.TryGetProperty("name", out JsonElement franchiseNameEl) &&
               franchiseNameEl.ValueKind == JsonValueKind.String)
            {
                string franchiseName = franchiseNameEl.GetString();

                SoftwareFamily family = await context.SoftwareFamilies
                                                       .FirstOrDefaultAsync(f => f.Name == franchiseName);

                if(family == null)
                {
                    family = new SoftwareFamily { Name = franchiseName };
                    context.SoftwareFamilies.Add(family);

                    if(!dryRun)
                        await context.SaveChangesAsync();

                    stats.FamiliesCreated++;
                    Console.WriteLine($"  [{igdbId}] Created software family \"{franchiseName}\".");
                }

                software.FamilyId = family.Id;
                stats.FamiliesLinked++;
                Console.WriteLine($"  [{igdbId}] Linked software family \"{franchiseName}\".");
            }
        }

        // -- Involved companies -> SoftwareCompanyRole -------------------------------------------
        var existingRoles = new HashSet<(int CompanyId, string RoleId)>(software.CompanyRoles
            .Select(r => (r.CompanyId, r.RoleId)));

        if(involvedByGame.TryGetValue(igdbId, out List<JsonElement> involved))
        {
            foreach(JsonElement ic in involved)
            {
                long companyIgdbId = ic.GetProperty("company").GetInt64();

                IgdbCompany igdbCompany = await context.IgdbCompanies
                                                         .FirstOrDefaultAsync(c => c.IgdbId == companyIgdbId &&
                                                             c.MatchStatus == IgdbMatchStatus.Matched &&
                                                             c.CompanyId != null);

                if(igdbCompany == null)
                {
                    IgdbCompany unmatched =
                        await context.IgdbCompanies.FirstOrDefaultAsync(c => c.IgdbId == companyIgdbId);

                    Console.WriteLine(
                        $"  [{igdbId}] Involved company \"{unmatched?.Name ?? "unknown"}\" (igdb id {companyIgdbId}) has no local match, skipping.");

                    continue;
                }

                int companyId = igdbCompany.CompanyId.Value;

                bool anyRoleToAdd = InvolvedCompanyRoleMap.Any(kv =>
                    ic.TryGetProperty(kv.Key, out JsonElement f) && f.ValueKind == JsonValueKind.True &&
                    !existingRoles.Contains((companyId, kv.Value)));

                string companyName = anyRoleToAdd
                                          ? (await context.Companies.FirstOrDefaultAsync(c => c.Id == companyId))
                                            ?.Name ?? companyId.ToString()
                                          : null;

                foreach((string flag, string roleId) in InvolvedCompanyRoleMap)
                {
                    if(!ic.TryGetProperty(flag, out JsonElement flagEl) || flagEl.ValueKind != JsonValueKind.True)
                        continue;

                    if(existingRoles.Contains((companyId, roleId)))
                        continue;

                    context.SoftwareCompanyRoles.Add(new SoftwareCompanyRole
                    {
                        SoftwareId = software.Id, CompanyId = companyId, RoleId = roleId
                    });

                    existingRoles.Add((companyId, roleId));
                    stats.CompanyRolesAdded++;
                    Console.WriteLine($"  [{igdbId}] Added company role {roleId} for company \"{companyName}\" (id {companyId}).");
                }
            }
        }

        // -- Screenshots (ids cached for a later download phase) --------------------------------
        List<long> screenshotIds = GetLongArray(game, "screenshots");

        if(screenshotIds.Count > 0 && string.IsNullOrWhiteSpace(igdbGame.ScreenshotImageIdsJson))
        {
            List<string> imageIds = screenshotIds.Select(id => screenshotsById.TryGetValue(id, out JsonElement s) &&
                                                                  s.TryGetProperty("image_id", out JsonElement iid) &&
                                                                  iid.ValueKind == JsonValueKind.String
                                                                      ? iid.GetString()
                                                                      : null).Where(s => s != null).ToList();

            if(imageIds.Count > 0)
            {
                igdbGame.ScreenshotImageIdsJson = JsonSerializer.Serialize(imageIds);
                stats.ScreenshotsCached += imageIds.Count;
                Console.WriteLine($"  [{igdbId}] Cached {imageIds.Count} screenshot image ids.");
            }
        }

        // -- Description from summary ------------------------------------------------------------
        if(game.TryGetProperty("summary", out JsonElement summaryEl) &&
           summaryEl.ValueKind == JsonValueKind.String &&
           !string.IsNullOrWhiteSpace(summaryEl.GetString()) &&
           !software.Descriptions.Any(d => d.LanguageCode == "eng"))
        {
            string markdown = summaryEl.GetString().Replace("\r\n", "\n").Replace("\n", "\n\n");

            software.Descriptions.Add(new SoftwareDescription
            {
                LanguageCode = "eng",
                Text         = markdown,
                Html = Markdown.ToHtml(markdown, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build())
            });

            stats.DescriptionsAdded++;
            Console.WriteLine($"  [{igdbId}] Description added.");
        }

        // -- Videos --------------------------------------------------------------------------------
        var existingVideos = new HashSet<string>(software.Videos.Where(v => v.Provider == "YouTube")
                                                          .Select(v => v.VideoId));

        foreach(long videoId in GetLongArray(game, "videos"))
        {
            if(!videosById.TryGetValue(videoId, out JsonElement v))
                continue;

            if(!v.TryGetProperty("video_id", out JsonElement vidEl) || vidEl.ValueKind != JsonValueKind.String)
                continue;

            string ytId = vidEl.GetString();

            if(existingVideos.Contains(ytId))
                continue;

            string title = v.TryGetProperty("name", out JsonElement nameEl) && nameEl.ValueKind == JsonValueKind.String
                                ? nameEl.GetString()
                                : null;

            software.Videos.Add(new SoftwareVideo { Provider = "YouTube", VideoId = ytId, Title = title });
            existingVideos.Add(ytId);
            stats.VideosAdded++;
            Console.WriteLine($"  [{igdbId}] Video added: {ytId}.");
        }

        // -- Releases from release_dates -----------------------------------------------------------
        int? firstPublisherCompanyId = null;

        if(involvedByGame.TryGetValue(igdbId, out List<JsonElement> involvedForPublisher))
            foreach(JsonElement ic in involvedForPublisher)
                if(ic.TryGetProperty("publisher", out JsonElement pubEl) && pubEl.ValueKind == JsonValueKind.True)
                {
                    long companyIgdbId = ic.GetProperty("company").GetInt64();

                    IgdbCompany pubCompany = await context.IgdbCompanies
                                                            .FirstOrDefaultAsync(c => c.IgdbId == companyIgdbId &&
                                                                c.MatchStatus == IgdbMatchStatus.Matched &&
                                                                c.CompanyId != null);

                    if(pubCompany != null)
                    {
                        firstPublisherCompanyId = pubCompany.CompanyId;

                        break;
                    }
                }

        var existingReleases = new HashSet<(ulong? PlatformId, DateTime Date)>(software.DirectReleases
            .Where(r => r.ReleaseDate != null).Select(r => (r.PlatformId, r.ReleaseDate.Value.Date)));

        foreach(long releaseDateId in GetLongArray(game, "release_dates"))
        {
            if(!releasesById.TryGetValue(releaseDateId, out JsonElement rd))
                continue;

            if(!rd.TryGetProperty("date", out JsonElement dateEl) || dateEl.ValueKind != JsonValueKind.Number)
                continue;

            if(!rd.TryGetProperty("platform", out JsonElement platformEl) ||
               platformEl.ValueKind != JsonValueKind.Number)
                continue;

            long platformIgdbId = platformEl.GetInt64();

            IgdbPlatform igdbPlatform =
                await context.IgdbPlatforms.FirstOrDefaultAsync(p => p.Id == (int) platformIgdbId);

            if(igdbPlatform == null)
            {
                igdbPlatform = await AutoCreatePlatformAsync(context, platformIgdbId, dryRun, stats, igdbId);

                if(igdbPlatform == null)
                    continue;
            }
            else if(igdbPlatform.SoftwarePlatformId == null)
            {
                Console.WriteLine(
                    $"  [{igdbId}] Platform {platformIgdbId} ({igdbPlatform.Name}) is mirrored but not matched, skipping release.");

                continue;
            }

            if(firstPublisherCompanyId == null)
            {
                Console.WriteLine(
                    $"  [{igdbId}] No resolved publisher for release on platform \"{igdbPlatform.Name}\" (id {platformIgdbId}), skipping.");

                continue;
            }

            DateTime releaseDate = DateTimeOffset.FromUnixTimeSeconds(dateEl.GetInt64()).UtcDateTime;

            int? dateFormat = rd.TryGetProperty("date_format", out JsonElement dfEl) &&
                               dfEl.ValueKind == JsonValueKind.Number
                                   ? dfEl.GetInt32()
                                   : null;

            DatePrecision precision = ToDatePrecision(dateFormat);

            ulong? localPlatformId = igdbPlatform.SoftwarePlatformId;

            if(existingReleases.Contains((localPlatformId, releaseDate.Date)))
                continue;

            var newRelease = new SoftwareRelease
            {
                SoftwareId           = software.Id,
                PlatformId           = localPlatformId,
                PublisherId          = firstPublisherCompanyId.Value,
                ReleaseDate          = releaseDate,
                ReleaseDatePrecision = precision,
                Attributes           = []
            };

            software.DirectReleases.Add(newRelease);
            existingReleases.Add((localPlatformId, releaseDate.Date));
            stats.ReleasesAdded++;
            Console.WriteLine(
                $"  [{igdbId}] Release added: platform \"{igdbPlatform.Name}\" (id {localPlatformId}), date {releaseDate:yyyy-MM-dd}.");
        }

        // -- Age ratings (after releases exist) ----------------------------------------------------
        var ratingsToApply = new List<(string Key, string Value)>();

        foreach(long ageRatingId in GetLongArray(game, "age_ratings"))
        {
            if(!ageRatingsById.TryGetValue(ageRatingId, out JsonElement ar))
                continue;

            if(!ar.TryGetProperty("organization", out JsonElement orgEl) || orgEl.ValueKind != JsonValueKind.Number)
                continue;

            if(!ar.TryGetProperty("rating_category", out JsonElement catEl) ||
               catEl.ValueKind != JsonValueKind.Number)
                continue;

            if(!orgNames.TryGetValue(orgEl.GetInt32(), out string orgName))
            {
                Console.WriteLine($"  [{igdbId}] Unknown age rating organization {orgEl.GetInt32()}, skipping.");

                continue;
            }

            if(!ratingTexts.TryGetValue(catEl.GetInt32(), out string ratingText))
            {
                Console.WriteLine($"  [{igdbId}] Unknown age rating category {catEl.GetInt32()}, skipping.");

                continue;
            }

            ratingsToApply.Add((orgName, ratingText));
        }

        if(ratingsToApply.Count > 0)
            foreach(SoftwareRelease release in software.DirectReleases)
            {
                var existingAttrs = new HashSet<string>(release.Attributes.Where(a => a.Category == "rating")
                                                                  .Select(a => a.Key));

                foreach((string key, string value) in ratingsToApply)
                {
                    if(existingAttrs.Contains(key))
                        continue;

                    release.Attributes.Add(new SoftwareAttribute { Category = "rating", Key = key, Value = value });
                    existingAttrs.Add(key);
                    stats.RatingsAdded++;

                    string releaseLabel = release.Id == 0
                                               ? $"new release on platform {release.PlatformId}, {release.ReleaseDate:yyyy-MM-dd}"
                                               : $"release {release.Id}";

                    Console.WriteLine($"  [{igdbId}] Rating added to {releaseLabel}: {key} = {value}.");
                }
            }

        // -- Similar games -> SoftwareSimilarTo ----------------------------------------------------
        List<long> similarGameIds = GetLongArray(game, "similar_games");

        if(similarGameIds.Count > 0)
        {
            List<ulong> similarSoftwareIds = await context.IgdbGames
                                                            .Where(g => similarGameIds.Contains(g.IgdbId) &&
                                                                        g.MatchStatus == IgdbMatchStatus.Matched &&
                                                                        g.SoftwareId != null)
                                                            .Select(g => g.SoftwareId.Value).ToListAsync();

            var existingPairs = new HashSet<(ulong, ulong)>();

            List<SoftwareSimilarTo> existingLinks = await context.SoftwareSimilarTo
                                                                   .Where(s => s.SoftwareId == software.Id ||
                                                                       s.SimilarSoftwareId == software.Id)
                                                                   .ToListAsync();

            foreach(SoftwareSimilarTo link in existingLinks)
                existingPairs.Add((link.SoftwareId, link.SimilarSoftwareId));

            foreach(ulong otherId in similarSoftwareIds.Distinct())
            {
                if(otherId == software.Id)
                    continue;

                ulong lo = Math.Min(software.Id, otherId);
                ulong hi = Math.Max(software.Id, otherId);

                if(existingPairs.Contains((lo, hi)))
                    continue;

                context.SoftwareSimilarTo.Add(new SoftwareSimilarTo { SoftwareId = lo, SimilarSoftwareId = hi });
                existingPairs.Add((lo, hi));
                stats.SimilarLinksAdded++;
                Console.WriteLine($"  [{igdbId}] Similar software link added: {lo} <-> {hi}.");
            }
        }

        if(!dryRun)
        {
            igdbGame.EnrichmentApplied = true;
            igdbGame.EnrichedOn        = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    async Task<IgdbPlatform> AutoCreatePlatformAsync(MarechaiContext context, long platformIgdbId, bool dryRun,
                                                        Stats stats, long gameIgdbId)
    {
        string query = new ApicalypseQueryBuilder().Fields("id,name,platform_logo.image_id")
                                                     .Where($"id = {platformIgdbId}").Build();

        using JsonDocument doc = await _client.QueryAsync("platforms", query);

        if(doc.RootElement.GetArrayLength() == 0)
        {
            Console.WriteLine($"  [{gameIgdbId}] IGDB platform {platformIgdbId} could not be fetched, skipping release.");

            return null;
        }

        JsonElement element = doc.RootElement[0];
        string      name    = element.GetProperty("name").GetString();

        string logoImageId = element.TryGetProperty("platform_logo", out JsonElement logo) &&
                              logo.TryGetProperty("image_id", out JsonElement imageId)
                                  ? imageId.GetString()
                                  : null;

        var softwarePlatform = new SoftwarePlatform { Name = name };
        context.SoftwarePlatforms.Add(softwarePlatform);

        if(!dryRun)
            await context.SaveChangesAsync();

        var igdbPlatform = new IgdbPlatform
        {
            Id                 = (int) platformIgdbId,
            Name               = name,
            MatchStatus        = IgdbMatchStatus.Matched,
            MatchType          = "igdb-game-enrich-autocreate",
            MatchedOn          = DateTime.UtcNow,
            SoftwarePlatformId = softwarePlatform.Id,
            LogoImageId        = logoImageId
        };

        context.IgdbPlatforms.Add(igdbPlatform);
        stats.PlatformsAutoCreated++;
        Console.WriteLine($"  [{gameIgdbId}] Auto-created platform \"{name}\" (igdb id {platformIgdbId}).");

        if(!string.IsNullOrWhiteSpace(logoImageId) && !dryRun)
        {
            await DownloadAndApplyLogoAsync(platformIgdbId, softwarePlatform, logoImageId);
            igdbPlatform.EnrichmentApplied = true;
            igdbPlatform.EnrichedOn        = DateTime.UtcNow;
        }

        if(!dryRun)
            await context.SaveChangesAsync();

        return igdbPlatform;
    }

    async Task DownloadAndApplyLogoAsync(long igdbId, SoftwarePlatform platform, string imageId)
    {
        byte[] bytes = await IgdbImageDownloader.DownloadBestAsync(_httpClient, imageId,
                                                                     IgdbImageDownloader.LogoSizeCandidates);

        if(bytes == null)
        {
            Console.WriteLine($"  [{igdbId}] Failed to download logo: no candidate size succeeded.");

            return;
        }

        var guid = Guid.NewGuid();

        string itemPhotosRoot         = Path.Combine(_assetRootPath, "photos", "platform-logos");
        string itemThumbsRoot         = Path.Combine(itemPhotosRoot, "thumbs");
        string itemOriginalPhotosRoot = Path.Combine(itemPhotosRoot, "originals");

        foreach(string dir in new[]
                {
                    itemPhotosRoot, itemThumbsRoot, itemOriginalPhotosRoot,
                    Path.Combine(itemThumbsRoot, "jpeg", "4k"),
                    Path.Combine(itemPhotosRoot, "jpeg", "4k"),
                    Path.Combine(itemThumbsRoot, "webp", "4k"),
                    Path.Combine(itemPhotosRoot, "webp", "4k"),
                    Path.Combine(itemThumbsRoot, "avif", "4k"),
                    Path.Combine(itemPhotosRoot, "avif", "4k")
                })
            Directory.CreateDirectory(dir);

        string originalPath = Path.Combine(itemOriginalPhotosRoot, $"{guid}.jpg");
        await File.WriteAllBytesAsync(originalPath, bytes);

        foreach((string format, string outExt) in new[]
                {
                    ("jpeg", "jpg"), ("webp", "webp"), ("avif", "avif")
                })
        {
            string fullPath  = Path.Combine(itemPhotosRoot, format, "4k", $"{guid}.{outExt}");
            string thumbPath = Path.Combine(itemThumbsRoot, format, "4k", $"{guid}.{outExt}");

            ConvertUsingImageMagick(originalPath, fullPath,  256, 256);
            ConvertUsingImageMagick(originalPath, thumbPath, 64,  64);
        }

        platform.LogoId        = guid;
        platform.LogoExtension = "jpg";

        Console.WriteLine($"  [{igdbId}] Logo downloaded and converted for auto-created platform {platform.Id}.");
    }

    // Mirrors PlatformEnricherService.ConvertUsingImageMagick.
    static void ConvertUsingImageMagick(string originalPath, string outputPath, int width, int height)
    {
        try
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName               = "magick",
                    CreateNoWindow         = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true
                }
            };

            p.StartInfo.ArgumentList.Add("-limit");
            p.StartInfo.ArgumentList.Add("thread");
            p.StartInfo.ArgumentList.Add("1");
            p.StartInfo.ArgumentList.Add($"{originalPath}[0]");
            p.StartInfo.ArgumentList.Add("-resize");
            p.StartInfo.ArgumentList.Add($"{width}x{height}>");
            p.StartInfo.ArgumentList.Add("-strip");
            p.StartInfo.ArgumentList.Add("-quality");
            p.StartInfo.ArgumentList.Add("75");
            p.StartInfo.ArgumentList.Add(outputPath);
            p.StartInfo.Environment["MAGICK_THREAD_LIMIT"] = "1";

            p.Start();

            Task<string> stderrTask = p.StandardError.ReadToEndAsync();
            Task<string> stdoutTask = p.StandardOutput.ReadToEndAsync();
            p.WaitForExit();
            _ = stderrTask.GetAwaiter().GetResult();
            _ = stdoutTask.GetAwaiter().GetResult();

            if(p.ExitCode != 0)
                Console.Error.WriteLine($"convert failed for {outputPath}: exit {p.ExitCode}");
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"convert spawn failed for {outputPath}: {ex.Message}");
        }
    }

    async Task<Dictionary<int, string>> FetchLookupAsync(string endpoint, string field)
    {
        var result = new Dictionary<int, string>();

        string query = new ApicalypseQueryBuilder().Fields($"id,{field}").Limit(IgdbMaxPageSize).Build();

        using JsonDocument doc = await _client.QueryAsync(endpoint, query);

        foreach(JsonElement el in doc.RootElement.EnumerateArray())
        {
            int id = el.GetProperty("id").GetInt32();

            if(el.TryGetProperty(field, out JsonElement v) && v.ValueKind == JsonValueKind.String)
                result[id] = v.GetString();
        }

        return result;
    }

    async Task<List<JsonElement>> FetchByFieldAsync(string endpoint, string fields, string filterField,
                                                       IEnumerable<long> ids)
    {
        List<long> distinctIds = ids.Distinct().ToList();
        var        result      = new List<JsonElement>();

        if(distinctIds.Count == 0)
            return result;

        foreach(List<long> chunk in Chunk(distinctIds, IgdbMaxPageSize))
        {
            string query = new ApicalypseQueryBuilder().Fields(fields)
                                                         .Where($"{filterField} = ({string.Join(",", chunk)})")
                                                         .Limit(IgdbMaxPageSize).Build();

            using JsonDocument doc = await _client.QueryAsync(endpoint, query);

            foreach(JsonElement el in doc.RootElement.EnumerateArray())
                result.Add(el.Clone());
        }

        return result;
    }

    static IEnumerable<List<long>> Chunk(List<long> source, int size)
    {
        for(int i = 0; i < source.Count; i += size)
            yield return source.GetRange(i, Math.Min(size, source.Count - i));
    }

    static List<long> GetLongArray(JsonElement element, string property) =>
        element.TryGetProperty(property, out JsonElement arr) && arr.ValueKind == JsonValueKind.Array
            ? arr.EnumerateArray().Select(x => x.GetInt64()).ToList()
            : [];

    static DatePrecision ToDatePrecision(int? igdbDateFormat) => igdbDateFormat switch
    {
        0 => DatePrecision.Full,
        1 => DatePrecision.MonthYear,
        _ => DatePrecision.YearOnly
    };
}
