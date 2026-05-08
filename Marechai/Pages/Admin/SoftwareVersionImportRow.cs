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

using System.Collections.Generic;
using Marechai.ApiClient.Models;

namespace Marechai.Pages.Admin;

public enum ParentVersionMatchType
{
    None,
    ExistingExact,
    SameBatch,
    NotFound
}

public enum LicenseMatchType
{
    None,
    Exact,
    Partial,
    NotFound
}

public sealed class SoftwareVersionImportRow
{
    public string VersionStringInput { get; set; }
    public string PublicVersionInput { get; set; }
    public string CodenameInput      { get; set; }
    public string ParentVersionInput { get; set; }
    public string LicenseInput       { get; set; }

    public List<SoftwareVersionDto> MatchedExistingParents { get; set; } = [];
    public SoftwareVersionDto       SelectedExistingParent { get; set; }
    public SoftwareVersionImportRow BatchParentRow         { get; set; }
    public ParentVersionMatchType   ParentVersionMatch     { get; set; } = ParentVersionMatchType.None;

    public List<LicenseDto> MatchedLicenses  { get; set; } = [];
    public LicenseDto       SelectedLicense  { get; set; }
    public LicenseMatchType LicenseMatch     { get; set; } = LicenseMatchType.None;

    public bool   IsDuplicate     { get; set; }
    public string ImportError     { get; set; }
    public string ValidationError { get; set; }

    /// <summary>Set after a successful import so dependent batch rows can resolve their parent id.</summary>
    public int? CreatedId { get; set; }

    public string ParentVersionDisplay
    {
        get
        {
            if(ParentVersionMatch == ParentVersionMatchType.ExistingExact && SelectedExistingParent is not null)
            {
                string display = SelectedExistingParent.VersionString ?? "";

                if(!string.IsNullOrWhiteSpace(SelectedExistingParent.PublicVersion))
                    display += $" ({SelectedExistingParent.PublicVersion})";

                return display;
            }

            return ParentVersionInput ?? "";
        }
    }

    public string LicenseDisplay
    {
        get
        {
            if(LicenseMatch is LicenseMatchType.Exact or LicenseMatchType.Partial && SelectedLicense is not null)
                return SelectedLicense.Name ?? LicenseInput ?? "";

            return LicenseInput ?? "";
        }
    }
}
