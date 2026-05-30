using System;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.OldDos.Services;

/// <summary>Museum-grade description rewrite pass. Translated → Described.</summary>
public sealed class DescribeEnricher
{
    const string SystemPrompt =
        "You are a museum curator writing exhibit descriptions for vintage software. " +
        "Given the user-written translation and any factual context provided, produce a " +
        "thorough English description in Markdown describing the software's purpose, " +
        "platform, era, distinctive features, technical details, reception, and " +
        "historical context. Use multiple paragraphs; the longer and richer the better, " +
        "as long as every claim is supported by the literal translation, the Wikipedia " +
        "summary, or the search snippet. Use neutral, encyclopedic language. Do not " +
        "invent specific dates, version numbers, sales figures, or quotes that are not " +
        "present in the provided context — when context is thin, stay close to the " +
        "literal translation rather than padding with speculation. Output plain Markdown " +
        "(paragraphs separated by blank lines); do not add a title or headings.";

    readonly IDbContextFactory<MarechaiContext> _factory;
    readonly OpenAiChatClient                   _openAi;
    readonly OldDosHttpClient                   _http;
    readonly bool                               _wikipediaEnabled;
    readonly bool                               _searchEnabled;
    readonly string                             _searchEngineUrl;

    public DescribeEnricher(IDbContextFactory<MarechaiContext> factory, OpenAiChatClient openAi,
                            OldDosHttpClient http, bool wikipediaEnabled, bool searchEnabled,
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
        var rows = await db.OldDosSoftwares
                           .Where(s => s.Status == OldDosSoftwareStatus.Translated)
                           .OrderBy(s => s.Id)
                           .Take(limit)
                           .ToListAsync();

        int done = 0;
        foreach(OldDosSoftware row in rows)
        {
            try
            {
                string wikipedia = _wikipediaEnabled ? await FetchWikipediaSummaryAsync(row.Name) : null;
                string search    = _searchEnabled    ? await FetchSearchSnippetAsync(row.Name)    : null;

                string userPrompt = $"""
                    Software name: {row.Name}
                    Operating system on old-dos.ru: {row.OsName}
                    Developer (raw from old-dos.ru): {row.DeveloperName}
                    Category path: {row.RussianCategoryPath}

                    Literal English translation of the original Russian description:
                    {row.EnglishDescriptionLiteral}

                    Wikipedia summary (may be empty or unrelated):
                    {wikipedia}

                    Web search snippet (may be empty or unrelated):
                    {search}
                    """;

                row.EnglishDescriptionMuseum = (await _openAi.CompleteAsync(SystemPrompt, userPrompt))?.Trim();
                row.MuseumDescriptionPromptVersion = 1;
                row.Status                  = OldDosSoftwareStatus.Described;
                row.LastError               = null;
                await db.SaveChangesAsync();
                done++;
                Console.WriteLine($"  Described #{row.Id}: {row.Name}");
            }
            catch(Exception ex)
            {
                row.LastError = "describe: " + ex.Message;
                await db.SaveChangesAsync();
                Console.WriteLine($"\e[31m  Describe failed #{row.Id}: {ex.Message}\e[0m");
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
            c.DefaultRequestHeaders.Add("User-Agent", "Marechai-OldDos-Importer/1.0");
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
            string url = _searchEngineUrl + Uri.EscapeDataString(name + " software vintage");
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
