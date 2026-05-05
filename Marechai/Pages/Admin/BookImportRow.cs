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

public sealed class BookImportRow
{
    public string  Title        { get; set; } = "";
    public string NativeTitle  { get; set; }
    public string SortTitle    { get; set; }
    public string Isbn         { get; set; }
    public int?    Edition      { get; set; }
    public int?    Pages        { get; set; }
    public int?    PublishedYear  { get; set; }
    public int?    PublishedMonth { get; set; }
    public int?    PublishedDay   { get; set; }
    public string CountryInput   { get; set; }

    public List<Iso31661NumericDto>    MatchedCountries { get; set; } = [];
    public Iso31661NumericDto         SelectedCountry  { get; set; }
    public CountryMatchType            CountryMatch     { get; set; } = CountryMatchType.None;
    public bool                        IsDuplicate      { get; set; }
    public string                     ImportError      { get; set; }
    public string                     ValidationError  { get; set; }

    public DateTime? PublishedDate
    {
        get
        {
            if(PublishedYear is null)
                return null;

            int month = PublishedMonth ?? 1;
            int day   = PublishedDay   ?? 1;

            try
            {
                return new DateTime(PublishedYear.Value, month, day);
            }
            catch
            {
                return null;
            }
        }
    }

    public int PublishedPrecision
    {
        get
        {
            if(PublishedYear is null)
                return 0;

            if(PublishedMonth is null)
                return 2;

            if(PublishedDay is null)
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
