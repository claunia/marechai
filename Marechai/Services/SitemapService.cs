/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Marechai.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Marechai.Services;

/// <summary>
///     Generates XML sitemaps for SEO by querying the backend API for all public entity IDs and
///     combining them with the static routes defined by the Blazor <c>@page</c> directives.
///     Results are cached for 24 hours via <see cref="IMemoryCache" />.
/// </summary>
public sealed class SitemapService(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<SitemapService> logger)
{
    const    string Namespace       = "http://www.sitemaps.org/schemas/sitemap/0.9";
    const    int    UrlsPerSitemap  = 10_000;
    static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    /// <summary>Entity types with their API path, URL prefix, and sitemap slug.</summary>
    static readonly (string ApiPath, string UrlPrefix, string Slug)[] EntityTypes =
    [
        ("machines",     "/machine/",    "machines"),
        ("companies",    "/company/",    "companies"),
        ("software",     "/software/",   "software"),
        ("books",        "/book/",       "books"),
        ("documents",    "/document/",   "documents"),
        ("magazines",    "/magazine/",   "magazines"),
        ("gpus",         "/gpu/",        "gpus"),
        ("sound-synths", "/soundsynth/", "soundsynths"),
        ("processors",   "/processor/",  "processors"),
        ("people",       "/person/",     "people")
    ];

    /// <summary>Generates a sitemap index that lists per-section sub-sitemaps, paginated for large entity sets.</summary>
    public async Task<string> GenerateSitemapIndexAsync()
    {
        const string cacheKey = "sitemap:index";

        if(cache.TryGetValue(cacheKey, out string? cached) && cached is not null)
            return cached;

        HttpClient client = httpClientFactory.CreateClient("SitemapApi");

        using var ms     = new MemoryStream();
        using var writer = XmlWriter.Create(ms, WriterSettings());

        writer.WriteStartDocument();
        writer.WriteStartElement("sitemapindex", Namespace);

        // Static sitemap (always one)
        WriteSitemapEntry(writer, "static");

        // Entity sitemaps — query count to determine page count
        foreach((string apiPath, _, string slug) in EntityTypes)
        {
            int count = await GetEntityCountAsync(client, apiPath);
            int pages = Math.Max(1, (int)Math.Ceiling(count / (double)UrlsPerSitemap));

            if(pages == 1)
            {
                WriteSitemapEntry(writer, slug);
            }
            else
            {
                for(int page = 1; page <= pages; page++)
                    WriteSitemapEntry(writer, $"{slug}-{page}");
            }
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();

        string xml = Encoding.UTF8.GetString(ms.ToArray());
        cache.Set(cacheKey, xml, CacheDuration);

        return xml;
    }

    /// <summary>Generates a sub-sitemap for the given section (e.g. "static", "software", "software-3").</summary>
    public async Task<string> GenerateSectionSitemapAsync(string section)
    {
        if(string.Equals(section, "static", StringComparison.OrdinalIgnoreCase))
            return GenerateStaticSitemap();

        // Parse "software-3" → slug="software", page=3; "software" → slug="software", page=1
        string slug = section.ToLowerInvariant();
        int    page = 1;

        int lastDash = slug.LastIndexOf('-');

        if(lastDash > 0 && int.TryParse(slug[(lastDash + 1)..], out int parsed) && parsed >= 1)
        {
            slug = slug[..lastDash];
            page = parsed;
        }

        foreach((string apiPath, string urlPrefix, string entitySlug) in EntityTypes)
        {
            if(string.Equals(slug, entitySlug, StringComparison.OrdinalIgnoreCase))
                return await GenerateEntitySitemapAsync(apiPath, urlPrefix, page);
        }

        return GenerateStaticSitemap();
    }

    /// <summary>Sitemap for all static and browse/category pages.</summary>
    string GenerateStaticSitemap()
    {
        string[] staticRoutes =
        [
            "/",
            "/about",
            "/contact",
            "/search",
            "/search/advanced",

            // Computers
            "/computers",
            "/computers/all",
            "/computers/companies",

            // Consoles
            "/consoles",
            "/consoles/all",
            "/consoles/companies",

            // Tablets
            "/tablets",
            "/tablets/all",
            "/tablets/companies",

            // PDAs
            "/pdas",
            "/pdas/all",
            "/pdas/companies",

            // Smartphones
            "/smartphones",
            "/smartphones/all",
            "/smartphones/companies",

            // Companies
            "/companies",

            // Software
            "/software",
            "/software/all",
            "/software/companies",
            "/software/rankings",
            "/software/spec",

            // Books
            "/books",
            "/books/all",
            "/books/companies",

            // Documents
            "/documents",
            "/documents/all",
            "/documents/companies",

            // Magazines
            "/magazines",
            "/magazines/all",
            "/magazines/companies",

            // Hardware
            "/gpus",
            "/soundsynths",
            "/processors",

            // People
            "/people",
            "/people/all"
        ];

        // Generate letter-based browse pages (A-Z, 0-9)
        var letterRoutes = new List<string>();
        string[] letterEntities = ["/computers", "/consoles", "/tablets", "/pdas", "/smartphones",
                                   "/companies", "/people"];
        string[] letterEntitiesWithCompanies = ["/computers/companies", "/consoles/companies",
                                                "/tablets/companies", "/pdas/companies",
                                                "/smartphones/companies", "/books/companies",
                                                "/documents/companies", "/magazines/companies",
                                                "/software/companies"];

        for(char c = 'A'; c <= 'Z'; c++)
        {
            foreach(string entity in letterEntities)
                letterRoutes.Add($"{entity}/{c}");

            foreach(string entity in letterEntitiesWithCompanies)
                letterRoutes.Add($"{entity}/{c}");

            letterRoutes.Add($"/software/letter/{c}");
            letterRoutes.Add($"/books/{c}");
            letterRoutes.Add($"/documents/{c}");
            letterRoutes.Add($"/magazines/{c}");
        }

        return BuildUrlSet(staticRoutes, letterRoutes);
    }

    /// <summary>Fetches a page of entity IDs from the API and generates a sitemap.</summary>
    async Task<string> GenerateEntitySitemapAsync(string apiEndpoint, string urlPrefix, int page)
    {
        string cacheKey = $"sitemap:{apiEndpoint}:{page}";

        if(cache.TryGetValue(cacheKey, out string? cached) && cached is not null)
            return cached;

        var urls = new List<string>();

        try
        {
            HttpClient client = httpClientFactory.CreateClient("SitemapApi");

            int skip = (page - 1) * UrlsPerSitemap;

            var ids = await client.GetFromJsonAsync<List<long>>(
                $"/sitemap/{apiEndpoint}/ids?skip={skip}&take={UrlsPerSitemap}");

            if(ids is not null)
            {
                foreach(long id in ids)
                    urls.Add($"{urlPrefix}{id}");
            }
        }
        catch(Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch IDs from /sitemap/{Endpoint}/ids page {Page} for sitemap",
                              apiEndpoint, page);
        }

        string xml = BuildUrlSet(urls);

        cache.Set(cacheKey, xml, CacheDuration);

        return xml;
    }

