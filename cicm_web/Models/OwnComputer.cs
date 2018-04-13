/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : OwnComputer.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Videogame console model
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
using System.Globalization;
using System.Linq;
using Cicm.Database.Schemas;

namespace cicm_web.Models
{
    public class OwnComputer
    {
        public DateTime   Acquired;
        public bool       Boxed;
        public int        Cap1;
        public int        Cap2;
        public Computer   Computer;
        public Cpu        Cpu1;
        public Cpu        Cpu2;
        public DiskFormat Disk1;
        public DiskFormat Disk2;
        public int        Id;
        public bool       Manuals;
        public float      Mhz1;
        public float      Mhz2;
        public int        Ram;
        public string     Rigid;
        public StatusType Status;
        public bool       Trade;
        public int        Vram;

        public static OwnComputer[] GetAllItems()
        {
            List<Cicm.Database.Schemas.OwnComputer> dbItems = null;
            bool?                                   result  = Program.Database?.Operations.GetOwnComputers(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            return dbItems.OrderByDescending(i => i.Id).Select(TransformItem) as OwnComputer[];
        }

        public static OwnComputer GetItem(int id)
        {
            Cicm.Database.Schemas.OwnComputer dbItem = Program.Database?.Operations.GetOwnComputer(id);
            return dbItem == null ? null : TransformItem(dbItem);
        }

        static OwnComputer TransformItem(Cicm.Database.Schemas.OwnComputer dbItem)
        {
            Computer computer = Computer.GetItem(dbItem.ComputerId);

            OwnComputer item = new OwnComputer
            {
                Acquired = DateTime.ParseExact(dbItem.Acquired, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture),
                Boxed    = dbItem.Boxed,
                Computer = computer,
                Id       = dbItem.Id,
                Manuals  = dbItem.Manuals,
                Ram      = dbItem.Ram,
                Rigid    = dbItem.Rigid,
                Status   = dbItem.Status,
                Trade    = dbItem.Trade,
                Vram     = dbItem.Vram
            };

            if(dbItem.Disk1 > 0)
            {
                item.Cap1  = dbItem.Cap1;
                item.Disk1 = DiskFormat.GetItem(dbItem.Disk1);
            }

            if(dbItem.Disk2 > 0)
            {
                item.Cap2  = dbItem.Cap2;
                item.Disk2 = DiskFormat.GetItem(dbItem.Disk2);
            }

            if(dbItem.Cpu1 > 0)
            {
                item.Cpu1 = Cpu.GetItem(dbItem.Cpu1);
                item.Mhz1 = dbItem.Mhz1;
            }

            if(dbItem.Cpu2 > 0)
            {
                item.Cpu2 = Cpu.GetItem(dbItem.Cpu2);
                item.Mhz2 = dbItem.Mhz2;
            }

            return computer == null ? null : item;
        }
    }
}