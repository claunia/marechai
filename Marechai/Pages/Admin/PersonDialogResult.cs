using System;

namespace Marechai.Pages.Admin;

public sealed class PersonDialogResult
{
    public string    Name        { get; set; } = null!;
    public string?   Surname     { get; set; }
    public string?   Alias       { get; set; }
    public string?   DisplayName { get; set; }
    public int?      CountryId   { get; set; }
    public DateTime? BirthDate          { get; set; }
    public int       BirthDatePrecision { get; set; }
    public DateTime? DeathDate          { get; set; }
    public int       DeathDatePrecision { get; set; }
    public string?   Webpage     { get; set; }
    public string?   Twitter     { get; set; }
    public string?   Facebook    { get; set; }
}
