﻿/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : ConsoleCompany.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Videogame console company model
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
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;

namespace cicm_web.Models
{
    public class ConsoleCompany
    {
        public int    Id;
        public string Name;

        public static ConsoleCompany GetItem(int id)
        {
            Cicm.Database.Schemas.ConsoleCompany dbItem = Program.Database?.Operations.GetConsoleCompany(id);
            return dbItem == null ? null : new ConsoleCompany {Name = dbItem.Name, Id = dbItem.Id};
        }

        public static ConsoleCompany[] GetAllItems()
        {
            List<Cicm.Database.Schemas.ConsoleCompany> dbItems = null;
            bool? result =
                Program.Database?.Operations.GetConsoleCompanies(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<ConsoleCompany> items = new List<ConsoleCompany>();

            foreach(Cicm.Database.Schemas.ConsoleCompany dbItem in dbItems)
                items.Add(new ConsoleCompany {Id = dbItem.Id, Name = dbItem.Name});

            return items.ToArray();
        }
    }
}