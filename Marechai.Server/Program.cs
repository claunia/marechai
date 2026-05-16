using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aaru.CommonTypes.Interop;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.Email;
using Marechai.Helpers;
using Marechai.Server.Helpers;
using Marechai.Server.Services;
using Marechai.Server.Filters;
using Marechai.Translation;
using Markdig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Version = Marechai.Server.Interop.Version;

namespace Marechai.Server;

file class Program
{
    private static IDbCore _database;

    public static void Main(string[] args)
    {
        Console.Clear();

        Console.Write("\e[32m                             .                ,,\n" +
                      "\e[32m                          ;,.                  '0d.\n" +
                      "\e[32m                        oc                       oWd                      \e[31m" +
                      @"________/\\\\\\\\\___/\\\\\\\\\\\_________/\\\\\\\\\___/\\\\____________/\\\\_" +
                      "\n\e[0m" +
                      "\e[32m                      ;X.                         'WN'                    \e[31m" +
                      @" _____/\\\////////___\/////\\\///_______/\\\////////___\/\\\\\\________/\\\\\\_" +
                      "\n\e[0m" +
                      "\e[32m                     oMo                           cMM:                   \e[31m" +
                      @"  ___/\\\/________________\/\\\________/\\\/____________\/\\\//\\\____/\\\//\\\_" +
                      "\n\e[0m" +
                      "\e[32m                    ;MM.                           .MMM;                  \e[31m" +
                      @"   __/\\\__________________\/\\\_______/\\\______________\/\\\\///\\\/\\\/_\/\\\_" +
                      "\n\e[0m" +
                      "\e[32m                    NMM                             WMMW                  \e[31m" +
                      @"    _\/\\\__________________\/\\\______\/\\\______________\/\\\__\///\\\/___\/\\\_" +
                      "\n\e[0m" +
                      "\e[32m                   'MMM                             MMMM;                 \e[31m" +
                      @"     _\//\\\_________________\/\\\______\//\\\_____________\/\\\____\///_____\/\\\_" +
                      "\n\e[0m" +
                      "\e[32m                   ,MMM:                           dMMMM:                 \e[31m" +
                      @"      __\///\\\_______________\/\\\_______\///\\\___________\/\\\_____________\/\\\_" +
                      "\n\e[0m" +
                      "\e[32m                   .MMMW.                         :MMMMM.                 \e[31m" +
                      @"       ____\////\\\\\\\\\___/\\\\\\\\\\\_____\////\\\\\\\\\__\/\\\_____________\/\\\_" +
                      "\n\e[0m" +
                      "\e[32m                    XMMMW:    .:xKNMMMMMMN0d,    lMMMMMd                  \e[31m" +
                      @"        _______\/////////___\///////////_________\/////////___\///______________\///__" +
                      "\n\e[0m" +
                      "\e[32m                    :MMMMMK; cWMNkl:;;;:lxKMXc .0MMMMMO                   \e[37;1m          MARECHAI\e[0m\n" +
                      "\e[32m                   ..KMMMMMMNo,.             ,OMMMMMMW:,.                 \e[37;1m          Master repository of computing history artifacts information\e[0m\n" +
                      "\e[32m            .;d0NMMMMMMMMMMMMMMW0d:'    .;lOWMMMMMMMMMMMMMXkl.            \e[37;1m          Version \e[0m\e[33m{0}\e[37;1m-\e[0m\e[31m{1}\e[0m\n" +
                      "\e[32m          :KMMMMMMMMMMMMMMMMMMMMMMMMc  WMMMMMMMMMMMMMMMMMMMMMMWk'\e[0m\n" +
                      "\e[32m        ;NMMMMWX0kkkkO0XMMMMMMMMMMM0'  dNMMMMMMMMMMW0xl:;,;:oOWMMX;       \e[37;1m          Running under \e[35;1m{2}\e[37;1m, \e[35m{3}-bit\e[37;1m in \e[35m{4}-bit\e[37;1m mode.\e[0m\n" +
                      "\e[32m       xMMWk:.           .c0MMMMMW'      OMMMMMM0c'..          .oNMO      \e[37;1m          Using \e[33;1m{5}\e[37;1m version \e[31;1m{6}\e[0m\n" +
                      "\e[32m      OMNc            .MNc   oWMMk       'WMMNl. .MMK             ;KX.\e[0m\n" +
                      "\e[32m     xMO               WMN     ;  .,    ,  ':    ,MMx               lK\e[0m\n" +
                      "\e[32m    ,Md                cMMl     .XMMMWWMMMO      XMW.                :\e[0m\n" +
                      "\e[32m    Ok                  xMMl     XMMMMMMMMc     0MW,\e[0m\n" +
                      "\e[32m    0                    oMM0'   lMMMMMMMM.   :NMN'\e[0m\n" +
                      "\e[32m    .                     .0MMKl ;MMMMMMMM  oNMWd\e[0m\n" +
                      "\e[32m                            .dNW cMMMMMMMM, XKl\e[0m\n" +
                      "\e[32m                                 0MMMMMMMMK\e[0m\n" +
                      "\e[32m                                ;MMMMMMMMMMO                              \e[37;1m          Proudly presented to you by:\e[0m\n" +
                      "\e[32m                               'WMMMMKxMMMMM0                             \e[34;1m          Natalia Portillo\e[0m\n" +
                      "\e[32m                              oMMMMNc  :WMMMMN:\e[0m\n" +
                      "\e[32m                           .dWMMM0;      dWMMMMXl.                        \e[37;1m          Thanks to all contributors, collaborators, translators, donators and friends.\e[0m\n" +
                      "\e[32m               .......,cd0WMMNk:           c0MMMMMWKkolc:clodc'\e[0m\n" +
                      "\e[32m                 .';loddol:'.                 ':oxkkOkkxoc,.\e[0m\n" +
                      "\e[0m\n",
                      Version.GetVersion(),
#if DEBUG
                      "DEBUG"
#else
                          "RELEASE"
#endif
                     ,
                      DetectOS.GetPlatformName(DetectOS.GetRealPlatformID()),
                      Environment.Is64BitOperatingSystem ? 64 : 32,
                      Environment.Is64BitProcess ? 64 : 32,
                      DetectOS.IsMono ? "Mono" : ".NET Core",
                      DetectOS.IsMono ? Version.GetMonoVersion() : Version.GetNetCoreVersion());

        Console.WriteLine("\e[31;1mUpdating MySQL database without Entity Framework if it exists...\e[0m");
        _database = new Mysql();

        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        IConfigurationRoot    configuration        = configurationBuilder.Build();
        string                connectionString     = configuration.GetConnectionString("DefaultConnection");

        if(connectionString is null)
            Console.WriteLine("\e[31;1mCould not find a correct connection string...\e[0m");
        else
        {
            string   server = null, user = null, database = null, password = null;
            ushort   port   = 0;
            string[] pieces = connectionString.Split(';');

            foreach(string piece in pieces)
            {
                if(piece.StartsWith("server=", StringComparison.Ordinal))
                    server = piece[7..];
                else if(piece.StartsWith("user=", StringComparison.Ordinal))
                    user = piece[5..];
                else if(piece.StartsWith("password=", StringComparison.Ordinal))
                    password = piece[9..];
                else if(piece.StartsWith("database=", StringComparison.Ordinal))
                    database = piece[9..];
                else if(piece.StartsWith("port=", StringComparison.Ordinal))
                {
                    string portString = piece[5..];

                    ushort.TryParse(portString, out port);
                }
            }

            if(server is null || user is null || database is null || password is null || port == 0)
                Console.WriteLine("\e[31;1mCould not find a correct connection string...\e[0m");

            else
            {
                bool res = _database.OpenDb(server, user, database, password, port);

                if(res)
                {
                    Console.WriteLine("\e[31;1mClosing database...\e[0m");
                    _database.CloseDb();
                }
            }
        }

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        string assetRootPath = builder.Configuration["AssetRootPath"];

        if(string.IsNullOrEmpty(assetRootPath))
        {
            Console.WriteLine("\e[31;1mAsset root path not set, cannot continue, check configuration...\e[0m");

            return;
        }

        Directory.CreateDirectory(assetRootPath);

        DateTime start = DateTime.Now;
        Console.WriteLine("\e[31;1mRendering new country flags...\e[0m");
        SvgRender.RenderCountries(assetRootPath);
        DateTime end = DateTime.Now;

        Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);

        start = DateTime.Now;
        Console.WriteLine("\e[31;1mEnsuring photo folders exist...\e[0m");
        Photos.EnsureCreated(assetRootPath, false, "machines");
        Photos.EnsureCreated(assetRootPath, false, "gpus");
        Photos.EnsureCreated(assetRootPath, false, "processors");
        Photos.EnsureCreated(assetRootPath, false, "sound-synths");
        Photos.EnsureCreated(assetRootPath, false, "people");
        Photos.EnsureCreated(assetRootPath, false, "software-screenshots");
    Photos.EnsureCreated(assetRootPath, false, "software-covers");
        Console.WriteLine("\e[31;1mEnsuring scan folders exist...\e[0m");
        Photos.EnsureCreated(assetRootPath, true, "books");
        Photos.EnsureCreated(assetRootPath, true, "documents");
        Photos.EnsureCreated(assetRootPath, true, "magazines");
        end = DateTime.Now;

        Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);

        start = DateTime.Now;
        Console.WriteLine("\e[31;1mBackfilling missing JPEG-XL variants...\e[0m");
        Photos.BackfillJxl(assetRootPath, false, "machines");
        Photos.BackfillJxl(assetRootPath, false, "gpus");
        Photos.BackfillJxl(assetRootPath, false, "processors");
        Photos.BackfillJxl(assetRootPath, false, "sound-synths");
        Photos.BackfillJxl(assetRootPath, false, "people");
        Photos.BackfillJxl(assetRootPath, false, "software-screenshots");
    Photos.BackfillJxl(assetRootPath, false, "software-covers");
        Photos.BackfillJxl(assetRootPath, true,  "books");
        Photos.BackfillJxl(assetRootPath, true,  "documents");
        Photos.BackfillJxl(assetRootPath, true,  "magazines");
        end = DateTime.Now;

        Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);

        // Add services to the container.
        builder.Services.AddControllers(options =>
                {
                    // Globally block authenticated requests from users in the GDPR deletion grace
                    // window, except where the action carries [AllowDeletionPending]. The filter is
                    // scoped (depends on MarechaiContext) so we register the type and let DI resolve
                    // it per-request.
                    options.Filters.Add<DeletionPendingFilter>();

                    // Keep the "Async" suffix in registered action names so that
                    // CreatedAtAction(nameof(GetAllAsync), ...) and similar route lookups match
                    // the action name as written in the source. Without this the framework
                    // strips the suffix and nameof(...) returns a string that no route matches,
                    // throwing "No route matches the supplied values" at response-formatting time.
                    options.SuppressAsyncSuffixInActionNames = false;
                })
               .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new IsoDateTimeConverter());
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.WriteIndented        = true;
                    options.JsonSerializerOptions.ReferenceHandler     = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.NumberHandling       = JsonNumberHandling.Strict;
                });

        // In-process memory cache used by SoftwareController for the global Marechai
        // ranking (avoids re-ranking the catalog on every page load).
        builder.Services.AddMemoryCache();

        // Output caching for the /software landing-page endpoints. The controller
        // layer already uses IMemoryCache for the heavy DB queries; the framework-
        // level OutputCache adds a second layer that short-circuits before the
        // controller method allocates / serialises (so repeated requests skip MVC
        // pipeline overhead entirely). Each action that opts in via [OutputCache]
        // picks up this 5-min base policy unless overridden.
        builder.Services.AddOutputCache(options =>
        {
            options.AddBasePolicy(b => b.Expire(TimeSpan.FromMinutes(5)));
        });

        // Compress JSON / text responses. Brotli + Gzip only — the application/json
        // payloads from this API compress to ~10–20% of their original size, which
        // is significant over real networks (and free on localhost).
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/json", "application/problem+json", "text/plain"
            });
        });

        builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
        builder.Services.Configure<GzipCompressionProviderOptions>(o   => o.Level = CompressionLevel.Fastest);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            // Tell OpenAPI generator to report number fields as integers/floats only, not strings
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Add custom route constraints
        builder.Services.Configure<RouteOptions>(options =>
        {
            options.ConstraintMap["ulong"] = typeof(UlongRouteConstraint);
            options.ConstraintMap["char"]  = typeof(CharRouteConstraint);
        });

        builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
                })
               .AddJwtBearer(options =>
                {
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew                = TimeSpan.Zero,
                        ValidateIssuer           = true,
                        ValidateAudience         = true,
                        ValidateLifetime         = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
                        ValidAudience            = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey =
                            new
                                SymmetricSecurityKey(System.Text.Encoding.UTF8
                                                          .GetBytes(builder.Configuration["Jwt:Key"]!))
                    };

                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnChallenge = ctx =>
                        {
                            ctx.HandleResponse();
                            ctx.Response.StatusCode  = 401;
                            ctx.Response.ContentType = "application/problem+json";

                            return ctx.Response.WriteAsJsonAsync(new
                            {
                                type   = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                                title  = "Unauthorized",
                                detail = "You must be logged in to perform this action.",
                                status = 401
                            });
                        },
                        OnForbidden = ctx =>
                        {
                            ctx.Response.StatusCode  = 403;
                            ctx.Response.ContentType = "application/problem+json";

                            return ctx.Response.WriteAsJsonAsync(new
                            {
                                type   = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                                title  = "Forbidden",
                                detail = "You do not have permission to perform this action.",
                                status = 403
                            });
                        }
                    };
                });

        builder.Services.AddDbContextFactory<MarechaiContext>(options => options.UseLazyLoadingProxies()
                                                                 .AddMarechaiInterceptors()
                                                                 .UseMySql(builder.Configuration
                                                                              .GetConnectionString("DefaultConnection"),
                                                                           new
                                                                               MariaDbServerVersion(new System.
                                                                                   Version(12, 0, 2)),
                                                                           // SingleQuery is the safer default when the DB is on a
                                                                           // remote host: SplitQuery turns one logical query into N+1
                                                                           // round-trips (one per collection navigation), each costing
                                                                           // the DB RTT (~57 ms in this deployment). Endpoints that
                                                                           // actually benefit from splitting can opt in per-query with
                                                                           // .AsSplitQuery().
                                                                           b => b.UseMicrosoftJson()
                                                                                    .EnableStringComparisonTranslations()
                                                                                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)));

        builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
               .AddRoles<ApplicationRole>()
               .AddEntityFrameworkStores<MarechaiContext>();

        builder.Services.AddMarechaiEmail(builder.Configuration);

        builder.Services.AddScoped<TokenService, TokenService>();
        builder.Services.AddSingleton<InvitationCodeGenerator>();
        builder.Services.AddSingleton<AvatarFileCleaner>();
        builder.Services.AddSingleton<FuzzySearchService>();
        builder.Services.AddScoped<UserAccountDeletionService>();
        builder.Services.AddScoped<DeletionPendingFilter>();
        builder.Services.AddHostedService<AccountDeletionPurgeService>();

        // In-memory queue + background drain that emails recipients when a new message lands in their inbox.
        // The MessagesController writes to the queue after each successful SaveChangesAsync; the worker
        // rehydrates from the DB inside its own scope, respects the recipient's NotifyOnNewMessage flag,
        // and renders the email in the recipient's LastLanguageVisited culture (fallback English).
        builder.Services.AddSingleton<Marechai.Server.Services.MessageNotifications.MessageNotificationQueue>();
        builder.Services.AddHostedService<Marechai.Server.Services.MessageNotifications.MessageNotificationWorker>();

        // OpenAI + NLLB HttpClients (each registered only when its Url is configured) plus the
        // shared TranslationService singleton. Reused by both the SoftwareController genre
        // endpoints (via SoftwareGenreTranslationCache) and the TranslationWorker.
        builder.Services.AddMarechaiTranslation(builder.Configuration);

        // Process-lifetime cache holding every SoftwareGenre + every SoftwareGenreTranslation row.
        // Populated once on startup (eager-warm below) and mutated thereafter by TranslationWorker
        // (registrations + translation upserts) or on-demand by the cache itself on miss. Controllers
        // READ-only via SoftwareGenreTranslationCache.GetNameAsync(...), which falls back to a per-id
        // DB load when the id isn't in memory yet.
        builder.Services.AddSingleton<SoftwareGenreTranslationCache>();

        // Process-lifetime cache holding the SoftwareAttributeStrings pool + every
        // SoftwareAttributeStringTranslation. Same lifecycle as the genre cache. Read-only from
        // controllers via SoftwareAttributeTranslationCache.GetTranslatedAsync(text, lang), which
        // falls back to a per-text DB load on miss.
        builder.Services.AddSingleton<SoftwareAttributeTranslationCache>();

        // Genre translation provider (one of N — future entities register their own provider).
        // The TranslationWorker iterates ITranslationProvider implementations in registration
        // order; keep registrations grouped by entity for readability.
        builder.Services.AddSingleton<Marechai.Translation.ITranslationProvider,
                                      SoftwareGenreTranslationProvider>();

        // Software attribute translation provider — pools every non-Rating Key/Value into
        // SoftwareAttributeStrings and translates each text once per language.
        builder.Services.AddSingleton<Marechai.Translation.ITranslationProvider,
                                      SoftwareAttributeTranslationProvider>();

        // Software promo-art group translation provider — NO in-memory cache singleton; the read
        // endpoints project the localized name directly from the DB via a LEFT JOIN sub-query.
        // The worker still fills SoftwarePromoArtGroupTranslations in the background.
        builder.Services.AddSingleton<Marechai.Translation.ITranslationProvider,
                                      SoftwarePromoArtGroupTranslationProvider>();

        // Software cover caption translation provider — flat string-pool translation table keyed
        // by the canonical English caption text. NO in-memory cache; read endpoints project the
        // localized caption via a correlated sub-query against
        // SoftwareCoverCaptionTranslations with English fallback.
        builder.Services.AddSingleton<Marechai.Translation.ITranslationProvider,
                                      SoftwareCoverCaptionTranslationProvider>();

        // PeopleBySoftware role translation provider — flat string-pool translation table keyed
        // by the canonical English role text. NO in-memory cache; read endpoints project the
        // localized role via a correlated sub-query against PeopleBySoftwareRoleTranslations
        // with English fallback. Multiple credits sharing the same role (e.g. "Programmer")
        // reuse a single translation row per language.
        builder.Services.AddSingleton<Marechai.Translation.ITranslationProvider,
                                      PeopleBySoftwareRoleTranslationProvider>();

        // Software screenshot caption translation provider — FK-link translation table keyed by
        // SoftwareScreenshot.Id (Guid). NO in-memory cache; read endpoints project the localized
        // caption via a correlated sub-query against SoftwareScreenshotCaptionTranslations with
        // English fallback. Each screenshot's caption is translated independently (no cross-
        // screenshot dedup); UpdateAsync invalidates the rows when the canonical caption text
        // changes so the worker re-translates on its next sweep.
        builder.Services.AddSingleton<Marechai.Translation.ITranslationProvider,
                                      SoftwareScreenshotCaptionTranslationProvider>();

        // Coordinator that hands off control between the primary TranslationWorker (running on
        // a 6-hour sweep cycle) and the companion DescriptionTranslationWorker (running ONLY
        // during the inter-sweep slumber). The companion fills missing per-language rows in
        // every description / synopsis table one at a time, yielding gracefully when the
        // primary worker wakes.
        builder.Services.AddSingleton<TranslationPhaseCoordinator>();

        // Per-table adapters the DescriptionTranslationWorker iterates over. One implementation
        // per description / synopsis table; each one owns its anti-join query (find an English
        // row that has no matching target-language row) and its insert path.
        builder.Services.AddSingleton<Marechai.Server.Services.DescriptionTranslation.IDescriptionTranslationSource,
                                      Marechai.Server.Services.DescriptionTranslation.SoftwareDescriptionSource>();
        builder.Services.AddSingleton<Marechai.Server.Services.DescriptionTranslation.IDescriptionTranslationSource,
                                      Marechai.Server.Services.DescriptionTranslation.CompanyDescriptionSource>();
        builder.Services.AddSingleton<Marechai.Server.Services.DescriptionTranslation.IDescriptionTranslationSource,
                                      Marechai.Server.Services.DescriptionTranslation.PersonDescriptionSource>();
        builder.Services.AddSingleton<Marechai.Server.Services.DescriptionTranslation.IDescriptionTranslationSource,
                                      Marechai.Server.Services.DescriptionTranslation.MachineDescriptionSource>();
        builder.Services.AddSingleton<Marechai.Server.Services.DescriptionTranslation.IDescriptionTranslationSource,
                                      Marechai.Server.Services.DescriptionTranslation.GpuDescriptionSource>();
        builder.Services.AddSingleton<Marechai.Server.Services.DescriptionTranslation.IDescriptionTranslationSource,
                                      Marechai.Server.Services.DescriptionTranslation.ProcessorDescriptionSource>();
        builder.Services.AddSingleton<Marechai.Server.Services.DescriptionTranslation.IDescriptionTranslationSource,
                                      Marechai.Server.Services.DescriptionTranslation.SoundSynthDescriptionSource>();
        builder.Services.AddSingleton<Marechai.Server.Services.DescriptionTranslation.IDescriptionTranslationSource,
                                      Marechai.Server.Services.DescriptionTranslation.BookSynopsisSource>();
        builder.Services.AddSingleton<Marechai.Server.Services.DescriptionTranslation.IDescriptionTranslationSource,
                                      Marechai.Server.Services.DescriptionTranslation.DocumentSynopsisSource>();
        builder.Services.AddSingleton<Marechai.Server.Services.DescriptionTranslation.IDescriptionTranslationSource,
                                      Marechai.Server.Services.DescriptionTranslation.MagazineSynopsisSource>();

        // Background worker that fills SoftwareGenreTranslations using OpenAI (preferred) /
        // NLLB (fallback). Exits permanently if neither provider is configured. MUST be
        // registered after AddMarechaiTranslation + AddSingleton<SoftwareGenreTranslationCache>.
        builder.Services.AddHostedService<TranslationWorker>();

        // Companion description-translation worker that runs during the primary worker's
        // 6-hour slumber. Coordinated via TranslationPhaseCoordinator (registered above).
        builder.Services.AddHostedService<DescriptionTranslationWorker>();

        // Backfills the rendered HTML column for any description row whose markdown Text has
        // changed since the last HTML render. Previously this work was done synchronously
        // before `app.Run()`, blocking the listening port; moving it to a hosted service lets
        // the API start serving immediately and processes descriptions in batches afterwards.
        builder.Services.AddHostedService<MarkdownHtmlBackfillWorker>();

        // Named HttpClient for the YouTube oEmbed endpoint
        // (https://www.youtube.com/oembed?url=...&format=json) used by
        // GpuVideoSuggestionApplier to auto-fetch the canonical video title at submission
        // time. Short timeout + UA so transient YouTube hiccups don't block the suggestion
        // submission for too long.
        builder.Services.AddHttpClient(Marechai.Server.Helpers.YouTubeOEmbedClient.HttpClientName, c =>
        {
            c.Timeout = TimeSpan.FromSeconds(8);
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Marechai/1.0 (+https://marechai.net)");
            c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                              policy =>
                              {
                                  policy.SetIsOriginAllowed(_ => true)
                                        .AllowAnyMethod()
                                        .AllowAnyHeader();
                              });
        });

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if(app.Environment.IsDevelopment()) app.MapOpenApi();

        // Only redirect to HTTPS in production
        if(!app.Environment.IsDevelopment()) app.UseHttpsRedirection();

        // Compression must run before CORS / auth so the response body is compressed.
        app.UseResponseCompression();

        // Use CORS before authentication/authorization
        app.UseCors("AllowFrontend");

        // OutputCache must run before authentication for [AllowAnonymous] cache hits to
        // avoid the per-request auth roundtrip, but after CORS so cached responses are
        // still returned with the right Access-Control-Allow-Origin headers.
        app.UseOutputCache();

        app.UseAuthentication();
        app.UseAuthorization();

        // Capture Accept-Language on every authenticated request and persist it to
        // ApplicationUser.LastLanguageVisited (with an in-memory cache so the DB is only touched on
        // change). Reads what HttpAuthHandler in the Blazor app forwards from CurrentUICulture, so
        // background workers (notably MessageNotificationWorker) can render emails in the recipient's
        // preferred language even when the recipient is offline at send time.
        app.UseMiddleware<Marechai.Server.Middleware.LastLanguageCaptureMiddleware>();

        app.MapControllers();

        // Set up custom content types - associating file extension to MIME type
        var provider = new FileExtensionContentTypeProvider
        {
            Mappings =
            {
                // Add new mappings
                [".avif"] = "image/avif",   // AVIF image format
                [".jxl"]  = "image/jxl",    // JPEG-XL image format
                [".webp"] = "image/webp",   // WebP image format
                [".svg"]  = "image/svg+xml" // SVG image format
            }
        };

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider        = new PhysicalFileProvider(assetRootPath),
            RequestPath         = "/assets",
            ContentTypeProvider = provider,
            // Asset filenames under /assets are content-addressed by GUID, so any file
            // served from here is byte-for-byte immutable for its lifetime. Tell browsers
            // and intermediate caches they can keep it for a full day and skip
            // revalidation. StaticFileMiddleware already emits ETag/Last-Modified and
            // honours If-None-Match/If-Modified-Since for free; this just upgrades the
            // freshness lifetime so the 304 round-trip itself disappears for hot assets.
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.CacheControl = "public, max-age=86400, immutable";
            }
        });

        using(IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;

            try
            {
                start = DateTime.Now;
                Console.WriteLine("\e[31;1mUpdating database with Entity Framework...\e[0m");
                MarechaiContext context = services.GetRequiredService<MarechaiContext>();
                context.Database.Migrate();
                end = DateTime.Now;

                Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);

                // Eager-warm the genre-translation cache so the first /software/genres request
                // doesn't pay the load cost. Singleton — resolved from the root provider, not the
                // per-request scope. Failure is logged but non-fatal: GetNameAsync() will fall
                // back to a per-id DB load on miss if the bulk warm never completed.
                start = DateTime.Now;
                Console.WriteLine("\e[31;1mWarming software genre translation cache...\e[0m");

                try
                {
                    SoftwareGenreTranslationCache genreCache =
                        app.Services.GetRequiredService<SoftwareGenreTranslationCache>();
                    genreCache.EnsureLoadedAsync().GetAwaiter().GetResult();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("\e[31;1mGenre cache warm-up failed: {0}\e[0m", ex.Message);
                }

                end = DateTime.Now;
                Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);

                start = DateTime.Now;
                Console.WriteLine("\e[31;1mWarming software attribute translation cache...\e[0m");

                try
                {
                    SoftwareAttributeTranslationCache attrCache =
                        app.Services.GetRequiredService<SoftwareAttributeTranslationCache>();
                    attrCache.EnsureLoadedAsync().GetAwaiter().GetResult();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("\e[31;1mAttribute cache warm-up failed: {0}\e[0m", ex.Message);
                }

                end = DateTime.Now;
                Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);

                start = DateTime.Now;
                Console.WriteLine("\e[31;1mImporting company logos...\e[0m");
                SvgRender.ImportCompanyLogos(assetRootPath, context);
                end = DateTime.Now;

                Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);

                // Markdown→HTML backfill for the 5 description tables previously lived here as
                // a synchronous foreach + SaveChanges per table. It now runs in
                // MarkdownHtmlBackfillWorker (a BackgroundService registered above) so the API
                // can start listening immediately. See comments on that worker for the
                // rationale.
            }
            catch(Exception ex)
            {
                Console.WriteLine("\e[31;1mCould not open database...\e[0m");
#if DEBUG
                Console.WriteLine("\e[31;1mException: {0}\e[0m", ex.Message);
#endif
                return;
            }
        }

        Console.WriteLine("\e[31;1mStarting API server...\e[0m");
        app.Run();
    }
}