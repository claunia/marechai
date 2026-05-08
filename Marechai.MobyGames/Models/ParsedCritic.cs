using System;
using Marechai.Data;

namespace Marechai.MobyGames.Models;

public class ParsedCritic
{
    public string        CountryName               { get; set; }
    public DateTime?     FirstPublication          { get; set; }
    public DatePrecision FirstPublicationPrecision { get; set; } = DatePrecision.YearOnly;
}
