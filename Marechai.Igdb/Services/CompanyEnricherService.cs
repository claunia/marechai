using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Markdig;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Igdb.Services;

public class CompanyEnricherService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly string                              _logoCachePath;
    readonly HttpClient                          _httpClient;

    public CompanyEnricherService(IDbContextFactory<MarechaiContext> contextFactory, string logoCachePath)
    {
        _contextFactory = contextFactory;
        _logoCachePath  = logoCachePath;
        _httpClient     = new HttpClient();
    }

    public class Stats
    {
        public int CompaniesProcessed;
        public int CompaniesSkippedNoLocalMatch;
        public int CountriesFilled;
        public int WebsitesFilled;
        public int FoundedFilled;
        public int StatusFilled;
        public int SoldFilled;
        public int SoldToFilled;
        public int DescriptionsAdded;
        public int LogosCached;
    }

    public async Task<Stats> RunAsync(bool dryRun)
    {
        var stats = new Stats();

        await using var context = await _contextFactory.CreateDbContextAsync();

        var pending = await context.IgdbCompanies
                                    .Where(c => c.MatchStatus == IgdbMatchStatus.Matched && c.CompanyId != null &&
                                                !c.EnrichmentApplied)
                                    .ToListAsync();

        foreach(IgdbCompany igdbCompany in pending)
        {
            Company company = await context.Companies.Include(c => c.Descriptions)
                                             .FirstOrDefaultAsync(c => c.Id == igdbCompany.CompanyId);

            if(company == null)
            {
                stats.CompaniesSkippedNoLocalMatch++;
                Console.WriteLine($"  [{igdbCompany.IgdbId}] No local company {igdbCompany.CompanyId} found, skipping.");

                continue;
            }

            stats.CompaniesProcessed++;

            if(company.CountryId == null && igdbCompany.Country != null)
            {
                company.CountryId = igdbCompany.Country;
                stats.CountriesFilled++;
                Console.WriteLine($"  [{igdbCompany.IgdbId}] Country filled: {igdbCompany.Country}.");
            }

            if(string.IsNullOrWhiteSpace(company.Website) && !string.IsNullOrWhiteSpace(igdbCompany.WebsitesJson))
            {
                string website = FirstWebsite(igdbCompany.WebsitesJson);

                if(!string.IsNullOrWhiteSpace(website) && website.Length <= 255)
                {
                    company.Website = website;
                    stats.WebsitesFilled++;
                    Console.WriteLine($"  [{igdbCompany.IgdbId}] Website filled: {website}.");
                }
            }

            if(company.Founded == null && igdbCompany.StartDate != null && igdbCompany.StartDateFormat != 7)
            {
                company.Founded          = DateTimeOffset.FromUnixTimeSeconds(igdbCompany.StartDate.Value).UtcDateTime;
                company.FoundedPrecision = ToDatePrecision(igdbCompany.StartDateFormat);
                stats.FoundedFilled++;
                Console.WriteLine($"  [{igdbCompany.IgdbId}] Founded date filled: {company.Founded:yyyy-MM-dd}.");
            }

            if(company.Status == CompanyStatus.Unknown && igdbCompany.Status != null)
            {
                CompanyStatus? status = ToCompanyStatus(igdbCompany.Status.Value);

                if(status != null)
                {
                    company.Status = status.Value;
                    stats.StatusFilled++;
                    Console.WriteLine($"  [{igdbCompany.IgdbId}] Status filled: {status.Value}.");
                }
            }

            if(company.Sold == null && igdbCompany.ChangeDate != null && igdbCompany.ChangeDateFormat != 7)
            {
                company.Sold          = DateTimeOffset.FromUnixTimeSeconds(igdbCompany.ChangeDate.Value).UtcDateTime;
                company.SoldPrecision = ToDatePrecision(igdbCompany.ChangeDateFormat);
                stats.SoldFilled++;
                Console.WriteLine($"  [{igdbCompany.IgdbId}] Sold date filled: {company.Sold:yyyy-MM-dd}.");
            }

            if(company.SoldToId == null && igdbCompany.ParentIgdbId != null)
            {
                IgdbCompany parent = await context.IgdbCompanies
                                                   .FirstOrDefaultAsync(c => c.IgdbId == igdbCompany.ParentIgdbId &&
                                                                             c.MatchStatus == IgdbMatchStatus.Matched);

                if(parent?.CompanyId != null)
                {
                    company.SoldToId = parent.CompanyId;
                    stats.SoldToFilled++;
                    Console.WriteLine($"  [{igdbCompany.IgdbId}] SoldTo resolved: company {parent.CompanyId}.");
                }
            }

            if(!string.IsNullOrWhiteSpace(igdbCompany.DescriptionRaw) &&
               !company.Descriptions.Any(d => d.LanguageCode == "eng"))
            {
                string markdown = igdbCompany.DescriptionRaw.Replace("\r\n", "\n").Replace("\n", "\n\n");

                company.Descriptions.Add(new CompanyDescription
                {
                    LanguageCode = "eng",
                    Text         = markdown,
                    Html         = Markdown.ToHtml(markdown, new MarkdownPipelineBuilder().UseAdvancedExtensions()
                                                                                            .Build())
                });

                stats.DescriptionsAdded++;
                Console.WriteLine($"  [{igdbCompany.IgdbId}] Description added.");
            }

            if(!string.IsNullOrWhiteSpace(igdbCompany.LogoImageId) && !dryRun)
            {
                await CacheLogoAsync(igdbCompany.IgdbId, company.Id, igdbCompany.LogoImageId);
                stats.LogosCached++;
            }
            else if(!string.IsNullOrWhiteSpace(igdbCompany.LogoImageId))
                stats.LogosCached++;

            if(!dryRun)
            {
                igdbCompany.EnrichmentApplied = true;
                igdbCompany.EnrichedOn        = DateTime.UtcNow;
            }
        }

        if(!dryRun)
            await context.SaveChangesAsync();

        return stats;
    }

    async Task CacheLogoAsync(long igdbId, int companyId, string imageId)
    {
        Directory.CreateDirectory(_logoCachePath);

        string destination = Path.Combine(_logoCachePath, $"{companyId}.jpg");

        byte[] bytes = await IgdbImageDownloader.DownloadBestAsync(_httpClient, imageId,
                                                                     IgdbImageDownloader.LogoSizeCandidates);

        if(bytes == null)
        {
            Console.WriteLine($"  [{igdbId}] Failed to download logo: no candidate size succeeded.");

            return;
        }

        await File.WriteAllBytesAsync(destination, bytes);
        Console.WriteLine($"  [{igdbId}] Logo cached to {destination}.");
    }

    static string FirstWebsite(string websitesJson)
    {
        using JsonDocument doc = JsonDocument.Parse(websitesJson);

        foreach(JsonElement element in doc.RootElement.EnumerateArray())
            if(element.TryGetProperty("url", out JsonElement url))
                return url.GetString();

        return null;
    }

    static DatePrecision ToDatePrecision(int? igdbDateFormat) => igdbDateFormat switch
    {
        0 => DatePrecision.Full,
        1 => DatePrecision.MonthYear,
        _ => DatePrecision.YearOnly
    };

    static CompanyStatus? ToCompanyStatus(int igdbStatus) => igdbStatus switch
    {
        0 => CompanyStatus.Active,
        1 => CompanyStatus.Defunct,
        2 => CompanyStatus.Merged,
        3 => CompanyStatus.Renamed,
        _ => null
    };
}
