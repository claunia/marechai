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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;
using Marechai.Database;

namespace Marechai.ViewModels
{
    public class CompanyViewModel : BaseViewModel<int>
    {
        public string        Name                  { get; set; }
        public DateTime?     Founded               { get; set; }
        public string        Website               { get; set; }
        public string        Twitter               { get; set; }
        public string        Facebook              { get; set; }
        public DateTime?     Sold                  { get; set; }
        public int?          SoldToId              { get; set; }
        public string        Address               { get; set; }
        public string        City                  { get; set; }
        public string        Province              { get; set; }
        public string        PostalCode            { get; set; }
        public short?        CountryId             { get; set; }
        public CompanyStatus Status                { get; set; }
        public Guid?         LastLogo              { get; set; }
        public string        SoldTo                { get; set; }
        public string        Country               { get; set; }
        public bool          FoundedDayIsUnknown   { get; set; }
        public bool          FoundedMonthIsUnknown { get; set; }
        public bool          SoldDayIsUnknown      { get; set; }
        public bool          SoldMonthIsUnknown    { get; set; }
        public string        LegalName             { get; set; }

        public string SoldView => Status != CompanyStatus.Active && Status != CompanyStatus.Unknown
                                      ? Sold?.ToShortDateString() ?? "Unknown"
                                      : Sold?.ToShortDateString() ?? (SoldTo is null ? "" : "Unknown");
    }
}