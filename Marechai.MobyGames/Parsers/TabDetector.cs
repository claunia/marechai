using HtmlAgilityPack;

namespace Marechai.MobyGames.Parsers;

public enum MobyTab
{
    Main,
    Credits,
    CoverArt,
    Releases,
    Specs,
    RatingSystems,
    Reviews,
    Screenshots,
    PromoArt,
    Media,
    Trivia,
    AdBlurb,
    BuyTrade,
    Unknown
}

/// <summary>
///     Identifies the MobyGames page layout the HTML uses. The site was
///     redesigned around 2023 from a static, multi-tab Bootstrap layout
///     (<see cref="Old" />) to a Vue-based, single-section layout
///     (<see cref="New" />). The two layouts use different CSS classes,
///     URL patterns and section ids, so each requires its own parser set.
/// </summary>
public enum MobyLayout
{
    Old,
    New,
    Unknown
}

public static class TabDetector
{
    /// <summary>
    ///     Backwards-compatible wrapper that only returns the tab type.
    ///     New callers should prefer <see cref="DetectWithLayout" />.
    /// </summary>
    public static MobyTab Detect(string html) => DetectWithLayout(html).Tab;

    /// <summary>
    ///     Detects both the tab type and the layout (Old vs New). The layout
    ///     dictates which parser to use; the tab dictates what to extract.
    /// </summary>
    public static (MobyTab Tab, MobyLayout Layout) DetectWithLayout(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // ---- NEW layout markers FIRST -------------------------------------
        // The redesigned site always emits <h1 class="mb-0">{game name}</h1>
        // on every game sub-page. The legacy site uses
        // <h1 class="niceHeaderTitle">. The new layout *also* renders a
        // <ul class="nav-tabs mb"> across the top, so we cannot use the
        // presence of nav-tabs as an Old marker — h1.mb-0 must win first.
        var h1New = doc.DocumentNode.SelectSingleNode("//h1[contains(concat(' ',@class,' '),' mb-0 ')]");

        if(h1New != null)
        {
            // Per-sub-page identification keys off og:title. Examples from a 2023 game:
            //   Main:        "Wo Long: Fallen Dynasty (2023) - MobyGames"
            //   Credits:     "Wo Long: Fallen Dynasty credits (PlayStation 5, 2023) - MobyGames"
            //   Releases:    "Wo Long: Fallen Dynasty Releases - MobyGames"
            //   Specs+Rate:  "Wo Long: Fallen Dynasty Attributes, Tech Specs, Ratings - MobyGames"
            //   Screenshots: "Wo Long: Fallen Dynasty screenshots - MobyGames"
            //   Promo art:   "Wo Long: Fallen Dynasty promo art, ads, magazines advertisements - MobyGames"
            //   Covers:      "Wo Long: Fallen Dynasty box cover art - MobyGames"
            //   Videos:      "Wo Long: Fallen Dynasty (2023) media - MobyGames"
            //   Reviews:     "{name} reviews - MobyGames"
            //   Trivia:      "{name} trivia - MobyGames"
            var ogTitle = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
            string title = ogTitle?.GetAttributeValue("content", "") ?? string.Empty;

            // Trim the trailing " - MobyGames" so we only inspect the relevant part.
            const string suffix = " - MobyGames";

            if(title.EndsWith(suffix)) title = title[..^suffix.Length];

            string lower = title.ToLowerInvariant();

            // Order matters: multi-word, more-specific patterns first so e.g.
            // "promo art" doesn't get eaten by the "art" fragment of "cover art".
            if(lower.Contains("tech specs") || lower.Contains("attributes") || lower.EndsWith(" ratings"))
                return (MobyTab.Specs, MobyLayout.New);

            if(lower.Contains("promo art"))                              return (MobyTab.PromoArt,    MobyLayout.New);
            if(lower.Contains("cover art") || lower.EndsWith(" covers")) return (MobyTab.CoverArt,    MobyLayout.New);
            if(lower.Contains(" credits"))                               return (MobyTab.Credits,     MobyLayout.New);
            if(lower.EndsWith(" releases"))                              return (MobyTab.Releases,    MobyLayout.New);
            if(lower.Contains(" screenshots"))                           return (MobyTab.Screenshots, MobyLayout.New);
            if(lower.Contains(" videos") || lower.EndsWith(" media"))    return (MobyTab.Media,       MobyLayout.New);
            if(lower.Contains(" reviews"))                               return (MobyTab.Reviews,     MobyLayout.New);
            if(lower.Contains(" trivia"))                                return (MobyTab.Trivia,      MobyLayout.New);

            // Bare title with "(YYYY)" suffix (or no descriptive tail at all) is the main page.
            return (MobyTab.Main, MobyLayout.New);
        }

        // ---- OLD layout markers --------------------------------------------
        // Legacy nav with an explicit <li class="active"> on the active tab.
        var navTabs = doc.DocumentNode.SelectSingleNode("//ul[contains(@class,'nav-tabs')]");

        if(navTabs != null)
        {
            var activeLi = doc.DocumentNode.SelectSingleNode("//ul[contains(@class,'nav-tabs')]//li[contains(@class,'active')]");

            if(activeLi != null)
            {
                string tabText = activeLi.InnerText.Trim();

                MobyTab oldTab = tabText switch
                {
                    "Main"           => MobyTab.Main,
                    "Credits"        => MobyTab.Credits,
                    "Cover Art"      => MobyTab.CoverArt,
                    "Releases"       => MobyTab.Releases,
                    "Specs"          => MobyTab.Specs,
                    "Rating Systems" => MobyTab.RatingSystems,
                    "Reviews"        => MobyTab.Reviews,
                    "Screenshots"    => MobyTab.Screenshots,
                    "Promo Art"      => MobyTab.PromoArt,
                    "Media"          => MobyTab.Media,
                    "Trivia"         => MobyTab.Trivia,
                    "Ad Blurb"       => MobyTab.AdBlurb,
                    "Buy/Trade"      => MobyTab.BuyTrade,
                    _                => MobyTab.Unknown
                };

                if(oldTab != MobyTab.Unknown) return (oldTab, MobyLayout.Old);
            }

            // Fallback: detect from h1 title suffix
            var h1Old = doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'niceHeaderTitle')]");

            if(h1Old != null)
            {
                string text = h1Old.GetDirectInnerText().Trim();

                if(text.EndsWith("Credits"))        return (MobyTab.Credits,       MobyLayout.Old);
                if(text.EndsWith("Covers"))         return (MobyTab.CoverArt,      MobyLayout.Old);
                if(text.EndsWith("Releases"))       return (MobyTab.Releases,      MobyLayout.Old);
                if(text.EndsWith("Specs"))          return (MobyTab.Specs,         MobyLayout.Old);
                if(text.EndsWith("Rating Systems")) return (MobyTab.RatingSystems, MobyLayout.Old);
                if(text.EndsWith("Reviews"))        return (MobyTab.Reviews,       MobyLayout.Old);
                if(text.EndsWith("Screenshots"))    return (MobyTab.Screenshots,   MobyLayout.Old);
            }

            var descH2 = doc.DocumentNode.SelectSingleNode("//h2[text()='Description']");

            if(descH2 != null) return (MobyTab.Main, MobyLayout.Old);

            return (MobyTab.Unknown, MobyLayout.Old);
        }

        return (MobyTab.Unknown, MobyLayout.Unknown);
    }
}
