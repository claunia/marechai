using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;
using PuppeteerSharp.BrowserData;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Embedded headless Chromium driver used to authenticate against MobyGames and fetch
///     pages that require a logged-in (and optionally MobyPlus) session.
///     <para>
///         Cookies obtained here are exported via <see cref="ExportCookiesAsync"/> and copied
///         into <see cref="MobyGamesHttpClient"/> so the existing fast HTTP code paths get the
///         authenticated experience without paying the browser-launch cost per request.
///     </para>
///     <para>
///         Chromium is auto-downloaded on first use via PuppeteerSharp's BrowserFetcher into
///         <c>state/puppeteer-chromium</c> (configurable via <c>MobyGames:Auth:BrowserCachePath</c>).
///         Cookies are persisted to <c>state/mobygames-cookies.json</c> between runs so we only
///         go through the login flow when the saved session is missing or expired.
///     </para>
/// </summary>
public sealed class MobyGamesBrowser : IAsyncDisposable
{
    // Must match the actual Chromium build PuppeteerSharp downloads. Advertising Firefox while
    // running Chrome was an instant Cloudflare bot flag (mismatched UA + Chrome TLS/JS
    // fingerprints). Use a plausible recent Chrome/Linux UA — the major version doesn't have to
    // be exactly the Chromium build PuppeteerSharp pinned, but it should look like Chrome.
    /// <summary>
    ///     User agent the headless browser presents to Cloudflare. Cloudflare binds the
    ///     <c>cf_clearance</c> cookie to the user agent that solved the challenge, so
    ///     <see cref="MobyGamesHttpClient" /> MUST send exactly this string too — otherwise every
    ///     request made with the exported cookies gets HTTP 403 even though the login succeeded.
    /// </summary>
    internal const string DefaultUserAgent =
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36";

