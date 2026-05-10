using System;
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
                    .AddJsonFile("appsettings.json")
                    .Build();

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

                for(int i = 0; i < args.Length - 1; i++)
                {
                    if(args[i] == "--batch-size" && int.TryParse(args[i + 1], out int bs))
                        batchSize = bs;
                }

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

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int cbs))
                        coverBatchSize = cbs;

                    if(args[i] == "--delay-ms" && i + 1 < args.Length && int.TryParse(args[i + 1], out int dms))
                        delayMs = dms;

                    if(args[i] == "--dry-run")
                        dryRun = true;
                }

                if(!dryRun && string.IsNullOrEmpty(assetRoot))
                {
                    Console.WriteLine("\e[31;1mMissing MobyGames:AssetRootPath in appsettings.json\e[0m");

                    return 1;
                }

                MobyGamesHttpClient httpClient = null;

                if(!dryRun)
                {
                    httpClient = new MobyGamesHttpClient(delayMs);

                    // Ensure output directories exist
                    ImageConverter.EnsureDirectoriesCreated(assetRoot);
                }

                var coverStateService = new CoverStateService(factory);

                var coverDownloadService = new CoverDownloadService(
                    factory, sourceDb, platformMatcher, countryMatcher,
                    coverStateService, httpClient, assetRoot ?? "");

                try
                {
                    await coverDownloadService.RunAsync(coverBatchSize, dryRun);
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

                using var reviewHttpClient = new MobyGamesHttpClient(reviewDelayMs);

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
                    promoHttpClient = new MobyGamesHttpClient(delayMs);

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

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int pbs))
                        promoBatchSize = pbs;
                    if(args[i] == "--delay-ms" && i + 1 < args.Length && int.TryParse(args[i + 1], out int dms))
                        delayMs = dms;
                    if(args[i] == "--dry-run")
                        dryRun = true;
                }

                if(!dryRun && string.IsNullOrEmpty(assetRoot))
                {
                    Console.WriteLine("\e[31;1mMissing MobyGames:AssetRootPath in appsettings.json\e[0m");
                    return 1;
                }

                MobyGamesHttpClient promoHttpClient2 = null;

                if(!dryRun)
                {
                    promoHttpClient2 = new MobyGamesHttpClient(delayMs);
                    ImageConverter.EnsureDirectoriesCreated(assetRoot, "software-promo-art");
                }

                var promoStateService = new PromoArtStateService(factory);

                var promoDownloadService = new PromoArtDownloadService(
                    factory, sourceDb, promoStateService, promoHttpClient2, assetRoot ?? "");

                try
                {
                    await promoDownloadService.RunAsync(promoBatchSize, dryRun);
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
                    screenshotHttpClient = new MobyGamesHttpClient(delayMs);

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

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int sbs))
                        screenshotBatchSize = sbs;
                    if(args[i] == "--delay-ms" && i + 1 < args.Length && int.TryParse(args[i + 1], out int dms))
                        delayMs = dms;
                    if(args[i] == "--dry-run")
                        dryRun = true;
                }

                if(!dryRun && string.IsNullOrEmpty(assetRoot))
                {
                    Console.WriteLine("\e[31;1mMissing MobyGames:AssetRootPath in appsettings.json\e[0m");
                    return 1;
                }

                MobyGamesHttpClient screenshotHttpClient2 = null;

                if(!dryRun)
                {
                    screenshotHttpClient2 = new MobyGamesHttpClient(delayMs);
                    ImageConverter.EnsureDirectoriesCreated(assetRoot, "software-screenshots");
                }

                var screenshotStateService = new ScreenshotStateService(factory);

                var screenshotDownloadService = new ScreenshotDownloadService(
                    factory, sourceDb, screenshotStateService, platformMatcher,
                    screenshotHttpClient2, assetRoot ?? "");

                try
                {
                    await screenshotDownloadService.RunAsync(screenshotBatchSize, dryRun);
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

                var mediaHttpClient = new MobyGamesHttpClient(delayMs);
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
                int dlcBatchSize = 50;
                int dlcDelayMs   = 2000;
                bool dlcDryRun   = false;

                for(int i = 1; i < args.Length - 1; i++)
                {
                    if(args[i] == "--batch-size" && int.TryParse(args[i + 1], out int bs))
                        dlcBatchSize = bs;

                    if(args[i] == "--delay-ms" && int.TryParse(args[i + 1], out int dm))
                        dlcDelayMs = dm;
                }

                if(args.Contains("--dry-run")) dlcDryRun = true;

                using var dlcHttpClient = new MobyGamesHttpClient(dlcDelayMs);

                // Create a separate import service with HTTP client for numeric ID resolution
                var dlcImportService = new ImportService(factory, sourceDb, companyMatcher,
                                                         personMatcher, platformMatcher,
                                                         countryMatcher, stateService, dlcHttpClient);

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
                int  compBatchSize = 50;
                bool compDryRun    = false;

                for(int i = 1; i < args.Length - 1; i++)
                {
                    if(args[i] == "--batch-size" && int.TryParse(args[i + 1], out int bs))
                        compBatchSize = bs;
                }

                if(args.Contains("--dry-run")) compDryRun = true;

                // Create import service without HTTP client (local only)
                var compImportService = new ImportService(factory, sourceDb, companyMatcher,
                                                          personMatcher, platformMatcher,
                                                          countryMatcher, stateService,
                                                          mobyHttpClient: null,
                                                          adminMessenger: adminMessenger);

                var compService = new CompilationRelationService(factory, sourceDb, compImportService,
                                                                 adminMessenger);

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

            default:
                Console.WriteLine("  Usage:");
                Console.WriteLine("    import [--batch-size N]                       Import next batch of games");
                Console.WriteLine("    download-covers [--batch-size N] [--delay-ms N] [--dry-run]");
                Console.WriteLine("                                                  Download covers for imported games");
                Console.WriteLine("    import-reviews [--batch-size N]");
                Console.WriteLine("                                                  Import critic reviews for imported games");
                Console.WriteLine("    status                                        Show import status counts");
                Console.WriteLine("    cover-status                                  Show cover download status counts");
                Console.WriteLine("    review-status                                 Show review import status counts");
                Console.WriteLine("    scrape-promo-pages [--batch-size N] [--delay-ms N] [--dry-run]");
                Console.WriteLine("                                                  Scrape promo art pages from new MobyGames");
                Console.WriteLine("    download-promo-art [--batch-size N] [--delay-ms N] [--dry-run]");
                Console.WriteLine("                                                  Download promo art images for scraped games");
                Console.WriteLine("    promo-art-status                              Show promo art download status counts");
                Console.WriteLine("    scrape-screenshot-pages [--batch-size N] [--delay-ms N] [--dry-run]");
                Console.WriteLine("                                                  Scrape screenshot pages from new MobyGames");
                Console.WriteLine("    download-screenshots [--batch-size N] [--delay-ms N] [--dry-run]");
                Console.WriteLine("                                                  Download screenshots for scraped games");
                Console.WriteLine("    screenshot-status                             Show screenshot download status counts");
                Console.WriteLine("    scrape-media-pages [--batch-size N] [--delay-ms N] [--dry-run]");
                Console.WriteLine("                                                  Scrape media (video) pages from new MobyGames");
                Console.WriteLine("    import-videos [--batch-size N] [--dry-run]");
                Console.WriteLine("                                                  Import video links from scraped media pages");
                Console.WriteLine("    video-status                                  Show video import status counts");
                Console.WriteLine("    import-dlc-relations [--batch-size N] [--delay-ms N] [--dry-run]");
                Console.WriteLine("                                                  Link DLC entries to their base games via MobyGames");
                Console.WriteLine("    resolve-compilation-relations [--batch-size N] [--dry-run]");
                Console.WriteLine("                                                  Convert compilation Software to proper compilation releases");
                Console.WriteLine("    reparse-specs [--batch-size N] [--dry-run]");
                Console.WriteLine("                                                  Reparse Specs tab from raw HTML and split multi-anchor values into one row each");
                Console.WriteLine("    reset --game <id>                             Reset a game to unprocessed");

                break;
        }

        return 0;
    }
}
