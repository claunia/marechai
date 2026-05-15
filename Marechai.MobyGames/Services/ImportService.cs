using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.Database.Seeders;
using Marechai.MobyGames.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Control-flow signal raised when the user picks [Q]uit at an interactive prompt
///     (e.g. the unknown-product-code-issuer prompt). Callers at CLI entry points should
///     catch this and exit cleanly without printing a stack trace.
/// </summary>
public sealed class UserQuitException : Exception
{
    public UserQuitException() : base("User requested to quit") { }
}

/// <summary>
///     Control-flow signal raised when an importer in <see cref="ImportService.Unattended" />
///     mode reaches a code path that would normally prompt the user (duplicate-name match,
///     fuzzy "possible duplicates" match, unknown product-code issuer, or a multi-candidate
///     company-soundex match). The per-game batch loop catches this and skips the game
///     without writing a row to <c>MobyGamesImportStates</c> so the game remains unprocessed
///     and will be retried on a future interactive run.
/// </summary>
public sealed class NeedsInteractionException : Exception
{
    public string Reason { get; }

    public NeedsInteractionException(string reason) : base($"Needs user interaction: {reason}") =>
        Reason = reason;
}

public class ImportService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService             _sourceDb;
    readonly CompanyMatcher                    _companyMatcher;
    readonly PersonMatcher                     _personMatcher;
    readonly PlatformMatcher                   _platformMatcher;
    readonly CountryMatcher                    _countryMatcher;
    readonly StateService                      _stateService;
    readonly MobyGamesHttpClient               _mobyHttpClient;
    readonly AdminMessageService               _adminMessenger;

    // Session-only cache of user mapping decisions for product code Type strings that
    // aren't covered by the hardcoded fast-path switch in ImportReleasesInternalAsync.
    // Value == null means the user chose [S]kip for that Type.
    readonly Dictionary<string, ProductCodeIssuer?> _productCodeIssuerCache =
        new(StringComparer.Ordinal);

    /// <summary>
    ///     When true, the importer never blocks on a <c>Console.ReadLine</c> prompt. The top-level
    ///     per-game Accept/Reject/Skip/All/Quit gate is bypassed (every game is auto-accepted), and
    ///     any game that would otherwise prompt for duplicate resolution, unknown product-code
    ///     issuer, or ambiguous company soundex match is skipped without being recorded in
    ///     <c>MobyGamesImportStates</c> so a future interactive run can process it. Used by
    ///     scheduled / CI imports where no operator is available.
    /// </summary>
    public bool Unattended { get; set; }

    public ImportService(
        IDbContextFactory<MarechaiContext> contextFactory,
        SourceDatabaseService sourceDb,
        CompanyMatcher companyMatcher,
        PersonMatcher personMatcher,
        PlatformMatcher platformMatcher,
        CountryMatcher countryMatcher,
        StateService stateService,
        MobyGamesHttpClient mobyHttpClient = null,
        AdminMessageService adminMessenger = null)
    {
        _contextFactory  = contextFactory;
        _sourceDb        = sourceDb;
        _companyMatcher  = companyMatcher;
        _personMatcher   = personMatcher;
        _platformMatcher = platformMatcher;
        _countryMatcher  = countryMatcher;
        _stateService    = stateService;
        _mobyHttpClient  = mobyHttpClient;
        _adminMessenger  = adminMessenger;
    }

    public async Task RunBatchAsync(int batchSize, int batchNumber)
    {
        Console.WriteLine("\n  Loading reference data...");

        await using(var seedContext = await _contextFactory.CreateDbContextAsync())
        {
            SoftwareRoles.Seed(seedContext);
            DocumentRoles.Seed(seedContext);
        }

        await _companyMatcher.LoadAsync();
        await _personMatcher.LoadAsync();
        await _platformMatcher.LoadAsync();
        await _countryMatcher.LoadAsync();

        var processedIds = await _stateService.GetProcessedGameIdsAsync();

        Console.WriteLine($"  Already processed: {processedIds.Count} games");

        var gameIds = await _sourceDb.GetUnprocessedGameIdsAsync(batchSize, processedIds);

        Console.WriteLine($"  Found {gameIds.Count} unprocessed games for this batch\n");

        if(gameIds.Count == 0)
        {
            Console.WriteLine("  No more games to process.");

            return;
        }

        bool acceptAll = Unattended;
        int  imported  = 0, rejected = 0, failed = 0, skippedUnattended = 0;

        if(Unattended)
            Console.WriteLine("  Unattended mode: auto-accepting all games; skipping games that would require prompts.\n");

        for(int i = 0; i < gameIds.Count; i++)
        {
            string gameId = gameIds[i];

            try
            {
                var rows = await _sourceDb.GetRowsForGameAsync(gameId);
                var game = GameAssembler.Assemble(gameId, rows);

                Console.WriteLine($"\n  [{i + 1}/{gameIds.Count}] Game ID: {gameId}");
                Console.WriteLine($"    Name:      {game.Name ?? "(unknown)"}");
                Console.WriteLine($"    Publisher:  {(game.Publishers.Count > 0 ? string.Join("; ", game.Publishers) : "(none)")}");
                Console.WriteLine($"    Developer:  {(game.Developers.Count > 0 ? string.Join("; ", game.Developers) : "(none)")}");
                Console.WriteLine($"    Date:       {game.ReleaseDate ?? "(none)"}");
                Console.WriteLine($"    Platforms:  {string.Join(", ", game.Platforms)}");
                Console.WriteLine($"    Genres:     {string.Join(", ", game.Genres.Select(g => $"{g.Type}:{g.Name}"))}");
                Console.WriteLine($"    Credits:    {game.Credits.Count} entries");
                Console.WriteLine($"    Releases:   {game.Releases.Count} entries");
                Console.WriteLine($"    Specs:      {game.Specs.Count} entries");
                Console.WriteLine($"    Ratings:    {game.Ratings.Count} entries");

                if(string.IsNullOrWhiteSpace(game.Name))
                {
                    Console.WriteLine("    SKIPPING: No game name found");
                    await _stateService.MarkFailedAsync(gameId, "No game name", batchNumber);
                    failed++;

                    continue;
                }

                if(!acceptAll)
                {
                    Console.Write("\n    [A]ccept / [R]eject / [S]kip / Accept A[l]l / [Q]uit: ");
                    string input = Console.ReadLine()?.Trim().ToUpperInvariant();

                    switch(input)
                    {
                        case "Q":
                            Console.WriteLine($"\n  Quitting. {imported} imported, {rejected} rejected, {failed} failed so far.");

                            return;

                        case "R":
                            Console.Write("    Rejection reason: ");
                            string reason = Console.ReadLine()?.Trim() ?? "User rejected";
                            await _stateService.MarkRejectedAsync(gameId, game.Name, reason, batchNumber);
                            rejected++;

                            continue;

                        case "S":
                            continue;

                        case "L":
                            acceptAll = true;

                            break;

                        case "A":
                            break;

                        default:
                            continue;
                    }
                }

                // Pre-validate: check for unrecognized company roles before importing
                var unmatchedRoles = game.Releases
                                        .SelectMany(r => r.CompanyRoles)
                                        .Where(cr => MapRoleLabel(cr.Key) is null)
                                        .ToList();

                if(unmatchedRoles.Count > 0)
                {
                    foreach(var (roleLabel, companyName) in unmatchedRoles)
                    {
                        Console.WriteLine($"    WARNING: Unrecognized company role \"{roleLabel}\" " +
                                          $"for company \"{companyName}\"");
                    }

                    Console.WriteLine("    Skipping game due to unrecognized company roles.");
                    failed++;

                    continue;
                }

                if(Unattended)
                {
                    await using var preflightContext = await _contextFactory.CreateDbContextAsync();
                    string         skipReason       = await WouldRequireUserInputAsync(preflightContext, game);

                    if(skipReason is not null)
                    {
                        Console.WriteLine($"    SKIPPING (needs interaction): {skipReason}");
                        skippedUnattended++;

                        continue;
                    }
                }

                await ImportGameAsync(game, batchNumber);
                imported++;
            }
            catch(UserQuitException)
            {
                Console.WriteLine($"\n  Quitting on user request. {imported} imported, {rejected} rejected, {failed} failed so far.");

                return;
            }
            catch(NeedsInteractionException ex)
            {
                // Defensive backstop — pre-flight should normally catch this. The game was not
                // marked in MobyGamesImportStates so it remains unprocessed and will be retried
                // on the next interactive run.
                Console.WriteLine($"    SKIPPING (needs interaction at import): {ex.Reason}");
                skippedUnattended++;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"    ERROR: {ex}");
                await _stateService.MarkFailedAsync(gameId, ex.Message, batchNumber);
                failed++;
            }
        }

        Console.WriteLine($"\n  Batch complete: {imported} imported, {rejected} rejected, {failed} failed" +
                          (skippedUnattended > 0 ? $", {skippedUnattended} skipped (needs interaction)" : ""));
    }

    /// <summary>
    ///     Resolves an unknown product code Type string to a <see cref="ProductCodeIssuer" /> by
    ///     prompting the user. Returns true if the user picked an issuer (assigned to
    ///     <paramref name="issuer" />), false if the user chose [S]kip. Throws
    ///     <see cref="UserQuitException" /> if the user chose [Q]uit. Decisions are cached for the
    ///     remainder of the session so the user is asked at most once per Type per CLI run.
    /// </summary>
    bool TryResolveProductCodeIssuer(string type, out ProductCodeIssuer issuer)
    {
        if(_productCodeIssuerCache.TryGetValue(type, out var cached))
        {
            if(cached.HasValue)
            {
                issuer = cached.Value;

                return true;
            }

            // Cached decision was [S]kip
            issuer = default;

            return false;
        }

        // In unattended mode we must never block on Console.ReadLine. The batch loop's
        // pre-flight (WouldRequireUserInputAsync) should have already skipped this game, so a
        // throw here is the defensive backstop.
        if(Unattended)
            throw new NeedsInteractionException($"unknown product code issuer \"{type}\"");

        var values = (ProductCodeIssuer[])Enum.GetValues(typeof(ProductCodeIssuer));

        while(true)
        {
            Console.WriteLine($"\n    Unknown product code issuer \"{type}\"");
            Console.WriteLine("    Pick the issuer it should map to:");

            for(int i = 0; i < values.Length; i++)
                Console.WriteLine($"      [{i + 1}] {values[i]}");

            Console.WriteLine("      [S] Skip this code");
            Console.WriteLine("      [Q] Quit batch (e.g. to add a new issuer enum value in code and re-run)");
            Console.Write("    Select: ");

            string input = Console.ReadLine()?.Trim();

            if(string.IsNullOrEmpty(input))
                continue;

            if(string.Equals(input, "Q", StringComparison.OrdinalIgnoreCase))
                throw new UserQuitException();

            if(string.Equals(input, "S", StringComparison.OrdinalIgnoreCase))
            {
                _productCodeIssuerCache[type] = null;
                issuer                        = default;

                return false;
            }

            if(int.TryParse(input, out int choice) && choice >= 1 && choice <= values.Length)
            {
                issuer                        = values[choice - 1];
                _productCodeIssuerCache[type] = issuer;

                return true;
            }

            Console.WriteLine("    Invalid input. Please enter a number, S, or Q.");
        }
    }

    /// <summary>
    ///     Imports a single game by its MobyGames slug (the id stored in mobygames_raw).
    ///     Returns the Software ID if successful, null otherwise.
    /// </summary>
    public async Task<ulong?> ImportGameBySlugAsync(string slug)
    {
        // Ensure matchers are loaded
        await _companyMatcher.LoadAsync();
        await _personMatcher.LoadAsync();
        await _platformMatcher.LoadAsync();
        await _countryMatcher.LoadAsync();

        var rows = await _sourceDb.GetRowsForGameAsync(slug);

        if(rows.Count == 0)
        {
            Console.WriteLine($"    No data found in mobygames_raw for slug '{slug}'");

            return null;
        }

        var game = GameAssembler.Assemble(slug, rows);

        if(string.IsNullOrWhiteSpace(game.Name))
        {
            Console.WriteLine($"    No game name found for slug '{slug}'");

            return null;
        }

        Console.WriteLine($"    Importing base game: {game.Name}");

        await ImportGameAsync(game, 0);

        // Look up the software ID we just created
        await using var context = await _contextFactory.CreateDbContextAsync();

        MobyGamesImportState state = await context.MobyGamesImportStates
            .FirstOrDefaultAsync(s => s.MobyGameId == slug);

        return state?.SoftwareId;
    }

    /// <summary>
    ///     Pre-flight scan that returns the first reason a game would require an interactive
    ///     prompt during <see cref="ImportGameAsync" />, or <c>null</c> if the game can be
    ///     imported with no prompts. Read-only — does not mutate the database, the company
    ///     matcher cache, or the product-code issuer cache. Mirrors every prompt site in
    ///     <see cref="ImportGameAsync" /> and the helpers it transitively calls so the batch
    ///     loop can skip a game cleanly (leaving no row in <c>MobyGamesImportStates</c>) when
    ///     <see cref="Unattended" /> is enabled.
    /// </summary>
    async Task<string> WouldRequireUserInputAsync(MarechaiContext context, ParsedGame game)
    {
        bool isCompilation = game.Genres.Any(g =>
            (g.Name.Contains("DLC", StringComparison.OrdinalIgnoreCase) &&
             g.Name.Contains("add-on", StringComparison.OrdinalIgnoreCase)) ||
            (g.Type.Equals("Genre", StringComparison.OrdinalIgnoreCase) &&
             g.Name.Equals("Add-on", StringComparison.OrdinalIgnoreCase)) ||
            g.Name.Contains("Compilation", StringComparison.OrdinalIgnoreCase));

        // The compilation path (ImportCompilationAsync) skips the duplicate-name and fuzzy
        // duplicate prompts entirely, but still imports releases — which transitively call
        // _companyMatcher and TryResolveProductCodeIssuer. Only run the duplicate checks for
        // the regular-game path.
        bool takesCompilationPath = isCompilation &&
                                    (game.CompilationGameSlugs.Count > 0 ||
                                     game.UnresolvableCompilationGames.Count > 0);

        if(!takesCompilationPath)
        {
            // (a) Existing-by-name prompt (ImportGameAsync line ~391)
            bool existingByName = await context.Softwares
                                                .AnyAsync(s => s.Name == game.Name);

            if(existingByName)
                return $"existing software with name \"{game.Name}\"";

            // (b) Fuzzy duplicates prompt (ImportGameAsync line ~490)
            var fuzzyMatches = await FindFuzzySoftwareMatchesAsync(context, game.Name);

            if(fuzzyMatches.Count > 0)
                return $"possible duplicates of \"{game.Name}\" ({fuzzyMatches.Count} fuzzy match(es))";
        }

        // (c) Unknown product code Type prompt (TryResolveProductCodeIssuer)
        foreach(var release in game.Releases)
        {
            foreach(var productCode in release.ProductCodes)
            {
                if(productCode.Type is null) continue;

                switch(productCode.Type)
                {
                    case "Sony PN":
                    case "PSN/SEN Code":
                    case "Microsoft PN":
                    case "Nintendo PN":
                    case "Nintendo Media PN":
                    case "Sega PN":
                    case "Sega Region Code":
                    case "Activision PN":
                    case "Amazon ASIN":
                    case "eBay Item No.":
                        continue;
                }

                if(_productCodeIssuerCache.ContainsKey(productCode.Type))
                    continue;

                return $"unknown product code issuer \"{productCode.Type}\"";
            }
        }

        // (d) Multi-candidate company soundex prompt (CompanyMatcher.PromptMultiple)
        // Collect every name that ImportGameAsync / ImportReleasesInternalAsync /
        // ImportBasicReleaseInternalAsync would pass to _companyMatcher.MatchOrCreateAsync.
        var companyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach(string dev in game.Developers)
            if(!string.IsNullOrWhiteSpace(dev))
                companyNames.Add(dev);

        if(game.Releases.Count > 0)
        {
            string fallbackPublisher = game.Publishers.FirstOrDefault();

            foreach(var release in game.Releases)
            {
                string pub = release.Publisher ?? fallbackPublisher;

                if(!string.IsNullOrWhiteSpace(pub))
                    companyNames.Add(pub);

                if(!string.IsNullOrWhiteSpace(release.Distributor))
                    companyNames.Add(release.Distributor);

                if(!string.IsNullOrWhiteSpace(release.Localizer))
                    companyNames.Add(release.Localizer);

                // RunBatchAsync already skips games with any unmapped role-label, so only the
                // mapped subset reaches the matcher.
                foreach(var (roleLabel, companyName) in release.CompanyRoles)
                {
                    if(MapRoleLabel(roleLabel) is null) continue;
                    if(string.IsNullOrWhiteSpace(companyName)) continue;

                    companyNames.Add(companyName);
                }
            }
        }
        else
        {
            string mainPublisher = game.Publishers.FirstOrDefault();

            if(!string.IsNullOrWhiteSpace(mainPublisher))
                companyNames.Add(mainPublisher);
        }

        foreach(string name in companyNames)
        {
            if(_companyMatcher.WouldPromptForMatch(name))
                return $"multiple Soundex matches for company \"{name}\"";
        }

        return null;
    }

    async Task ImportGameAsync(ParsedGame game, int batchNumber)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Determine Kind from genres
        // Note: old MobyGames HTML uses &nbsp; (U+00A0) around the slash, so use Contains
        bool isDlc = game.Genres.Any(g =>
            (g.Name.Contains("DLC", StringComparison.OrdinalIgnoreCase) &&
             g.Name.Contains("add-on", StringComparison.OrdinalIgnoreCase)) ||
            (g.Type.Equals("Genre", StringComparison.OrdinalIgnoreCase) &&
             g.Name.Equals("Add-on", StringComparison.OrdinalIgnoreCase)));

        SoftwareKind kind = isDlc ? SoftwareKind.Dlc : SoftwareKind.Game;

        // Check if this is a compilation
        bool isCompilation = game.Genres.Any(g =>
            g.Name.Contains("Compilation", StringComparison.OrdinalIgnoreCase));

        if(isCompilation)
        {
            if(game.CompilationGameSlugs.Count > 0 || game.UnresolvableCompilationGames.Count > 0)
            {
                await ImportCompilationAsync(context, game, batchNumber);

                return;
            }

            Console.WriteLine("    WARNING: Compilation genre detected but no game links found in description. " +
                              "Importing as regular game.");
        }

        // 1. Check for existing Software with same name
        var existingByName = await context.Softwares
                                          .Where(s => s.Name == game.Name)
                                          .Select(s => new { s.Id, s.Name, s.Kind })
                                          .ToListAsync();

        Software software;

        if(existingByName.Count > 0)
        {
            // Parse the incoming game's year for comparison
            string incomingYear = ExtractYear(game.ReleaseDate) ?? "(no date)";

            // Also check releases tab for earliest date
            if(incomingYear == "(no date)" && game.Releases.Count > 0)
            {
                var firstRelDate = game.Releases
                                       .Where(r => !string.IsNullOrWhiteSpace(r.ReleaseDate))
                                       .Select(r => r.ReleaseDate)
                                       .FirstOrDefault();

                if(firstRelDate != null)
                    incomingYear = ExtractYear(firstRelDate) ?? "(no date)";
            }

            Console.WriteLine($"\n    WARNING: {existingByName.Count} existing entries named \"{game.Name}\"");
            Console.WriteLine($"    Incoming game year: {incomingYear}, platforms: {string.Join(", ", game.Platforms)}");

            // Fetch release info for each existing match
            foreach(var existing in existingByName)
            {
                var releases = await context.SoftwareReleases
                                            .Where(r => r.SoftwareId == (ulong)existing.Id)
                                            .Select(r => new
                                            {
                                                r.ReleaseDate,
                                                Platform = r.Platform != null ? r.Platform.Name : null
                                            })
                                            .ToListAsync();

                var years     = releases.Where(r => r.ReleaseDate.HasValue)
                                        .Select(r => r.ReleaseDate.Value.Year.ToString())
                                        .Distinct();
                var platforms = releases.Where(r => r.Platform != null)
                                        .Select(r => r.Platform)
                                        .Distinct();

                string yearStr     = years.Any()     ? string.Join("/", years)     : "no releases";
                string platformStr = platforms.Any() ? string.Join(", ", platforms) : "no platforms";

                Console.WriteLine($"      [{existingByName.IndexOf(existing) + 1}] ID: {existing.Id}, " +
                                  $"Year(s): {yearStr}, Platforms: {platformStr}");
            }

            // Defensive backstop: in unattended mode the batch loop's pre-flight should have
            // already skipped this game. Throw rather than block on Console.ReadLine.
            if(Unattended)
                throw new NeedsInteractionException($"existing software with name \"{game.Name}\"");

            Console.Write("    [N]ew entry / [1-N] Link to existing / [S]kip: ");
            string input = Console.ReadLine()?.Trim().ToUpperInvariant();

            if(input == "S")
            {
                Console.WriteLine("    Skipped.");

                return;
            }

            if(int.TryParse(input, out int choice) && choice >= 1 && choice <= existingByName.Count)
            {
                software = await context.Softwares.FindAsync(existingByName[choice - 1].Id);

                context.News.Add(new News
                {
                    AddedId = (long)software.Id,
                    Date    = DateTime.UtcNow,
                    Type    = NewsType.UpdatedSoftwareInDb,
                    Name    = software.Name
                });

                await context.SaveChangesAsync();
                Console.WriteLine($"    Linked to existing Software ID: {software.Id}");
            }
            else
            {
                software = new Software { Name = game.Name, Kind = kind };
                context.Softwares.Add(software);
                await context.SaveChangesAsync();

                context.News.Add(new News
                {
                    AddedId = (long)software.Id,
                    Date    = DateTime.UtcNow,
                    Type    = NewsType.NewSoftwareInDb,
                    Name    = game.Name
                });

                await context.SaveChangesAsync();
                Console.WriteLine($"    Created new Software ID: {software.Id}");
            }
        }
        else
        {
            // Fuzzy match: check for existing software with similar names
            var fuzzyMatches = await FindFuzzySoftwareMatchesAsync(context, game.Name);

            if(fuzzyMatches.Count > 0)
            {
                string incomingYear = ExtractYear(game.ReleaseDate) ?? "(no date)";

                if(incomingYear == "(no date)" && game.Releases.Count > 0)
                {
                    var firstRelDate = game.Releases
                                           .Where(r => !string.IsNullOrWhiteSpace(r.ReleaseDate))
                                           .Select(r => r.ReleaseDate)
                                           .FirstOrDefault();

                    if(firstRelDate != null)
                        incomingYear = ExtractYear(firstRelDate) ?? "(no date)";
                }

                Console.WriteLine($"\n    POSSIBLE DUPLICATES for \"{game.Name}\":");
                Console.WriteLine($"    Incoming game year: {incomingYear}, platforms: {string.Join(", ", game.Platforms)}");

                for(int m = 0; m < fuzzyMatches.Count; m++)
                {
                    var match = fuzzyMatches[m];

                    var releases = await context.SoftwareReleases
                                                .Where(r => r.SoftwareId == match.Id)
                                                .Select(r => new
                                                {
                                                    r.ReleaseDate,
                                                    Platform = r.Platform != null ? r.Platform.Name : null
                                                })
                                                .ToListAsync();

                    var years     = releases.Where(r => r.ReleaseDate.HasValue)
                                            .Select(r => r.ReleaseDate.Value.Year.ToString())
                                            .Distinct();
                    var platforms = releases.Where(r => r.Platform != null)
                                            .Select(r => r.Platform)
                                            .Distinct();

                    string yearStr     = years.Any()     ? string.Join("/", years)     : "no releases";
                    string platformStr = platforms.Any() ? string.Join(", ", platforms) : "no platforms";

                    Console.WriteLine($"      [{m + 1}] \"{match.Name}\" (ID: {match.Id}, " +
                                      $"Score: {match.Score:F2}, Year(s): {yearStr}, Platforms: {platformStr})");
                }

                // Defensive backstop: in unattended mode the batch loop's pre-flight should
                // have already skipped this game. Throw rather than block on Console.ReadLine.
                if(Unattended)
                    throw new NeedsInteractionException(
                        $"possible duplicates of \"{game.Name}\" ({fuzzyMatches.Count} fuzzy match(es))");

                Console.Write("    [N]ew entry / [1-N] Link to existing / [S]kip: ");
                string input = Console.ReadLine()?.Trim().ToUpperInvariant();

                if(input == "S")
                {
                    Console.WriteLine("    Skipped.");

                    return;
                }

                if(int.TryParse(input, out int choice) && choice >= 1 && choice <= fuzzyMatches.Count)
                {
                    software = await context.Softwares.FindAsync(fuzzyMatches[choice - 1].Id);

                    context.News.Add(new News
                    {
                        AddedId = (long)software.Id,
                        Date    = DateTime.UtcNow,
                        Type    = NewsType.UpdatedSoftwareInDb,
                        Name    = software.Name
                    });

                    await context.SaveChangesAsync();
                    Console.WriteLine($"    Linked to existing Software ID: {software.Id}");
                }
                else
                {
                    software = new Software { Name = game.Name, Kind = kind };
                    context.Softwares.Add(software);
                    await context.SaveChangesAsync();

                    context.News.Add(new News
                    {
                        AddedId = (long)software.Id,
                        Date    = DateTime.UtcNow,
                        Type    = NewsType.NewSoftwareInDb,
                        Name    = game.Name
                    });

                    await context.SaveChangesAsync();
                    Console.WriteLine($"    Created new Software ID: {software.Id}");
                }
            }
            else
            {
                software = new Software { Name = game.Name, Kind = kind };
                context.Softwares.Add(software);
                await context.SaveChangesAsync();

                context.News.Add(new News
                {
                    AddedId = (long)software.Id,
                    Date    = DateTime.UtcNow,
                    Type    = NewsType.NewSoftwareInDb,
                    Name    = game.Name
                });

                await context.SaveChangesAsync();
            }
        }

        // 2. Description
        if(!string.IsNullOrWhiteSpace(game.Description))
        {
            bool descExists = await context.SoftwareDescriptions
                                           .AnyAsync(d => d.SoftwareId == software.Id &&
                                                          d.LanguageCode == "eng");

            if(!descExists)
            {
                context.SoftwareDescriptions.Add(new SoftwareDescription
                {
                    SoftwareId   = software.Id,
                    LanguageCode = "eng",
                    Text         = game.Description,
                    Html         = game.DescriptionHtml
                });
            }
        }

        // 2b. Resolve base game for DLCs
        if(isDlc && software.BaseSoftwareId is null && _mobyHttpClient is not null)
        {
            await ResolveDlcBaseGameAsync(context, software, game);
        }

        // 3. Genres
        foreach(var genre in game.Genres)
        {
            var genreType = genre.Type switch
            {
                "Genre"       => SoftwareGenreType.Genre,
                "Perspective" => SoftwareGenreType.Perspective,
                "Gameplay"    => SoftwareGenreType.Gameplay,
                "Setting"     => SoftwareGenreType.Setting,
                _             => SoftwareGenreType.Genre
            };

            var dbGenre = await context.SoftwareGenres
                                       .FirstOrDefaultAsync(g => g.Name == genre.Name && g.Type == genreType);

            if(dbGenre is null)
            {
                dbGenre = new SoftwareGenre { Name = genre.Name, Type = genreType };
                context.SoftwareGenres.Add(dbGenre);
                await context.SaveChangesAsync();
            }

            bool genreExists = await context.GenresBySoftware
                                             .AnyAsync(g => g.SoftwareId == software.Id &&
                                                            g.GenreId == dbGenre.Id);

            if(!genreExists)
            {
                context.GenresBySoftware.Add(new GenreBySoftware
                {
                    SoftwareId = software.Id,
                    GenreId    = dbGenre.Id
                });
            }
        }

        // 4. Company roles from Main tab (developers)
        var addedCompanyRoles = new HashSet<(ulong, int, string)>();

        foreach(string devName in game.Developers)
        {
            if(string.IsNullOrWhiteSpace(devName)) continue;

            var (devCompany, _) = await _companyMatcher.MatchOrCreateAsync(devName);

            if(devCompany != null)
            {
                var roleKey = (software.Id, devCompany.Id, "dev");

                if(addedCompanyRoles.Add(roleKey))
                {
                    bool exists = await context.SoftwareCompanyRoles
                                               .AnyAsync(r => r.SoftwareId == software.Id &&
                                                              r.CompanyId == devCompany.Id &&
                                                              r.RoleId == "dev");

                    if(!exists)
                    {
                        context.SoftwareCompanyRoles.Add(new SoftwareCompanyRole
                        {
                            SoftwareId = software.Id,
                            CompanyId  = devCompany.Id,
                            RoleId     = "dev"
                        });
                    }
                }
            }
        }

        // 5. Process releases from Releases tab (or create basic release from Main tab)
        if(game.Releases.Count > 0)
            await ImportReleasesAsync(context, software, game, addedCompanyRoles);
        else
            await ImportBasicReleaseAsync(context, software, game);

        // 6. Credits
        var addedCredits = new HashSet<(ulong, int, string)>(
            new CaseInsensitiveCreditComparer());

        foreach(var credit in game.Credits)
        {
            var person = await _personMatcher.MatchOrCreateAsync(credit.PersonName);

            if(person != null)
            {
                var key = (software.Id, person.Id, credit.Role);

                if(!addedCredits.Add(key))
                    continue;

                // Check for duplicates in DB
                bool exists = await context.PeopleBySoftware
                                          .AnyAsync(p => p.SoftwareId == software.Id &&
                                                         p.PersonId == person.Id &&
                                                         p.Role == credit.Role);

                if(!exists)
                {
                    context.PeopleBySoftware.Add(new PeopleBySoftware
                    {
                        SoftwareId = software.Id,
                        PersonId   = person.Id,
                        Role       = credit.Role,
                        RoleId     = MapCreditRole(credit.Role)
                    });
                }
            }
        }

        // 7. Specs and Ratings stored as SoftwareAttributes (need release IDs — done inside ImportReleasesAsync)

        await context.SaveChangesAsync();

        // Resolve MobyGames numeric ID for future lookups
        int? mobyNumericId = null;

        if(_mobyHttpClient is not null)
            mobyNumericId = await _mobyHttpClient.ResolveNumericGameIdAsync(game.MobyGameId);

        await _stateService.MarkImportedAsync(game.MobyGameId, batchNumber, software.Id, mobyNumericId);

        Console.WriteLine($"    Imported as Software ID: {software.Id}" +
                          (mobyNumericId is not null ? $" (MobyID: {mobyNumericId})" : ""));
    }

    async Task ImportCompilationAsync(MarechaiContext context, ParsedGame game, int batchNumber)
    {
        Console.WriteLine($"    Compilation detected with {game.CompilationGameSlugs.Count} game slug(s) " +
                          $"and {game.UnresolvableCompilationGames.Count} unresolvable anchor(s):");

        foreach(string slug in game.CompilationGameSlugs)
            Console.WriteLine($"      - {slug}");

        if(game.UnresolvableCompilationGames.Count > 0)
        {
            Console.WriteLine($"    WARNING: {game.UnresolvableCompilationGames.Count} anchor(s) do not point at " +
                              "a MobyGames game entry (search URL or non-game link):");

            foreach(UnresolvableCompilationLink link in game.UnresolvableCompilationGames)
                Console.WriteLine($"      ? {link.Name}  →  {link.Href}");
        }

        // Resolution phase: try to resolve every contained slug, but DO NOT abort on individual
        // failures — we want to import a partial compilation and report the rest to admins.
        var containedSoftwareIds = new List<ulong>();
        var unresolvedSlugs      = new List<string>();

        foreach(string slug in game.CompilationGameSlugs)
        {
            ulong? softwareId = await ResolveGameSlugToSoftwareIdAsync(context, slug);

            if(softwareId is null)
            {
                Console.WriteLine($"      WARNING: Cannot resolve contained game '{slug}'.");
                unresolvedSlugs.Add(slug);

                continue;
            }

            containedSoftwareIds.Add(softwareId.Value);
            Console.WriteLine($"      Resolved '{slug}' → Software ID: {softwareId}");
        }

        bool hasUnresolved = unresolvedSlugs.Count > 0 || game.UnresolvableCompilationGames.Count > 0;

        // If nothing at all could be linked we cannot create a meaningful compilation; mark Failed
        // and still report the missing entries to admins so they can act.
        if(containedSoftwareIds.Count == 0)
        {
            Console.WriteLine("    FAILED: No contained games could be resolved; compilation not created.");

            if(hasUnresolved && _adminMessenger is not null)
            {
                await _adminMessenger.SendCompilationReportAsync(game.Name, game.MobyGameId,
                                                                 resolvedSoftwareIds: [],
                                                                 unresolvedSlugs: unresolvedSlugs,
                                                                 unresolvableLinks: game.UnresolvableCompilationGames,
                                                                 compilationCreated: false);
            }

            await _stateService.MarkFailedAsync(game.MobyGameId,
                $"No contained games could be resolved (unresolved slugs: {unresolvedSlugs.Count}, " +
                $"unresolvable anchors: {game.UnresolvableCompilationGames.Count})", batchNumber);

            return;
        }

        // Heavy-mutation phase. We track success in a flag so the partial-report send (in the
        // outer finally) can describe accurately whether the compilation actually got created.
        // The flag also lets us re-raise the original exception without losing the report.
        bool compilationCreated = false;

        try
        {
            // Creation phase: create compilation releases (no Software record)
            List<SoftwareRelease> createdReleases;

            if(game.Releases.Count > 0)
                createdReleases = await ImportCompilationReleasesAsync(context, game, game.Name);
            else
                createdReleases = await ImportBasicCompilationReleaseAsync(context, game, game.Name);

            if(createdReleases.Count == 0)
            {
                Console.WriteLine("    WARNING: No releases created for compilation.");
                await _stateService.MarkFailedAsync(game.MobyGameId, "No releases created", batchNumber);

                return;
            }

            // Add SoftwareBySoftwareRelease junction entries
            foreach(var release in createdReleases)
            {
                foreach(ulong containedId in containedSoftwareIds)
                {
                    context.SoftwareBySoftwareRelease.Add(new SoftwareBySoftwareRelease
                    {
                        ReleaseId  = release.Id,
                        SoftwareId = containedId
                    });
                }
            }

            await context.SaveChangesAsync();

            // Clean up any orphaned Software record with the same name as this compilation.
            // This handles the case where a previous import (before compilation detection existed)
            // created a Software for this compilation, then the game was reset and re-imported.
            // ResetGameAsync deletes the import state but NOT the Software, so we can't rely
            // on import state — we search by name instead.
            var orphanedSoftware = await context.Softwares
                .Where(s => s.Name == game.Name)
                .ToListAsync();

            foreach(var orphan in orphanedSoftware)
            {
                // Convert any releases on the orphaned Software to compilation releases
                var existingReleases = await context.SoftwareReleases
                    .Where(r => r.SoftwareId == orphan.Id)
                    .ToListAsync();

                foreach(var release in existingReleases)
                {
                    release.IsCompilation = true;
                    release.Title         = game.Name;
                    release.SoftwareId    = null;

                    // Add junction entries for contained games
                    foreach(ulong containedId in containedSoftwareIds)
                    {
                        bool junctionExists = await context.SoftwareBySoftwareRelease
                            .AnyAsync(j => j.ReleaseId == release.Id && j.SoftwareId == containedId);

                        if(!junctionExists)
                        {
                            context.SoftwareBySoftwareRelease.Add(new SoftwareBySoftwareRelease
                            {
                                ReleaseId  = release.Id,
                                SoftwareId = containedId
                            });
                        }
                    }
                }

                // Clear SoftwareId from any import state referencing this orphan
                var orphanStates = await context.MobyGamesImportStates
                    .Where(s => s.SoftwareId == orphan.Id)
                    .ToListAsync();

                foreach(var state in orphanStates)
                    state.SoftwareId = null;

                await context.SaveChangesAsync();

                // Delete orphaned Software (company roles need explicit removal — no cascade configured)
                var companyRoles = await context.SoftwareCompanyRoles
                    .Where(r => r.SoftwareId == orphan.Id)
                    .ToListAsync();

                context.SoftwareCompanyRoles.RemoveRange(companyRoles);

                context.Softwares.Remove(orphan);
                await context.SaveChangesAsync();
                Console.WriteLine($"    Cleaned up orphaned Software ID: {orphan.Id}");
            }

            // Mark as imported with no SoftwareId (compilations don't have a Software record)
            await _stateService.MarkImportedAsync(game.MobyGameId, batchNumber, null);

            compilationCreated = true;

            if(hasUnresolved)
            {
                Console.WriteLine($"    Imported as PARTIAL compilation with {createdReleases.Count} release(s), " +
                                  $"{containedSoftwareIds.Count} resolved game(s), " +
                                  $"{unresolvedSlugs.Count} unresolved slug(s), " +
                                  $"{game.UnresolvableCompilationGames.Count} unresolvable anchor(s)");
            }
            else
            {
                Console.WriteLine($"    Imported as compilation with {createdReleases.Count} release(s), " +
                                  $"{containedSoftwareIds.Count} contained game(s)");
            }
        }
        finally
        {
            // Always send the partial-compilation report when there were unresolved entries — even
            // if a downstream mutation throws. The outer batch loop catches Exception and marks the
            // game Failed, but admins still need to see what was missing. Report-send failures are
            // swallowed inside AdminMessageService so this finally never masks the original exception.
            if(hasUnresolved && _adminMessenger is not null)
            {
                await _adminMessenger.SendCompilationReportAsync(game.Name, game.MobyGameId,
                                                                 resolvedSoftwareIds: containedSoftwareIds,
                                                                 unresolvedSlugs: unresolvedSlugs,
                                                                 unresolvableLinks: game.UnresolvableCompilationGames,
                                                                 compilationCreated: compilationCreated);
            }
        }
    }

    /// <summary>
    ///     Resolves a MobyGames game slug to a Software ID in the local database.
    ///     Tries all slug variants (with/without leading dash) in MobyGamesImportState,
    ///     then attempts to import from mobygames_raw if not found.
    /// </summary>
    async Task<ulong?> ResolveGameSlugToSoftwareIdAsync(MarechaiContext context, string slug)
    {
        if(string.IsNullOrWhiteSpace(slug)) return null;

        string trimmed = slug.TrimStart('-');

        // Try all slug variants in MobyGamesImportState
        string[] slugsToTry = [slug, $"-{slug}", trimmed, $"-{trimmed}"];

        foreach(string trySlug in slugsToTry.Distinct())
        {
            MobyGamesImportState state = await context.MobyGamesImportStates
                .FirstOrDefaultAsync(s => s.MobyGameId == trySlug &&
                                          s.Status == MobyGamesImportStatus.Imported);

            if(state?.SoftwareId is not null) return state.SoftwareId;
        }

        // Not found in import state — try importing from mobygames_raw
        foreach(string trySlug in slugsToTry.Distinct())
        {
            var rows = await _sourceDb.GetRowsForGameAsync(trySlug);

            if(rows.Count > 0)
            {
                Console.Write($"      Importing '{trySlug}' from mobygames_raw...");
                ulong? importedId = await ImportGameBySlugAsync(trySlug);

                if(importedId is not null)
                {
                    Console.WriteLine($" OK (ID: {importedId})");

                    return importedId;
                }

                Console.WriteLine(" failed");
            }
        }

        return null;
    }

    async Task ImportReleasesAsync(MarechaiContext context, Software software, ParsedGame game,
                                   HashSet<(ulong, int, string)> addedCompanyRoles)
    {
        await ImportReleasesInternalAsync(context, software?.Id, game, addedCompanyRoles,
                                          false, null);
    }

    async Task<List<SoftwareRelease>> ImportCompilationReleasesAsync(
        MarechaiContext context, ParsedGame game, string compilationTitle)
    {
        return await ImportReleasesInternalAsync(context, null, game,
                                                 new HashSet<(ulong, int, string)>(),
                                                 true, compilationTitle);
    }

    async Task<List<SoftwareRelease>> ImportReleasesInternalAsync(
        MarechaiContext context, ulong? softwareId, ParsedGame game,
        HashSet<(ulong, int, string)> addedCompanyRoles,
        bool isCompilation, string compilationTitle)
    {
        var addedProductCodes = new HashSet<(ProductCodeIssuer, string)>();
        var addedBarcodes     = new HashSet<string>();
        var createdReleases   = new List<SoftwareRelease>();

        // Group releases by platform
        var platformGroups = game.Releases.GroupBy(r => r.Platform ?? "Unknown");

        foreach(var platformGroup in platformGroups)
        {
            var platform = await _platformMatcher.MatchOrCreateAsync(platformGroup.Key);

            foreach(var release in platformGroup)
            {
                var (publisher, _) = await _companyMatcher.MatchOrCreateAsync(
                    release.Publisher ?? game.Publishers.FirstOrDefault());

                if(publisher is null) continue;

                var (releaseDate, precision) = ParseDate(release.ReleaseDate);

                var dbRelease = new SoftwareRelease
                {
                    SoftwareId           = isCompilation ? null : softwareId,
                    PlatformId           = platform?.Id,
                    PublisherId          = publisher.Id,
                    ReleaseDate          = releaseDate,
                    ReleaseDatePrecision = precision,
                    IsCompilation        = isCompilation,
                    Title                = isCompilation ? compilationTitle : release.Comments
                };

                context.SoftwareReleases.Add(dbRelease);
                await context.SaveChangesAsync();
                createdReleases.Add(dbRelease);

                // Barcodes
                foreach(var barcode in release.Barcodes)
                {
                    var barcodeType = barcode.Type switch
                    {
                        "UPC-A"  => BarcodeType.UPC_A,
                        "EAN-13" => BarcodeType.EAN_13,
                        _        => BarcodeType.Unknown
                    };

                    // Check for existing barcode
                    bool exists = await context.SoftwareBarcodes
                                               .AnyAsync(b => b.Code == barcode.Code);

                    if(exists) continue;

                    if(!addedBarcodes.Add(barcode.Code)) continue;

                    context.SoftwareBarcodes.Add(new SoftwareBarcode
                    {
                        ReleaseId = dbRelease.Id,
                        Code      = barcode.Code,
                        Type      = barcodeType
                    });
                }

                // Product codes
                foreach(var productCode in release.ProductCodes)
                {
                    ProductCodeIssuer issuer;

                    switch(productCode.Type)
                    {
                        case "Sony PN":         issuer = ProductCodeIssuer.Sony;       break;
                        case "PSN/SEN Code":    issuer = ProductCodeIssuer.PSN;        break;
                        case "Microsoft PN":    issuer = ProductCodeIssuer.Microsoft;  break;
                        case "Nintendo PN":     issuer = ProductCodeIssuer.Nintendo;   break;
                        case "Nintendo Media PN": issuer = ProductCodeIssuer.Nintendo; break;
                        case "Sega PN":         issuer = ProductCodeIssuer.Sega;       break;
                        case "Sega Region Code": issuer = ProductCodeIssuer.Sega;      break;
                        case "Activision PN":   issuer = ProductCodeIssuer.Activision; break;
                        case "Amazon ASIN":     issuer = ProductCodeIssuer.Amazon;     break;
                        case "eBay Item No.":   issuer = ProductCodeIssuer.eBay;       break;

                        default:
                            // Unknown Type — ask the user to map it instead of silently
                            // defaulting to ProductCodeIssuer.Other.
                            if(!TryResolveProductCodeIssuer(productCode.Type, out issuer))
                                continue; // user chose [S]kip

                            break;
                    }

                    bool codeExists = await context.SoftwareProductCodes
                                                   .AnyAsync(c => c.Issuer == issuer &&
                                                                   c.Code == productCode.Code);

                    if(codeExists) continue;

                    if(!addedProductCodes.Add((issuer, productCode.Code))) continue;

                    context.SoftwareProductCodes.Add(new SoftwareProductCode
                    {
                        ReleaseId = dbRelease.Id,
                        Issuer    = issuer,
                        Code      = productCode.Code
                    });
                }

                // Countries → UnM49 regions
                var addedRegions = new HashSet<short>();

                foreach(string country in release.Countries)
                {
                    var region = _countryMatcher.Match(country);

                    if(region != null && addedRegions.Add(region.Id))
                    {
                        bool exists = await context.UnM49BySoftwareRelease
                                                   .AnyAsync(u => u.SoftwareReleaseId == dbRelease.Id &&
                                                                  u.UnM49Id == region.Id);

                        if(!exists)
                        {
                            context.UnM49BySoftwareRelease.Add(new UnM49BySoftwareRelease
                            {
                                SoftwareReleaseId = dbRelease.Id,
                                UnM49Id           = region.Id
                            });
                        }
                    }
                }

                // Distributor/Localizer company roles on the software (skip for compilations)
                if(softwareId.HasValue && !string.IsNullOrWhiteSpace(release.Distributor))
                {
                    var (dist, _) = await _companyMatcher.MatchOrCreateAsync(release.Distributor);

                    if(dist != null)
                    {
                        var roleKey = (softwareId.Value, dist.Id, "dis");

                        if(addedCompanyRoles.Add(roleKey))
                        {
                            bool exists = await context.SoftwareCompanyRoles
                                                       .AnyAsync(r => r.SoftwareId == softwareId.Value &&
                                                                      r.CompanyId == dist.Id &&
                                                                      r.RoleId == "dis");

                            if(!exists)
                            {
                                context.SoftwareCompanyRoles.Add(new SoftwareCompanyRole
                                {
                                    SoftwareId = softwareId.Value,
                                    CompanyId  = dist.Id,
                                    RoleId     = "dis"
                                });
                            }
                        }
                    }
                }

                if(softwareId.HasValue && !string.IsNullOrWhiteSpace(release.Localizer))
                {
                    var (loc, _) = await _companyMatcher.MatchOrCreateAsync(release.Localizer);

                    if(loc != null)
                    {
                        var roleKey = (softwareId.Value, loc.Id, "loc");

                        if(addedCompanyRoles.Add(roleKey))
                        {
                            bool exists = await context.SoftwareCompanyRoles
                                                       .AnyAsync(r => r.SoftwareId == softwareId.Value &&
                                                                      r.CompanyId == loc.Id &&
                                                                      r.RoleId == "loc");

                            if(!exists)
                            {
                                context.SoftwareCompanyRoles.Add(new SoftwareCompanyRole
                                {
                                    SoftwareId = softwareId.Value,
                                    CompanyId  = loc.Id,
                                    RoleId     = "loc"
                                });
                            }
                        }
                    }
                }

                // Extra company roles from releases tab (e.g., "Ported by", etc.)
                if(softwareId.HasValue)
                foreach(var (roleLabel, companyName) in release.CompanyRoles)
                {
                    string roleId = MapRoleLabel(roleLabel);

                    var (roleCompany, _) = await _companyMatcher.MatchOrCreateAsync(companyName);

                    if(roleCompany != null)
                    {
                        var roleKey = (softwareId.Value, roleCompany.Id, roleId);

                        if(addedCompanyRoles.Add(roleKey))
                        {
                            bool exists = await context.SoftwareCompanyRoles
                                                       .AnyAsync(r => r.SoftwareId == softwareId.Value &&
                                                                      r.CompanyId == roleCompany.Id &&
                                                                      r.RoleId == roleId);

                            if(!exists)
                            {
                                context.SoftwareCompanyRoles.Add(new SoftwareCompanyRole
                                {
                                    SoftwareId = softwareId.Value,
                                    CompanyId  = roleCompany.Id,
                                    RoleId     = roleId
                                });
                            }
                        }
                    }
                }

                // Specs for this platform
                foreach(var spec in game.Specs.Where(s => s.Platform == platformGroup.Key))
                {
                    context.SoftwareAttributes.Add(new SoftwareAttribute
                    {
                        SoftwareReleaseId = dbRelease.Id,
                        Category          = "Spec",
                        Key               = spec.Key,
                        Value             = spec.Value
                    });
                }

                // Ratings for this platform
                foreach(var rating in game.Ratings.Where(r => r.Platform == platformGroup.Key))
                {
                    string value = rating.Rating;

                    if(!string.IsNullOrWhiteSpace(rating.Descriptors))
                        value += $" ({rating.Descriptors})";

                    context.SoftwareAttributes.Add(new SoftwareAttribute
                    {
                        SoftwareReleaseId = dbRelease.Id,
                        Category          = "Rating",
                        Key               = rating.System,
                        Value             = value
                    });
                }
            }
        }

        return createdReleases;
    }

    async Task ImportBasicReleaseAsync(MarechaiContext context, Software software, ParsedGame game)
    {
        await ImportBasicReleaseInternalAsync(context, software?.Id, game, false, null);
    }

    async Task<List<SoftwareRelease>> ImportBasicCompilationReleaseAsync(
        MarechaiContext context, ParsedGame game, string compilationTitle)
    {
        return await ImportBasicReleaseInternalAsync(context, null, game, true, compilationTitle);
    }

    async Task<List<SoftwareRelease>> ImportBasicReleaseInternalAsync(
        MarechaiContext context, ulong? softwareId, ParsedGame game,
        bool isCompilation, string compilationTitle)
    {
        // Create one release per platform from Main tab data
        var (publisher, _) = await _companyMatcher.MatchOrCreateAsync(game.Publishers.FirstOrDefault());
        var createdReleases = new List<SoftwareRelease>();

        if(publisher is null) return createdReleases;

        var (releaseDate, precision) = ParseDate(game.ReleaseDate);

        foreach(string platformName in game.Platforms.Count > 0 ? game.Platforms : ["Unknown"])
        {
            var platform = await _platformMatcher.MatchOrCreateAsync(platformName);

            var dbRelease = new SoftwareRelease
            {
                SoftwareId           = isCompilation ? null : softwareId,
                PlatformId           = platform?.Id,
                PublisherId          = publisher.Id,
                ReleaseDate          = releaseDate,
                ReleaseDatePrecision = precision,
                IsCompilation        = isCompilation,
                Title                = isCompilation ? compilationTitle : null
            };

            context.SoftwareReleases.Add(dbRelease);
            createdReleases.Add(dbRelease);
        }

        return createdReleases;
    }

    static (DateTime? date, DatePrecision precision) ParseDate(string dateStr)
    {
        if(string.IsNullOrWhiteSpace(dateStr))
            return (null, DatePrecision.Full);

        dateStr = dateStr.Trim();

        // Full date: "Oct 07, 2010" or "Nov 14, 2007"
        if(DateTime.TryParseExact(dateStr, "MMM dd, yyyy",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None, out var fullDate))
            return (fullDate, DatePrecision.Full);

        // Month+Year: "Oct, 2010" or "Dec 2007"
        if(DateTime.TryParseExact(dateStr, "MMM, yyyy",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthYear))
            return (monthYear, DatePrecision.MonthYear);

        if(DateTime.TryParseExact(dateStr, "MMM yyyy",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None, out monthYear))
            return (monthYear, DatePrecision.MonthYear);

        // Year only: "2008"
        if(int.TryParse(dateStr, out int year) && year is > 1900 and < 2100)
            return (new DateTime(year, 1, 1), DatePrecision.YearOnly);

        return (null, DatePrecision.Full);
    }

    static string ExtractYear(string dateStr)
    {
        if(string.IsNullOrWhiteSpace(dateStr))
            return null;

        dateStr = dateStr.Trim();

        // Full date: "Oct 07, 2010"
        if(DateTime.TryParseExact(dateStr, "MMM dd, yyyy",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None, out var full))
            return full.Year.ToString();

        // Month+Year: "Oct, 2010" or "Dec 2007"
        if(DateTime.TryParseExact(dateStr, "MMM, yyyy",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None, out var my))
            return my.Year.ToString();

        if(DateTime.TryParseExact(dateStr, "MMM yyyy",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None, out my))
            return my.Year.ToString();

        // Year only: "2008"
        if(int.TryParse(dateStr, out int year) && year is > 1900 and < 2100)
            return year.ToString();

        return null;
    }

    async Task<List<(ulong Id, string Name, double Score)>> FindFuzzySoftwareMatchesAsync(
        MarechaiContext context, string gameName)
    {
        var results = new List<(ulong Id, string Name, double Score)>();

        // Normalize for comparison
        string normalized = gameName.ToUpperInvariant();

        // Load candidates that share a common prefix to limit DB load
        // Use first two words for short first words, or first word if long enough
        string[] words     = gameName.Split([' ', ':', '-'], StringSplitOptions.RemoveEmptyEntries);
        string   prefix    = words.Length >= 2 && words[0].Length < 3
                                 ? string.Join(" ", words.Take(2))
                                 : words.Length > 0 ? words[0] : null;

        if(string.IsNullOrWhiteSpace(prefix))
            return results;

        var candidates = await context.Softwares
                                       .Where(s => s.Name.StartsWith(prefix))
                                       .Select(s => new { s.Id, s.Name })
                                       .ToListAsync();

        foreach(var candidate in candidates)
        {
            if(string.Equals(candidate.Name, gameName, StringComparison.OrdinalIgnoreCase))
                continue; // exact matches handled elsewhere

            double score = 0;

            string candNorm = candidate.Name.ToUpperInvariant();

            // Prefix match: one name starts with the other
            if(normalized.StartsWith(candNorm) || candNorm.StartsWith(normalized))
            {
                // Score based on length ratio — closer lengths = higher score
                double lenRatio = (double)Math.Min(normalized.Length, candNorm.Length) /
                                  Math.Max(normalized.Length, candNorm.Length);

                score = 0.85 + 0.15 * lenRatio; // 0.85–1.0 range
            }
            else
            {
                score = JaroWinklerSimilarity(normalized, candNorm);
            }

            if(score >= 0.85)
                results.Add(((ulong)candidate.Id, candidate.Name, score));
        }

        return results.OrderByDescending(r => r.Score).Take(10).ToList();
    }

    static double JaroWinklerSimilarity(string a, string b)
    {
        if(string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return 0.0;

        if(string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        int matchWindow = Math.Max(a.Length, b.Length) / 2 - 1;

        if(matchWindow < 0) matchWindow = 0;

        var aMatched = new bool[a.Length];
        var bMatched = new bool[b.Length];
        int matches  = 0, transpositions = 0;

        for(int i = 0; i < a.Length; i++)
        {
            int start = Math.Max(0, i - matchWindow);
            int end   = Math.Min(i + matchWindow + 1, b.Length);

            for(int j = start; j < end; j++)
            {
                if(bMatched[j] || a[i] != b[j]) continue;

                aMatched[i] = true;
                bMatched[j] = true;
                matches++;

                break;
            }
        }

        if(matches == 0) return 0.0;

        int k = 0;

        for(int i = 0; i < a.Length; i++)
        {
            if(!aMatched[i]) continue;

            while(!bMatched[k]) k++;

            if(a[i] != b[k]) transpositions++;

            k++;
        }

        double m    = matches;
        double jaro = (m / a.Length + m / b.Length + (m - transpositions / 2.0) / m) / 3.0;

        // Winkler prefix bonus
        int prefixLen = 0;

        for(int i = 0; i < Math.Min(Math.Min(a.Length, b.Length), 4); i++)
        {
            if(a[i] == b[i])
                prefixLen++;
            else
                break;
        }

        return jaro + prefixLen * 0.1 * (1.0 - jaro);
    }

    /// <summary>
    ///     Maps a MobyGames company role label to a Marechai SoftwareRole code.
    ///     Returns null for unrecognized roles (which will be logged as warnings).
    /// </summary>
    static string MapRoleLabel(string roleLabel) => roleLabel switch
    {
        "Published by"     => "pub",
        "Developed by"     => "dev",
        "Distributed by"   => "dis",
        "Localized by"     => "loc",
        "Ported by"                => "por",
        "Manufactured by"          => "mfg",
        "Licensed by"              => "lic",
        "Additional Development by" => "dev",
        "Additional Programming by" => "dev",
        "Additional Graphics by"   => "gfx",
        "Graphics by"              => "gfx",
        "Copy Protection by"       => "cpy",
        "Cutscenes by"             => "cut",
        "Motion Capture by"        => "moc",
        "Additional Sound by"      => "snd",
        "Sound by"                 => "snd",
        "Music by"                 => "snd",
        "Package Design by"        => "pkg",
        "Voice Recording by"       => "vrc",
        "Game Engine by"           => "eng",
        "Graphic Engine by"        => "eng",
        "Sound Engine by"          => "eng",
        "Middleware by"            => "mdw",
        "Testing by"               => "tst",
        "Contributions by"         => "ctb",
        "Fonts by"                 => "fnt",
        "Produced by"              => "prd",
        "Voice Production by"      => "vrc",
        "Manual by"                => "doc",
        "Additional Music by"      => "snd",
        "Original Concept by"      => "ocp",
        "Funded by"                => "fnd",
        "Casting by"               => "cst",
        "Marketed by"              => "mkt",
        "Additional Design by"     => "des",
        "Writing by"               => "wri",
        _                          => null
    };

    /// <summary>
    ///     Maps a MobyGames credit role string to a DocumentRole ID.
    ///     Returns null for unmappable roles (character names, DLC names, etc.)
    /// </summary>
    static string MapCreditRole(string role) => role switch
    {
        // Acting / Voice
        "Cast"                           => "act",
        "Voice"                          => "vac",
        "Voice Acting"                   => "vac",
        "Voice Actors"                   => "vac",
        "Voice Cast"                     => "vac",
        "Voice Over"                     => "vac",
        "Voice Talent"                   => "vac",

        // Art / Graphics
        "Art"                            => "art",
        "Artist"                         => "art",
        "Graphics"                       => "art",
        "Graphics Design"                => "art",
        "Graphic Design"                 => "art",
        "Graphic Artist"                 => "art",
        "Additional Art"                 => "art",
        "Additional Graphics"            => "art",
        "Concept Art"                    => "art",
        "Background Art"                 => "art",
        "Character Art"                  => "art",
        "Pixel Art"                      => "art",

        // Animation
        "Animation"                      => "anm",
        "Animator"                       => "anm",
        "Animations"                     => "anm",
        "Lead Animator"                  => "anm",

        // Art Direction
        "Art Director"                   => "adi",
        "Art Direction"                  => "adi",

        // Audio / Sound
        "Audio"                          => "sds",
        "Audio Director"                 => "sds",
        "Audio Lead"                     => "sds",
        "Audio Design"                   => "sds",
        "Audio Engineering"              => "sds",
        "Sound"                          => "sds",
        "Sound Design"                   => "sds",
        "Sound Designer"                 => "sds",
        "Sound Effects"                  => "sds",
        "Sound Engineering"              => "sds",
        "SFX"                            => "sds",
        "Additional Sound"               => "sds",
        "Music and FX"                   => "sds",

        // Music / Composition
        "Music"                          => "cmp",
        "Composer"                       => "cmp",
        "Music Composition"              => "cmp",
        "Original Music"                 => "cmp",
        "Original Score"                 => "cmp",
        "Soundtrack"                     => "cmp",
        "Music (Muzyka)"                 => "cmp",

        // Design
        "Design"                         => "dsr",
        "Designer"                       => "dsr",
        "Game Design"                    => "dsr",
        "Game Designer"                  => "dsr",
        "Lead Designer"                  => "dsr",
        "Level Design"                   => "dsr",
        "Leveldesign"                    => "dsr",
        "Levels"                         => "dsr",
        "Level Designer"                 => "dsr",
        "World Design"                   => "dsr",
        "UI Design"                      => "dsr",
        "Interface Design"               => "dsr",

        // Direction
        "Director"                       => "drt",
        "Directed by"                    => "drt",
        "Game Director"                  => "drt",
        "Creative Director"              => "drt",

        // Programming
        "Program"                        => "prg",
        "Programmer"                     => "prg",
        "Programmers"                    => "prg",
        "Programming"                    => "prg",
        "Programmed by"                  => "prg",
        "Lead Programmer"                => "prg",
        "Additional Programming"         => "prg",
        "Engine Programming"             => "prg",
        "Tools Programming"              => "prg",
        "Code"                           => "prg",
        "Code (Kod)"                     => "prg",

        // Production
        "Producer"                       => "pro",
        "Executive Producer"             => "pro",
        "Associate Producer"             => "pro",
        "Co-Producer"                    => "pro",
        "Line Producer"                  => "pro",
        "Production"                     => "prd",
        "Production Lead"                => "pmn",
        "Production Manager"             => "pmn",
        "Production Management"          => "pmn",
        "Product Manager"                => "pmn",

        // Writing
        "Writer"                         => "aus",
        "Written by"                     => "aus",
        "Story"                          => "aus",
        "Scenario"                       => "aus",
        "Script"                         => "aus",
        "Screenwriter"                   => "aus",
        "Dialogue"                       => "aud",
        "Dialog"                         => "aud",

        // QA / Testing
        "Quality Assurance"              => "res",
        "Quality Assurance Lead"         => "res",
        "Quality Assurance Management"   => "res",
        "QA"                             => "res",
        "QA Lead"                        => "res",
        "QA Manager"                     => "res",
        "QA Testing"                     => "res",
        "Testers"                        => "res",
        "Testing"                        => "res",
        "Test Manager"                   => "res",
        "Betatesting"                    => "res",
        "Beta Testing"                   => "res",
        "Lead Tester"                    => "res",

        // Lead roles
        "Lead"                           => "led",
        "Creative Lead"                  => "led",
        "Technical Lead"                 => "tcd",
        "Technical Director"             => "tcd",

        // Creator
        "Game Creator"                   => "cre",
        "Creator"                        => "cre",
        "Developed by"                   => "cre",

        // Engineering
        "Engineer"                       => "eng",
        "Engineering"                    => "eng",

        // Special Thanks
        "Special Thanks"                 => "hnr",
        "Thanks to"                      => "hnr",
        "Special Thanks to"              => "hnr",

        // Localization / Translation
        "Localization"                   => "trl",
        "Translation"                    => "trl",
        "Translator"                     => "trl",
        "Localization Management"        => "trl",

        // Editing
        "Editor"                         => "edt",
        "Editing"                        => "edt",

        // Narration
        "Narrator"                       => "nrt",
        "Narration"                      => "nrt",

        // Other documented roles
        "Illustrator"                    => "ill",
        "Illustration"                   => "ill",
        "Cover Art"                      => "cov",
        "Cover Design"                   => "cov",
        "Package Design"                 => "dsr",
        "Manual"                         => "aut",
        "Documentation"                  => "aut",
        "Marketing"                      => "mrk",
        "Public Relations"               => "mrk",

        // Catch-all
        _                                => null
    };

    async Task ResolveDlcBaseGameAsync(MarechaiContext context, Software software, ParsedGame game)
    {
        try
        {
            // Resolve DLC's numeric ID from stored slug
            int? dlcNumericId = game.BaseGameMobyId;

            if(dlcNumericId is null)
            {
                // Try to resolve via slug redirect
                int? resolved = await _mobyHttpClient!.ResolveNumericGameIdAsync(game.MobyGameId);

                if(resolved is null)
                {
                    Console.WriteLine("    \u001b[33mWarning: Could not resolve DLC numeric ID for base game lookup\u001b[0m");

                    return;
                }

                // Fetch new-site page and parse base game ID
                string slug = game.MobyGameId.TrimStart('-');
                string url  = $"https://www.mobygames.com/game/{resolved}/{slug}/";
                string html = await _mobyHttpClient.FetchPageAsync(url);

                dlcNumericId = Parsers.NewSiteMainPageParser.ParseBaseGameId(html);

                if(dlcNumericId is null)
                {
                    Console.WriteLine("    \u001b[33mWarning: No base game found on MobyGames page\u001b[0m");

                    return;
                }
            }

            Console.WriteLine($"    Base game MobyGames ID: {dlcNumericId}");

            // Look up base game in our DB by MobyGames numeric ID
            MobyGamesImportState baseImportState = await context.MobyGamesImportStates
                .FirstOrDefaultAsync(s => s.MobyNumericId == dlcNumericId);

            if(baseImportState is not null)
            {
                software.BaseSoftwareId = (ulong)baseImportState.SoftwareId;
                await context.SaveChangesAsync();
                Console.WriteLine($"    Linked to base game Software ID: {baseImportState.SoftwareId}");
            }
            else
            {
                Console.WriteLine($"    \u001b[33mWarning: Base game (MobyGames ID {dlcNumericId}) not imported yet. " +
                                  "Run import-dlc-relations after importing base games.\u001b[0m");
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"    \u001b[33mWarning: Error resolving base game: {ex.Message}\u001b[0m");
        }
    }
}

sealed class CaseInsensitiveCreditComparer : IEqualityComparer<(ulong, int, string)>
{
    public bool Equals((ulong, int, string) x, (ulong, int, string) y)
        => x.Item1 == y.Item1 &&
           x.Item2 == y.Item2 &&
           string.Equals(x.Item3, y.Item3, StringComparison.OrdinalIgnoreCase);

    public int GetHashCode((ulong, int, string) obj)
        => HashCode.Combine(obj.Item1, obj.Item2, StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item3));
}
