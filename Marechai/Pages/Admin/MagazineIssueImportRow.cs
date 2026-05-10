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

namespace Marechai.Pages.Admin;

public sealed class MagazineIssueImportRow
{
    public string Caption        { get; set; }
    public string NativeCaption  { get; set; }
    public int?   IssueNumber    { get; set; }
    public int?   PublishedYear  { get; set; }
    public int?   PublishedMonth { get; set; }
    public int?   PublishedDay   { get; set; }
    public string ProductCode    { get; set; }
    public short? Pages          { get; set; }

    public bool   IsDuplicate     { get; set; }
    public string ValidationError { get; set; }
    public string ImportError     { get; set; }

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

    public string DisplayLabel
    {
        get
        {
            if(IssueNumber.HasValue)
                return string.IsNullOrWhiteSpace(Caption)
                           ? $"#{IssueNumber.Value}"
                           : $"#{IssueNumber.Value} {Caption}";

            return Caption ?? "";
        }
    }
}
