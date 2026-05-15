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

using System.Linq;
using Marechai.Database.Models;

namespace Marechai.Database.Seeders;

public static class SoftwareRoles
{
    public static void Seed(MarechaiContext context)
    {
        SoftwareRole[] roles =
        [
            new() { Id = "dev", Name = "Developer",   Enabled = true },
            new() { Id = "pub", Name = "Publisher",    Enabled = true },
            new() { Id = "dis", Name = "Distributor",  Enabled = true },
            new() { Id = "por", Name = "Porter",       Enabled = true },
            new() { Id = "loc", Name = "Localizer",    Enabled = true },
            new() { Id = "mfg", Name = "Manufacturer", Enabled = true },
            new() { Id = "lic", Name = "Licensor",             Enabled = true },
            new() { Id = "gfx", Name = "Additional Graphics", Enabled = true },
            new() { Id = "cpy", Name = "Copy Protection",     Enabled = true },
            new() { Id = "cut", Name = "Cutscenes",            Enabled = true },
            new() { Id = "moc", Name = "Motion Capture",       Enabled = true },
            new() { Id = "snd", Name = "Additional Sound",     Enabled = true },
            new() { Id = "pkg", Name = "Package Design",       Enabled = true },
            new() { Id = "vrc", Name = "Voice Recording",      Enabled = true },
            new() { Id = "eng", Name = "Game Engine",          Enabled = true },
            new() { Id = "mdw", Name = "Middleware",            Enabled = true },
            new() { Id = "tst", Name = "Testing",               Enabled = true },
            new() { Id = "ctb", Name = "Contributions",          Enabled = true },
            new() { Id = "fnt", Name = "Fonts",                  Enabled = true },
            new() { Id = "prd", Name = "Producer",               Enabled = true },
            new() { Id = "doc", Name = "Documentation",             Enabled = true },
            new() { Id = "ocp", Name = "Original Concept",          Enabled = true },
            new() { Id = "fnd", Name = "Funder",                    Enabled = true },
            new() { Id = "cst", Name = "Casting",                   Enabled = true },
            new() { Id = "mkt", Name = "Marketing",                 Enabled = true },
            new() { Id = "des", Name = "Design",                    Enabled = true },
            new() { Id = "wri", Name = "Writing",                   Enabled = true }
        ];

        foreach(SoftwareRole role in roles)
        {
            SoftwareRole existing = context.SoftwareRoles.FirstOrDefault(r => r.Id == role.Id);

            if(existing is null)
            {
                context.SoftwareRoles.Add(role);

                continue;
            }

            if(existing.Name    == role.Name &&
               existing.Enabled == role.Enabled)
                continue;

            existing.Name    = role.Name;
            existing.Enabled = role.Enabled;
        }

        context.SaveChanges();
    }
}
