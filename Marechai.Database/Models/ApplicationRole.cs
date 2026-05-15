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
using Microsoft.AspNetCore.Identity;

namespace Marechai.Database.Models;

public class ApplicationRole : IdentityRole
{
    public const string RoleUberAdmin       = "UberAdmin";
    public const string RoleWriter          = "Writer";
    public const string RoleProofreader     = "Proofreader";
    public const string RoleTranslator      = "Translator";
    public const string RoleSuperTranslator = "SuperTranslator";
    public const string RoleCollaborator    = "Collaborator";
    public const string RoleCurator         = "Curator";
    public const string RolePhysicalCurator = "PhysicalCurator";
    public const string RoleTechnician      = "Technician";
    public const string RoleSuperTechnician = "SuperTechnician";
    public const string RoleAdmin           = "Admin";
    public const string RoleNone            = "NormalUser";

    public ApplicationRole() => Created = DateTime.UtcNow;

    public ApplicationRole(string name) : base(name)
    {
        Description = name;
        Created     = DateTime.UtcNow;
    }

    public ApplicationRole(string name, string description) : base(name)
    {
        Description = description;
        Created     = DateTime.UtcNow;
    }

    public string   Description { get; set; }
    public DateTime Created     { get; set; }
}