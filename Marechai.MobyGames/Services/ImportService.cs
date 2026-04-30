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

public class ImportService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService             _sourceDb;
    readonly CompanyMatcher                    _companyMatcher;
    readonly PersonMatcher                     _personMatcher;
    readonly PlatformMatcher                   _platformMatcher;
    readonly CountryMatcher                    _countryMatcher;
    readonly StateService                      _stateService;

    public ImportService(
        IDbContextFactory<MarechaiContext> contextFactory,
        SourceDatabaseService sourceDb,
        CompanyMatcher companyMatcher,
        PersonMatcher personMatcher,
        PlatformMatcher platformMatcher,
        CountryMatcher countryMatcher,
        StateService stateService)
    {
        _contextFactory  = contextFactory;
        _sourceDb        = sourceDb;
        _companyMatcher  = companyMatcher;
        _personMatcher   = personMatcher;
        _platformMatcher = platformMatcher;
        _countryMatcher  = countryMatcher;
        _stateService    = stateService;
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

        bool acceptAll = false;
        int  imported  = 0, rejected = 0, failed = 0;

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

                await ImportGameAsync(game, batchNumber);
                imported++;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"    ERROR: {ex}");
                await _stateService.MarkFailedAsync(gameId, ex.Message, batchNumber);
                failed++;
            }
        }

        Console.WriteLine($"\n  Batch complete: {imported} imported, {rejected} rejected, {failed} failed");
    }

    async Task ImportGameAsync(ParsedGame game, int batchNumber)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // 1. Check for existing Software with same name
        var existingByName = await context.Softwares
                                          .Where(s => s.Name == game.Name)
                                          .Select(s => new { s.Id, s.Name, s.IsGame })
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
                software = new Software { Name = game.Name, IsGame = true };
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
            software = new Software { Name = game.Name, IsGame = true };
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

        // 2. Description
        if(!string.IsNullOrWhiteSpace(game.Description))
        {
            context.SoftwareDescriptions.Add(new SoftwareDescription
            {
                SoftwareId   = software.Id,
                LanguageCode = "eng",
                Text         = game.Description,
                Html         = game.DescriptionHtml
            });
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

            context.GenresBySoftware.Add(new GenreBySoftware
            {
                SoftwareId = software.Id,
                GenreId    = dbGenre.Id
            });
        }

        // 4. Company roles from Main tab (developers)
        foreach(string devName in game.Developers)
        {
            if(string.IsNullOrWhiteSpace(devName)) continue;

            var (devCompany, _) = await _companyMatcher.MatchOrCreateAsync(devName);

            if(devCompany != null)
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

        // 5. Process releases from Releases tab (or create basic release from Main tab)
        if(game.Releases.Count > 0)
            await ImportReleasesAsync(context, software, game);
        else
            await ImportBasicReleaseAsync(context, software, game);

        // 6. Credits
        var addedCredits = new HashSet<(ulong, int, string)>();

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
        await _stateService.MarkImportedAsync(game.MobyGameId, batchNumber, software.Id);

        Console.WriteLine($"    Imported as Software ID: {software.Id}");
    }

    async Task ImportReleasesAsync(MarechaiContext context, Software software, ParsedGame game)
    {
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
                    SoftwareId           = software.Id,
                    PlatformId           = platform?.Id,
                    PublisherId          = publisher.Id,
                    ReleaseDate          = releaseDate,
                    ReleaseDatePrecision = precision,
                    IsCompilation        = false,
                    Title                = release.Comments
                };

                context.SoftwareReleases.Add(dbRelease);
                await context.SaveChangesAsync();

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

                    if(!exists)
                    {
                        context.SoftwareBarcodes.Add(new SoftwareBarcode
                        {
                            ReleaseId = dbRelease.Id,
                            Code      = barcode.Code,
                            Type      = barcodeType
                        });
                    }
                }

                // Product codes
                foreach(var productCode in release.ProductCodes)
                {
                    var issuer = productCode.Type switch
                    {
                        "Sony PN"        => ProductCodeIssuer.Sony,
                        "PSN/SEN Code"   => ProductCodeIssuer.PSN,
                        "Microsoft PN"   => ProductCodeIssuer.Microsoft,
                        "Nintendo PN"    => ProductCodeIssuer.Nintendo,
                        "Sega PN"        => ProductCodeIssuer.Sega,
                        "Activision PN"  => ProductCodeIssuer.Activision,
                        "Amazon ASIN"    => ProductCodeIssuer.Amazon,
                        "eBay Item No."  => ProductCodeIssuer.eBay,
                        _                => ProductCodeIssuer.Other
                    };

                    context.SoftwareProductCodes.Add(new SoftwareProductCode
                    {
                        ReleaseId = dbRelease.Id,
                        Issuer    = issuer,
                        Code      = productCode.Code
                    });
                }

                // Countries → UnM49 regions
                foreach(string country in release.Countries)
                {
                    var region = _countryMatcher.Match(country);

                    if(region != null)
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

                // Distributor/Localizer company roles on the software
                if(!string.IsNullOrWhiteSpace(release.Distributor))
                {
                    var (dist, _) = await _companyMatcher.MatchOrCreateAsync(release.Distributor);

                    if(dist != null)
                    {
                        bool exists = await context.SoftwareCompanyRoles
                                                   .AnyAsync(r => r.SoftwareId == software.Id &&
                                                                  r.CompanyId == dist.Id &&
                                                                  r.RoleId == "dis");

                        if(!exists)
                        {
                            context.SoftwareCompanyRoles.Add(new SoftwareCompanyRole
                            {
                                SoftwareId = software.Id,
                                CompanyId  = dist.Id,
                                RoleId     = "dis"
                            });
                        }
                    }
                }

                if(!string.IsNullOrWhiteSpace(release.Localizer))
                {
                    var (loc, _) = await _companyMatcher.MatchOrCreateAsync(release.Localizer);

                    if(loc != null)
                    {
                        bool exists = await context.SoftwareCompanyRoles
                                                   .AnyAsync(r => r.SoftwareId == software.Id &&
                                                                  r.CompanyId == loc.Id &&
                                                                  r.RoleId == "loc");

                        if(!exists)
                        {
                            context.SoftwareCompanyRoles.Add(new SoftwareCompanyRole
                            {
                                SoftwareId = software.Id,
                                CompanyId  = loc.Id,
                                RoleId     = "loc"
                            });
                        }
                    }
                }

                // Extra company roles from releases tab (e.g., "Ported by", etc.)
                foreach(var (roleLabel, companyName) in release.CompanyRoles)
                {
                    string roleId = MapRoleLabel(roleLabel);

                    var (roleCompany, _) = await _companyMatcher.MatchOrCreateAsync(companyName);

                    if(roleCompany != null)
                    {
                        bool exists = await context.SoftwareCompanyRoles
                                                   .AnyAsync(r => r.SoftwareId == software.Id &&
                                                                  r.CompanyId == roleCompany.Id &&
                                                                  r.RoleId == roleId);

                        if(!exists)
                        {
                            context.SoftwareCompanyRoles.Add(new SoftwareCompanyRole
                            {
                                SoftwareId = software.Id,
                                CompanyId  = roleCompany.Id,
                                RoleId     = roleId
                            });
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
    }

    async Task ImportBasicReleaseAsync(MarechaiContext context, Software software, ParsedGame game)
    {
        // Create one release per platform from Main tab data
        var (publisher, _) = await _companyMatcher.MatchOrCreateAsync(game.Publishers.FirstOrDefault());

        if(publisher is null) return;

        var (releaseDate, precision) = ParseDate(game.ReleaseDate);

        foreach(string platformName in game.Platforms.Count > 0 ? game.Platforms : ["Unknown"])
        {
            var platform = await _platformMatcher.MatchOrCreateAsync(platformName);

            var dbRelease = new SoftwareRelease
            {
                SoftwareId           = software.Id,
                PlatformId           = platform?.Id,
                PublisherId          = publisher.Id,
                ReleaseDate          = releaseDate,
                ReleaseDatePrecision = precision,
                IsCompilation        = false
            };

            context.SoftwareReleases.Add(dbRelease);
        }
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
        "Additional Graphics by"   => "gfx",
        "Graphics by"              => "gfx",
        "Copy Protection by"       => "cpy",
        "Cutscenes by"             => "cut",
        "Motion Capture by"        => "moc",
        "Additional Sound by"      => "snd",
        "Sound by"                 => "snd",
        "Package Design by"        => "pkg",
        "Voice Recording by"       => "vrc",
        "Game Engine by"           => "eng",
        "Middleware by"            => "mdw",
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
}
