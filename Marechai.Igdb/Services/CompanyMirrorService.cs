using System;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Igdb.Services;

public class CompanyMirrorService
{
    const int IgdbMaxPageSize = 500;

    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly IgdbHttpClient                     _client;

    public CompanyMirrorService(IDbContextFactory<MarechaiContext> contextFactory, IgdbHttpClient client)
    {
        _contextFactory = contextFactory;
        _client         = client;
    }

    /// <summary>Mirrors one batch (one IGDB request, at most <paramref name="batchSize" /> rows, clamped to
    /// IGDB's 500-row page limit). Run the command again to mirror the next batch. Pass <paramref name="batchSize" />
    /// = 0 to instead loop until the whole catalog has been mirrored in this single invocation.</summary>
    public async Task<int> RunAsync(int batchSize, bool dryRun)
    {
        bool all      = batchSize <= 0;
        int  pageSize = all ? IgdbMaxPageSize : Math.Min(batchSize, IgdbMaxPageSize);

        long lastId;
        int  alreadyMirrored;

        await using(var probe = await _contextFactory.CreateDbContextAsync())
        {
            lastId          = await probe.IgdbCompanies.MaxAsync(c => (long?)c.IgdbId) ?? 0;
            alreadyMirrored = await probe.IgdbCompanies.CountAsync();
        }

        int total = 0;

        while(true)
        {
            int batchNumber = batchSize > 0 ? (alreadyMirrored + total) / batchSize + 1 : 1;

            (int count, long newLastId) = await FetchAndUpsertPageAsync(lastId, pageSize, batchNumber, dryRun);

            total += count;
            lastId = newLastId;

            if(!all || count < pageSize)
                break;
        }

        return total;
    }

    async Task<(int count, long lastId)> FetchAndUpsertPageAsync(long afterId, int pageSize, int batchNumber,
                                                                   bool dryRun)
    {
        string query = new ApicalypseQueryBuilder().Fields("id,name,country,start_date,start_date_format," +
                                                            "change_date,change_date_format,status,parent," +
                                                            "websites.url,websites.category,description,logo.image_id," +
                                                            "company_type_histories")
                                                     .Where($"id > {afterId}")
                                                     .Sort("id asc")
                                                     .Limit(pageSize)
                                                     .Build();

        using JsonDocument doc = await _client.QueryAsync("companies", query);

        int  count  = doc.RootElement.GetArrayLength();
        long lastId = afterId;

        if(count == 0)
        {
            Console.WriteLine("  No more companies to mirror.");

            return (0, lastId);
        }

        if(!dryRun)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            foreach(JsonElement element in doc.RootElement.EnumerateArray())
            {
                long   igdbId = element.GetProperty("id").GetInt64();
                string name   = element.GetProperty("name").GetString();

                context.IgdbCompanies.Add(new IgdbCompany
                {
                    IgdbId                 = igdbId,
                    Name                   = name,
                    MatchStatus            = IgdbMatchStatus.Pending,
                    BatchNumber            = batchNumber,
                    Country                = element.TryGetProperty("country", out JsonElement country)
                                                  ? (short)country.GetInt32()
                                                  : null,
                    StartDate              = element.TryGetProperty("start_date", out JsonElement startDate)
                                                  ? startDate.GetInt64()
                                                  : null,
                    StartDateFormat        = element.TryGetProperty("start_date_format", out JsonElement startFmt)
                                                  ? startFmt.GetInt32()
                                                  : null,
                    ChangeDate              = element.TryGetProperty("change_date", out JsonElement changeDate)
                                                  ? changeDate.GetInt64()
                                                  : null,
                    ChangeDateFormat        = element.TryGetProperty("change_date_format", out JsonElement changeFmt)
                                                  ? changeFmt.GetInt32()
                                                  : null,
                    Status                  = element.TryGetProperty("status", out JsonElement status)
                                                  ? status.GetInt32()
                                                  : null,
                    ParentIgdbId            = element.TryGetProperty("parent", out JsonElement parent)
                                                  ? parent.GetInt64()
                                                  : null,
                    LogoImageId             = element.TryGetProperty("logo", out JsonElement logo) &&
                                               logo.TryGetProperty("image_id", out JsonElement imageId)
                                                  ? imageId.GetString()
                                                  : null,
                    DescriptionRaw          = element.TryGetProperty("description", out JsonElement description)
                                                  ? description.GetString()
                                                  : null,
                    WebsitesJson            = element.TryGetProperty("websites", out JsonElement websites)
                                                  ? websites.GetRawText()
                                                  : null,
                    CompanyTypeHistoryJson  = element.TryGetProperty("company_type_histories",
                                                                      out JsonElement typeHistories)
                                                  ? typeHistories.GetRawText()
                                                  : null
                });

                lastId = igdbId;
            }

            await context.SaveChangesAsync();
        }
        else
        {
            foreach(JsonElement element in doc.RootElement.EnumerateArray())
                lastId = element.GetProperty("id").GetInt64();
        }

        Console.WriteLine($"  Mirrored batch {batchNumber}: {count} companies (up to IGDB id {lastId}).");

        return (count, lastId);
    }
}
