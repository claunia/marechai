using System;

namespace Marechai.Pages.Admin;

public sealed class CompanyDialogResult
{
    public string    Name                  { get; set; } = null!;
    public string?   LegalName             { get; set; }
    public int       Status                { get; set; }
    public DateTime? Founded               { get; set; }
    public int       FoundedPrecision      { get; set; }
    public DateTime? Sold                  { get; set; }
    public int       SoldPrecision         { get; set; }
    public int?      SoldToId              { get; set; }
    public int?      CountryId             { get; set; }
    public string?   Address               { get; set; }
    public string?   City                  { get; set; }
    public string?   Province              { get; set; }
    public string?   PostalCode            { get; set; }
    public string?   Website               { get; set; }
    public string?   Twitter               { get; set; }
    public string?   Facebook              { get; set; }
}
