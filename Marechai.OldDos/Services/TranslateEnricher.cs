using System;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.OldDos.Services;

/// <summary>Literal Russian→English translation pass. Crawled → Translated.</summary>
public sealed class TranslateEnricher
{
    const string SystemPrompt =
        "You are a precise Russian-to-English translator for software catalog entries. " +
        "Translate the provided Russian text to clear, literal English. Preserve technical " +
        "terms, product names, version numbers and dates verbatim. Do not summarize or " +
        "embellish. If the input is empty or only punctuation, return an empty string.";

    readonly IDbContextFactory<MarechaiContext> _factory;
    readonly OpenAiChatClient                   _openAi;

    public TranslateEnricher(IDbContextFactory<MarechaiContext> factory, OpenAiChatClient openAi)
    {
        _factory = factory;
        _openAi  = openAi;
    }

    public async Task<int> RunAsync(int limit)
    {
        await using MarechaiContext db = await _factory.CreateDbContextAsync();
        var rows = await db.OldDosSoftwares
                           .Where(s => s.Status == OldDosSoftwareStatus.Crawled)
                           .OrderBy(s => s.Id)
                           .Take(limit)
                           .ToListAsync();

        int done    = 0;
        int skipped = 0;
        foreach(OldDosSoftware row in rows)
        {
            // Refuse to silently advance rows that have nothing to translate — that hides crawler
            // bugs (we shipped one). Leave them in Crawled with a clear LastError so the admin's
            // "Has enrichment error" filter surfaces them and a re-crawl can fix the missing text.
            if(string.IsNullOrWhiteSpace(row.RussianDescription))
            {
                row.LastError = "translate: RussianDescription is empty — re-crawl this row " +
                                "before retrying. The site stores the description in the cell " +
                                "below the 'Описание' header; if that cell was empty on the " +
                                "source page nothing can be translated.";
                await db.SaveChangesAsync();
                skipped++;
                Console.WriteLine($"\e[33m  Skipped #{row.Id} ({row.Name}): no Russian description.\e[0m");
                continue;
            }

            try
            {
                row.EnglishDescriptionLiteral = await TranslateAsync(row.RussianDescription);
                row.Status                    = OldDosSoftwareStatus.Translated;
                row.LastError                 = null;
                await db.SaveChangesAsync();
                done++;
                Console.WriteLine($"  Translated #{row.Id}: {row.Name}");
            }
            catch(Exception ex)
            {
                row.LastError = "translate: " + ex.Message;
                await db.SaveChangesAsync();
                Console.WriteLine($"\e[31m  Translate failed #{row.Id}: {ex.Message}\e[0m");
            }
        }
        if(skipped > 0)
            Console.WriteLine($"\e[33m  {skipped} row(s) skipped with empty descriptions.\e[0m");
        return done;
    }

    async Task<string> TranslateAsync(string russian)
    {
        if(string.IsNullOrWhiteSpace(russian)) return string.Empty;
        return (await _openAi.CompleteAsync(SystemPrompt, russian))?.Trim() ?? string.Empty;
    }
}
