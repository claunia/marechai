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
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class SoftwareVersion : BaseModel<ulong>
{
    [Required]
    public ulong SoftwareId { get;          set; }
    public virtual Software Software { get; set; }
    public         string   Codename { get; set; }
    [Required]
    public string VersionString { get;                                         set; } // e.g. "4.00.950"
    public         string                               PublicVersion   { get; set; } // e.g. "Windows 95"
    public         ulong?                               ParentVersionId { get; set; }
    public         int?                                 LicenseId       { get; set; }
    public virtual SoftwareVersion                      ParentVersion   { get; set; }
    public virtual License                              License         { get; set; }
    public virtual ICollection<SoftwareVersion>         Children        { get; set; }
    public virtual ICollection<SoftwareRequirement>     Requirements    { get; set; }
    public virtual ICollection<SoftwareOSCompatibility>   OSCompatibility { get; set; }
    public virtual ICollection<SoftwareRelease>                  Releases            { get; set; }
    public virtual ICollection<CompanyBySoftwareVersion>           Companies           { get; set; }
    public virtual ICollection<SoftwareScreenshot>                 Screenshots         { get; set; }
    public virtual ICollection<SoftwareVersionBySoftwareCompilation> CompilationMemberships { get; set; }
}