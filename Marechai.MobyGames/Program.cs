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
                      .AddInterceptors(new MariaDb12CollationInterceptor())
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
        var importService   = new ImportService(factory, sourceDb, companyMatcher,
                                                personMatcher, platformMatcher,
                                                countryMatcher, stateService);

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

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "--batch-size" && i + 1 < args.Length && int.TryParse(args[i + 1], out int rbs))
                        reviewBatchSize = rbs;
                }

                var magazineMatcher    = new MagazineMatcher(factory);
                var reviewStateService = new ReviewStateService(factory);

                var reviewImportService = new ReviewImportService(
                    factory, sourceDb, platformMatcher, magazineMatcher,
                    reviewStateService);

                await reviewImportService.RunAsync(reviewBatchSize);

                break;
            }

            case "review-status":
            {
                var reviewStateService2 = new ReviewStateService(factory);
                await reviewStateService2.PrintStatusAsync();

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
                Console.WriteLine("    reset --game <id>                             Reset a game to unprocessed");

                break;
        }

        return 0;
    }
}
