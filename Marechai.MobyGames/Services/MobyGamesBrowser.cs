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
    readonly string _proxy;
    readonly string _proxyUser;
    readonly string _proxyPassword;

    IBrowser _browser;
    IPage    _page;
    DateTime _lastRequestUtc = DateTime.MinValue;
    bool     _initialized;

    /// <param name="headlessOverride">
    ///     When set, wins over <c>MobyGames:Auth:Headless</c>. The <c>cf-login</c> command passes
    ///     <c>false</c> so the operator can click the Turnstile checkbox regardless of appsettings.
    /// </param>
    /// <param name="proxy">
    ///     Optional proxy for Chromium, e.g. <c>socks5://127.0.0.1:1080</c> (an <c>ssh -D 1080 server</c>
    ///     tunnel) or <c>http://host:3128</c>. Cloudflare binds <c>cf_clearance</c> to the IP that
    ///     solved the challenge, so routing the visible browser through the headless server's IP is
    ///     the only way to mint a cookie the server can use.
    /// </param>
    /// <param name="proxyUser">Optional proxy username (applied via CDP authentication).</param>
    /// <param name="proxyPassword">Optional proxy password.</param>
    /// <param name="stateDir">
    ///     Optional directory that replaces the configured cookie file and Chromium profile
    ///     (<c>&lt;stateDir&gt;/mobygames-cookies.json</c>, <c>&lt;stateDir&gt;/puppeteer-profile</c>) so a
    ///     session minted for another IP does not overwrite this machine's own. The Chromium
    ///     binary cache is shared.
    /// </param>
    public MobyGamesBrowser(IConfiguration cfg, int rateLimitMs, bool? headlessOverride = null,
                            string proxy = null, string proxyUser = null, string proxyPassword = null,
                            string stateDir = null)
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
        _headless         = headlessOverride ?? (!bool.TryParse(auth["Headless"], out bool h) || h);
        _proxy            = string.IsNullOrWhiteSpace(proxy) ? null : proxy.Trim();
        _proxyUser        = proxyUser;
        _proxyPassword    = proxyPassword;

        if(!string.IsNullOrWhiteSpace(stateDir))
        {
            _cookieCachePath = Path.Combine(stateDir, "mobygames-cookies.json");
            _userDataDir     = Path.Combine(stateDir, "puppeteer-profile");
        }
    }

    /// <summary>Cookie cache file this instance writes (for operator instructions).</summary>
    public string CookieCachePath => _cookieCachePath;

    /// <summary>Chromium profile directory this instance uses (for operator instructions).</summary>
    public string UserDataDir => _userDataDir;

    public bool IsLoggedIn { get; private set; }

    public bool HasMobyPlus { get; private set; }

    /// <summary>
    ///     True when this session performed the form login on <c>/user/login/</c> with the
    ///     configured credentials (as opposed to being restored from cached cookies).
    /// </summary>
    public bool LoginPerformed { get; private set; }

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

        // Headful without a display (headless server with Headless=false): run Chromium inside a
        // private Xvfb screen, the way FlareSolverr does. Headless Chrome is fingerprinted by
        // Cloudflare and gets the interactive checkbox; a headful one usually gets the invisible
        // pass, and when it doesn't, the Turnstile widget can be clicked with real input events.
        string display = _headless ? null : await EnsureDisplayAsync();

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

            if(display is not null)
                opts.Env["DISPLAY"] = display;

            if(_proxy is not null)
            {
                // Route every request (including DNS for socks5h-style behaviour Chromium applies to
                // SOCKS proxies by default) through the proxy so Cloudflare sees the proxy's IP.
                opts.Args = [..opts.Args, $"--proxy-server={_proxy}"];
                Console.WriteLine($"  Chromium proxy: {_proxy}");
            }

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

        if(_proxy is not null && !string.IsNullOrEmpty(_proxyUser))
            await _page.AuthenticateAsync(new Credentials { Username = _proxyUser, Password = _proxyPassword ?? "" });

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
            LoginPerformed = true;
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

        try
        {
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
            // The page may already have moved on (htmx swap / redirect to the profile page) by the
            // time we get here, in which case the submit button no longer exists. Never fail on
            // that: if a logout link is visible we are done; otherwise try any submit control and
            // finally the Enter key in the password field.
            bool alreadyLoggedIn = await _page.QuerySelectorAsync("a[href='/user/logout/']") is not null;

            if(!alreadyLoggedIn)
            {
                IElementHandle submit = await _page.QuerySelectorAsync("form button.btn.btn-primary")
                                        ?? await _page.QuerySelectorAsync("form button[type='submit']")
                                        ?? await _page.QuerySelectorAsync("form input[type='submit']");

                if(submit is not null)
                    await submit.ClickAsync();
                else
                {
                    Console.WriteLine("\e[33m  Submit button not found — pressing Enter in the password field.\e[0m");
                    await _page.FocusAsync("input[name='password']");
                    await _page.Keyboard.PressAsync("Enter");
                }
            }

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
        }
        catch(Exception ex) when(IsTargetClosed(ex))
        {
            // Observed in headful mode behind a proxy: the POST lands and the session cookie is
            // written to the profile, but the tab is replaced/closed (Target.detachedFromTarget)
            // before the logout link can be observed. The login usually DID succeed — re-acquire a
            // tab, land on the homepage and let the login-state probe below decide.
            Console.WriteLine("\e[33m  Login tab was closed while waiting for the response — re-checking the session...\e[0m");

            IPage page = await GetLivePageAsync();

            await page.GoToAsync("https://www.mobygames.com/", new NavigationOptions
            {
                Timeout   = 60_000,
                WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded }
            });

            await WaitForCloudflareAsync();
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
            IPage page = await GetLivePageAsync();

            return await page.EvaluateFunctionAsync<bool>(
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
            // Cloudflare's managed challenge is often NON-interactive: the page runs its JavaScript
            // for a few seconds and redirects without any checkbox. Give a real (logged-in)
            // Chromium profile that chance before concluding a human is needed.
            Console.WriteLine("  Cloudflare interstitial detected (headless) — waiting up to 45s for a non-interactive pass...");

            DateTime autoDeadline = DateTime.UtcNow.AddSeconds(45);

            while(DateTime.UtcNow < autoDeadline)
            {
                await Task.Delay(3000);

                if(!await IsCloudflareChallengePresentAsync())
                {
                    Console.WriteLine("  Cloudflare challenge cleared without interaction.");
                    await Task.Delay(1500);

                    return;
                }
            }

            await DumpLoginDiagnosticsAsync("cloudflare-interstitial");

            throw new InvalidOperationException(
                "Cloudflare's \"Verify you are human\" challenge cannot be solved in headless mode.\n" +
                "\n" +
                "  Fix: set \"MobyGames:Auth:Headless\": false in Marechai.MobyGames/appsettings.json and\n" +
                "  re-run. Chromium then runs headful — in a window when a DISPLAY exists, otherwise inside\n" +
                "  an automatically started Xvfb (install the xvfb package on servers) — and the Turnstile\n" +
                "  checkbox is ticked automatically with real input events; only if that fails does it wait\n" +
                "  for a human click. Detailed manual flow:\n" +
                "    1. Set Headless to false.\n" +
                "    2. Re-run the same command — a Chromium window (or Xvfb screen) opens.\n" +
                "    3. If the automatic solve fails, click the \"Verify you are human\" checkbox; the login\n" +
                "       form (if shown) is filled from the credentials in appsettings.json.\n" +
                "    4. Once the homepage loads logged-in, the program continues automatically.\n" +
                "    5. After it finishes you may revert Headless back to true. Headless runs reuse\n" +
                "       the cf_clearance stored in state/puppeteer-profile until Cloudflare re-challenges\n" +
                "       (bound to this IP + user agent; lifetime is set by MobyGames, not by us).\n" +
                "  Alternatively mint the session elsewhere with `cf-login --proxy` and copy state/ over.");
        }

        // Headful: first give the managed challenge a few seconds to pass by itself, then try to
        // tick the Turnstile checkbox with real input events (mouse click on the widget, then
        // Tab + Space), and only then ask a human — who, on a server with Xvfb, does not exist.
        if(await TryAutoSolveTurnstileAsync())
            return;

        // On a virtual display (Xvfb) there is nobody at the seat: waiting for a click would only
        // burn five minutes. Fail now so the caller's backoff / retry logic takes over.
        if(_virtualDisplay)
        {
            await DumpLoginDiagnosticsAsync("cloudflare-interstitial");

            throw new InvalidOperationException(
                "Cloudflare's \"Verify you are human\" challenge could not be passed automatically on the " +
                "virtual display (no human can click it there). See state/mobygames-cloudflare-interstitial.* " +
                "diagnostics; the request will be retried after the challenge backoff.");
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

        // Pull cookies for both apex and www subdomain so HttpClient sees all of them. Use a
        // live page: the original tab may have been swapped away by Cloudflare/htmx.
        IPage page = await GetLivePageAsync();

        CookieParam[] cookies = await page.GetCookiesAsync(
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

    /// <summary>True when <c>MobyGames:Auth:Headless</c> is explicitly <c>false</c> (an operator can click).</summary>
    public static bool IsAttendedMode(IConfiguration cfg) =>
        bool.TryParse(cfg.GetSection("MobyGames:Auth")["Headless"], out bool headless) && !headless;

    /// <summary>
    ///     Attended-mode challenge resolver used by <see cref="MobyGamesHttpClient" /> mid-run: opens
    ///     a VISIBLE Chromium on the shared profile, navigates to the challenged page, waits for the
    ///     operator to click "Verify you are human" (up to 5 minutes), then waits until the profile
    ///     holds a <c>cf_clearance</c> different from the one the HTTP client already has, imports
    ///     the fresh cookies into <paramref name="http" />, persists them and closes the window.
    ///     Returns <c>true</c> when a new clearance was obtained.
    /// </summary>
    public static async Task<bool> SolveChallengeInteractivelyAsync(IConfiguration      cfg,
                                                                    MobyGamesHttpClient http,
                                                                    string              challengedUrl,
                                                                    int                 rateLimitMs,
                                                                    CancellationToken   ct = default)
    {
        string oldClearance = http.CurrentClearance;

        await using var browser = new MobyGamesBrowser(cfg, rateLimitMs, headlessOverride: false);

        // InitializeAsync opens the homepage, waits (headful) for the challenge to be clicked away
        // and re-establishes the login; the profile's cf_clearance is replaced in the process.
        await browser.InitializeAsync(ct);

        // The challenged URL may sit behind its own challenge (Cloudflare scores per path too):
        // visit it so any second checkbox is presented while the window is still open.
        if(!string.IsNullOrWhiteSpace(challengedUrl) && challengedUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            IPage page = await browser.GetLivePageAsync();

            IResponse nav = await page.GoToAsync(challengedUrl, new NavigationOptions
            {
                Timeout   = 60_000,
                WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded }
            });

            // The browser is trusted where the HTTP client is not: if it simply gets an error page
            // here, the URL is dead and no amount of clearance will change the HTTP client's answer.
            if(nav is not null && (int)nav.Status >= 400 && !await browser.IsCloudflareChallengePresentAsync())
            {
                http.NoteUrlUnavailable((int)nav.Status);

                return false;
            }

            await browser.WaitForCloudflareAsync();
        }

        // Wait for a NEW clearance to land in the profile (the cookie is rewritten shortly after
        // the redirect that follows the click).
        IReadOnlyList<CookieParam> cookies = null;
        DateTime deadline = DateTime.UtcNow.AddSeconds(60);

        while(DateTime.UtcNow < deadline)
        {
            cookies = await browser.ExportCookiesAsync();
            string fresh = cookies.FirstOrDefault(c => c.Name == "cf_clearance")?.Value;

            if(!string.IsNullOrEmpty(fresh) && fresh != oldClearance) break;

            await Task.Delay(2000, ct);
        }

        string newClearance = cookies?.FirstOrDefault(c => c.Name == "cf_clearance")?.Value;

        if(string.IsNullOrEmpty(newClearance))
        {
            Console.WriteLine("\e[31m  No cf_clearance cookie in the browser profile after the challenge.\e[0m");

            return false;
        }

        http.ImportCookies(cookies);
        await browser.SaveCookiesAsync();

        Console.WriteLine(newClearance == oldClearance
                              ? "\e[33m  Challenge cleared but cf_clearance did not change — retrying with the existing cookie.\e[0m"
                              : "  New cf_clearance obtained and imported; resuming.");

        return true;
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

        if(_xvfb is { HasExited: false })
        {
            try { _xvfb.Kill(); _xvfb.WaitForExit(3000); } catch { /* best effort */ }
        }

        _xvfb?.Dispose();

        // A killed Xvfb leaves its socket and lock behind, which would make the next run pick a
        // new display number every time.
        if(_xvfbDisplayNumber is int n)
        {
            foreach(string leftover in new[] { $"/tmp/.X11-unix/X{n}", $"/tmp/.X{n}-lock" })
            {
                try { if(File.Exists(leftover)) File.Delete(leftover); } catch { /* best effort */ }
            }
        }
    }

    int? _xvfbDisplayNumber;

    Process _xvfb;

    /// <summary>True when Chromium runs on an Xvfb screen we started, i.e. no human can see or click it.</summary>
    bool _virtualDisplay;

    /// <summary>
    ///     Returns the X display Chromium should use in headful mode. When <c>DISPLAY</c> is set,
    ///     that one; otherwise starts a private <c>Xvfb</c> (must be installed: package
    ///     <c>xorg-server-xvfb</c> / <c>xvfb</c>) and returns its display. Returns <c>null</c>
    ///     when no display can be provided, in which case Chromium falls back to headless.
    /// </summary>
    async Task<string> EnsureDisplayAsync()
    {
        string existing = Environment.GetEnvironmentVariable("DISPLAY");

        if(!string.IsNullOrWhiteSpace(existing))
            return existing;

        string xvfbExe = new[] { "/usr/bin/Xvfb", "/usr/local/bin/Xvfb", "/usr/X11R6/bin/Xvfb" }.FirstOrDefault(File.Exists);

        if(xvfbExe is null)
        {
            Console.WriteLine("\e[33m  No DISPLAY and Xvfb is not installed — running Chromium headless instead " +
                              "(install xvfb to let it pass Cloudflare like a real browser).\e[0m");

            return null;
        }

        // Pick a display number that is not already in use.
        int number = 90;

        while(File.Exists($"/tmp/.X11-unix/X{number}") && number < 200) number++;

        string display = $":{number}";

        try
        {
            _xvfb = Process.Start(new ProcessStartInfo(xvfbExe,
                                                       $"{display} -screen 0 1366x768x24 -nolisten tcp -ac")
            {
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true
            });

            _xvfb.BeginOutputReadLine();
            _xvfb.BeginErrorReadLine();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[33m  Xvfb failed to start ({ex.Message}) — running Chromium headless instead.\e[0m");

            return null;
        }

        // Wait for the socket to appear.
        for(int i = 0; i < 50 && !File.Exists($"/tmp/.X11-unix/X{number}"); i++)
            await Task.Delay(100);

        if(_xvfb.HasExited || !File.Exists($"/tmp/.X11-unix/X{number}"))
        {
            Console.WriteLine("\e[33m  Xvfb did not come up — running Chromium headless instead.\e[0m");

            return null;
        }

        Console.WriteLine($"  Started Xvfb on {display} for headful Chromium (virtual display: no human can click).");
        _virtualDisplay     = true;
        _xvfbDisplayNumber  = number;

        return display;
    }

    /// <summary>
    ///     Attempts to clear a Cloudflare interstitial without a human: waits a few seconds for the
    ///     non-interactive managed challenge to pass, then, if a Turnstile widget is present, sends
    ///     REAL input events the way FlareSolverr does — a mouse click on the checkbox area of the
    ///     widget's iframe, then Tab + Space — and checks after each attempt. Returns <c>true</c>
    ///     when the challenge is gone.
    /// </summary>
    async Task<bool> TryAutoSolveTurnstileAsync()
    {
        Console.WriteLine("  Cloudflare interstitial detected — trying to pass it automatically...");

        // 1. Non-interactive pass.
        for(int i = 0; i < 5; i++)
        {
            await Task.Delay(2000);

            if(!await IsCloudflareChallengePresentAsync())
            {
                Console.WriteLine("  Cloudflare challenge cleared without interaction.");
                await Task.Delay(1500);

                return true;
            }
        }

        IPage page = await GetLivePageAsync();

        // 2. Click the checkbox: the Turnstile widget is an iframe from challenges.cloudflare.com;
        //    the checkbox sits at the left edge of it, vertically centred.
        for(int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                IElementHandle frame = await page.QuerySelectorAsync("iframe[src*='challenges.cloudflare.com']")
                                       ?? await page.QuerySelectorAsync("#turnstile-wrapper iframe")
                                       ?? await page.QuerySelectorAsync("#challenge-stage iframe")
                                       ?? await page.QuerySelectorAsync("iframe[title*='Widget' i]");

                BoundingBox box = frame is null ? null : await frame.BoundingBoxAsync();

                if(box is not null && box.Width > 0)
                {
                    decimal x = box.X + Math.Min(30, box.Width / 2);
                    decimal y = box.Y + box.Height / 2;

                    Console.WriteLine($"  Turnstile widget found at ({box.X:0},{box.Y:0}) {box.Width:0}x{box.Height:0} — clicking the checkbox (attempt {attempt + 1}/3)...");
                    await page.Mouse.MoveAsync(x - 40, y + 15);
                    await Task.Delay(300);
                    await page.Mouse.MoveAsync(x, y, new PuppeteerSharp.Input.MoveOptions { Steps = 12 });
                    await Task.Delay(200);
                    await page.Mouse.ClickAsync(x, y);
                }
                else
                {
                    // 3. Keyboard fallback: focus the document and tab into the widget, then Space.
                    Console.WriteLine($"  Turnstile iframe not found — trying Tab + Space (attempt {attempt + 1}/3)...");
                    await page.Keyboard.PressAsync("Tab");
                    await Task.Delay(300);
                    await page.Keyboard.PressAsync("Space");
                }
            }
            catch(Exception ex) when(IsTargetClosed(ex))
            {
                // The click worked and the page navigated away under us.
                page = await GetLivePageAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\e[33m  Auto-solve attempt failed: {ex.Message}\e[0m");
            }

            for(int i = 0; i < 5; i++)
            {
                await Task.Delay(2000);

                if(!await IsCloudflareChallengePresentAsync())
                {
                    Console.WriteLine("  Cloudflare challenge cleared by the automatic click.");
                    await Task.Delay(1500);

                    return true;
                }
            }
        }

        Console.WriteLine("\e[33m  Automatic Turnstile solve did not work.\e[0m");

        return false;
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

    /// <summary>True when the exception (or anything in its chain) means the CDP target/session went away.</summary>
    static bool IsTargetClosed(Exception ex)
    {
        for(Exception e = ex; e != null; e = e.InnerException)
        {
            if(e is TargetClosedException) return true;

            if(e.Message.Contains("Target closed", StringComparison.OrdinalIgnoreCase) ||
               e.Message.Contains("Session closed", StringComparison.OrdinalIgnoreCase) ||
               e.Message.Contains("detachedFromTarget", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Returns a usable page. Cloudflare's managed challenge (and MobyGames' htmx login) can
    ///     replace the tab's CDP target mid-flow (<c>Target.detachedFromTarget</c>), which makes every
    ///     call on the old <see cref="IPage" /> throw "Target closed". Instead of failing, look for the
    ///     replacement tab in the browser; if none is left (window closed), open a fresh one on the
    ///     homepage — the profile already holds whatever cookies were written before the swap.
    /// </summary>
    async Task<IPage> GetLivePageAsync()
    {
        if(_page is { IsClosed: false })
        {
            try
            {
                await _page.EvaluateExpressionAsync<string>("document.readyState");

                return _page;
            }
            catch(Exception ex) when(IsTargetClosed(ex)) { /* fall through and re-acquire */ }
        }

        IPage[] pages = await _browser.PagesAsync();

        IPage candidate = pages.LastOrDefault(p => !p.IsClosed &&
                                                    p.Url.Contains("mobygames.com", StringComparison.OrdinalIgnoreCase))
                          ?? pages.LastOrDefault(p => !p.IsClosed && p.Url != "about:blank");

        if(candidate is null)
        {
            Console.WriteLine("\e[33m  Browser tab is gone — opening a new one on the MobyGames homepage...\e[0m");

            candidate = await _browser.NewPageAsync();
            await candidate.SetUserAgentAsync(DefaultUserAgent, null);

            if(_proxy is not null && !string.IsNullOrEmpty(_proxyUser))
                await candidate.AuthenticateAsync(new Credentials { Username = _proxyUser, Password = _proxyPassword ?? "" });

            await candidate.GoToAsync("https://www.mobygames.com/", new NavigationOptions
            {
                Timeout   = 60_000,
                WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded }
            });
        }
        else if(!ReferenceEquals(candidate, _page))
            Console.WriteLine("\e[33m  Browser tab was replaced — continuing on the new tab.\e[0m");

        _page = candidate;

        return _page;
    }

    async Task UpdateLoginStateAsync()
    {
        IPage page = await GetLivePageAsync();

        IElementHandle logoutLink = await page.QuerySelectorAsync("a[href='/user/logout/']");
        IsLoggedIn = logoutLink is not null;

        if(!IsLoggedIn)
        {
            HasMobyPlus = false;

            return;
        }

        IElementHandle mobyPlusMarker = await page.QuerySelectorAsync("[data-has-mobyplus='true']");
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
