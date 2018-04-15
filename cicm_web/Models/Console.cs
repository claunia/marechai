/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Console.cs
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
    public class Console
    {
        public int            Bits;
        public int            Cap;
        public int            Colors;
        public string         Comments;
        public ConsoleCompany Company;
        public Cpu            Cpu1;
        public Cpu            Cpu2;
        public DiskFormat     Format;
        public Gpu            Gpu;
        public int            Id;
        public float          Mhz1;
        public float          Mhz2;
        public Mpu            Mpu;
        public int            MusicChannels;
        public string         Name;
        public int            Palette;
        public int            Ram;
        public string         Resolution;
        public int            Rom;
        public int            SoundChannels;
        public Dsp            Spu;
        public int            Vram;
        public int            Year;

        public static Console[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Console> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetConsoles(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            return dbItems.OrderByDescending(i => i.Id).Select(TransformItem) as Console[];
        }

        public static Console[] GetItemsFromCompany(int id)
        {
            List<Cicm.Database.Schemas.Console> dbItems = null;
            bool?                                result  = Program.Database?.Operations.GetConsoles(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            // TODO: Company chosen by DB
            return dbItems.Where(t => t.Company == id).Select(TransformItem).OrderBy(t => t.Name).ToArray();
        }

        public static Console GetItem(int id)
        {
            Cicm.Database.Schemas.Console dbItem = Program.Database?.Operations.GetConsole(id);
            return dbItem == null ? null : TransformItem(dbItem);
        }

        static Console TransformItem(Cicm.Database.Schemas.Console dbItem)
        {
            Console item = new Console
            {
                Bits       = dbItem.Bits,
                Colors     = dbItem.Colors,
                Comments   = dbItem.Comments,
                Company    = ConsoleCompany.GetItem(dbItem.Company),
                Gpu        = Gpu.GetItem(dbItem.Gpu),
                Id         = dbItem.Id,
                Name       = dbItem.Name,
                Palette    = dbItem.Palette,
                Ram        = dbItem.Ram,
                Resolution = dbItem.Resolution,
                Rom        = dbItem.Rom,
                Vram       = dbItem.Vram,
                Year       = dbItem.Year
            };

            if(dbItem.Format > 0)
            {
                item.Cap    = dbItem.Cap;
                item.Format = DiskFormat.GetItem(dbItem.Format);
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

    public class ConsoleMini
    {
        public ConsoleCompany Company;
        public int     Id;
        public string  Name;

        public static ConsoleMini[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Console> dbItems = null;
            bool?                                result  = Program.Database?.Operations.GetConsoles(out dbItems);

            if(result == null || result.Value == false || dbItems == null) return null;

            List<ConsoleMini> items = new List<ConsoleMini>();
            foreach(Cicm.Database.Schemas.Console dbItem in dbItems) items.Add(TransformItem(dbItem));

            return items.OrderBy(t => t.Company.Name).ThenBy(t => t.Name).ToArray();
        }

        public static ConsoleMini[] GetItemsStartingWithLetter(char letter)
        {
            List<Cicm.Database.Schemas.Console> dbItems = null;
            bool?                                result  = Program.Database?.Operations.GetConsoles(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<ConsoleMini> items = new List<ConsoleMini>();
            foreach(Cicm.Database.Schemas.Console dbItem in dbItems)
                if(dbItem.Name.StartsWith(new string(letter, 1), StringComparison.InvariantCultureIgnoreCase))
                    items.Add(TransformItem(dbItem));

            return items.OrderBy(t => t.Company.Name).ThenBy(t => t.Name).ToArray();
        }

        public static ConsoleMini[] GetItemsFromYear(int year)
        {
            List<Cicm.Database.Schemas.Console> dbItems = null;
            bool?                                result  = Program.Database?.Operations.GetConsoles(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<ConsoleMini> items = new List<ConsoleMini>();
            foreach(Cicm.Database.Schemas.Console dbItem in dbItems)
                if(dbItem.Year == year)
                    items.Add(TransformItem(dbItem));

            return items.OrderBy(t => t.Company.Name).ThenBy(t => t.Name).ToArray();
        }

        static ConsoleMini TransformItem(Cicm.Database.Schemas.Console dbItem)
        {
            return new ConsoleMini {Company = ConsoleCompany.GetItem(dbItem.Company), Id = dbItem.Id, Name = dbItem.Name};
        }
    }
}