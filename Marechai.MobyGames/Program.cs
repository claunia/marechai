using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.MobyGames.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Marechai.MobyGames;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("\e[32;1mMarechai MobyGames HTML Import Tool\e[0m\n");

        var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();

        // The `convert-images` command runs entirely offline (no DB, no browser, no
        // HTTP). Branch BEFORE touching connection strings / the EF context so it
        // can be invoked on a machine that has only the photos/ tree.
        string commandName = args.Length > 0 ? args[0].ToLowerInvariant() : "import";

        if(commandName == "convert-images")
            return RunConvertImages(args, config);

        string marechaiConn = config.GetConnectionString("DefaultConnection");
        string mobyConn     = config.GetConnectionString("MobyGamesSource");

        if(string.IsNullOrEmpty(marechaiConn) || string.IsNullOrEmpty(mobyConn))
        {
            Console.WriteLine("\e[31;1mMissing connection strings in appsettings.json\e[0m");

            return 1;
        }

        // Setup EF context factory
        var optionsBuilder = new DbContextOptionsBuilder<MarechaiContext>();

        optionsBuilder.UseLazyLoadingProxies()
                      .AddMarechaiInterceptors()
                      .UseMySql(marechaiConn,
                                new MariaDbServerVersion(new Version(12, 0, 2)),
                                b => b.UseMicrosoftJson().EnableStringComparisonTranslations()
                                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));

        var factory = new MarechaiContextFactory(optionsBuilder.Options);

        var sourceDb        = new SourceDatabaseService(mobyConn);
        var companyMatcher  = new CompanyMatcher(factory);
        var personMatcher   = new PersonMatcher(factory);
        var platformMatcher = new PlatformMatcher(factory);
        var countryMatcher  = new CountryMatcher(factory);
        var stateService    = new StateService(factory);
        var adminMessenger  = new AdminMessageService(factory);
        var importService   = new ImportService(factory, sourceDb, companyMatcher,
                                                personMatcher, platformMatcher,
                                                countryMatcher, stateService,
                                                mobyHttpClient: null,
                                                adminMessenger: adminMessenger);

        string command = args.Length > 0 ? args[0].ToLowerInvariant() : "import";

        switch(command)
        {
            case "import":
                int batchSize = config.GetValue("Import:BatchSize", 500);
                bool unattended = false;
                bool yesToAll   = false;

                for(int i = 0; i < args.Length - 1; i++)
                {
                    if(args[i] == "--batch-size" && int.TryParse(args[i + 1], out int bs))
                        batchSize = bs;
                }

                if(args.Contains("--unattended")) unattended = true;
                if(args.Contains("--yes-to-all"))
                {
                    yesToAll   = true;
                    unattended = true; // --yes-to-all implies --unattended
                }

                importService.Unattended  = unattended;
                importService.YesToAll    = yesToAll;
                companyMatcher.Unattended = unattended;
                companyMatcher.YesToAll   = yesToAll;

                // Determine batch number
                var processed = await stateService.GetProcessedGameIdsAsync();
                int batchNumber = (processed.Count / batchSize) + 1;

                Console.WriteLine($"  Batch size: {batchSize}, Batch number: {batchNumber}");

                int totalGames = await sourceDb.GetTotalGameCountAsync();
                Console.WriteLine($"  Total games in source: {totalGames}");

                await importService.RunBatchAsync(batchSize, batchNumber);

                break;

            case "download-covers":
            {
                int coverBatchSize = config.GetValue("Import:BatchSize", 500);
                int delayMs        = config.GetValue("MobyGames:DelayMs", 2000);
                string assetRoot   = config.GetValue<string>("MobyGames:AssetRootPath");
                bool dryRun        = false;
                bool downloadOnly  = false;

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int cbs))
                        coverBatchSize = cbs;

                    if(args[i] == "--delay-ms" && i + 1 < args.Length && int.TryParse(args[i + 1], out int dms))
                        delayMs = dms;

                    if(args[i] == "--dry-run")
                        dryRun = true;

                    if(args[i] == "--download-only")
                        downloadOnly = true;
                }

                if(!dryRun && string.IsNullOrEmpty(assetRoot))
                {
                    Console.WriteLine("\e[31;1mMissing MobyGames:AssetRootPath in appsettings.json\e[0m");

                    return 1;
                }

                MobyGamesHttpClient httpClient = null;

                if(!dryRun)
                {
                    // Authenticate first so the cover detail page yields the MobyPlus
                    // <a download> original-resolution link instead of the thumbnail rewrite.
                    httpClient = await MobyGamesHttpClient.CreateAsync(config, delayMs);

                    // Ensure output directories exist
                    ImageConverter.EnsureDirectoriesCreated(assetRoot);
                }

                var coverStateService = new CoverStateService(factory);

                var coverDownloadService = new CoverDownloadService(
                    factory, sourceDb, coverStateService, httpClient, assetRoot ?? "");

                try
                {
                    await coverDownloadService.RunAsync(coverBatchSize, dryRun, downloadOnly);
                }
                finally
                {
                    httpClient?.Dispose();
                }

                break;
            }

            case "cover-status":
            {
                var coverStateService2 = new CoverStateService(factory);
                await coverStateService2.PrintCoverStatusAsync();

                break;
            }

            case "repair-covers":
            {
                int repairBatchSize = 1000;
                bool repairDryRun   = false;

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int rbs))
                        repairBatchSize = rbs;

                    if(args[i] == "--dry-run")
                        repairDryRun = true;
                }

                var repairStateService = new CoverStateService(factory);

                var repairService = new CoverDownloadService(
                    factory, sourceDb, repairStateService, httpClient: null, assetRootPath: "");

                await repairService.RepairMisassignedCoversAsync(repairDryRun, repairBatchSize);

                break;
            }

            case "import-reviews":
            {
                int reviewBatchSize = config.GetValue("Import:BatchSize", 500);
                int reviewDelayMs   = config.GetValue("MobyGames:DelayMs", 2000);

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int rbs))
                        reviewBatchSize = rbs;

                    if(args[i] == "--delay-ms" && i + 1 < args.Length && int.TryParse(args[i + 1], out int rdm))
                        reviewDelayMs = rdm;
                }

                using var reviewHttpClient = await MobyGamesHttpClient.CreateAsync(config, reviewDelayMs);

                var magazineMatcher    = new MagazineMatcher(factory, countryMatcher, reviewHttpClient);
                var reviewStateService = new ReviewStateService(factory);

                var reviewImportService = new ReviewImportService(
                    factory, sourceDb, platformMatcher, magazineMatcher,
                    reviewStateService, countryMatcher);

                await reviewImportService.RunAsync(reviewBatchSize);

                break;
            }

            case "review-status":
            {
                var reviewStateService2 = new ReviewStateService(factory);
                await reviewStateService2.PrintStatusAsync();

                break;
            }

            case "discover-games":
            {
                int  fromYear = 2019;
                int  toYear   = DateTime.UtcNow.Year;
                int  delayMs  = config.GetValue("MobyGames:DelayMs", 2000);
                bool dryRun   = false;

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--from-year" && i + 1 < args.Length && int.TryParse(args[i + 1], out int fy))
                        fromYear = fy;
                    if(args[i] == "--to-year" && i + 1 < args.Length && int.TryParse(args[i + 1], out int ty))
                        toYear = ty;
                    if(args[i] == "--delay-ms" && i + 1 < args.Length && int.TryParse(args[i + 1], out int dms))
                        delayMs = dms;
                    if(args[i] == "--dry-run")
                        dryRun = true;
                }

                if(fromYear > toYear)
                {
                    Console.WriteLine($"\e[31;1m--from-year ({fromYear}) must be <= --to-year ({toYear})\e[0m");

                    return 1;
                }

                // Discovery uses the SPA's `?format=json` endpoint over plain HTTP for speed.
                // PuppeteerSharp is only used (transparently inside MobyGamesHttpClient.CreateAsync)
                // to log in and bypass Cloudflare/Turnstile; once cookies are imported into the
                // HTTP client, Chromium is disposed. The session lifts the anonymous 14-page cap
                // and the `perPage=100` preference cookie lifts the page size from 18 to 100.
                using var discoveryHttp = await MobyGamesHttpClient.CreateAsync(config, delayMs);

                var discoveryState   = new DiscoveryStateService(factory);
                var discoveryScraper = new SearchDiscoveryScraper(discoveryHttp, discoveryState);

                await discoveryScraper.RunAsync(fromYear, toYear, dryRun);

                break;
            }

            case "scrape-new-games":
            {
                int  newBatchSize = config.GetValue("Import:BatchSize", 500);
                int  delayMs      = config.GetValue("MobyGames:DelayMs", 2000);
                bool dryRun       = false;

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int bs))
                        newBatchSize = bs;
                    if(args[i] == "--delay-ms" && i + 1 < args.Length && int.TryParse(args[i + 1], out int dms))
                        delayMs = dms;
                    if(args[i] == "--dry-run")
                        dryRun = true;
                }

                using var newGameHttp = await MobyGamesHttpClient.CreateAsync(config, delayMs);

                var newGameDiscovery = new DiscoveryStateService(factory);
                var newGameFetcher   = new NewGameRawFetcher(newGameHttp, sourceDb, newGameDiscovery);

                await newGameFetcher.RunAsync(newBatchSize, dryRun);

                break;
            }

            case "discovery-status":
            {
                var discoveryStateOnly = new DiscoveryStateService(factory);
                await discoveryStateOnly.PrintStatusAsync();

                break;
            }

            case "scrape-promo-pages":
            {
                int promoBatchSize = config.GetValue("Import:BatchSize", 500);
                int delayMs        = config.GetValue("MobyGames:DelayMs", 2000);
                bool dryRun        = false;

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int pbs))
                        promoBatchSize = pbs;
                    if(args[i] == "--delay-ms" && i + 1 < args.Length && int.TryParse(args[i + 1], out int dms))
                        delayMs = dms;
                    if(args[i] == "--dry-run")
                        dryRun = true;
                }

                MobyGamesHttpClient promoHttpClient = null;

                if(!dryRun)
                {
                    // Anonymous sessions cap promo art index pagination — authenticate first.
                    promoHttpClient = await MobyGamesHttpClient.CreateAsync(config, delayMs);
                }

                var promoScraper = new PromoArtScraper(factory, sourceDb, promoHttpClient);

                try
                {
                    await promoScraper.RunAsync(promoBatchSize, dryRun);
                }
                finally
                {
                    promoHttpClient?.Dispose();
                }

                break;
            }

            case "download-promo-art":
            {
                int promoBatchSize = config.GetValue("Import:BatchSize", 500);
                int delayMs        = config.GetValue("MobyGames:DelayMs", 2000);
                string assetRoot   = config.GetValue<string>("MobyGames:AssetRootPath");
                bool dryRun        = false;
                bool downloadOnly  = false;

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int pbs))
                        promoBatchSize = pbs;
                    if(args[i] == "--delay-ms" && i + 1 < args.Length && int.TryParse(args[i + 1], out int dms))
                        delayMs = dms;
                    if(args[i] == "--dry-run")
                        dryRun = true;
                    if(args[i] == "--download-only")
                        downloadOnly = true;
                }

                if(!dryRun && string.IsNullOrEmpty(assetRoot))
                {
                    Console.WriteLine("\e[31;1mMissing MobyGames:AssetRootPath in appsettings.json\e[0m");
                    return 1;
                }

                MobyGamesHttpClient promoHttpClient2 = null;

                if(!dryRun)
                {
                    // Promo art always needs the MobyPlus original — no fallback URL pattern.
                    promoHttpClient2 = await MobyGamesHttpClient.CreateAsync(config, delayMs);

                    ImageConverter.EnsureDirectoriesCreated(assetRoot, "software-promo-art");
                }

                var promoStateService = new PromoArtStateService(factory);

                var promoDownloadService = new PromoArtDownloadService(
                    factory, sourceDb, promoStateService, promoHttpClient2, assetRoot ?? "");

                try
                {
                    await promoDownloadService.RunAsync(promoBatchSize, dryRun, downloadOnly);
                }
                finally
                {
                    promoHttpClient2?.Dispose();
                }

                break;
            }

            case "promo-art-status":
            {
                var promoStateService2 = new PromoArtStateService(factory);
                await promoStateService2.PrintPromoArtStatusAsync();

                break;
            }

            case "scrape-screenshot-pages":
            {
                int screenshotBatchSize = config.GetValue("Import:BatchSize", 500);
                int delayMs             = config.GetValue("MobyGames:DelayMs", 2000);
                bool dryRun             = false;

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int sbs))
                        screenshotBatchSize = sbs;
                    if(args[i] == "--delay-ms" && i + 1 < args.Length && int.TryParse(args[i + 1], out int dms))
                        delayMs = dms;
                    if(args[i] == "--dry-run")
                        dryRun = true;
                }

                MobyGamesHttpClient screenshotHttpClient = null;

                if(!dryRun)
                {
                    // The screenshot index pages also paginate behind the anonymous cap, so we
                    // need authenticated cookies even before we hit any detail page.
                    screenshotHttpClient = await MobyGamesHttpClient.CreateAsync(config, delayMs);
                }

                var screenshotScraper = new ScreenshotScraper(factory, sourceDb, screenshotHttpClient);

                try
                {
                    await screenshotScraper.RunAsync(screenshotBatchSize, dryRun);
                }
                finally
                {
                    screenshotHttpClient?.Dispose();
                }

                break;
            }

            case "download-screenshots":
            {
                int screenshotBatchSize = config.GetValue("Import:BatchSize", 500);
                int delayMs             = config.GetValue("MobyGames:DelayMs", 2000);
                string assetRoot        = config.GetValue<string>("MobyGames:AssetRootPath");
                bool dryRun             = false;
                bool downloadOnly       = false;

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int sbs))
                        screenshotBatchSize = sbs;
                    if(args[i] == "--delay-ms" && i + 1 < args.Length && int.TryParse(args[i + 1], out int dms))
                        delayMs = dms;
                    if(args[i] == "--dry-run")
                        dryRun = true;
                    if(args[i] == "--download-only")
                        downloadOnly = true;
                }

                if(!dryRun && string.IsNullOrEmpty(assetRoot))
                {
                    Console.WriteLine("\e[31;1mMissing MobyGames:AssetRootPath in appsettings.json\e[0m");
                    return 1;
                }

                MobyGamesHttpClient screenshotHttpClient2 = null;

                if(!dryRun)
                {
                    // Authenticated cookies unlock the higher-res MobyPlus screenshots and
                    // bypass anonymous page caps for detail-page traversal.
                    screenshotHttpClient2 = await MobyGamesHttpClient.CreateAsync(config, delayMs);

                    ImageConverter.EnsureDirectoriesCreated(assetRoot, "software-screenshots");
                }

                var screenshotStateService = new ScreenshotStateService(factory);

                var screenshotDownloadService = new ScreenshotDownloadService(
                    factory, sourceDb, screenshotStateService, platformMatcher,
                    screenshotHttpClient2, assetRoot ?? "");

                try
                {
                    await screenshotDownloadService.RunAsync(screenshotBatchSize, dryRun, downloadOnly);
                }
                finally
                {
                    screenshotHttpClient2?.Dispose();
                }

                break;
            }

            case "screenshot-status":
            {
                var screenshotStateService2 = new ScreenshotStateService(factory);
                await screenshotStateService2.PrintScreenshotStatusAsync();

                break;
            }

            case "scrape-media-pages":
            {
                int mediaBatchSize = config.GetValue("Import:BatchSize", 500);
                int delayMs        = 2000;
                bool dryRun        = false;

                for(int i = 0; i < args.Length - 1; i++)
                {
                    if(args[i] == "--batch-size" && int.TryParse(args[i + 1], out int bs))
                        mediaBatchSize = bs;

                    if(args[i] == "--delay-ms" && int.TryParse(args[i + 1], out int d))
                        delayMs = d;
                }

                if(args.Contains("--dry-run")) dryRun = true;

                var mediaHttpClient = await MobyGamesHttpClient.CreateAsync(config, delayMs);
                var mediaScraper    = new MediaScraper(factory, sourceDb, mediaHttpClient);

                try
                {
                    await mediaScraper.RunAsync(mediaBatchSize, dryRun);
                }
                finally
                {
                    mediaHttpClient.Dispose();
                }

                break;
            }

            case "import-videos":
            {
                int videoBatchSize = config.GetValue("Import:BatchSize", 500);
                bool dryRun        = false;

                for(int i = 0; i < args.Length - 1; i++)
                {
                    if(args[i] == "--batch-size" && int.TryParse(args[i + 1], out int bs))
                        videoBatchSize = bs;
                }

                if(args.Contains("--dry-run")) dryRun = true;

                var videoStateService = new VideoStateService(factory);

                var videoImportService = new VideoImportService(
                    factory, sourceDb, videoStateService);

                await videoImportService.RunAsync(videoBatchSize, dryRun);

                break;
            }

            case "video-status":
            {
                var videoStateService2 = new VideoStateService(factory);
                await videoStateService2.PrintVideoStatusAsync();

                break;
            }

            case "status":
                await stateService.PrintStatusAsync();

                break;

            case "reset":
                string resetId = null;

                for(int i = 0; i < args.Length - 1; i++)
                {
                    if(args[i] == "--game")
                        resetId = args[i + 1];
                }

                if(!string.IsNullOrEmpty(resetId))
                    await stateService.ResetGameAsync(resetId);
                else
                    Console.WriteLine("  Usage: reset --game <mobyGameId>");

                break;

            case "import-dlc-relations":
            {
                int dlcBatchSize    = 50;
                int dlcDelayMs      = 2000;
                bool dlcDryRun      = false;
                bool dlcUnattended  = false;

                for(int i = 1; i < args.Length - 1; i++)
                {
                    if(args[i] == "--batch-size" && int.TryParse(args[i + 1], out int bs))
                        dlcBatchSize = bs;

                    if(args[i] == "--delay-ms" && int.TryParse(args[i + 1], out int dm))
                        dlcDelayMs = dm;
                }

                if(args.Contains("--dry-run"))   dlcDryRun     = true;
                if(args.Contains("--unattended")) dlcUnattended = true;

                // Cookies attached automatically by CreateAsync — without them the new-site DLC
                // page comes back as the Cloudflare "Just a moment..." challenge HTML and
                // NewSiteMainPageParser.ParseBaseGameId silently returns null for every entry.
                using var dlcHttpClient = await MobyGamesHttpClient.CreateAsync(config, dlcDelayMs);

                // Create a separate import service with HTTP client for numeric ID resolution
                var dlcImportService = new ImportService(factory, sourceDb, companyMatcher,
                                                         personMatcher, platformMatcher,
                                                         countryMatcher, stateService, dlcHttpClient);

                dlcImportService.Unattended = dlcUnattended;
                companyMatcher.Unattended   = dlcUnattended;

                var dlcService = new DlcRelationService(factory, dlcHttpClient, dlcImportService, sourceDb);

                try
                {
                    await dlcService.RunAsync(dlcBatchSize, dlcDryRun);
                }
                catch(UserQuitException)
                {
                    Console.WriteLine("\n  Quitting on user request.");
                }

                break;
            }

            case "resolve-compilation-relations":
            {
                int  compBatchSize  = 50;
                bool compDryRun     = false;
                bool compUnattended = false;

                for(int i = 1; i < args.Length - 1; i++)
                {
                    if(args[i] == "--batch-size" && int.TryParse(args[i + 1], out int bs))
                        compBatchSize = bs;
                }

                if(args.Contains("--dry-run"))    compDryRun     = true;
                if(args.Contains("--unattended")) compUnattended = true;

                // Compilation resolver re-scrapes stale legacy-layout caches before parsing
                // so the cookied HTTP client must be available.
                using var compHttpClient = await MobyGamesHttpClient.CreateAsync(config,
                    config.GetValue("MobyGames:DelayMs", 2000));

                // Create import service WITH HTTP client so re-imported base games can also
                // refresh their own caches if needed by the downstream chain.
                var compImportService = new ImportService(factory, sourceDb, companyMatcher,
                                                          personMatcher, platformMatcher,
                                                          countryMatcher, stateService,
                                                          mobyHttpClient: compHttpClient,
                                                          adminMessenger: adminMessenger);

                compImportService.Unattended = compUnattended;
                companyMatcher.Unattended    = compUnattended;

                var compService = new CompilationRelationService(factory, sourceDb, compImportService,
                                                                 adminMessenger, compHttpClient);

                try
                {
                    await compService.RunAsync(compBatchSize, compDryRun);
                }
                catch(UserQuitException)
                {
                    Console.WriteLine("\n  Quitting on user request.");
                }

                break;
            }

            case "reparse-specs":
            {
                int  reparseBatchSize = 50;
                bool reparseDryRun    = false;

                for(int i = 1; i < args.Length - 1; i++)
                {
                    if(args[i] == "--batch-size" && int.TryParse(args[i + 1], out int bs))
                        reparseBatchSize = bs;
                }

                if(args.Contains("--dry-run")) reparseDryRun = true;

                var reparseService = new SpecsReparseService(factory, sourceDb);
                await reparseService.RunAsync(reparseBatchSize, reparseDryRun);

                break;
            }

            case "reparse-covers":
            {
                int  coverReparseBatchSize = 50;
                bool coverReparseDryRun    = false;

                for(int i = 1; i < args.Length - 1; i++)
                {
                    if(args[i] == "--batch-size" && int.TryParse(args[i + 1], out int bs))
                        coverReparseBatchSize = bs;
                }

                if(args.Contains("--dry-run")) coverReparseDryRun = true;

                var coversReparseService = new CoversReparseService(factory, sourceDb);
                await coversReparseService.RunAsync(coverReparseBatchSize, coverReparseDryRun);

                break;
            }

            case "reparse-descriptions":
            {
                int  descReparseBatchSize = 50;
                bool descReparseDryRun    = false;

                for(int i = 1; i < args.Length - 1; i++)
                {
                    if(args[i] == "--batch-size" && int.TryParse(args[i + 1], out int bs))
                        descReparseBatchSize = bs;
                }

                if(args.Contains("--dry-run")) descReparseDryRun = true;

                var descriptionsReparseService = new DescriptionsReparseService(factory, sourceDb);
                await descriptionsReparseService.RunAsync(descReparseBatchSize, descReparseDryRun);

                break;
            }

            case "cleanup-orphan-duplicates":
            {
                bool cleanupDryRun = args.Contains("--dry-run");
                bool cleanupYes    = args.Contains("--yes");

                var cleanupService = new OrphanCleanupService(factory);
                await cleanupService.RunAsync(cleanupDryRun, cleanupYes);

                break;
            }

            case "refresh-slug":
            {
                string refreshSlug    = null;
                int?   refreshId      = null;
                int    refreshDelayMs = config.GetValue("MobyGames:DelayMs", 2000);

                for(int i = 1; i < args.Length; i++)
                {
                    if(args[i] == "--slug"     && i + 1 < args.Length) refreshSlug = args[i + 1];
                    if(args[i] == "--id"       && i + 1 < args.Length && int.TryParse(args[i + 1], out int rid)) refreshId = rid;
                    if(args[i] == "--delay-ms" && i + 1 < args.Length && int.TryParse(args[i + 1], out int rdm)) refreshDelayMs = rdm;
                }

                if(string.IsNullOrWhiteSpace(refreshSlug))
                {
                    Console.WriteLine("  Usage: refresh-slug --slug <slug> [--id <numericId>] [--delay-ms N]");
                    Console.WriteLine("         Downloads ALL chunks for the slug from the live site and replaces any cached rows.");
                    Console.WriteLine("         --id is optional; if omitted, the numeric ID is resolved from the slug.");
                    return 1;
                }

                using var refreshHttp = await MobyGamesHttpClient.CreateAsync(config, refreshDelayMs);

                int? numericId = refreshId
                                 ?? await refreshHttp.ResolveNumericGameIdAsync(refreshSlug);

                if(numericId is null)
                {
                    Console.WriteLine($"  Could not resolve numeric ID for '{refreshSlug}'. Pass --id explicitly or check the slug.");
                    return 1;
                }

                string trimmed = refreshSlug.TrimStart('-');
                string baseUrl = $"https://www.mobygames.com/game/{numericId}/{trimmed}/";

                Console.WriteLine($"  Refreshing slug='{trimmed}', numericId={numericId} from {baseUrl}");

                string mainBody = await refreshHttp.FetchPageAsync(baseUrl);

                if(string.IsNullOrWhiteSpace(mainBody))
                {
                    Console.WriteLine("  Live fetch returned empty body. Aborting; cache left untouched.");
                    return 1;
                }

                // Delete every existing cached chunk for both '<slug>' and '-<slug>' variants
                // before inserting so we end up with a single canonical set under the trimmed key.
                int deletedTrimmed = await sourceDb.DeleteAllChunksAsync(trimmed);
                int deletedDashed  = await sourceDb.DeleteAllChunksAsync($"-{trimmed}");
                Console.WriteLine($"  Deleted {deletedTrimmed + deletedDashed} stale chunk(s) ({deletedTrimmed} under '{trimmed}', {deletedDashed} under '-{trimmed}')");

                int saved = 0;

                async Task<bool> SaveAsync(int chunk, string url, string body = null)
                {
                    body ??= await refreshHttp.FetchPageAsync(url);

                    if(string.IsNullOrWhiteSpace(body)) return false;

                    try
                    {
                        await sourceDb.InsertRowAsync(trimmed, chunk, body);
                        saved++;
                        Console.WriteLine($"    chunk {chunk}: {body.Length} bytes");
                        return true;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"    chunk {chunk}: INSERT failed: {ex.GetType().Name}: {ex.Message}");
                        return false;
                    }
                }

                await SaveAsync(0, baseUrl, mainBody);                  // main
                await SaveAsync(1, baseUrl + "credits/");
                await SaveAsync(2, baseUrl + "releases/");
                await SaveAsync(3, baseUrl + "specs/");

                if(Parsers.NewSite.MediaPresenceDetector.HasCoverArt(mainBody))
                    await SaveAsync(4, baseUrl + "covers/");

                if(Parsers.NewSite.MediaPresenceDetector.HasReviews(mainBody))
                    await SaveAsync(5, baseUrl + "reviews/");

                Console.WriteLine($"  Refresh complete: {saved} chunk(s) written.");

                break;
            }

            case "migrate-chunks":
            {
                Console.WriteLine("\n  Migrating dynamically-allocated chunks to fixed slot numbers...\n");
                Console.WriteLine("  Fixed slots: Promo=10, Screenshots=11, Media=12\n");

                var misplaced = await sourceDb.GetMisplacedChunksAsync();

                Console.WriteLine($"  Found {misplaced.Count} chunk(s) outside fixed slots\n");

                int migrated = 0;
                int skipped  = 0;
                int errors   = 0;
                var skipReasons = new Dictionary<string, int>();

                for(int i = 0; i < misplaced.Count; i++)
                {
                    var (gameId, chunk) = misplaced[i];

                    if((i + 1) % 1000 == 0)
                        Console.WriteLine($"  [{i + 1}/{misplaced.Count}] migrated={migrated} skipped={skipped} errors={errors}");

                    string body = await sourceDb.GetChunkBodyAsync(gameId, chunk);

                    if(body is null)
                    {
                        skipped++;
                        continue;
                    }

                    var (tab, _) = Parsers.TabDetector.DetectWithLayout(body);

                    int? targetChunk = tab switch
                    {
                        Parsers.MobyTab.PromoArt    => NewGameRawFetcher.ChunkPromo,
                        Parsers.MobyTab.Screenshots => NewGameRawFetcher.ChunkScreenshots,
                        Parsers.MobyTab.Media       => NewGameRawFetcher.ChunkMedia,
                        _                           => null
                    };

                    if(targetChunk is null)
                    {
                        string reason = tab.ToString();
                        skipReasons.TryGetValue(reason, out int cnt);
                        skipReasons[reason] = cnt + 1;
                        skipped++;
                        continue;
                    }

                    // Don't overwrite if target slot already has data
                    if(await sourceDb.ChunkExistsAsync(gameId, targetChunk.Value))
                    {
                        string reason = $"{tab} → {targetChunk.Value} (slot occupied)";
                        skipReasons.TryGetValue(reason, out int cnt);
                        skipReasons[reason] = cnt + 1;
                        skipped++;
                        continue;
                    }

                    try
                    {
                        await sourceDb.MoveChunkAsync(gameId, chunk, targetChunk.Value);
                        migrated++;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"  ERROR {gameId} chunk {chunk} → {targetChunk.Value}: {ex.Message}");
                        errors++;
                    }
                }

                Console.WriteLine($"\n  ────────────────────────────────────");
                Console.WriteLine($"    Chunks scanned:  {misplaced.Count}");
                Console.WriteLine($"    Migrated:        {migrated}");
                Console.WriteLine($"    Skipped:         {skipped}");
                Console.WriteLine($"    Errors:          {errors}");

                if(skipReasons.Count > 0)
                {
                    Console.WriteLine("    Skip breakdown:");

                    foreach(var kv in skipReasons.OrderByDescending(kv => kv.Value))
                        Console.WriteLine($"      {kv.Key}: {kv.Value}");
                }
                Console.WriteLine($"  ────────────────────────────────────\n");

                break;
            }

            default:
                Console.WriteLine("  Usage:");
                Console.WriteLine("    import [--batch-size N] [--unattended] [--yes-to-all]");
                Console.WriteLine("                                                  Import next batch of games (--unattended skips games needing prompts;");
                Console.WriteLine("                                                  --yes-to-all implies --unattended and auto-picks 'create new entry',");
                Console.WriteLine("                                                  'create new company', and ProductCodeIssuer.Other instead of skipping)");
                Console.WriteLine("    download-covers [--batch-size N] [--delay-ms N] [--dry-run] [--download-only]");
                Console.WriteLine("                                                  Download covers for imported games (--download-only skips the");
                Console.WriteLine("                                                  ImageMagick conversion step; run `convert-images` later)");
                Console.WriteLine("    repair-covers [--batch-size N] [--dry-run]");
                Console.WriteLine("                                                  Retroactively regroup already-downloaded covers by MobyGames GroupId");
                Console.WriteLine("                                                  and clear any stale SoftwareReleaseId. DB-only, no re-download.");
                Console.WriteLine("    reparse-covers [--batch-size N] [--dry-run]");
                Console.WriteLine("                                                  Reparse the Covers tab from cached raw HTML and correct GroupId/Platform on");
                Console.WriteLine("                                                  MobyGamesCoverDownloadState rows. No network access. Run `repair-covers`");
                Console.WriteLine("                                                  afterwards to propagate the fix onto SoftwareCover/SoftwareCoverGroup.");
                Console.WriteLine("    import-reviews [--batch-size N]");
                Console.WriteLine("                                                  Import critic reviews for imported games");
                Console.WriteLine("    status                                        Show import status counts");
                Console.WriteLine("    cover-status                                  Show cover download status counts");
                Console.WriteLine("    review-status                                 Show review import status counts");
                Console.WriteLine("    discover-games [--from-year N] [--to-year N] [--delay-ms N] [--dry-run]");
                Console.WriteLine("                                                  Discover MobyGames games by walking the year-filtered search results");
                Console.WriteLine("                                                  via an embedded headless Chromium login (lifts the 14-page-per-filter cap");
                Console.WriteLine("                                                  imposed on anonymous sessions). Default: --from-year 2019 --to-year <current>.");
                Console.WriteLine("    scrape-new-games [--batch-size N] [--delay-ms N] [--dry-run]");
                Console.WriteLine("                                                  Fetch main/credits/releases/specs HTML for discovered games into mobygames_raw");
                Console.WriteLine("    discovery-status                              Show MobyGamesDiscoveredGames status counts");
                Console.WriteLine("    scrape-promo-pages [--batch-size N] [--delay-ms N] [--dry-run]");
                Console.WriteLine("                                                  Scrape promo art pages from new MobyGames");
                Console.WriteLine("    download-promo-art [--batch-size N] [--delay-ms N] [--dry-run] [--download-only]");
                Console.WriteLine("                                                  Download promo art images for scraped games");
                Console.WriteLine("    promo-art-status                              Show promo art download status counts");
                Console.WriteLine("    scrape-screenshot-pages [--batch-size N] [--delay-ms N] [--dry-run]");
                Console.WriteLine("                                                  Scrape screenshot pages from new MobyGames");
                Console.WriteLine("    download-screenshots [--batch-size N] [--delay-ms N] [--dry-run] [--download-only]");
                Console.WriteLine("                                                  Download screenshots for scraped games");
                Console.WriteLine("    convert-images --type covers|promo-art|screenshots|all [--batch-size N] [--parallel N] [--asset-root <path>] [--dry-run]");
                Console.WriteLine("                                                  Run the ImageMagick conversion pass on already-downloaded originals.");
                Console.WriteLine("                                                  Walks photos/<type>/originals/ on disk only - no DB, no browser, no HTTP.");
                Console.WriteLine("                                                  --parallel N caps concurrent ImageMagick invocations (default: one per CPU core).");
                Console.WriteLine("                                                  Designed to be offloaded to a more powerful machine.");
                Console.WriteLine("    screenshot-status                             Show screenshot download status counts");
                Console.WriteLine("    scrape-media-pages [--batch-size N] [--delay-ms N] [--dry-run]");
                Console.WriteLine("                                                  Scrape media (video) pages from new MobyGames");
                Console.WriteLine("    import-videos [--batch-size N] [--dry-run]");
                Console.WriteLine("                                                  Import video links from scraped media pages");
                Console.WriteLine("    video-status                                  Show video import status counts");
                Console.WriteLine("    import-dlc-relations [--batch-size N] [--delay-ms N] [--dry-run] [--unattended]");
                Console.WriteLine("                                                  Link DLC entries to their base games via MobyGames");
                Console.WriteLine("    resolve-compilation-relations [--batch-size N] [--dry-run] [--unattended]");
                Console.WriteLine("                                                  Convert compilation Software to proper compilation releases");
                Console.WriteLine("    reparse-specs [--batch-size N] [--dry-run]");
                Console.WriteLine("                                                  Reparse Specs tab from raw HTML and split multi-anchor values into one row each");
                Console.WriteLine("    reparse-descriptions [--batch-size N] [--dry-run]");
                Console.WriteLine("                                                  Repair SoftwareDescription rows polluted with leaked MobyGames page chrome");
                Console.WriteLine("                                                  (sidebar links, review tables, <moby> tags) by reparsing from cached raw HTML.");
                Console.WriteLine("                                                  Only fingerprint-matching rows are rewritten; their non-English machine");
                Console.WriteLine("                                                  translations are deleted for later retranslation. No network access.");
                Console.WriteLine("    cleanup-orphan-duplicates [--dry-run] [--yes]");
                Console.WriteLine("                                                  Merge duplicate orphan Software rows into their state-linked twin (backfill for");
                Console.WriteLine("                                                  legacy data created before MarkSoftwareLinkedAsync). Prompts unless --yes is passed.");
                Console.WriteLine("    refresh-slug --slug <slug> [--id <numericId>] [--delay-ms N]");
                Console.WriteLine("                                                  Re-download ALL chunks for one slug from the live site, deleting any");
                Console.WriteLine("                                                  cached rows under both '<slug>' and '-<slug>' first. Use to evict stale");
                Console.WriteLine("                                                  legacy-layout captures (e.g. since-corrected MobyGames data).");
                Console.WriteLine("    reset --game <id>                             Reset a game to unprocessed");
                Console.WriteLine("    migrate-chunks                                Move dynamically-allocated chunks (6-9, 13+) to fixed slots:");
                Console.WriteLine("                                                  Promo→10, Screenshots→11, Media→12. Run once after upgrading.");

                break;
        }

        return 0;
    }
    /// <summary>
    /// Synchronous, fully offline handler for the `convert-images` command. Walks
    /// the asset root's photos/<type>/originals/ tree and runs the ImageMagick
    /// conversion pass for every original whose 8 output variants are not all
    /// present on disk. Touches no database, no browser, no HTTP client.
    /// </summary>
    static int RunConvertImages(string[] args, IConfiguration config)
    {
        int batchSize = 0; // 0 = no cap
        int parallelism = 0; // 0 = auto = Environment.ProcessorCount
        bool dryRun = false;
        string typeArg = "all";
        string assetRoot = config.GetValue<string>("MobyGames:AssetRootPath");

        for(int i = 1; i < args.Length; i++)
        {
            if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int bs))
                batchSize = bs;
            else if(args[i] == "--parallel" && i + 1 < args.Length && int.TryParse(args[i + 1], out int p))
                parallelism = p;
            else if(args[i] == "--type" && i + 1 < args.Length)
                typeArg = args[i + 1].ToLowerInvariant();
            else if(args[i] == "--asset-root" && i + 1 < args.Length)
                assetRoot = args[i + 1];
            else if(args[i] == "--dry-run")
                dryRun = true;
        }

        if(string.IsNullOrEmpty(assetRoot))
        {
            Console.WriteLine("\e[31;1mMissing asset root.\e[0m Pass --asset-root <path> or set MobyGames:AssetRootPath in appsettings.json.");

            return 1;
        }

        if(!System.IO.Directory.Exists(assetRoot))
        {
            Console.WriteLine($"\e[31;1mAsset root does not exist:\e[0m {assetRoot}");

            return 1;
        }

        ImageConversionItemType[] types;

        switch(typeArg)
        {
            case "covers":
                types = new[] { ImageConversionItemType.Covers };
                break;
            case "promo-art":
                types = new[] { ImageConversionItemType.PromoArt };
                break;
            case "screenshots":
                types = new[] { ImageConversionItemType.Screenshots };
                break;
            case "all":
                types = new[]
                {
                    ImageConversionItemType.Covers, ImageConversionItemType.PromoArt,
                    ImageConversionItemType.Screenshots
                };
                break;
            default:
                Console.WriteLine($"\e[31;1mUnknown --type value:\e[0m {typeArg}");
                Console.WriteLine("  Valid values: covers, promo-art, screenshots, all");

                return 1;
        }

        Console.WriteLine($"  Asset root: {assetRoot}");
        Console.WriteLine($"  Types:      {string.Join(", ", types)}");
        Console.WriteLine($"  Batch size: {(batchSize <= 0 ? "no cap" : batchSize.ToString())}");
        Console.WriteLine($"  Parallel:   {(parallelism <= 0 ? $"auto ({Environment.ProcessorCount})" : parallelism.ToString())}");
        Console.WriteLine($"  Dry run:    {dryRun}");

        int totalFailed = 0;

        foreach(ImageConversionItemType type in types)
            totalFailed += ImageConversionPassService.Run(assetRoot, type, batchSize, dryRun, parallelism);

        if(totalFailed > 0)
        {
            Console.WriteLine($"\e[33;1m{totalFailed} conversion(s) failed.\e[0m");

            return 1;
        }

        Console.WriteLine("\e[32;1mConversion pass complete.\e[0m");

        return 0;
    }}
