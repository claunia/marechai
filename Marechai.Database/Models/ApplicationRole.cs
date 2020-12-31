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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System;
using Microsoft.AspNetCore.Identity;

namespace Marechai.Database.Models
{
    public class ApplicationRole : IdentityRole
    {
        public const string ROLE_UBERADMIN       = "UberAdmin";
        public const string ROLE_WRITER          = "Writer";
        public const string ROLE_PROOFREADER     = "Proofreader";
        public const string ROLE_TRANSLATOR      = "Translator";
        public const string ROLE_SUPERTRANSLATOR = "SuperTranslator";
        public const string ROLE_COLLABORATOR    = "Collaborator";
        public const string ROLE_CURATOR         = "Curator";
        public const string ROLE_PHYSICALCURATOR = "PhysicalCurator";
        public const string ROLE_TECHNICIAN      = "Technician";
        public const string ROLE_SUPERTECHNICIAN = "SuperTechnician";
        public const string ROLE_ADMIN           = "Administrator";
        public const string ROLE_NONE            = "NormalUser";

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
}