/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using Marechai.ApiClient.Models;

namespace Marechai.Pages.Admin;

public enum CountryMatchType
{
    Exact,
    Partial,
    None
}

public sealed class PersonImportRow
{
    public string  Name        { get; set; } = "";
    public string Surname     { get; set; }
    public string Alias       { get; set; }
    public string DisplayName { get; set; }
    public string CountryInput { get; set; }
    public int?    BirthYear   { get; set; }
    public int?    BirthMonth  { get; set; }
    public int?    BirthDay    { get; set; }
    public int?    DeathYear   { get; set; }
    public int?    DeathMonth  { get; set; }
    public int?    DeathDay    { get; set; }
    public string Webpage     { get; set; }
    public string Twitter     { get; set; }
    public string Facebook    { get; set; }

    public List<Iso31661NumericDto>    MatchedCountries { get; set; } = [];
    public Iso31661NumericDto         SelectedCountry  { get; set; }
    public CountryMatchType            CountryMatch     { get; set; } = CountryMatchType.None;
    public bool                        IsDuplicate      { get; set; }
    public string                     ImportError      { get; set; }
    public string                     ValidationError  { get; set; }

    public DateTime? BirthDate
    {
        get
        {
            if(BirthYear is null)
                return null;

            int month = BirthMonth ?? 1;
            int day   = BirthDay   ?? 1;

            try
            {
                return new DateTime(BirthYear.Value, month, day);
            }
            catch
            {
                return null;
            }
        }
    }

    public int BirthDatePrecision
    {
        get
        {
            if(BirthYear is null)
                return 0;

            if(BirthMonth is null)
                return 2;

            if(BirthDay is null)
                return 1;

            return 0;
        }
    }

    public DateTime? DeathDate
    {
        get
        {
            if(DeathYear is null)
                return null;

            int month = DeathMonth ?? 1;
            int day   = DeathDay   ?? 1;

            try
            {
                return new DateTime(DeathYear.Value, month, day);
            }
            catch
            {
                return null;
            }
        }
    }

    public int DeathDatePrecision
    {
        get
        {
            if(DeathYear is null)
                return 0;

            if(DeathMonth is null)
                return 2;

            if(DeathDay is null)
                return 1;

            return 0;
        }
    }

    public string CountryDisplay
    {
        get
        {
            if(CountryMatch is CountryMatchType.Exact or CountryMatchType.Partial && SelectedCountry is not null)
                return SelectedCountry.Name ?? CountryInput ?? "";

            return CountryInput ?? "";
        }
    }
}
