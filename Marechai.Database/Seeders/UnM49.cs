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
using System.Linq;
using Marechai.Data;
using Marechai.Database.Models;

namespace Marechai.Database.Seeders;

public static class UnM49Seeder
{
    /// <summary>
    ///     Seeds the UN M.49 geographic regions (non-country entries).
    ///     Countries are seeded via migration SQL from Iso31661Numeric.
    /// </summary>
    public static void Seed(MarechaiContext context)
    {
        // World
        EnsureRegion(context, 1,   "World",                          null, UnM49Type.World);

        // Continents
        EnsureRegion(context, 2,   "Africa",                         1,    UnM49Type.Continent);
        EnsureRegion(context, 19,  "Americas",                       1,    UnM49Type.Continent);
        EnsureRegion(context, 142, "Asia",                           1,    UnM49Type.Continent);
        EnsureRegion(context, 150, "Europe",                         1,    UnM49Type.Continent);
        EnsureRegion(context, 9,   "Oceania",                        1,    UnM49Type.Continent);

        // Africa sub-regions
        EnsureRegion(context, 15,  "Northern Africa",                2,    UnM49Type.SubRegion);
        EnsureRegion(context, 202, "Sub-Saharan Africa",             2,    UnM49Type.SubRegion);
        EnsureRegion(context, 14,  "Eastern Africa",                 202,  UnM49Type.SubRegion);
        EnsureRegion(context, 17,  "Middle Africa",                  202,  UnM49Type.SubRegion);
        EnsureRegion(context, 18,  "Southern Africa",                202,  UnM49Type.SubRegion);
        EnsureRegion(context, 11,  "Western Africa",                 202,  UnM49Type.SubRegion);

        // Americas sub-regions
        EnsureRegion(context, 419, "Latin America and the Caribbean", 19,  UnM49Type.SubRegion);
        EnsureRegion(context, 29,  "Caribbean",                      419, UnM49Type.SubRegion);
        EnsureRegion(context, 13,  "Central America",                419, UnM49Type.SubRegion);
        EnsureRegion(context, 5,   "South America",                  419, UnM49Type.SubRegion);
        EnsureRegion(context, 21,  "Northern America",               19,  UnM49Type.SubRegion);

        // Asia sub-regions
        EnsureRegion(context, 143, "Central Asia",                   142, UnM49Type.SubRegion);
        EnsureRegion(context, 30,  "Eastern Asia",                   142, UnM49Type.SubRegion);
        EnsureRegion(context, 35,  "South-eastern Asia",             142, UnM49Type.SubRegion);
        EnsureRegion(context, 34,  "Southern Asia",                  142, UnM49Type.SubRegion);
        EnsureRegion(context, 145, "Western Asia",                   142, UnM49Type.SubRegion);

        // Europe sub-regions
        EnsureRegion(context, 151, "Eastern Europe",                 150, UnM49Type.SubRegion);
        EnsureRegion(context, 154, "Northern Europe",                150, UnM49Type.SubRegion);
        EnsureRegion(context, 39,  "Southern Europe",                150, UnM49Type.SubRegion);
        EnsureRegion(context, 155, "Western Europe",                 150, UnM49Type.SubRegion);

        // Oceania sub-regions
        EnsureRegion(context, 53,  "Australia and New Zealand",      9,   UnM49Type.SubRegion);
        EnsureRegion(context, 54,  "Melanesia",                     9,   UnM49Type.SubRegion);
        EnsureRegion(context, 57,  "Micronesia",                    9,   UnM49Type.SubRegion);
        EnsureRegion(context, 61,  "Polynesia",                     9,   UnM49Type.SubRegion);

        // Antarctica (not in any region in M.49, placed directly under World)
        EnsureRegion(context, 10,  "Antarctica",                    1,   UnM49Type.SubRegion);
    }

    static void EnsureRegion(MarechaiContext context, short id, string name, short? parentId, UnM49Type type)
    {
        if(context.UnM49.Any(r => r.Id == id)) return;

        context.UnM49.Add(new Models.UnM49
        {
            Id       = id,
            Name     = name,
            ParentId = parentId,
            Type     = type
        });
    }
}
