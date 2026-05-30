using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.OldDos.Parsers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.OldDos.Services;

/// <summary>
///     Top-down crawler. Walks the category tree breadth-first, then per category enumerates every
///     paginated listing page and upserts (category, software, versions). Dedupe is by
///     <see cref="OldDosSoftware.SourceUrl" /> — once a software is in our staging table, we never
///     re-add it regardless of its current status (Discarded blocks re-add too).
/// </summary>
public sealed class OldDosCrawler
{
    readonly OldDosHttpClient            _http;
    readonly IDbContextFactory<MarechaiContext> _factory;

    public OldDosCrawler(OldDosHttpClient http, IDbContextFactory<MarechaiContext> factory)
    {
        _http    = http;
        _factory = factory;
    }

    /// <summary>
    ///     Crawl starting from the configured root. Returns the number of new <see cref="OldDosSoftware" />
    ///     rows inserted.
    /// </summary>
    public async Task<int> CrawlAsync(int? rootCategoryId, int? maxNewSoftware, IEnumerable<int> seedCategoryIds)
    {
        int newCount = 0;
        var visited  = new HashSet<int>();
        var queue    = new Queue<int>();

        if(rootCategoryId.HasValue) queue.Enqueue(rootCategoryId.Value);
        foreach(int s in seedCategoryIds ?? Array.Empty<int>()) queue.Enqueue(s);

        // If nothing seeded, discover top-level categories by scraping the home page.
        if(queue.Count == 0)
        {
            string home = await _http.FetchPageAsync("/");
            foreach(int cat in DiscoverTopLevelCategoryIds(home))
                queue.Enqueue(cat);
        }

        while(queue.Count > 0)
        {
            if(maxNewSoftware.HasValue && newCount >= maxNewSoftware.Value) break;

            int catId = queue.Dequeue();
            if(!visited.Add(catId)) continue;

            Console.WriteLine($"\e[36m  Crawling category {catId}\e[0m");
            try
            {
                int added = await CrawlCategoryAsync(catId, queue, visited, maxNewSoftware - newCount);
                newCount += added;
                Console.WriteLine($"    +{added} new software (cumulative {newCount})");
            }
            catch(Exception ex)
            {
                // Walk the InnerException chain — EF wraps the real DB error inside
                // DbUpdateException → MySqlException. The outer message alone ("An error
                // occurred while saving the entity changes") is useless on its own.
                Console.WriteLine($"\e[31m    Category {catId} failed: {FormatExceptionChain(ex)}\e[0m");
            }
        }

        return newCount;
    }

    static string FormatExceptionChain(Exception ex)
    {
        var sb = new System.Text.StringBuilder();
        for(Exception cur = ex; cur != null; cur = cur.InnerException)
        {
            if(sb.Length > 0) sb.Append(" -> ");
            sb.Append(cur.GetType().Name).Append(": ").Append(cur.Message);
        }
        return sb.ToString();
    }

    static IEnumerable<int> DiscoverTopLevelCategoryIds(string html)
    {
        if(string.IsNullOrEmpty(html)) yield break;
        var seen = new HashSet<int>();
        foreach(System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(
                    html, @"do=list&(?:amp;)?cat=(\d+)"))
            if(int.TryParse(m.Groups[1].Value, out int id) && seen.Add(id))
                yield return id;
    }

    async Task<int> CrawlCategoryAsync(int categoryId, Queue<int> queue, HashSet<int> visited, int? remainingBudget)
    {
        int newCount = 0;
        var listingIds = new Queue<int>();
        listingIds.Enqueue(0); // 0 == first page (no id qualifier; old-dos.ru also serves the same content for id=1)
        var seenListing = new HashSet<int> { 0, 1 };

        while(listingIds.Count > 0)
        {
            if(remainingBudget is <= 0) break;
            int pageId = listingIds.Dequeue();
            string url = pageId == 0
                ? $"/index.php?page=files&mode=files&do=list&cat={categoryId}"
                : $"/index.php?page=files&mode=files&do=list&cat={categoryId}&id={pageId}";

            string html = await _http.FetchPageAsync(url);
            if(html == null) break;

            CategoryPageHeader header = CategoryListPageParser.ParseHeader(categoryId, html);
            await UpsertCategoryAsync(header);

            foreach(int sub in header.SubcategoryIds)
                if(!visited.Contains(sub))
                    queue.Enqueue(sub);

            foreach(int p in header.NextPageIds)
                if(seenListing.Add(p)) listingIds.Enqueue(p);

            List<CategorySoftwareEntry> entries = CategoryListPageParser.ParseEntries(html);
            // pageId==0 is our sentinel for "no id parameter" == site page 1; surface that in the log
            // so it doesn't look like a missing page when later links jump to id=2.
            Console.WriteLine($"    {entries.Count} entries on page {(pageId == 0 ? "1 (default)" : pageId.ToString())}");

            foreach(CategorySoftwareEntry entry in entries)
            {
                if(remainingBudget is <= 0) break;

                bool inserted = await UpsertSoftwareAsync(entry, categoryId);
                if(inserted)
                {
                    newCount++;
                    remainingBudget--;
                }
            }
        }

        return newCount;
    }

