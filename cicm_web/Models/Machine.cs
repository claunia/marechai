/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Machine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Machine model
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
using Cicm.Database.Schemas;

namespace cicm_web.Models
{
    public class Machine
    {
        public string      Cap1;
        public string      Cap2;
        public int         Colors;
        public Company     Company;
        public Processor   Cpu1;
        public Processor   Cpu2;
        public DiskFormat  Disk1;
        public DiskFormat  Disk2;
        public Gpu         Gpu;
        public DiskFormat  Hdd1;
        public DiskFormat  Hdd2;
        public DiskFormat  Hdd3;
        public int         Id;
        public float       Mhz1;
        public float       Mhz2;
        public string      Model;
        public int         MusicChannels;
        public SoundSynth  MusicSynth;
        public int         Ram;
        public string      Resolution;
        public int         Rom;
        public int         SoundChannels;
        public SoundSynth  SoundSynth;
        public MachineType Type;
        public int         Vram;
        public int         Year;

        public static Machine[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Machine> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetMachines(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<Machine> items = new List<Machine>();

            return dbItems.Select(TransformItem).OrderBy(t => t.Company.Name).ThenBy(t => t.Model).ToArray();
        }

        public static Machine[] GetItemsFromCompany(int id)
        {
            List<Cicm.Database.Schemas.Machine> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetMachines(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            // TODO: Company chosen by DB
            return dbItems.Where(t => t.Company == id).Select(TransformItem).OrderBy(t => t.Model).ToArray();
        }

        public static Machine GetItem(int id)
        {
            Cicm.Database.Schemas.Machine dbItem = Program.Database?.Operations.GetMachine(id);
            return dbItem == null ? null : TransformItem(dbItem);
        }

        internal static Machine TransformItem(Cicm.Database.Schemas.Machine dbItem)
        {
            Machine item = new Machine
            {
                Colors     = dbItem.Colors,
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
                Year       = dbItem.Year,
                Type       = dbItem.Type
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
                item.Cpu1 = Processor.GetItem(dbItem.Cpu1);
                item.Mhz1 = dbItem.Mhz1;
            }

            if(dbItem.Cpu2 > 0)
            {
                item.Cpu2 = Processor.GetItem(dbItem.Cpu2);
                item.Mhz2 = dbItem.Mhz2;
            }

            if(dbItem.MusicSynth > 0)
            {
                item.MusicSynth    = SoundSynth.GetItem(dbItem.MusicSynth);
                item.MusicChannels = dbItem.MusicChannels;
            }

            if(dbItem.SoundSynth > 0)
            {
                item.SoundSynth    = SoundSynth.GetItem(dbItem.SoundSynth);
                item.SoundChannels = dbItem.SoundChannels;
            }

            return item;
        }
    }

    public class MachineMini
    {
        public Company Company;
        public int     Id;
        public string  Model;

        public static MachineMini[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Machine> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetMachines(out dbItems);

            if(result == null || result.Value == false || dbItems == null) return null;

            List<MachineMini> items = new List<MachineMini>();
            foreach(Cicm.Database.Schemas.Machine dbItem in dbItems) items.Add(TransformItem(dbItem));

            return items.OrderBy(t => t.Company.Name).ThenBy(t => t.Model).ToArray();
        }

        public static MachineMini[] GetItemsStartingWithLetter(char letter)
        {
            List<Cicm.Database.Schemas.Machine> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetMachines(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<MachineMini> items = new List<MachineMini>();
            foreach(Cicm.Database.Schemas.Machine dbItem in dbItems)
                if(dbItem.Model.StartsWith(new string(letter, 1), StringComparison.InvariantCultureIgnoreCase))
                    items.Add(TransformItem(dbItem));

            return items.OrderBy(t => t.Company.Name).ThenBy(t => t.Model).ToArray();
        }

        public static MachineMini[] GetItemsFromYear(int year)
        {
            List<Cicm.Database.Schemas.Machine> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetMachines(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<MachineMini> items = new List<MachineMini>();
            foreach(Cicm.Database.Schemas.Machine dbItem in dbItems)
                if(dbItem.Year == year)
                    items.Add(TransformItem(dbItem));

            return items.OrderBy(t => t.Company.Name).ThenBy(t => t.Model).ToArray();
        }

        public static MachineMini[] GetItemsWithCompany(int id, string companyName)
        {
            List<Cicm.Database.Schemas.Machine> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetMachines(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            // TODO: Company chosen by DB
            return dbItems.Where(t => t.Company == id)
                          .Select(t => new MachineMini
                           {
                               Company = new Company {Id = id, Name = companyName},
                               Id      = t.Id,
                               Model   = t.Model
                           }).OrderBy(t => t.Model).ToArray();
        }

        public static MachineMini[] GetItemsFromCompany(int id)
        {
            List<Cicm.Database.Schemas.Machine> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetMachines(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            // TODO: Company chosen by DB
            return dbItems.Where(t => t.Company == id).Select(TransformItem).OrderBy(t => t.Model).ToArray();
        }

        internal static MachineMini TransformItem(Cicm.Database.Schemas.Machine dbItem)
        {
            return new MachineMini {Company = Company.GetItem(dbItem.Company), Id = dbItem.Id, Model = dbItem.Model};
        }
    }
}