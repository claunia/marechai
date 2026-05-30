using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.OldDos.Services;

/// <summary>Category-picker pass. Described → ReadyForReview. Uses JSON-schema enforced output.</summary>
public sealed class CategorizeEnricher
{
    const string SystemPrompt =
        "You assign vintage software to Marechai catalog categories. You will receive an " +
        "exhaustive allow-list of categories (id = name). Choose the 1 to 3 ids that best " +
        "describe what the software IS, not what it does with files.\n" +
        "\n" +
        "Rules:\n" +
        "- Pick the closest fit even if it is imperfect. Returning an empty array is reserved " +
        "  for inputs that are NOT software (e.g. blank pages, advertisements, broken entries).\n" +
        "- Operating systems, DOS distributions, kernels and bootable system disks → 'Operating Systems'.\n" +
        "- Plain device drivers, TSRs and kernel modules → 'Device Drivers' or 'Kernel Extensions & Modules'.\n" +
        "- Generic 'system utility', 'TSR toolkit', 'CONFIG.SYS helper' and similar → 'System Utilities'.\n" +
        "- Programming languages, compilers, assemblers, debuggers, IDEs → the matching dev category.\n" +
        "- Office, document, spreadsheet and database tools → their respective office/database category.\n" +
        "- Games of any genre → 'Video Games' (use sub-genres only if very clearly applicable).\n" +
        "- If the software is a hybrid (e.g. a small OS shipped with utilities), include both ids " +
        "  (e.g. Operating Systems + System Utilities).\n" +
        "- Use ONLY ids that appear in the allow-list. Never invent ids.\n" +
        "\n" +
        "Reply with JSON matching the given schema.";

    static readonly object ResponseSchema = new
    {
        name   = "category_picks",
        strict = true,
        schema = new
        {
            type                 = "object",
            additionalProperties = false,
            required             = new[] { "category_ids" },
            properties           = new
            {
                category_ids = new
                {
                    type    = "array",
                    items   = new { type = "integer" },
                    maxItems = 5
                }
            }
        }
    };

    readonly IDbContextFactory<MarechaiContext> _factory;
    readonly OpenAiChatClient                   _openAi;

    public CategorizeEnricher(IDbContextFactory<MarechaiContext> factory, OpenAiChatClient openAi)
    {
        _factory = factory;
        _openAi  = openAi;
    }

    public async Task<int> RunAsync(int limit)
    {
        await using MarechaiContext db = await _factory.CreateDbContextAsync();
        var rows = await db.OldDosSoftwares
                           .Where(s => s.Status == OldDosSoftwareStatus.Described)
                           .OrderBy(s => s.Id)
                           .Take(limit)
                           .ToListAsync();

        if(rows.Count == 0) return 0;

        var categories = await db.SoftwareGenres
                                 .Where(g => g.Type == SoftwareGenreType.Category)
                                 .OrderBy(g => g.Name)
                                 .Select(g => new { g.Id, g.Name })
                                 .ToListAsync();

        var allow = string.Join("\n", categories.Select(c => $"  {c.Id} = {c.Name}"));

        int done = 0;
        foreach(OldDosSoftware row in rows)
        {
            try
            {
                string userPrompt = $"""
                    Allow-list (id = name):
                    {allow}

                    Software: {row.Name}
                    Old-dos.ru category path: {row.RussianCategoryPath}
                    Museum description:
                    {row.EnglishDescriptionMuseum}
                    """;

                string raw = await _openAi.CompleteAsync(SystemPrompt, userPrompt, ResponseSchema);
                var picked = ExtractIds(raw)
                            .Where(id => categories.Any(c => c.Id == id))
                            .Distinct()
                            .Take(5)
                            .ToList();

                row.SuggestedGenreIdsJson = JsonSerializer.Serialize(picked);
                row.Status                = OldDosSoftwareStatus.ReadyForReview;
                row.LastError             = null;
                await db.SaveChangesAsync();
                done++;
                Console.WriteLine($"  Categorized #{row.Id}: {row.Name} → [{string.Join(", ", picked)}]");
            }
            catch(Exception ex)
            {
                row.LastError = "categorize: " + ex.Message;
                await db.SaveChangesAsync();
                Console.WriteLine($"\e[31m  Categorize failed #{row.Id}: {ex.Message}\e[0m");
            }
        }
        return done;
    }

    static IEnumerable<int> ExtractIds(string json)
    {
        if(string.IsNullOrWhiteSpace(json)) yield break;
        JsonDocument doc;
        try { doc = JsonDocument.Parse(json); }
        catch { yield break; }
        using(doc)
        {
            if(!doc.RootElement.TryGetProperty("category_ids", out JsonElement arr) ||
               arr.ValueKind != JsonValueKind.Array)
                yield break;
            foreach(JsonElement el in arr.EnumerateArray())
                if(el.TryGetInt32(out int id)) yield return id;
        }
    }
}
