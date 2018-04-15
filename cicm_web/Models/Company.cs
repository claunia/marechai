/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Company.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Company model
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace cicm_web.Models
{
    public class Company
    {
        public int    Id;
        public string Name;

        public static Company GetItem(int id)
        {
            Cicm.Database.Schemas.Company dbItem = Program.Database?.Operations.GetCompany(id);
            return dbItem == null ? null : new Company {Name = dbItem.Name, Id = dbItem.Id};
        }

        public static Company[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Company> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetCompanies(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            return dbItems.Select(t => new Company {Id = t.Id, Name = t.Name}).OrderBy(t => t.Name).ToArray();
        }

        public static Company[] GetItemsStartingWithLetter(char letter)
        {
            List<Cicm.Database.Schemas.Company> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetCompanies(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            return dbItems
                  .Where(t => t.Name.StartsWith(new string(letter, 1), StringComparison.InvariantCultureIgnoreCase))
                  .Select(t => new Company {Id = t.Id, Name = t.Name}).OrderBy(t => t.Name).ToArray();
        }
    }
}