    async Task UpsertCategoryAsync(CategoryPageHeader header)
    {
        if(header.Breadcrumb == null || header.Breadcrumb.Count == 0) return;

        await using MarechaiContext db = await _factory.CreateDbContextAsync();
        int? parentId = null;
        foreach((int id, string name) in header.Breadcrumb)
        {
            OldDosCategory existing = await db.OldDosCategories.FirstOrDefaultAsync(c => c.Id == id);
            string path = string.Join(" >> ", header.Breadcrumb
                                                    .TakeWhile(b => b.Id != id)
                                                    .Select(b => b.Name)
                                                    .Concat(new[] { name }));

            if(existing == null)
            {
                db.OldDosCategories.Add(new OldDosCategory
                {
                    Id            = id,
                    ParentId      = parentId,
                    RussianName   = name,
                    Path          = path,
                    LastCrawledOn = id == header.CurrentCategoryId ? DateTime.UtcNow : null
                });
            }
            else
            {
                existing.RussianName = name;
                existing.ParentId    = parentId;
                existing.Path        = path;
                if(id == header.CurrentCategoryId) existing.LastCrawledOn = DateTime.UtcNow;
            }

            parentId = id;
        }

        await db.SaveChangesAsync();
    }

    async Task<bool> UpsertSoftwareAsync(CategorySoftwareEntry entry, int categoryId)
    {
        await using MarechaiContext db = await _factory.CreateDbContextAsync();

        // Dedupe rule: ANY existing row for this SourceUrl blocks re-add, even Discarded.
        if(await db.OldDosSoftwares.AnyAsync(s => s.SourceUrl == entry.SourceUrl)) return false;

        string detailHtml = await _http.FetchPageAsync(entry.SourceUrl);
        SoftwarePageData detail = SoftwarePageParser.Parse(entry.SourceId, detailHtml ?? "");

        var row = new OldDosSoftware
        {
            SourceId            = entry.SourceId,
            SourceUrl           = entry.SourceUrl,
            Status              = OldDosSoftwareStatus.Crawled,
            Name                = SoftwarePageParser.Cap(detail.Name ?? entry.Name ?? $"#{entry.SourceId}", 512),
            DeveloperName       = SoftwarePageParser.Cap(detail.DeveloperName ?? entry.DeveloperName, 512),
            OsName              = SoftwarePageParser.Cap(detail.OsName ?? entry.OsName, 256),
            RussianDescription  = string.IsNullOrWhiteSpace(detail.RussianDescription)
                                      ? entry.ShortRussianDesc
                                      : detail.RussianDescription,
            RussianCategoryPath = SoftwarePageParser.Cap(detail.BreadcrumbPath, 2048),
            OldDosCategoryId    = detail.CategoryId ?? categoryId,
            CrawledOn           = DateTime.UtcNow
        };
        db.OldDosSoftwares.Add(row);
        await db.SaveChangesAsync();

        foreach(SoftwareVersionEntry v in detail.Versions)
        {
            db.OldDosVersions.Add(new OldDosVersion
            {
                OldDosSoftwareId     = row.Id,
                VersionString        = string.IsNullOrWhiteSpace(v.VersionString) ? "(unspecified)" : v.VersionString,
                ReleaseDate          = v.ReleaseDate,
                ReleaseDatePrecision = v.ReleaseDatePrecision,
                OsHint               = row.OsName,
                DownloadUrl          = NormalizeDownloadUrl(v.DownloadUrl),
                FileName             = v.FileName,
                Notes                = v.Notes,
                IsEnabledByDefault   = true
            });
        }

        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    ///     Normalize an old-dos.ru download URL to a host-relative path. The site emits absolute
    ///     URLs (<c>http://old-dos.ru/dl.php?id=4</c>); we store the path-and-query so consumers
    ///     (Blazor review dialog, future re-download) can prefix any scheme/host they want.
    /// </summary>
    static string NormalizeDownloadUrl(string url)
    {
        if(string.IsNullOrWhiteSpace(url)) return url;
        if(Uri.TryCreate(url, UriKind.Absolute, out Uri abs) &&
           abs.Host.EndsWith("old-dos.ru", StringComparison.OrdinalIgnoreCase))
            return abs.PathAndQuery;
        return url;
    }
}
