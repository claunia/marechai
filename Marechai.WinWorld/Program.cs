using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.WinWorld.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Marechai.WinWorld;

internal static class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("\e[32;1mMarechai WinWorldPC import tool\e[0m");

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
            case "crawl":              return await RunCrawlAsync(args, config, factory);
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
              crawl [--section <applications|dev|sys|all>] [--max-new <n>] [--start-page <n>] [--end-page <n>]
                  Walk a winworldpc.com library section and upsert WwpcSoftware/WwpcVersion/WwpcScreenshot rows.
                  Dedupe is by SourceUrl regardless of current row status. The OS and Game sections are NEVER
                  crawled by this tool. Default section is "all" (applications + dev + sys, in that order).

              enrich-describe [--limit <n>]
                  For each Crawled row, generate a museum-grade English description using the raw description
                  plus Wikipedia and a search engine snippet. Moves Crawled → Described.

              enrich-categorize [--limit <n>]
                  For each Described row, ask OpenAI to pick the best Marechai category genres
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
        string baseUrl   = config["WinWorld:BaseUrl"]   ?? "https://winworldpc.com";
        int    delayMs   = int.TryParse(config["WinWorld:DelayMs"], out int d) ? d : 1500;
        string userAgent = config["WinWorld:UserAgent"] ?? "Marechai-WinWorld-Importer/1.0";
        string section   = (ArgStr(args, "--section", "all") ?? "all").ToLowerInvariant();
        int?   maxNew    = int.TryParse(ArgStr(args, "--max-new", null),    out int m)  ? m  : (int?)null;
        int?   startPage = int.TryParse(ArgStr(args, "--start-page", null), out int sp) ? sp : (int?)null;
        int?   endPage   = int.TryParse(ArgStr(args, "--end-page",  null),  out int ep) ? ep : (int?)null;

        var sections = new List<(string Slug, WwpcProductType Type)>();
        if(section == "all")
        {
            sections.Add(("applications", WwpcProductType.Application));
            sections.Add(("dev",          WwpcProductType.DevTool));
            sections.Add(("sys",          WwpcProductType.System));
        }
        else if(WwpcCrawler.SectionMap.TryGetValue(section, out WwpcProductType pt))
            sections.Add((section, pt));
        else
        {
            Console.WriteLine($"\e[31mUnknown section: {section}. Use applications | dev | sys | all.\e[0m");
            return 2;
        }

        using var http    = new WinWorldHttpClient(baseUrl, delayMs, userAgent);
        var       crawler = new WwpcCrawler(http, factory);
        int       total   = 0;
        foreach((string slug, WwpcProductType type) in sections)
        {
            Console.WriteLine($"\e[36;1m== section /library/{slug} ({type}) ==\e[0m");
            int remaining = maxNew.HasValue ? Math.Max(0, maxNew.Value - total) : (int?)null ?? int.MaxValue;
            int got       = await crawler.CrawlSectionAsync(slug, type, maxNew.HasValue ? remaining : (int?)null,
                                                            startPage, endPage);
            total += got;
            Console.WriteLine($"\e[32m== section {slug} done. +{got} new (cumulative {total}) ==\e[0m");
            if(maxNew.HasValue && total >= maxNew.Value) break;
        }
        Console.WriteLine($"\e[32mCrawl done. {total} new software inserted.\e[0m");
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
            case "describe":
            {
                string baseUrl   = config["WinWorld:BaseUrl"]   ?? "https://winworldpc.com";
                int    delayMs   = int.TryParse(config["WinWorld:DelayMs"], out int dd) ? dd : 1500;
                string userAgent = config["WinWorld:UserAgent"] ?? "Marechai-WinWorld-Importer/1.0";
                bool   wiki      = !bool.TryParse(config["Enrich:WebContext:WikipediaEnabled"], out bool w)  || w;
                bool   srch      = !bool.TryParse(config["Enrich:WebContext:SearchEnabled"],    out bool ss) || ss;
                string se        = config["Enrich:WebContext:SearchEngineUrl"] ?? "https://duckduckgo.com/html/?q=";
                using var http2  = new WinWorldHttpClient(baseUrl, delayMs, userAgent);
                var       e      = new DescribeEnricher(factory, openAi, http2, wiki, srch, se);
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
