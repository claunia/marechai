﻿/******************************************************************************
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
        public int        Bits;
        public int        Cap;
        public int        Colors;
        public Company    Company;
        public Processor  Cpu1;
        public Processor  Cpu2;
        public DiskFormat Format;
        public Gpu        Gpu;
        public int        Id;
        public float      Mhz1;
        public float      Mhz2;
        public string     Model;
        public int        MusicChannels;
        public SoundSynth MusicSynth;
        public int        Palette;
        public int        Ram;
        public string     Resolution;
        public int        Rom;
        public int        SoundChannels;
        public SoundSynth SoundSynth;
        public int        Vram;
        public int        Year;

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
            bool?                               result  = Program.Database?.Operations.GetConsoles(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            // TODO: Company chosen by DB
            return dbItems.Where(t => t.Company == id).Select(TransformItem).OrderBy(t => t.Model).ToArray();
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
                Company    = Company.GetItem(dbItem.Company),
                Gpu        = Gpu.GetItem(dbItem.Gpu),
                Id         = dbItem.Id,
                Model      = dbItem.Model,
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

    public class ConsoleMini
    {
        public Company Company;
        public int     Id;
        public string  Model;

        public static ConsoleMini[] GetAllItems()
        {
            List<Cicm.Database.Schemas.Console> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetConsoles(out dbItems);

            if(result == null || result.Value == false || dbItems == null) return null;

            List<ConsoleMini> items = new List<ConsoleMini>();
            foreach(Cicm.Database.Schemas.Console dbItem in dbItems) items.Add(TransformItem(dbItem));

            return items.OrderBy(t => t.Company.Name).ThenBy(t => t.Model).ToArray();
        }

        public static ConsoleMini[] GetItemsStartingWithLetter(char letter)
        {
            List<Cicm.Database.Schemas.Console> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetConsoles(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<ConsoleMini> items = new List<ConsoleMini>();
            foreach(Cicm.Database.Schemas.Console dbItem in dbItems)
                if(dbItem.Model.StartsWith(new string(letter, 1), StringComparison.InvariantCultureIgnoreCase))
                    items.Add(TransformItem(dbItem));

            return items.OrderBy(t => t.Company.Name).ThenBy(t => t.Model).ToArray();
        }

        public static ConsoleMini[] GetItemsFromYear(int year)
        {
            List<Cicm.Database.Schemas.Console> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetConsoles(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<ConsoleMini> items = new List<ConsoleMini>();
            foreach(Cicm.Database.Schemas.Console dbItem in dbItems)
                if(dbItem.Year == year)
                    items.Add(TransformItem(dbItem));

            return items.OrderBy(t => t.Company.Name).ThenBy(t => t.Model).ToArray();
        }

        public static ConsoleMini[] GetItemsWithCompany(int id, string companyName)
        {
            List<Cicm.Database.Schemas.Console> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetConsoles(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            // TODO: Company chosen by DB
            return dbItems.Where(t => t.Company == id)
                          .Select(t => new ConsoleMini
                           {
                               Company = new Company {Id = id, Name = companyName},
                               Id      = t.Id,
                               Model   = t.Model
                           }).OrderBy(t => t.Model).ToArray();
        }

        public static ConsoleMini[] GetItemsFromCompany(int id)
        {
            List<Cicm.Database.Schemas.Console> dbItems = null;
            bool?                               result  = Program.Database?.Operations.GetConsoles(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            // TODO: Company chosen by DB
            return dbItems.Where(t => t.Company == id).Select(TransformItem).OrderBy(t => t.Model).ToArray();
        }

        static ConsoleMini TransformItem(Cicm.Database.Schemas.Console dbItem)
        {
            return new ConsoleMini {Company = Company.GetItem(dbItem.Company), Id = dbItem.Id, Model = dbItem.Model};
        }
    }
}