    /// <summary>Queries the count endpoint for an entity type.</summary>
    async Task<int> GetEntityCountAsync(HttpClient client, string apiEndpoint)
    {
        try
        {
            return await client.GetFromJsonAsync<int>($"/{apiEndpoint}/count");
        }
        catch(Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch count from /{Endpoint}/count for sitemap index", apiEndpoint);

            return 0;
        }
    }

    static void WriteSitemapEntry(XmlWriter writer, string section)
    {
        writer.WriteStartElement("sitemap");
        writer.WriteElementString("loc", $"{SeoMeta.CanonicalHost}/sitemap-{section}.xml");
        writer.WriteEndElement();
    }

    string BuildUrlSet(IEnumerable<string> routes, IEnumerable<string>? extraRoutes = null)
    {
        using var ms     = new MemoryStream();
        using var writer = XmlWriter.Create(ms, WriterSettings());

        writer.WriteStartDocument();
        writer.WriteStartElement("urlset", Namespace);

        foreach(string route in routes)
        {
            writer.WriteStartElement("url");
            writer.WriteElementString("loc", SeoMeta.BuildCanonicalUrl(route));
            writer.WriteEndElement();
        }

        if(extraRoutes is not null)
        {
            foreach(string route in extraRoutes)
            {
                writer.WriteStartElement("url");
                writer.WriteElementString("loc", SeoMeta.BuildCanonicalUrl(route));
                writer.WriteEndElement();
            }
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    static XmlWriterSettings WriterSettings() => new()
    {
        Encoding    = new UTF8Encoding(false),
        Indent      = true,
        OmitXmlDeclaration = false
    };
}
