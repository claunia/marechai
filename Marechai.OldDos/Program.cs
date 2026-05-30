using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.OldDos.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Marechai.OldDos;

internal static class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("\e[32;1mMarechai old-dos.ru import tool\e[0m");

        IConfigurationRoot config = new ConfigurationBuilder()
                                   .AddJsonFile("appsettings.json", optional: false)
                                   .AddEnvironmentVariables()
                                   .Build();

        string conn = config.GetConnectionString("DefaultConnection");
        if(string.IsNullOrEmpty(conn))
        {
            Console.WriteLine("\e[31;1mMissing DefaultConnection in appsettings.json\e[0m");
            return 1;
        }

        var optionsBuilder = new DbContextOptionsBuilder<MarechaiContext>();
        optionsBuilder.UseLazyLoadingProxies()
                      .AddMarechaiInterceptors()
                      .UseMySql(conn, new MariaDbServerVersion(new Version(12, 0, 2)),
                                b => b.UseMicrosoftJson().EnableStringComparisonTranslations()
                                      .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        var factory = new MarechaiContextFactory(optionsBuilder.Options);

        string command = args.Length > 0 ? args[0].ToLowerInvariant() : "help";

        switch(command)
        {
            case "crawl":   return await RunCrawlAsync(args, config, factory);
            case "enrich-translate":   return await RunEnrichAsync(args, config, factory, "translate");
            case "enrich-describe":    return await RunEnrichAsync(args, config, factory, "describe");
            case "enrich-categorize":  return await RunEnrichAsync(args, config, factory, "categorize");
            case "help":
            case "-h":
            case "--help":
                PrintHelp();
                return 0;
            default:
                Console.WriteLine($"\e[31mUnknown command: {command}\e[0m");
                PrintHelp();
                return 2;
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine("""
            Commands:
              crawl [--root <id>] [--seed <id,id,…>] [--max-new <n>]
                  Walk old-dos.ru and upsert OldDosSoftware/OldDosVersion/OldDosCategory rows.
                  Dedupe is by SourceUrl regardless of current row status.

              enrich-translate [--limit <n>]
                  For each Crawled row, call OpenAI to translate the Russian description to
                  literal English. Moves Crawled → Translated.

              enrich-describe [--limit <n>]
                  For each Translated row, generate a museum-grade English description using
                  the literal translation + Wikipedia + a search engine snippet. Moves
                  Translated → Described.

              enrich-categorize [--limit <n>]
                  For each Described row, ask OpenAI to pick the best Marechai categories
                  (SoftwareGenre type=Category). Moves Described → ReadyForReview.
            """);
    }

    static int ArgInt(string[] args, string name, int @default)
    {
        for(int i = 0; i < args.Length - 1; i++)
            if(args[i] == name && int.TryParse(args[i + 1], out int v))
                return v;
        return @default;
    }

    static string ArgStr(string[] args, string name, string @default)
    {
        for(int i = 0; i < args.Length - 1; i++)
            if(args[i] == name) return args[i + 1];
        return @default;
    }

    static async Task<int> RunCrawlAsync(string[] args, IConfigurationRoot config, MarechaiContextFactory factory)
    {
        string baseUrl   = config["OldDos:BaseUrl"]   ?? "https://old-dos.ru";
        int    delayMs   = int.TryParse(config["OldDos:DelayMs"], out int d) ? d : 1500;
        string userAgent = config["OldDos:UserAgent"] ?? "Marechai-OldDos-Importer/1.0";
        int?   root      = int.TryParse(ArgStr(args, "--root", null), out int r) ? r : (int?)null;
        int?   maxNew    = int.TryParse(ArgStr(args, "--max-new", null), out int m) ? m : (int?)null;
        var seeds = (ArgStr(args, "--seed", null) ?? "")
                   .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                   .Select(s => int.TryParse(s, out int x) ? x : -1)
                   .Where(x => x > 0)
                   .ToList();

        using var http   = new OldDosHttpClient(baseUrl, delayMs, userAgent);
        var      crawler = new OldDosCrawler(http, factory);
        int      n       = await crawler.CrawlAsync(root, maxNew, seeds);
        Console.WriteLine($"\e[32mCrawl done. {n} new software inserted.\e[0m");
        return 0;
    }

    static async Task<int> RunEnrichAsync(string[] args, IConfigurationRoot config, MarechaiContextFactory factory,
                                          string kind)
    {
        int    limit  = ArgInt(args, "--limit", 25);
        string url    = config["OpenAI:Url"];
        string key    = config["OpenAI:ApiKey"];
        string model  = config["OpenAI:Model"];
        int    tos    = int.TryParse(config["OpenAI:TimeoutSeconds"], out int t) ? t : 600;
        int?   maxTok = int.TryParse(config["OpenAI:MaxTokens"], out int mt) ? mt : (int?)null;

        if(string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(model))
        {
            Console.WriteLine("\e[31;1mOpenAI:Url and OpenAI:Model must be configured.\e[0m");
            return 1;
        }

        using var openAi = new OpenAiChatClient(url, key, model, tos, maxTok);
        int n;
        switch(kind)
        {
            case "translate":
            {
                var e = new TranslateEnricher(factory, openAi);
                n = await e.RunAsync(limit);
                break;
            }
            case "describe":
            {
                string baseUrl   = config["OldDos:BaseUrl"]   ?? "https://old-dos.ru";
                int    delayMs   = int.TryParse(config["OldDos:DelayMs"], out int dd) ? dd : 1500;
                string userAgent = config["OldDos:UserAgent"] ?? "Marechai-OldDos-Importer/1.0";
                bool wiki  = !bool.TryParse(config["Enrich:WebContext:WikipediaEnabled"], out bool w) || w;
                bool srch  = !bool.TryParse(config["Enrich:WebContext:SearchEnabled"], out bool ss) || ss;
                string se  = config["Enrich:WebContext:SearchEngineUrl"] ?? "https://duckduckgo.com/html/?q=";
                using var http2 = new OldDosHttpClient(baseUrl, delayMs, userAgent);
                var e = new DescribeEnricher(factory, openAi, http2, wiki, srch, se);
                n = await e.RunAsync(limit);
                break;
            }
            case "categorize":
            {
                var e = new CategorizeEnricher(factory, openAi);
                n = await e.RunAsync(limit);
                break;
            }
            default:
                Console.WriteLine($"\e[31mUnknown enrich kind: {kind}\e[0m");
                return 2;
        }

        Console.WriteLine($"\e[32m{kind}: {n} rows updated.\e[0m");
        return 0;
    }
}
