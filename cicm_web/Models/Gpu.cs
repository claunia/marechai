/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Gpu.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Graphics Processing Unit model
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
    public class Gpu
    {
        public int    Id;
        public string Name;

        public static Gpu GetItem(int id)
        {
            Cicm.Database.Schemas.Gpu dbItem = Program.Database?.Operations.GetGpu(id);
            return dbItem == null ? null : new Gpu {Name = dbItem.Name, Id = dbItem.Id};
        }

        public static Gpu[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Gpu> dbItems = null;
            bool?                           result  = Program.Database?.Operations.GetGpus(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<Gpu> items = new List<Gpu>();

            foreach(Cicm.Database.Schemas.Gpu dbItem in dbItems)
                items.Add(new Gpu {Id = dbItem.Id, Name = dbItem.Name});

            return items.ToArray();
        }
    }
}