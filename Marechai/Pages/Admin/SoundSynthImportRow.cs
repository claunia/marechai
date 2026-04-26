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

public sealed class SoundSynthImportRow
{
    public string  Name        { get; set; } = "";
    public string? CompanyInput { get; set; }
    public string? ModelCode   { get; set; }
    public int?    IntroducedYear  { get; set; }
    public int?    IntroducedMonth { get; set; }
    public int?    IntroducedDay   { get; set; }
    public int?    Voices     { get; set; }
    public double? Frequency  { get; set; }
    public int?    Depth      { get; set; }
    public int?    SquareWave { get; set; }
    public int?    WhiteNoise { get; set; }
    public int?    Type       { get; set; }

    public List<CompanyDto>    MatchedCompanies { get; set; } = [];
    public CompanyDto?         SelectedCompany  { get; set; }
    public CompanyMatchType    MatchType        { get; set; } = CompanyMatchType.None;
    public bool                IsDuplicate      { get; set; }
    public string?             ImportError      { get; set; }
    public string?             ValidationError  { get; set; }

    public DateTime? IntroducedDate
    {
        get
        {
            if(IntroducedYear is null)
                return null;

            int month = IntroducedMonth ?? 1;
            int day   = IntroducedDay   ?? 1;

            try
            {
                return new DateTime(IntroducedYear.Value, month, day);
            }
            catch
            {
                return null;
            }
        }
    }

    public int IntroducedPrecision
    {
        get
        {
            if(IntroducedYear is null)
                return 0;

            if(IntroducedMonth is null)
                return 2;

            if(IntroducedDay is null)
                return 1;

            return 0;
        }
    }

    public string CompanyDisplay
    {
        get
        {
            if(MatchType is CompanyMatchType.Exact or CompanyMatchType.Partial && SelectedCompany is not null)
                return SelectedCompany.Name ?? CompanyInput ?? "";

            return CompanyInput ?? "";
        }
    }
}
