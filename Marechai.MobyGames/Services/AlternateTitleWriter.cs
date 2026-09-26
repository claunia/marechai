using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Writes parsed MobyGames alternate titles into <see cref="SoftwareAlternativeTitle" />.
///     Shared by the importer (fresh imports) and <see cref="AlternateTitlesBackfillService" />
///     (already-imported games) so both behave identically.
///     <para>
///         Strictly additive: rows are inserted when missing and a null/empty
///         <see cref="SoftwareAlternativeTitle.Comment" /> is filled in when MobyGames has one,
///         but an existing non-empty comment is never overwritten and no row is ever deleted —
///         hand-curated titles and comments must survive a re-run.
///     </para>
/// </summary>
public static class AlternateTitleWriter
{
    /// <summary>Number of rows inserted and comments filled by a single apply.</summary>
    public readonly record struct Result(int Inserted, int CommentsFilled)
    {
        public bool Changed => Inserted > 0 || CommentsFilled > 0;
    }

    /// <summary>
    ///     Applies <paramref name="parsed" /> to the alternate titles of
    ///     <paramref name="software" />. Does not call <c>SaveChangesAsync</c>: the caller decides
    ///     when to commit (and skips committing entirely on a dry run).
    /// </summary>
    public static async Task<Result> ApplyAsync(MarechaiContext context, Software software,
                                                List<ParsedAlternateTitle> parsed)
    {
        if(parsed is null || parsed.Count == 0) return new Result(0, 0);

        List<SoftwareAlternativeTitle> existing = await context.SoftwareAlternativeTitles
                                                              .Where(t => t.SoftwareId == software.Id)
                                                              .ToListAsync();

        // Rows added in this pass are not visible to a DB query, so track them here too.
        var byTitle = new Dictionary<string, SoftwareAlternativeTitle>(StringComparer.OrdinalIgnoreCase);

        foreach(SoftwareAlternativeTitle row in existing)
            byTitle.TryAdd(row.Title, row);

        int inserted       = 0;
        int commentsFilled = 0;

        foreach(ParsedAlternateTitle candidate in parsed)
        {
            string title = candidate.Title?.Trim();

            if(string.IsNullOrWhiteSpace(title)) continue;

            // MobyGames sometimes lists the canonical title as one of its own variants.
            if(string.Equals(title, software.Name, StringComparison.OrdinalIgnoreCase)) continue;

            // Title is capped at 255 chars and cannot be meaningfully truncated — a cut-off
            // title is not a title. Comments are prose, so truncating those is fine.
            if(title.Length > 255) continue;

            string comment = candidate.Comment?.Trim();

            if(comment?.Length > 500) comment = comment[..500];

            if(byTitle.TryGetValue(title, out SoftwareAlternativeTitle row))
            {
                if(!string.IsNullOrWhiteSpace(comment) && string.IsNullOrWhiteSpace(row.Comment))
                {
                    row.Comment = comment;
                    commentsFilled++;
                }

                continue;
            }

            var added = new SoftwareAlternativeTitle
            {
                SoftwareId = software.Id,
                Title      = title,
                Comment    = string.IsNullOrWhiteSpace(comment) ? null : comment
            };

            context.SoftwareAlternativeTitles.Add(added);
            byTitle[title] = added;
            inserted++;
        }

        return new Result(inserted, commentsFilled);
    }
}
