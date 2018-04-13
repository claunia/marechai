/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Cpu.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Central Processing Unit model
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
    public class Cpu
    {
        public int    Id;
        public string Name;

        public static Cpu GetItem(int id)
        {
            Cicm.Database.Schemas.Cpu dbItem = Program.Database?.Operations.GetCpu(id);
            return dbItem == null ? null : new Cpu {Name = dbItem.Name, Id = dbItem.Id};
        }

        public static Cpu[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Cpu> dbItems = null;
            bool?                           result  = Program.Database?.Operations.GetCpus(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<Cpu> items = new List<Cpu>();

            foreach(Cicm.Database.Schemas.Cpu dbItem in dbItems)
                items.Add(new Cpu {Id = dbItem.Id, Name = dbItem.Name});

            return items.ToArray();
        }
    }
}