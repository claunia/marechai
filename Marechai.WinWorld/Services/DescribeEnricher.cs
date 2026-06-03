using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.WinWorld.Services;

/// <summary>
///     Museum-grade description rewrite pass. WinWorldPC entries are already in English so there is
///     no translation step — this enricher just expands the raw description into a richer one using
///     Wikipedia + a web-search snippet as supporting context. Crawled → Described.
/// </summary>
public sealed class DescribeEnricher
{
    const int PromptVersion = 1;

    const string SystemPrompt =
        "You are a museum curator writing exhibit descriptions for vintage software. " +
        "Given the original WinWorldPC entry text and any factual context provided, produce a " +
        "thorough English description in Markdown describing the software's purpose, " +
        "platform, era, distinctive features, technical details, reception, and " +
        "historical context. Use multiple paragraphs; the longer and richer the better, " +
        "as long as every claim is supported by the WinWorldPC description, the Wikipedia " +
        "summary, or the search snippet. Use neutral, encyclopedic language. Do not " +
        "invent specific dates, version numbers, sales figures, or quotes that are not " +
        "present in the provided context — when context is thin, stay close to the " +
        "WinWorldPC text rather than padding with speculation. Output plain Markdown " +
        "(paragraphs separated by blank lines); do not add a title or headings.";

    readonly IDbContextFactory<MarechaiContext> _factory;
    readonly OpenAiChatClient                   _openAi;
    readonly WinWorldHttpClient                 _http;
    readonly bool                               _wikipediaEnabled;
    readonly bool                               _searchEnabled;
    readonly string                             _searchEngineUrl;

    public DescribeEnricher(IDbContextFactory<MarechaiContext> factory, OpenAiChatClient openAi,
                            WinWorldHttpClient http, bool wikipediaEnabled, bool searchEnabled,
                            string searchEngineUrl)
    {
        _factory          = factory;
        _openAi           = openAi;
        _http             = http;
        _wikipediaEnabled = wikipediaEnabled;
        _searchEnabled    = searchEnabled;
        _searchEngineUrl  = searchEngineUrl;
    }

    public async Task<int> RunAsync(int limit)
    {
        await using MarechaiContext db = await _factory.CreateDbContextAsync();
        var rows = await db.WwpcSoftwares
                           .Where(s => s.Status == WwpcSoftwareStatus.Crawled)
                           .OrderBy(s => s.Id)
                           .Take(limit)
                           .ToListAsync();

        Console.WriteLine($"\e[36mDescribe: {rows.Count} Crawled row(s) to process (limit {limit}).\e[0m");

        int done = 0;
        for(int idx = 0; idx < rows.Count; idx++)
        {
            WwpcSoftware row = rows[idx];
            try
            {
                Console.WriteLine($"\e[36m[{idx + 1}/{rows.Count}] #{row.Id} {row.Name}\e[0m");

                string wikipedia = null;
                if(_wikipediaEnabled)
                {
                    Console.Write("    wikipedia… ");
                    wikipedia = await FetchWikipediaSummaryAsync(row.Name);
                    Console.WriteLine(wikipedia is null ? "(none)" : $"{wikipedia.Length} chars");
                }
                string search = null;
                if(_searchEnabled)
                {
                    Console.Write("    search…    ");
                    search = await FetchSearchSnippetAsync(row.Name);
                    Console.WriteLine(search is null ? "(none)" : $"{search.Length} chars");
                }

                string userPrompt = $"""
                    Software name: {row.Name}
                    Vendor (raw from WinWorldPC): {row.VendorName}
                    Product type: {row.ProductType}
                    Raw categories: {row.RawCategoriesCsv}
                    Platforms: {row.PlatformsCsv}
                    Release date (raw): {row.ReleaseDateText}

                    Original description from WinWorldPC:
                    {row.RawDescription}

                    Wikipedia summary (may be empty or unrelated):
                    {wikipedia}

                    Web search snippet (may be empty or unrelated):
                    {search}
                    """;

                Console.Write("    openai…    ");
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                row.EnglishDescriptionMuseum       = (await _openAi.CompleteAsync(SystemPrompt, userPrompt))?.Trim();
                row.MuseumDescriptionPromptVersion = PromptVersion;
                row.Status                         = WwpcSoftwareStatus.Described;
                row.LastError                      = null;
                await db.SaveChangesAsync();
                done++;
                Console.WriteLine($"\e[32m{sw.ElapsedMilliseconds} ms, {row.EnglishDescriptionMuseum?.Length ?? 0} chars → Described\e[0m");
            }
            catch(Exception ex)
            {
                row.LastError = "describe: " + ex.Message;
                await db.SaveChangesAsync();
                Console.WriteLine($"\e[31m    FAILED: {ex.Message}\e[0m");
            }
        }
        return done;
    }

    async Task<string> FetchWikipediaSummaryAsync(string name)
    {
        if(string.IsNullOrWhiteSpace(name)) return null;
        string url = $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(name)}";
        try
        {
            using var c = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            c.DefaultRequestHeaders.Add("User-Agent", "Marechai-WinWorld-Importer/1.0");
            var resp = await c.GetAsync(url);
            if(!resp.IsSuccessStatusCode) return null;
            using JsonDocument doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            return doc.RootElement.TryGetProperty("extract", out JsonElement extract) ? extract.GetString() : null;
        }
        catch { return null; }
    }

    async Task<string> FetchSearchSnippetAsync(string name)
    {
        if(string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(_searchEngineUrl)) return null;
        try
        {
            string url  = _searchEngineUrl + Uri.EscapeDataString(name + " software vintage");
            string html = await _http.FetchPageAsync(url);
            if(string.IsNullOrEmpty(html)) return null;
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode snippet = doc.DocumentNode.SelectSingleNode("//a[contains(@class,'result__snippet')]") ??
                               doc.DocumentNode.SelectSingleNode("//*[contains(@class,'snippet')]");
            string text = snippet == null ? null : HttpUtility.HtmlDecode(snippet.InnerText);
            return string.IsNullOrWhiteSpace(text) ? null : Regex.Replace(text.Trim(), @"\s+", " ");
        }
        catch { return null; }
    }
}
