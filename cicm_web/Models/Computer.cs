/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Computer.cs
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
using System.Linq;

namespace cicm_web.Models
{
    public class Computer
    {
        public int        Bits;
        public string     Cap1;
        public string     Cap2;
        public int        Colors;
        public string     Comment;
        public Company    Company;
        public Cpu        Cpu1;
        public Cpu        Cpu2;
        public DiskFormat Disk1;
        public DiskFormat Disk2;
        public Gpu        Gpu;
        public DiskFormat Hdd1;
        public DiskFormat Hdd2;
        public DiskFormat Hdd3;
        public int        Id;
        public float      Mhz1;
        public float      Mhz2;
        public string     Model;
        public Mpu        Mpu;
        public int        MusicChannels;
        public int        Ram;
        public string     Resolution;
        public int        Rom;
        public int        SoundChannels;
        public Dsp        Spu;
        public int        Vram;
        public int        Year;

        public static Computer[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Computer> dbItems = null;
            bool?                                result  = Program.Database?.Operations.GetComputers(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<Computer> items = new List<Computer>();

            return dbItems.Select(TransformItem).OrderBy(t => t.Company.Name).ThenBy(t => t.Model).ToArray();
        }

        public static Computer[] GetItemsFromCompany(int id)
        {
            List<Cicm.Database.Schemas.Computer> dbItems = null;
            bool?                                result  = Program.Database?.Operations.GetComputers(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            // TODO: Company chosen by DB
            return dbItems.Where(t => t.Company == id).Select(TransformItem).OrderBy(t => t.Model).ToArray();
        }

        public static Computer GetItem(int id)
        {
            Cicm.Database.Schemas.Computer dbItem = Program.Database?.Operations.GetComputer(id);
            return dbItem == null ? null : TransformItem(dbItem);
        }

        static Computer TransformItem(Cicm.Database.Schemas.Computer dbItem)
        {
            Computer item = new Computer
            {
                Bits       = dbItem.Bits,
                Colors     = dbItem.Colors,
                Comment    = dbItem.Comment,
                Company    = Company.GetItem(dbItem.Company),
                Gpu        = Gpu.GetItem(dbItem.Gpu),
                Hdd1       = DiskFormat.GetItem(dbItem.Hdd1),
                Hdd2       = DiskFormat.GetItem(dbItem.Hdd2),
                Hdd3       = DiskFormat.GetItem(dbItem.Hdd3),
                Id         = dbItem.Id,
                Model      = dbItem.Model,
                Ram        = dbItem.Ram,
                Resolution = dbItem.Resolution,
                Rom        = dbItem.Rom,
                Vram       = dbItem.Vram,
                Year       = dbItem.Year
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

            if(dbItem.Mpu > 0)
            {
                item.Mpu           = Mpu.GetItem(dbItem.Mpu);
                item.MusicChannels = dbItem.MusicChannels;
            }

            if(dbItem.Spu > 0)
            {
                item.Spu           = Dsp.GetItem(dbItem.Spu);
                item.SoundChannels = dbItem.SoundChannels;
            }

            return item;
        }
    }

    public class ComputerMini
    {
        public Company Company;
        public int     Id;
        public string  Model;

        public static ComputerMini[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Computer> dbItems = null;
            bool?                                result  = Program.Database?.Operations.GetComputers(out dbItems);

            if(result == null || result.Value == false || dbItems == null) return null;

            List<ComputerMini> items = new List<ComputerMini>();
            foreach(Cicm.Database.Schemas.Computer dbItem in dbItems) items.Add(TransformItem(dbItem));

            return items.OrderBy(t => t.Company.Name).ThenBy(t => t.Model).ToArray();
        }

        public static ComputerMini[] GetItemsStartingWithLetter(char letter)
        {
            List<Cicm.Database.Schemas.Computer> dbItems = null;
            bool?                                result  = Program.Database?.Operations.GetComputers(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<ComputerMini> items = new List<ComputerMini>();
            foreach(Cicm.Database.Schemas.Computer dbItem in dbItems)
                if(dbItem.Model.StartsWith(new string(letter, 1), StringComparison.InvariantCultureIgnoreCase))
                    items.Add(TransformItem(dbItem));

            return items.OrderBy(t => t.Company.Name).ThenBy(t => t.Model).ToArray();
        }

        public static ComputerMini[] GetItemsFromYear(int year)
        {
            List<Cicm.Database.Schemas.Computer> dbItems = null;
            bool?                                result  = Program.Database?.Operations.GetComputers(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<ComputerMini> items = new List<ComputerMini>();
            foreach(Cicm.Database.Schemas.Computer dbItem in dbItems)
                if(dbItem.Year == year)
                    items.Add(TransformItem(dbItem));

            return items.OrderBy(t => t.Company.Name).ThenBy(t => t.Model).ToArray();
        }

        static ComputerMini TransformItem(Cicm.Database.Schemas.Computer dbItem)
        {
            return new ComputerMini {Company = Company.GetItem(dbItem.Company), Id = dbItem.Id, Model = dbItem.Model};
        }
    }
}