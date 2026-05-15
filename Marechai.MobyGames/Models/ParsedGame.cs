using System;
using System.Collections.Generic;

namespace Marechai.MobyGames.Models;

public class ParsedGame
{
    public string MobyGameId { get; set; }
    public string Name       { get; set; }

    // From Main tab
    public List<string>          Publishers  { get; set; } = [];
    public List<string>          Developers  { get; set; } = [];
    public string                ReleaseDate { get; set; }
    public List<string>          Platforms   { get; set; } = [];
    public List<ParsedGenre>     Genres      { get; set; } = [];
    public int?                  BaseGameMobyId { get; set; }
    public string                Description     { get; set; }
    public string                DescriptionHtml { get; set; }
    public List<string>          Groups          { get; set; } = [];

    /// <summary>
    ///     Slugs of games contained in this compilation, extracted from description links.
    ///     Only populated when genre is "Compilation".
    /// </summary>
    public List<string>          CompilationGameSlugs { get; set; } = [];

    /// <summary>
    ///     Anchors in this compilation's description that do not point to a MobyGames
    ///     game entry (typically /search/quick?game= placeholders or external links).
    ///     Captured with the original href so the importer can include them in the
    ///     admin report when the compilation is imported partially.
    /// </summary>
    public List<UnresolvableCompilationLink> UnresolvableCompilationGames { get; set; } = [];

    // From Credits tab
    public List<ParsedCredit> Credits { get; set; } = [];

    // From Releases tab
    public List<ParsedRelease> Releases { get; set; } = [];

    // From Specs tab
    public List<ParsedSpec> Specs { get; set; } = [];

    // From Rating Systems tab
    public List<ParsedRating> Ratings { get; set; } = [];

    // Tab availability flags
    public bool HasMainTab     { get; set; }
    public bool HasCreditsTab  { get; set; }
    public bool HasReleasesTab { get; set; }
    public bool HasSpecsTab    { get; set; }
    public bool HasRatingsTab  { get; set; }

    /// <summary>
    ///     Media availability flags, derived from sub-page anchors on the Main page.
    ///     Used by the downstream scrapers (cover, screenshot, promo, media, review)
    ///     to decide whether to fetch the respective sub-page for new-site games.
    /// </summary>
    public bool HasScreenshots { get; set; }
    public bool HasPromoArt    { get; set; }
    public bool HasCoverArt    { get; set; }
    public bool HasMedia       { get; set; }
    public bool HasReviews     { get; set; }

    /// <summary>
    ///     True when the Main row was parsed from the new MobyGames layout
    ///     (Vue-based, post-2023). False for the legacy layout the original
    ///     importer was written for. Used by media scrapers to choose the
    ///     correct sub-page URL form (slug-based vs numeric-id-based).
    /// </summary>
    public bool IsNewLayout { get; set; }

    /// <summary>
    ///     Numeric MobyGames id parsed from new-site main page sub-page anchors
    ///     (e.g. <c>/game/200023/wo-long-fallen-dynasty/</c>). Required by media
    ///     scrapers to build the numeric-id-based sub-page URLs used by the new
    ///     site. Null when the row came from the legacy layout.
    /// </summary>
    public int? NumericId { get; set; }

    /// <summary>
    ///     Slug parsed from new-site main page sub-page anchors. Used together
    ///     with <see cref="NumericId" /> to build new-site sub-page URLs.
    /// </summary>
    public string Slug { get; set; }
}

public class ParsedGenre
{
    public string Type { get; set; } // Genre, Perspective, Gameplay, Setting
    public string Name { get; set; }
}

public class ParsedCredit
{
    public string Role       { get; set; }
    public string PersonName { get; set; }
}

public class ParsedRelease
{
    public string              Platform    { get; set; }
    public string              Publisher   { get; set; }
    public string              Developer   { get; set; }
    public string              Distributor { get; set; }
    public string              Localizer   { get; set; }
    public Dictionary<string, string> CompanyRoles { get; set; } = new();
    public List<string>        Countries   { get; set; } = [];
    public string              ReleaseDate { get; set; }
    public string              Comments    { get; set; }
    public List<ParsedBarcode> Barcodes    { get; set; } = [];
    public List<ParsedProductCode> ProductCodes { get; set; } = [];
}

public class ParsedBarcode
{
    public string Type { get; set; } // UPC-A, EAN-13
    public string Code { get; set; }
}

public class ParsedProductCode
{
    public string Type { get; set; } // Sony PN, PSN/SEN Code
    public string Code { get; set; }
}

public class ParsedSpec
{
    public string Platform { get; set; }
    public string Key      { get; set; }
    public string Value    { get; set; }
}

/// <summary>
///     A description anchor that the importer could not resolve to a MobyGames
///     game entry. <see cref="Name" /> is the visible link text and
///     <see cref="Href" /> is the absolute URL the link pointed at (typically a
///     <c>/search/quick?game=</c> URL on mobygames.com but may be any non-game href).
/// </summary>
public class UnresolvableCompilationLink
{
    public string Name { get; set; }
    public string Href { get; set; }
}

public class ParsedRating
{
    public string Platform    { get; set; }
    public string System      { get; set; } // ESRB, PEGI, etc.
    public string Rating      { get; set; }
    public string Descriptors { get; set; }
}
