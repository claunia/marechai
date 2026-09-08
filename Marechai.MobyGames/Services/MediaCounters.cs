namespace Marechai.MobyGames.Services;

/// <summary>
///     Mutable per-run counters shared between a media service's batch loop (<c>RunAsync</c>)
///     and its per-game entry point (<c>ProcessGameAsync</c>), so the <c>update-year</c>
///     orchestrator can drive one game at a time and still get the same tallies.
/// </summary>
public sealed class MediaCounters
{
    /// <summary>Items (covers / screenshots / promo images / videos) seen on the page(s).</summary>
    public int Total { get; set; }

    /// <summary>Items newly downloaded / imported this run.</summary>
    public int Added { get; set; }

    /// <summary>Items skipped because they already existed.</summary>
    public int Skipped { get; set; }

    /// <summary>Items that failed to download / import.</summary>
    public int Failed { get; set; }

    /// <summary>Games with no scraped page for this media type.</summary>
    public int NoPage { get; set; }

    /// <summary>Games whose page was present but yielded no parseable groups.</summary>
    public int ParseFailed { get; set; }

    /// <summary>Dry-run only: items that would be downloaded.</summary>
    public int WouldAdd { get; set; }

    public void Reset()
    {
        Total       = 0;
        Added       = 0;
        Skipped     = 0;
        Failed      = 0;
        NoPage      = 0;
        ParseFailed = 0;
        WouldAdd    = 0;
    }
}
