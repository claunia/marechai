using System;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.Igdb.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Marechai.Igdb;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("\e[32;1mMarechai IGDB Mapping Tool\e[0m\n");

        string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true)
                                                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                                                .Build();

        string marechaiConn = config.GetConnectionString("DefaultConnection");

        if(string.IsNullOrEmpty(marechaiConn))
        {
            Console.WriteLine("\e[31;1mMissing connection string in appsettings.json\e[0m");

            return 1;
        }

        var optionsBuilder = new DbContextOptionsBuilder<MarechaiContext>();

        optionsBuilder.UseLazyLoadingProxies()
                      .AddMarechaiInterceptors()
                      .UseMySql(marechaiConn,
                                new MariaDbServerVersion(new Version(12, 0, 2)),
                                b => b.UseMicrosoftJson().EnableStringComparisonTranslations()
                                       .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));

        var factory = new MarechaiContextFactory(optionsBuilder.Options);

        string command = args.Length > 0 ? args[0].ToLowerInvariant() : "help";
        bool   dryRun   = args.Contains("--dry-run");
        int    batchSize = GetIntOption(args, "--batch-size", config.GetValue("Import:BatchSize", 500));

        string clientId     = config.GetValue<string>("Igdb:ClientId");
        string clientSecret = config.GetValue<string>("Igdb:ClientSecret");
        string tokenCachePath = config.GetValue("Igdb:TokenCachePath", "state/igdb-token.json");
        int    requestsPerSecond = config.GetValue("Igdb:RequestsPerSecond", 4);
        int    maxConcurrentRequests = config.GetValue("Igdb:MaxConcurrentRequests", 8);

        switch(command)
        {
            case "mirror-platforms":
            {
                IgdbHttpClient client = RequireClient();

                if(client == null)
                    return 1;

                var service = new PlatformMirrorService(factory, client);
                int total = await service.RunAsync(batchSize, dryRun);
                Console.WriteLine($"\nDone. Mirrored {total} platforms.");

                break;
            }

            case "mirror-game-types":
            {
                IgdbHttpClient client = RequireClient();

                if(client == null)
                    return 1;

                var service = new GameTypeMirrorService(factory, client);
                int total = await service.RunAsync(batchSize, dryRun);
                Console.WriteLine($"\nDone. Mirrored {total} game types.");

                break;
            }

            case "mirror-companies":
            {
                IgdbHttpClient client = RequireClient();

                if(client == null)
                    return 1;

                var service = new CompanyMirrorService(factory, client);
                int total = await service.RunAsync(batchSize, dryRun);
                Console.WriteLine($"\nDone. Mirrored {total} companies.");

                break;
            }

            case "mirror-games":
            {
                IgdbHttpClient client = RequireClient();

                if(client == null)
                    return 1;

                var service = new GameMirrorService(factory, client);
                int total = await service.RunAsync(batchSize, dryRun);
                Console.WriteLine($"\nDone. Mirrored {total} games.");

                break;
            }

            case "backfill-game-slugs":
            {
                IgdbHttpClient client = RequireClient();

                if(client == null)
                    return 1;

                var service = new GameMirrorService(factory, client);
                int total = await service.BackfillSlugsAsync(batchSize, dryRun);
                Console.WriteLine($"\nDone. Backfilled {total} game slugs.");

                break;
            }

            case "mirror-involved-companies":
            {
                IgdbHttpClient client = RequireClient();

                if(client == null)
                    return 1;

                var service = new InvolvedCompanyMirrorService(factory, client);
                int total = await service.RunAsync(batchSize, dryRun);
                Console.WriteLine($"\nDone. Mirrored {total} involved companies.");

                break;
            }

            case "mirror-alternative-names":
            {
                IgdbHttpClient client = RequireClient();

                if(client == null)
                    return 1;

                var service = new IgdbAlternativeNameMirrorService(factory, client);
                int total = await service.RunAsync(batchSize, dryRun);
                Console.WriteLine($"\nDone. Mirrored {total} alternative names.");

                break;
            }

            case "match-platforms":
            {
                var matcher = new PlatformMatcher(factory);
                (int matched, int needsReview) = await matcher.RunAsync(dryRun);
                Console.WriteLine($"\nDone. Matched: {matched}, needs review: {needsReview}.");

                break;
            }

            case "match-companies":
            {
                var matcher = new Services.CompanyMatcher(factory);
                (int matched, int needsReview, int noMatch) = await matcher.RunAsync(dryRun);
                Console.WriteLine($"\nDone. Matched: {matched}, needs review: {needsReview}, no match: {noMatch}.");

                break;
            }

            case "match-games":
            {
                var matcher = new GameMatcher(factory);
                (int matched, int needsReview, int noMatch) = await matcher.RunAsync(dryRun);
                Console.WriteLine($"\nDone. Matched: {matched}, needs review: {needsReview}, no match: {noMatch}.");

                break;
            }

            case "enrich-companies":
            {
                string logoCachePath = config.GetValue("LogoCachePath", "state/igdb-logo-cache");
                var enricher = new CompanyEnricherService(factory, logoCachePath);
                CompanyEnricherService.Stats stats = await enricher.RunAsync(dryRun);

                Console.WriteLine($"""

                                    Done. Processed {stats.CompaniesProcessed} companies ({stats.CompaniesSkippedNoLocalMatch} skipped, no local match).
                                      Countries filled:     {stats.CountriesFilled}
                                      Websites filled:      {stats.WebsitesFilled}
                                      Founded dates filled: {stats.FoundedFilled}
                                      Status updated:       {stats.StatusFilled}
                                      Sold dates filled:    {stats.SoldFilled}
                                      SoldTo resolved:      {stats.SoldToFilled}
                                      Descriptions added:   {stats.DescriptionsAdded}
                                      Logos cached:         {stats.LogosCached}
                                    """);

                break;
            }

            case "enrich-platforms":
            {
                string assetRootPath = config.GetValue<string>("AssetRootPath");

                if(string.IsNullOrEmpty(assetRootPath))
                {
                    Console.WriteLine("\e[31;1mMissing AssetRootPath in appsettings.json\e[0m");

                    return 1;
                }

                var enricher = new PlatformEnricherService(factory, assetRootPath);
                PlatformEnricherService.Stats stats = await enricher.RunAsync(dryRun);

                Console.WriteLine($"""

                                    Done. Processed {stats.PlatformsProcessed} platforms ({stats.PlatformsSkippedNoLocalMatch} skipped, no local match).
                                      Logos downloaded:        {stats.LogosDownloaded}
                                      Logos already present:   {stats.LogosSkippedAlreadyPresent}
                                    """);

                break;
            }

            case "stats":
            {
                var stats = new MatchStatsService(factory);
                await stats.PrintAsync();

                break;
            }

            case "review-ambiguous":
            {
                string type  = GetStringOption(args, "--type", null);
                int    limit = GetIntOption(args, "--limit", 20);

                if(type == null)
                {
                    Console.WriteLine("  --type companies|games|platforms is required.");

                    return 1;
                }

                var review = new AmbiguousReviewService(factory);
                await review.RunAsync(type, limit);

                break;
            }

            case "reset":
            {
                long?  igdbId = long.TryParse(GetStringOption(args, "--igdb-id", null), out long id) ? id : null;
                string type   = GetStringOption(args, "--type", null);

                if(igdbId == null || type == null)
                {
                    Console.WriteLine("  --igdb-id <id> and --type companies|companies-enrichment|games|platforms|platforms-enrichment are required.");

                    return 1;
                }

                await ResetAsync(factory, type, igdbId.Value);

                break;
            }

            default:
                PrintUsage();

                break;
        }

        return 0;

        IgdbHttpClient RequireClient()
        {
            if(string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                Console.WriteLine("\e[31;1mMissing Igdb:ClientId / Igdb:ClientSecret in appsettings.json\e[0m");

                return null;
            }

            return IgdbHttpClient.Create(clientId, clientSecret, tokenCachePath, requestsPerSecond,
                                          maxConcurrentRequests);
        }
    }

    static async Task ResetAsync(MarechaiContextFactory factory, string type, long igdbId)
    {
        await using MarechaiContext context = await factory.CreateDbContextAsync();

        switch(type)
        {
            case "platforms":
            {
                IgdbPlatform entity = await context.IgdbPlatforms.FirstOrDefaultAsync(p => p.Id == igdbId);

                if(entity != null)
                {
                    entity.MatchStatus        = IgdbMatchStatus.Pending;
                    entity.SoftwarePlatformId = null;
                    entity.MatchType          = null;
                    entity.MatchedOn          = null;
                }

                break;
            }

            case "companies":
            {
                IgdbCompany entity = await context.IgdbCompanies.FirstOrDefaultAsync(c => c.IgdbId == igdbId);

                if(entity != null)
                {
                    entity.MatchStatus     = IgdbMatchStatus.Pending;
                    entity.CompanyId       = null;
                    entity.MatchType       = null;
                    entity.MatchScore      = null;
                    entity.MatchedOn       = null;
                    entity.CandidatesJson  = null;
                }

                break;
            }

            case "companies-enrichment":
            {
                IgdbCompany entity = await context.IgdbCompanies.FirstOrDefaultAsync(c => c.IgdbId == igdbId);

                if(entity != null)
                {
                    entity.EnrichmentApplied = false;
                    entity.EnrichedOn        = null;
                }

                break;
            }

            case "platforms-enrichment":
            {
                IgdbPlatform entity = await context.IgdbPlatforms.FirstOrDefaultAsync(p => p.Id == igdbId);

                if(entity != null)
                {
                    entity.EnrichmentApplied = false;
                    entity.EnrichedOn        = null;
                }

                break;
            }

            case "games":
            {
                IgdbGame entity = await context.IgdbGames.FirstOrDefaultAsync(g => g.IgdbId == igdbId);

                if(entity != null)
                {
                    entity.MatchStatus    = IgdbMatchStatus.Pending;
                    entity.SoftwareId     = null;
                    entity.MatchType      = null;
                    entity.MatchScore     = null;
                    entity.MatchedOn      = null;
                    entity.CandidatesJson = null;
                }

                break;
            }

            default:
                Console.WriteLine($"  Unknown reset type \"{type}\".");

                return;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"  Reset {type} IGDB id {igdbId} to Pending.");
    }

    static int GetIntOption(string[] args, string name, int defaultValue)
    {
        for(int i = 0; i < args.Length - 1; i++)
            if(args[i] == name && int.TryParse(args[i + 1], out int value))
                return value;

        return defaultValue;
    }

    static string GetStringOption(string[] args, string name, string defaultValue)
    {
        for(int i = 0; i < args.Length - 1; i++)
            if(args[i] == name)
                return args[i + 1];

        return defaultValue;
    }

    static void PrintUsage()
    {
        Console.WriteLine("""
                             Usage: dotnet Marechai.Igdb.dll <command> [options]

                             Commands:
                               mirror-platforms          Pull IGDB /platforms
                               mirror-game-types          Pull IGDB /game_types
                               mirror-companies           Pull IGDB /companies (resumable)
                               mirror-games               Pull IGDB /games (resumable)
                               backfill-game-slugs       Fill in Slug for games mirrored before it was tracked (resumable)
                               mirror-involved-companies  Pull IGDB /involved_companies (resumable)
                               mirror-alternative-names  Pull IGDB /alternative_names (resumable)
                               match-platforms             Match mirrored platforms against SoftwarePlatform
                               match-companies              Match mirrored companies against Company
                               match-games                  Match mirrored games against Software
                               enrich-companies              Fill empty Company fields from matched IGDB data (resumable)
                               enrich-platforms               Download missing SoftwarePlatform logos from matched IGDB data (resumable)
                               stats                        Print match status counts
                               review-ambiguous --type <companies|games|platforms> [--limit N]
                               reset --igdb-id <id> --type <companies|companies-enrichment|games|platforms|platforms-enrichment>

                             Options:
                               --batch-size N   Mirror one batch of up to N rows (default Import:BatchSize).
                                                Pass 0 to mirror the entire remaining catalog in one run.
                               --dry-run        Don't write to the database
                             """);
    }
}
