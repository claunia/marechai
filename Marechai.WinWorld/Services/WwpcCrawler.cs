using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.WinWorld.Parsers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.WinWorld.Services;

/// <summary>
///     Walks the WinWorldPC library section-by-section, page-by-page. For each new product slug it
///     fetches the product page (which redirects to the first release), then every "Available release"
///     page for that product, then the screenshot index page for each release. Everything is upserted
///     into <see cref="WwpcSoftware" /> / <see cref="WwpcVersion" /> / <see cref="WwpcScreenshot" />.
///     Dedupe is by <see cref="WwpcSoftware.SourceUrl" />; existing rows are never re-imported.
/// </summary>
public sealed class WwpcCrawler
{
    /// <summary>Mapping of WinWorldPC library URL slug → product type enum.</summary>
    public static readonly Dictionary<string, WwpcProductType> SectionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["applications"] = WwpcProductType.Application,
        ["dev"]          = WwpcProductType.DevTool,
        ["sys"]          = WwpcProductType.System
    };

    readonly WinWorldHttpClient                _http;
    readonly IDbContextFactory<MarechaiContext> _factory;

    public WwpcCrawler(WinWorldHttpClient http, IDbContextFactory<MarechaiContext> factory)
    {
        _http    = http;
        _factory = factory;
    }

    /// <summary>
    ///     Crawl the given section. Returns the number of new <see cref="WwpcSoftware" /> rows inserted.
    /// </summary>
    public async Task<int> CrawlSectionAsync(string sectionSlug, WwpcProductType productType,
                                             int? maxNewSoftware, int? startPage, int? endPage)
    {
        int newCount = 0;
        int page     = startPage ?? 1;
        int maxPage  = int.MaxValue;

        while(page <= maxPage)
        {
            if(endPage.HasValue && page > endPage.Value) break;
            if(maxNewSoftware.HasValue && newCount >= maxNewSoftware.Value) break;

            Console.WriteLine($"\e[36m  /library/{sectionSlug}?page={page}\e[0m");
            string html = await _http.FetchPageAsync($"/library/{sectionSlug}?page={page}");
            if(html == null) break;

            LibraryListingPage listing = LibraryListingParser.Parse(html);
            if(page == 1) maxPage = Math.Max(listing.MaxPage, 1);

            Console.WriteLine($"    {listing.Entries.Count} entries on page {page} (maxPage={maxPage})");

            foreach(LibraryListingEntry entry in listing.Entries)
            {
                if(maxNewSoftware.HasValue && newCount >= maxNewSoftware.Value) break;
                try
                {
                    bool inserted = await UpsertSoftwareAsync(entry, productType);
                    if(inserted) newCount++;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"\e[31m    {entry.Slug} failed: {FormatExceptionChain(ex)}\e[0m");
                }
            }

            page++;
        }

        return newCount;
    }

    async Task<bool> UpsertSoftwareAsync(LibraryListingEntry entry, WwpcProductType productType)
    {
        // Dedupe: any existing row for this SourceUrl blocks re-add (even Discarded). Match either
        // the canonical /product/{slug} OR a /product/{slug}/{releaseSlug} (we store the canonical
        // /product/{slug} but tolerate either form being passed in).
        string canonical = entry.SourceUrl;
        await using(MarechaiContext predb = await _factory.CreateDbContextAsync())
        {
            if(await predb.WwpcSoftwares.AnyAsync(s => s.SourceUrl == canonical || s.Slug == entry.Slug &&
                                                       s.ProductType == productType))
                return false;
        }

        // Fetch product page (auto-redirects to first release). Use the final URL to seed the
        // current-release label; not strictly needed but useful for logs.
        (string finalUrl, string productHtml) = await _http.FetchPageWithFinalUrlAsync(canonical);
        if(productHtml == null)
        {
            Console.WriteLine($"\e[33m    skipping {entry.Slug} — no product page\e[0m");
            return false;
        }

        ProductReleasePageData firstReleaseData = ProductReleasePageParser.Parse(productHtml);
        // Description usually only appears on the FIRST release page; cache it.
        string description = firstReleaseData.Description;

        var versions    = new List<WwpcVersion>();
        var screenshots = new List<WwpcScreenshot>();

        // === First (active) release: harvest downloads + screenshots ===
        await HarvestReleaseAsync(firstReleaseData, firstReleaseData.CurrentReleaseLabel
                                                  ?? firstReleaseData.Releases.FirstOrDefault()?.Label,
                                  versions, screenshots);

        // === Walk the rest of the releases ===
        foreach(ProductReleaseLink rel in firstReleaseData.Releases)
        {
            if(rel.IsActive) continue;
            string relHtml = await _http.FetchPageAsync(rel.Url);
            if(relHtml == null) continue;
            ProductReleasePageData relData = ProductReleasePageParser.Parse(relHtml);
            // The description may differ per release but the spec said it's the same; keep the first.
            await HarvestReleaseAsync(relData, rel.Label, versions, screenshots);
        }

        // === Vendor auto-link (exact normalized match against Company.Name) ===
        int? vendorCompanyId = null;
        if(!string.IsNullOrWhiteSpace(firstReleaseData.VendorName))
        {
            string targetNorm = NormalizeName(firstReleaseData.VendorName);
            if(!string.IsNullOrEmpty(targetNorm))
            {
                await using MarechaiContext namedb = await _factory.CreateDbContextAsync();
                List<(int Id, string Name)> candidates = await namedb.Companies
                                                                     .Where(c => c.Name != null)
                                                                     .Select(c => new ValueTuple<int, string>(c.Id, c.Name))
                                                                     .ToListAsync();
                List<int> matched = candidates.Where(c => NormalizeName(c.Name) == targetNorm)
                                              .Select(c => c.Id).ToList();
                if(matched.Count == 1) vendorCompanyId = matched[0];
            }
        }

        // === Screenshot platform heuristic ===
        ulong? defaultPlatformId = null;
        if(firstReleaseData.Platforms.Count == 1)
        {
            string platName = MapPlatformName(firstReleaseData.Platforms[0]);
            await using MarechaiContext platdb = await _factory.CreateDbContextAsync();
            List<(ulong Id, string Name)> plats = await platdb.SoftwarePlatforms
                                                              .Where(p => p.Name != null)
                                                              .Select(p => new ValueTuple<ulong, string>(p.Id, p.Name))
                                                              .ToListAsync();
            List<ulong> hits = plats.Where(p => string.Equals(p.Name, platName, StringComparison.OrdinalIgnoreCase))
                                    .Select(p => p.Id).ToList();
            if(hits.Count == 1) defaultPlatformId = hits[0];
        }

        // === Insert software + versions + screenshots ===
        await using MarechaiContext db = await _factory.CreateDbContextAsync();

        // Re-check inside the writing transaction (cheap race-condition guard).
        if(await db.WwpcSoftwares.AnyAsync(s => s.SourceUrl == canonical)) return false;

        var row = new WwpcSoftware
        {
            SourceUrl                = Cap(canonical, 1024),
            Slug                     = Cap(entry.Slug, 256),
            Status                   = WwpcSoftwareStatus.Crawled,
            ProductType              = productType,
            Name                     = Cap(firstReleaseData.ProductName ?? entry.Name ?? entry.Slug, 512),
            VendorName               = Cap(firstReleaseData.VendorName, 512),
            VendorUrl                = Cap(firstReleaseData.VendorUrl, 1024),
            RawCategoriesCsv         = Cap(string.Join(",", CombineDistinct(entry.Categories, firstReleaseData.Categories)), 1024),
            PlatformsCsv             = Cap(string.Join(",", CombineDistinct(entry.Platforms, firstReleaseData.Platforms)), 256),
            ReleaseDateText          = Cap(firstReleaseData.ReleaseDateText, 64),
            UserInterface            = Cap(firstReleaseData.UserInterface, 32),
            RawDescription           = string.IsNullOrWhiteSpace(description) ? entry.ShortDescription : description,
            SuggestedVendorCompanyId = vendorCompanyId,
            CrawledOn                = DateTime.UtcNow
        };
        db.WwpcSoftwares.Add(row);
        await db.SaveChangesAsync();

        foreach(WwpcVersion v in versions)
        {
            v.WwpcSoftwareId = row.Id;
            db.WwpcVersions.Add(v);
        }

        foreach(WwpcScreenshot s in screenshots)
        {
            s.WwpcSoftwareId              = row.Id;
            s.SuggestedSoftwarePlatformId = defaultPlatformId;
            db.WwpcScreenshots.Add(s);
        }

        await db.SaveChangesAsync();
        Console.WriteLine($"\e[32m    +{row.Name} ({versions.Count} versions, {screenshots.Count} screenshots, vendor={vendorCompanyId?.ToString() ?? "?"})\e[0m");
        return true;
    }

    async Task HarvestReleaseAsync(ProductReleasePageData data, string majorLabel,
                                   List<WwpcVersion> versions, List<WwpcScreenshot> screenshots)
    {
        string major = string.IsNullOrWhiteSpace(majorLabel) ? "(unspecified)" : majorLabel.Trim();
        // Active release URL (the one currently displayed on the page); used purely for display.
        string majorUrl = data.Releases.FirstOrDefault(r => r.IsActive)?.Url;

        foreach(ProductDownload d in data.Downloads)
        {
            versions.Add(new WwpcVersion
            {
                MajorRelease       = Cap(major, 64),
                MajorReleaseUrl    = Cap(majorUrl, 1024),
                VersionString      = Cap(string.IsNullOrWhiteSpace(d.VersionString) ? "(unspecified)" : d.VersionString, 128),
                Language           = Cap(d.Language, 64),
                Architecture       = Cap(d.Architecture, 64),
                MediaKind          = Cap(d.MediaKind, 128),
                SizeText           = Cap(d.SizeText, 32),
                DownloadUrl        = Cap(d.DownloadUrl, 2048),
                IsEnabledByDefault = true
            });
        }

        if(!string.IsNullOrEmpty(data.ScreenshotIndexUrl))
        {
            string shotHtml = await _http.FetchPageAsync(data.ScreenshotIndexUrl);
            List<ScreenshotIndexEntry> shots = ScreenshotIndexParser.Parse(shotHtml);
            foreach(ScreenshotIndexEntry s in shots)
            {
                if(string.IsNullOrEmpty(s.SourceUrl)) continue;
                if(screenshots.Any(x => x.SourceUrl == s.SourceUrl)) continue;
                screenshots.Add(new WwpcScreenshot
                {
                    MajorRelease       = Cap(major, 64),
                    SourceUrl          = Cap(s.SourceUrl, 1024),
                    ImageUrl           = Cap(s.ImageUrl, 1024),
                    Caption            = Cap(s.Caption, 1024),
                    IsEnabledByDefault = true,
                    CrawledOn          = DateTime.UtcNow
                });
            }
        }
    }

    /// <summary>Lowercase, strip diacritics, collapse to alphanumerics only.</summary>
    public static string NormalizeName(string s)
    {
        if(string.IsNullOrWhiteSpace(s)) return string.Empty;
        string lower = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var    sb    = new StringBuilder(lower.Length);
        foreach(char c in lower)
        {
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if(uc == UnicodeCategory.NonSpacingMark) continue;
            if(char.IsLetterOrDigit(c)) sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    ///     Translate a WinWorldPC platform chip label to the matching Marechai
    ///     <c>SoftwarePlatform.Name</c>. WinWorldPC uses a few abbreviated / legacy spellings
    ///     that do not match Marechai 1:1; the lookup falls back to the original string when no
    ///     mapping is known so unknown platforms still get the chance to match by exact name.
    /// </summary>
    public static string MapPlatformName(string wwpcName)
    {
        if(string.IsNullOrWhiteSpace(wwpcName)) return wwpcName;
        return wwpcName.Trim() switch
        {
            "MacOS" => "Macintosh",
            "OS2"   => "OS/2",
            _       => wwpcName.Trim()
        };
    }

    static IEnumerable<string> CombineDistinct(IEnumerable<string> a, IEnumerable<string> b)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach(string s in (a ?? Enumerable.Empty<string>()).Concat(b ?? Enumerable.Empty<string>()))
        {
            string t = (s ?? "").Trim();
            if(t.Length == 0) continue;
            if(seen.Add(t)) yield return t;
        }
    }

    static string Cap(string s, int max)
    {
        if(s == null) return null;
        s = s.Trim();
        return s.Length <= max ? s : s.Substring(0, max);
    }

    static string FormatExceptionChain(Exception ex)
    {
        var sb = new StringBuilder();
        for(Exception cur = ex; cur != null; cur = cur.InnerException)
        {
            if(sb.Length > 0) sb.Append(" -> ");
            sb.Append(cur.GetType().Name).Append(": ").Append(cur.Message);
        }
        return sb.ToString();
    }
}