    static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true
    };

    readonly string _email;
    readonly string _password;
    readonly string _cookieCachePath;
    readonly string _browserCachePath;
    readonly string _userDataDir;
    readonly bool   _headless;
    readonly int    _rateLimitMs;

    IBrowser _browser;
    IPage    _page;
    DateTime _lastRequestUtc = DateTime.MinValue;
    bool     _initialized;

    public MobyGamesBrowser(IConfiguration cfg, int rateLimitMs)
    {
        _rateLimitMs = rateLimitMs;

        IConfigurationSection auth = cfg.GetSection("MobyGames:Auth");
        _email            = auth["Email"]             ?? string.Empty;
        _password         = auth["Password"]          ?? string.Empty;
        _cookieCachePath  = auth["CookieCachePath"]   ?? "state/mobygames-cookies.json";
        _browserCachePath = auth["BrowserCachePath"]  ?? "state/puppeteer-chromium";
        // Persistent profile so the cf_clearance cookie (and any other CF state) survives between
        // runs. Critical because Cloudflare's Turnstile cannot be solved automatically — once the
        // user passes it once (set Headless=false), the cleared profile keeps subsequent runs
        // working without manual intervention.
        _userDataDir      = auth["UserDataDir"]       ?? "state/puppeteer-profile";
        _headless         = !bool.TryParse(auth["Headless"], out bool h) || h;
    }

    public bool IsLoggedIn { get; private set; }

    public bool HasMobyPlus { get; private set; }

    /// <summary>
    ///     Returns true once Chromium has been launched, the cached cookies (if any) loaded,
    ///     and login verified. Throws if neither cached cookies nor credentials are usable.
    /// </summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if(_initialized)
            return;

        // Download Chromium on first launch (no-op when already cached).
        Directory.CreateDirectory(_browserCachePath);
        Directory.CreateDirectory(_userDataDir);

        // Chromium writes a SingletonLock symlink into the profile dir to prevent two browser
        // instances from sharing the same profile. If the previous run crashed (or was killed),
        // this lock is left behind and the next launch aborts with
        // "Failed to create .../SingletonLock: File exists". Since we are the only owner of this
        // profile dir, it is safe to delete the stale lock files before launching.
        foreach(string staleLock in new[] { "SingletonLock", "SingletonCookie", "SingletonSocket" })
        {
            string p = Path.Combine(_userDataDir, staleLock);

            if(File.Exists(p) || Directory.Exists(p))
            {
                try { File.Delete(p); }
                catch { /* ignore — Chromium will surface a clearer error if it really matters */ }
            }
        }

        var fetcher = new BrowserFetcher(new BrowserFetcherOptions
        {
            Path = _browserCachePath
        });

        Console.WriteLine("  Ensuring headless Chromium is available...");
        InstalledBrowser installed = await fetcher.DownloadAsync();

        string chromiumExe = installed.GetExecutablePath();
        Console.WriteLine($"  Chromium {installed.BuildId} ready at {chromiumExe}");

        // Redirect XDG base-dir env vars into the state/ tree so Chromium's PathService
        // resolves its config / data / cache (and therefore its crashpad database) inside our
        // controlled location instead of $HOME/.config, which may not exist on a fresh box.
        // When XDG_CONFIG_HOME is unset/missing, chrome::DIR_CRASH_DUMPS ends up empty and
        // chrome_crashpad_handler is spawned with `--database=` → "--database is required".
        string stateRoot = Path.GetFullPath(Path.Combine(_userDataDir, ".."));
        string xdgConfig = Path.Combine(stateRoot, "xdg-config");
        string xdgData   = Path.Combine(stateRoot, "xdg-data");
        string xdgCache  = Path.Combine(stateRoot, "xdg-cache");
        Directory.CreateDirectory(xdgConfig);
        Directory.CreateDirectory(xdgData);
        Directory.CreateDirectory(xdgCache);

        try
        {
            LaunchOptions opts = new()
            {
                Headless       = _headless,
                ExecutablePath = chromiumExe,
                UserDataDir    = _userDataDir,
                DefaultViewport = new ViewPortOptions
                {
                    Width  = 1366,
                    Height = 768
                },
                Args = new[]
                {
                    "--disable-blink-features=AutomationControlled",
                    "--no-sandbox",
                    // Suppress some additional automation fingerprints that Cloudflare looks for.
                    "--disable-features=IsolateOrigins,site-per-process,AutomationControlled",
                    "--disable-infobars",
                    $"--lang={Environment.GetEnvironmentVariable("LANG")?.Split('.')[0]?.Replace('_', '-') ?? "en-US"}"
                }
            };

            opts.Env["XDG_CONFIG_HOME"] = xdgConfig;
            opts.Env["XDG_DATA_HOME"]   = xdgData;
            opts.Env["XDG_CACHE_HOME"]  = xdgCache;

            _browser = await Puppeteer.LaunchAsync(opts);
        }
        catch(Exception launchEx)
        {
            // PuppeteerSharp wraps anything that happens before the CDP handshake in a generic
            // ProcessException with no inner. To diagnose the real cause, probe the binary
            // directly: ldd reveals missing shared libs, and a `--version` invocation surfaces
            // crashes/SIGSEGV from glibc, seccomp, or namespace restrictions.
            await ProbeChromiumAsync(chromiumExe);

            throw new InvalidOperationException(
                $"Chromium launch failed at {chromiumExe}. See ldd/--version diagnostics above.",
                launchEx);
        }

        _page = await _browser.NewPageAsync();
        await _page.SetUserAgentAsync(DefaultUserAgent, null);

        // Restore cached cookies if present.
        bool cookiesRestored = await TryRestoreCookiesAsync();

        // Quick probe — visit the homepage and see whether we land in a logged-in state.
        await _page.GoToAsync("https://www.mobygames.com/", new NavigationOptions
        {
            Timeout   = 60_000,
            WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded }
        });

        await WaitForCloudflareAsync();

        await UpdateLoginStateAsync();

        if(!IsLoggedIn)
        {
            if(string.IsNullOrWhiteSpace(_email) || string.IsNullOrWhiteSpace(_password))
            {
                if(cookiesRestored)
                    throw new InvalidOperationException(
                        "Cached MobyGames cookies are no longer valid and no credentials are configured. " +
                        "Set MobyGames:Auth:Email and MobyGames:Auth:Password in appsettings.json.");

                throw new InvalidOperationException(
                    "MobyGames login required. Set MobyGames:Auth:Email and MobyGames:Auth:Password " +
                    "in appsettings.json (or in user-secrets).");
            }

            // Mark initialized BEFORE LoginAsync/SaveCookiesAsync so the cookie-export path
            // (which defensively calls EnsureInitializedAsync) doesn't re-enter InitializeAsync
            // and try to launch a second Chromium against the same UserDataDir (which would
            // collide with the SingletonLock of the already-running browser).
            _initialized = true;

            await LoginAsync(ct);
            await SaveCookiesAsync();
        }
        else
        {
            Console.WriteLine($"  Logged in to MobyGames{(HasMobyPlus ? " (MobyPlus active)" : "")}");
            _initialized = true;
        }
    }

    /// <summary>
    ///     Performs the form-based login flow on /user/login/. Throws if the post-submit page
    ///     does not show the logout link.
    /// </summary>
    public async Task LoginAsync(CancellationToken ct = default)
    {
        Console.WriteLine("  Logging in to MobyGames...");

        await _page.GoToAsync("https://www.mobygames.com/user/login/", new NavigationOptions
        {
            Timeout = 90_000,
            // Networkidle2 lets Cloudflare's challenge complete (the /user/login/ URL is gated by
            // a managed-challenge that flips the document twice before the real form appears).
            WaitUntil = new[] { WaitUntilNavigation.Networkidle2 }
        });

        await WaitForCloudflareAsync();

        // MobyGames' form field names have shifted over time; try each known variant and the
        // first one that matches wins. The current site exposes `login` for the username and
        // `password` for the secret, but older builds used `username`/`email`.
        string[] userSelectors =
        {
            "input[name='login']", "input[name='username']", "input[name='email']", "input[type='email']"
        };

        string usernameSelector = null;

        foreach(string sel in userSelectors)
        {
            try
            {
                await _page.WaitForSelectorAsync(sel,
                                                 new WaitForSelectorOptions { Visible = true, Timeout = 15_000 });

                usernameSelector = sel;

                break;
            }
            catch(WaitTaskTimeoutException) { /* try next */ }
        }

        if(usernameSelector is null)
        {
            await DumpLoginDiagnosticsAsync("login-form-not-found");

            throw new InvalidOperationException(
                "MobyGames login form not found — no known username-field selector matched. " +
                "Cloudflare may have served an interstitial; see state/mobygames-login-* diagnostics.");
        }

        await _page.TypeAsync(usernameSelector, _email);
        await _page.TypeAsync("input[name='password']", _password);

        // MobyGames uses htmx for form submission and injects a per-page CSRF token via an
        // `htmx:configRequest` listener that adds the `X-CSRF-Token` header. If we click submit
        // before htmx's script has registered the listener, the server returns
        // "No CSRF token submitted." Wait until htmx is loaded AND its listener is attached.
        try
        {
            await _page.WaitForFunctionAsync(
                "() => typeof window.htmx !== 'undefined'",
                new WaitForFunctionOptions { Timeout = 30_000 });
        }
        catch(WaitTaskTimeoutException)
        {
            // htmx not detected — the form may be a vanilla POST. Continue and let the click
            // either succeed (vanilla form) or fail with the same CSRF message (in which case
            // diagnostics get dumped below).
        }

        // MobyGames uses htmx to submit the form: the click fires an AJAX POST that swaps the
        // page body in place rather than navigating, so WaitForNavigationAsync can time out
        // even on a successful login. Instead we poll for the logged-in marker (a logout link)
        // and surface a clear failure if it never appears.
        await _page.ClickAsync("form button.btn.btn-primary", null);

        try
        {
            await _page.WaitForFunctionAsync(
                @"() => {
                    if (document.querySelector('a[href=""/user/logout/""]')) return true;
                    // Surface explicit server-rendered failures so we can fail fast.
                    const text = document.body ? document.body.innerText : '';
                    if (/no csrf token|invalid credentials|incorrect password|invalid username/i.test(text)) return true;
                    return false;
                }",
                new WaitForFunctionOptions { Timeout = 60_000 });
        }
        catch(WaitTaskTimeoutException)
        {
            await DumpLoginDiagnosticsAsync("login-timeout");

            throw new InvalidOperationException(
                "MobyGames login form submitted but the page did not update within 60s. " +
                "See state/mobygames-login-timeout.* diagnostics.");
        }

        await UpdateLoginStateAsync();

        if(!IsLoggedIn)
        {
            // Try to surface the on-page error message so the user sees the real reason.
            string errorText = await _page.EvaluateFunctionAsync<string>(
                @"() => {
                    const el = document.querySelector('.alert-danger, .error, .form-error, .invalid-feedback');
                    if (el && el.innerText) return el.innerText.trim();
                    const text = document.body ? document.body.innerText : '';
                    const m = text.match(/no csrf token[^.\n]*|invalid credentials[^.\n]*|incorrect password[^.\n]*|invalid username[^.\n]*/i);
                    return m ? m[0] : '';
                }");

            await DumpLoginDiagnosticsAsync("login-failure");

            throw new InvalidOperationException(
                $"MobyGames login failed — no logout link found after submitting the form." +
                (string.IsNullOrEmpty(errorText) ? "" : $" Server message: {errorText}") +
                " See state/mobygames-login-failure* diagnostics.");
        }

        Console.WriteLine($"  Logged in to MobyGames{(HasMobyPlus ? " (MobyPlus active)" : "")}");

        if(!HasMobyPlus)
            Console.WriteLine(
                "\e[33m  Warning: account is not MobyPlus — original-resolution screenshots and promo " +
                "art will not be downloaded.\e[0m");
    }

    async Task DumpLoginDiagnosticsAsync(string tag)
    {
        string dir = Path.GetDirectoryName(_cookieCachePath) ?? "state";

        try
        {
            Directory.CreateDirectory(dir);
            string screenshotPath = Path.Combine(dir, $"mobygames-{tag}.png");
            string htmlPath       = Path.Combine(dir, $"mobygames-{tag}.html");

            await _page.ScreenshotAsync(screenshotPath, new ScreenshotOptions { FullPage = true });
            string html = await _page.GetContentAsync();
            await File.WriteAllTextAsync(htmlPath, html);

            Console.WriteLine(
                $"\e[33m  Saved login diagnostics: {screenshotPath} (page URL: {_page.Url})\e[0m");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[33m  Failed to capture login diagnostics: {ex.Message}\e[0m");
        }
    }

    /// <summary>
    ///     Detects whether the current page is a Cloudflare interstitial (Turnstile checkbox,
    ///     managed-challenge spinner, or "Attention Required" block page). Cloudflare's checks
    ///     pass each request through one of these gates before serving the real content.
    /// </summary>
    async Task<bool> IsCloudflareChallengePresentAsync()
    {
        try
        {
            return await _page.EvaluateFunctionAsync<bool>(
                @"() => {
                    const title = (document.title || '').toLowerCase();
                    if (/just a moment|attention required|cloudflare/.test(title)) return true;
                    if (document.querySelector('iframe[src*=""challenges.cloudflare.com""]')) return true;
                    if (document.querySelector('#challenge-running, #challenge-form, #challenge-stage')) return true;
                    // Plain text hint used by the 'Performing security verification' page.
                    const bodyText = document.body ? document.body.innerText : '';
                    if (/verify you are human|performing security verification/i.test(bodyText)) return true;
                    return false;
                }");
        }
        catch
        {
            // Page navigation in flight or context destroyed — treat as transient.
            return false;
        }
    }

    /// <summary>
    ///     If a Cloudflare interstitial is detected: in headful mode, wait up to 5 minutes for
    ///     the user to solve it manually (the cleared cf_clearance cookie is persisted to
    ///     <see cref="_userDataDir"/> so subsequent headless runs skip the challenge). In
    ///     headless mode, fail fast with bootstrap instructions — there is no automated way to
    ///     pass Turnstile's "Verify you are human" checkbox.
    /// </summary>
    async Task WaitForCloudflareAsync()
    {
        if(!await IsCloudflareChallengePresentAsync())
            return;

        if(_headless)
        {
            await DumpLoginDiagnosticsAsync("cloudflare-interstitial");

            throw new InvalidOperationException(
                "Cloudflare's \"Verify you are human\" challenge cannot be solved in headless mode.\n" +
                "\n" +
                "  One-time bootstrap:\n" +
                "    1. Set \"MobyGames:Auth:Headless\": false in Marechai.MobyGames/appsettings.json\n" +
                "    2. Re-run the same command — a Chromium window will open.\n" +
                "    3. Click the \"Verify you are human\" checkbox; if the login form appears,\n" +
                "       it will auto-fill and submit from the credentials in appsettings.json.\n" +
                "    4. Once the homepage loads logged-in, the program continues automatically.\n" +
                "    5. After it finishes you may revert Headless back to true. The cf_clearance\n" +
                "       cookie stored in state/puppeteer-profile typically lasts ~30 days; until\n" +
                "       it expires, headless runs reuse it without hitting the challenge again.");
        }

        Console.WriteLine(
            "\e[33m  Cloudflare interstitial detected. Click the \"Verify you are human\" checkbox in the\n" +
            "  browser window. Waiting up to 5 minutes for the challenge to clear...\e[0m");

        DateTime deadline = DateTime.UtcNow.AddMinutes(5);

        while(DateTime.UtcNow < deadline)
        {
            await Task.Delay(2000);

            if(!await IsCloudflareChallengePresentAsync())
            {
                Console.WriteLine("  Cloudflare challenge cleared.");

                // Give the post-challenge redirect a moment to settle before the caller proceeds.
                await Task.Delay(1500);

                return;
            }
        }

        await DumpLoginDiagnosticsAsync("cloudflare-timeout");

        throw new InvalidOperationException(
            "Cloudflare challenge did not clear within 5 minutes. See state/mobygames-cloudflare-timeout.* " +
            "diagnostics.");
    }

    /// <summary>
    ///     Returns all cookies currently held by the embedded browser, suitable for copying
    ///     into <see cref="System.Net.CookieContainer"/>.
    /// </summary>
    public async Task<IReadOnlyList<CookieParam>> ExportCookiesAsync()
    {
        await EnsureInitializedAsync(CancellationToken.None);

        // Pull cookies for both apex and www subdomain so HttpClient sees all of them.
        CookieParam[] cookies = await _page.GetCookiesAsync(
            "https://www.mobygames.com/",
            "https://mobygames.com/");

        return cookies;
    }

    /// <summary>
    ///     Persist the current cookie set to <see cref="_cookieCachePath"/> as JSON.
    /// </summary>
    public async Task SaveCookiesAsync()
    {
        IReadOnlyList<CookieParam> cookies = await ExportCookiesAsync();
        Directory.CreateDirectory(Path.GetDirectoryName(_cookieCachePath) ?? ".");
        await File.WriteAllTextAsync(_cookieCachePath, JsonSerializer.Serialize(cookies, s_jsonOptions));
    }

    /// <summary>
    ///     Convenience wrapper used by the download CLIs: when <c>MobyGames:Auth</c> is configured,
    ///     spin up a transient browser session, export cookies into <paramref name="http"/>, then
    ///     dispose Chromium so the rest of the command stays on the fast HTTP path. Returns
    ///     <c>false</c> when auth is not configured (anonymous mode); the caller decides whether
    ///     that's fatal.
    /// </summary>
    public static async Task<bool> TryAttachCookiesAsync(IConfiguration            cfg,
                                                         MobyGamesHttpClient       http,
                                                         int                       rateLimitMs,
                                                         CancellationToken         ct = default)
    {
        IConfigurationSection auth = cfg.GetSection("MobyGames:Auth");

        if(string.IsNullOrWhiteSpace(auth["Email"]) || string.IsNullOrWhiteSpace(auth["Password"]))
        {
            // Cached cookies may still be present from a previous run — try them.
            string cookiePath = auth["CookieCachePath"] ?? "state/mobygames-cookies.json";

            if(File.Exists(cookiePath))
            {
                try
                {
                    string        json = await File.ReadAllTextAsync(cookiePath, ct);
                    CookieParam[] cookies = JsonSerializer.Deserialize<CookieParam[]>(json);

                    if(cookies is { Length: > 0 })
                    {
                        http.ImportCookies(cookies);
                        Console.WriteLine("  Loaded cached MobyGames cookies (no credentials configured).");

                        return true;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"\e[33m  Warning: failed to read cached cookies: {ex.Message}\e[0m");
                }
            }

            Console.WriteLine(
                "\e[33m  MobyGames:Auth not configured — proceeding anonymous (no MobyPlus, page caps may apply).\e[0m");

            return false;
        }

        await using var browser = new MobyGamesBrowser(cfg, rateLimitMs);

        try
        {
            await browser.InitializeAsync(ct);
            IReadOnlyList<CookieParam> cookies = await browser.ExportCookiesAsync();
            http.ImportCookies(cookies);

            Console.WriteLine(
                $"  Authenticated to MobyGames (MobyPlus={(browser.HasMobyPlus ? "yes" : "no")}).");

            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[31m  MobyGames authentication failed: {ex.GetType().FullName}: {ex.Message}\e[0m");

            // PuppeteerSharp's "Failed to launch browser!" exception is a generic ProcessException
            // wrapper. The actual reason (missing shared library, sandbox denial, profile lock,
            // Chromium stderr, etc.) is in the inner exception chain. Unwrap and surface everything.
            Exception inner = ex.InnerException;

            while(inner != null)
            {
                Console.WriteLine($"\e[31m    caused by {inner.GetType().FullName}: {inner.Message}\e[0m");
                inner = inner.InnerException;
            }

            if(ex.StackTrace != null) Console.WriteLine($"\e[90m{ex.StackTrace}\e[0m");

            Console.WriteLine("\e[33m  Continuing anonymous — downloads may be incomplete or rate-limited.\e[0m");

            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if(_initialized)
                await SaveCookiesAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[33m  Warning: failed to persist cookies: {ex.Message}\e[0m");
        }

        if(_browser is { IsClosed: false })
            await _browser.CloseAsync();

        _browser?.Dispose();
    }

    /// <summary>
    ///     Runs <c>ldd</c> and <c>&lt;chrome&gt; --version --no-sandbox</c> against the downloaded
    ///     Chromium binary so launch failures surface a concrete cause (missing shared libs,
    ///     SIGSEGV, glibc mismatch) instead of PuppeteerSharp's opaque ProcessException wrapper.
    /// </summary>
    static async Task ProbeChromiumAsync(string chromiumExe)
    {
        Console.WriteLine("\e[33m  Probing Chromium binary directly to diagnose launch failure...\e[0m");

        await RunDiagnosticAsync("ldd",      chromiumExe,  highlightMissing: true);
        await RunDiagnosticAsync(chromiumExe, "--version --no-sandbox", highlightMissing: false);
    }

    static async Task RunDiagnosticAsync(string fileName, string arguments, bool highlightMissing)
    {
        try
        {
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName               = fileName,
                    Arguments              = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false
                }
            };

            proc.Start();
            string stdout = await proc.StandardOutput.ReadToEndAsync();
            string stderr = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            Console.WriteLine($"\e[90m    $ {fileName} {arguments}\e[0m");
            Console.WriteLine($"\e[90m    exit={proc.ExitCode}\e[0m");

            foreach(string line in stdout.Split('\n'))
            {
                if(string.IsNullOrWhiteSpace(line)) continue;

                bool red = highlightMissing && line.Contains("not found", StringComparison.Ordinal);
                Console.WriteLine($"\e[{(red ? "31" : "90")}m      {line.TrimEnd()}\e[0m");
            }

            foreach(string line in stderr.Split('\n'))
                if(!string.IsNullOrWhiteSpace(line))
                    Console.WriteLine($"\e[31m      stderr: {line.TrimEnd()}\e[0m");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[31m    probe ({fileName}) threw {ex.GetType().Name}: {ex.Message}\e[0m");
        }
    }

    async Task<bool> TryRestoreCookiesAsync()
    {
        if(!File.Exists(_cookieCachePath))
            return false;

        try
        {
            string json = await File.ReadAllTextAsync(_cookieCachePath);

            CookieParam[] cookies = JsonSerializer.Deserialize<CookieParam[]>(json);

            if(cookies is null || cookies.Length == 0)
                return false;

            // Project the saved CookieParam list into CookieData entries (the browser-level
            // SetCookieAsync API in PuppeteerSharp 24 expects CookieData).
            CookieData[] data = cookies.Select(c => new CookieData
            {
                Name         = c.Name,
                Value        = c.Value,
                Domain       = c.Domain,
                Path         = c.Path,
                Expires      = c.Expires,
                HttpOnly     = c.HttpOnly,
                Secure       = c.Secure,
                SameSite     = c.SameSite,
                Priority     = c.Priority,
                SourceScheme = c.SourceScheme
            }).ToArray();

            await _browser.SetCookieAsync(data);

            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[33m  Warning: failed to restore cached cookies: {ex.Message}\e[0m");

            return false;
        }
    }

    async Task UpdateLoginStateAsync()
    {
        IElementHandle logoutLink = await _page.QuerySelectorAsync("a[href='/user/logout/']");
        IsLoggedIn = logoutLink is not null;

        if(!IsLoggedIn)
        {
            HasMobyPlus = false;

            return;
        }

        IElementHandle mobyPlusMarker = await _page.QuerySelectorAsync("[data-has-mobyplus='true']");
        HasMobyPlus = mobyPlusMarker is not null;
    }

    async Task EnsureInitializedAsync(CancellationToken ct)
    {
        if(!_initialized)
            await InitializeAsync(ct);
    }

    async Task ApplyRateLimitAsync(CancellationToken ct)
    {
        if(_rateLimitMs <= 0)
            return;

        TimeSpan since = DateTime.UtcNow - _lastRequestUtc;

        if(since.TotalMilliseconds < _rateLimitMs)
            await Task.Delay(_rateLimitMs - (int)since.TotalMilliseconds, ct);

        _lastRequestUtc = DateTime.UtcNow;
    }
}
