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
    Trivia,
    AdBlurb,
    BuyTrade,
    Unknown
}

public static class TabDetector
{
    public static MobyTab Detect(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Try active tab detection from <li class="active"> or <li class="disabled"> with matching text
        var activeLi = doc.DocumentNode.SelectSingleNode("//ul[contains(@class,'nav-tabs')]//li[contains(@class,'active')]");

        if(activeLi != null)
        {
            string tabText = activeLi.InnerText.Trim();

            return tabText switch
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
                "Trivia"         => MobyTab.Trivia,
                "Ad Blurb"       => MobyTab.AdBlurb,
                "Buy/Trade"      => MobyTab.BuyTrade,
                _                => MobyTab.Unknown
            };
        }

        // Fallback: detect from h1 title suffix
        var h1 = doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'niceHeaderTitle')]");

        if(h1 != null)
        {
            string text = h1.GetDirectInnerText().Trim();

            if(text.EndsWith("Credits"))        return MobyTab.Credits;
            if(text.EndsWith("Covers"))         return MobyTab.CoverArt;
            if(text.EndsWith("Releases"))       return MobyTab.Releases;
            if(text.EndsWith("Specs"))          return MobyTab.Specs;
            if(text.EndsWith("Rating Systems")) return MobyTab.RatingSystems;
            if(text.EndsWith("Reviews"))        return MobyTab.Reviews;
            if(text.EndsWith("Screenshots"))    return MobyTab.Screenshots;
        }

        // If no active tab and no suffix, it's likely the Main tab
        // (Main tab has no suffix — just the game name)
        var descH2 = doc.DocumentNode.SelectSingleNode("//h2[text()='Description']");

        if(descH2 != null) return MobyTab.Main;

        return MobyTab.Unknown;
    }
}
