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
    public string                Description { get; set; }
    public List<string>          Groups      { get; set; } = [];

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

public class ParsedRating
{
    public string Platform    { get; set; }
    public string System      { get; set; } // ESRB, PEGI, etc.
    public string Rating      { get; set; }
    public string Descriptors { get; set; }
}
