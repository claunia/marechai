using System;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Igdb.Services;

public class AmbiguousReviewService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;

    public AmbiguousReviewService(IDbContextFactory<MarechaiContext> contextFactory) =>
        _contextFactory = contextFactory;

    public async Task RunAsync(string type, int limit)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        switch(type)
        {
            case "platforms":
                await ReviewPlatformsAsync(context, limit);

                break;
            case "companies":
                await ReviewCompaniesAsync(context, limit);

                break;
            case "games":
                await ReviewGamesAsync(context, limit);

                break;
            default:
                Console.WriteLine($"  Unknown review type \"{type}\". Use platforms, companies, or games.");

                break;
        }
    }

    static async Task ReviewPlatformsAsync(MarechaiContext context, int limit)
    {
        var platforms = await context.IgdbPlatforms.Where(p => p.MatchStatus == IgdbMatchStatus.NeedsReview)
                                      .OrderBy(p => p.Name)
                                      .Take(limit)
                                      .ToListAsync();

        var softwarePlatforms = await context.SoftwarePlatforms.ToListAsync();

        foreach(IgdbPlatform platform in platforms)
        {
            Console.WriteLine($"\n  IGDB platform \"{platform.Name}\" (id {platform.Id}) has no confident match:");

            for(int i = 0; i < softwarePlatforms.Count; i++)
                Console.WriteLine($"    [{i + 1}] {softwarePlatforms[i].Name} (id {softwarePlatforms[i].Id})");

            Console.WriteLine("    [0] Skip");
            Console.Write("  Select: ");

            if(int.TryParse(Console.ReadLine()?.Trim(), out int choice) && choice >= 1 &&
               choice <= softwarePlatforms.Count)
            {
                platform.SoftwarePlatformId = softwarePlatforms[choice - 1].Id;
                platform.MatchStatus        = IgdbMatchStatus.Matched;
                platform.MatchType          = "manual-review";
                platform.MatchedOn          = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
    }

    static async Task ReviewCompaniesAsync(MarechaiContext context, int limit)
    {
        var companies = await context.IgdbCompanies.Where(c => c.MatchStatus == IgdbMatchStatus.NeedsReview)
                                      .OrderBy(c => c.Name)
                                      .Take(limit)
                                      .ToListAsync();

        foreach(IgdbCompany company in companies)
        {
            Console.WriteLine($"\n  IGDB company \"{company.Name}\" (id {company.IgdbId}):");
            Console.WriteLine($"    Candidates: {company.CandidatesJson}");
            Console.Write("  Enter local Company id to link, or blank to skip: ");

            string input = Console.ReadLine()?.Trim();

            if(int.TryParse(input, out int companyId))
            {
                company.CompanyId   = companyId;
                company.MatchStatus = IgdbMatchStatus.Matched;
                company.MatchType   = "manual-review";
                company.MatchedOn   = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
    }

    static async Task ReviewGamesAsync(MarechaiContext context, int limit)
    {
        var games = await context.IgdbGames.Where(g => g.MatchStatus == IgdbMatchStatus.NeedsReview)
                                  .OrderBy(g => g.Name)
                                  .Take(limit)
                                  .ToListAsync();

        foreach(IgdbGame game in games)
        {
            Console.WriteLine($"\n  IGDB game \"{game.Name}\" (id {game.IgdbId}):");
            Console.WriteLine($"    Candidates: {game.CandidatesJson}");
            Console.Write("  Enter local Software id to link, or blank to skip: ");

            string input = Console.ReadLine()?.Trim();

            if(ulong.TryParse(input, out ulong softwareId))
            {
                game.SoftwareId  = softwareId;
                game.MatchStatus = IgdbMatchStatus.Matched;
                game.MatchType   = "manual-review";
                game.MatchedOn   = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
    }
}
