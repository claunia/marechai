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
                                                                 .AddInterceptors(new MariaDb12CollationInterceptor())
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
                                                                                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                                                                                    // Transient resiliency: retry up to 3 times
                                                                                    // on connection-pool-exhausted / dropped-conn
                                                                                    // errors instead of failing the request.
                                                                                    .EnableRetryOnFailure(3)));

        builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
               .AddRoles<ApplicationRole>()
               .AddEntityFrameworkStores<MarechaiContext>();

        builder.Services.AddMarechaiEmail(builder.Configuration);

        builder.Services.AddScoped<TokenService, TokenService>();
        builder.Services.AddSingleton<InvitationCodeGenerator>();
        builder.Services.AddSingleton<AvatarFileCleaner>();
        builder.Services.AddScoped<UserAccountDeletionService>();
        builder.Services.AddScoped<DeletionPendingFilter>();
        builder.Services.AddHostedService<AccountDeletionPurgeService>();

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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Set up custom content types - associating file extension to MIME type
        var provider = new FileExtensionContentTypeProvider
        {
            Mappings =
            {
                // Add new mappings
                [".avif"] = "image/avif",   // AVIF image format
                [".heic"] = "image/heic",   // HEIC image format
                [".heif"] = "image/heif",   // HEIF image format
                [".jxl"]  = "image/jxl",    // JPEG-XL image format
                [".webp"] = "image/webp",   // WebP image format
                [".svg"]  = "image/svg+xml" // SVG image format
            }
        };

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider        = new PhysicalFileProvider(assetRootPath),
            RequestPath         = "/assets",
            ContentTypeProvider = provider
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

                start = DateTime.Now;
                Console.WriteLine("\e[31;1mImporting company logos...\e[0m");
                SvgRender.ImportCompanyLogos(assetRootPath, context);
                end = DateTime.Now;

                Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);

                start = DateTime.Now;
                Console.WriteLine("\e[31;1mRendering markdown in company descriptions...\e[0m");
                MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

                foreach(CompanyDescription companyDescription in
                        context.CompanyDescriptions.Where(cd => cd.Html == null))
                {
                    companyDescription.Html = Markdown.ToHtml(companyDescription.Text, pipeline);
                    context.Update(companyDescription);
                }

                context.SaveChanges();

                end = DateTime.Now;

                Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);

                start = DateTime.Now;
                Console.WriteLine("\e[31;1mRendering markdown in machine descriptions...\e[0m");

                foreach(MachineDescription machineDescription in
                        context.MachineDescriptions.Where(md => md.Html == null))
                {
                    machineDescription.Html = Markdown.ToHtml(machineDescription.Text, pipeline);
                    context.Update(machineDescription);
                }

                context.SaveChanges();

                end = DateTime.Now;

                Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);

                start = DateTime.Now;
                Console.WriteLine("\e[31;1mRendering markdown in sound synth descriptions...\e[0m");

                foreach(SoundSynthDescription soundSynthDescription in
                        context.SoundSynthDescriptions.Where(sd => sd.Html == null))
                {
                    soundSynthDescription.Html = Markdown.ToHtml(soundSynthDescription.Text, pipeline);
                    context.Update(soundSynthDescription);
                }

                context.SaveChanges();

                end = DateTime.Now;

                Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);

                start = DateTime.Now;
                Console.WriteLine("\e[31;1mRendering markdown in processor descriptions...\e[0m");

                foreach(ProcessorDescription processorDescription in
                        context.ProcessorDescriptions.Where(pd => pd.Html == null))
                {
                    processorDescription.Html = Markdown.ToHtml(processorDescription.Text, pipeline);
                    context.Update(processorDescription);
                }

                context.SaveChanges();

                end = DateTime.Now;

                Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);

                start = DateTime.Now;
                Console.WriteLine("\e[31;1mRendering markdown in GPU descriptions...\e[0m");

                foreach(GpuDescription gpuDescription in
                        context.GpuDescriptions.Where(gd => gd.Html == null))
                {
                    gpuDescription.Html = Markdown.ToHtml(gpuDescription.Text, pipeline);
                    context.Update(gpuDescription);
                }

                context.SaveChanges();

                end = DateTime.Now;

                Console.WriteLine("\e[31;1mTook \e[32;1m{0} seconds\e[31;1m...\e[0m", (end - start).TotalSeconds);
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