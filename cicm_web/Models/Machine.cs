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
        public Company              Company;
        public GpuByMachine[]       Gpus;
        public int                  Id;
        public DateTime             Introduced;
        public MemoryByMachine[]    Memories;
        public string               Model;
        public ProcessorByMachine[] Processors;
        public SoundByMachine[]     SoundSynths;
        public StorageByMachine[]   Storage;
        public MachineType          Type;

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
            return new Machine
            {
                Company     = Company.GetItem(dbItem.Company),
                Gpus        = GpuByMachine.GetAllItems(dbItem.Id),
                Id          = dbItem.Id,
                Model       = dbItem.Model,
                Introduced  = dbItem.Introduced,
                Type        = dbItem.Type,
                Processors  = ProcessorByMachine.GetAllItems(dbItem.Id),
                SoundSynths = SoundByMachine.GetAllItems(dbItem.Id),
                Memories    = MemoryByMachine.GetAllItems(dbItem.Id),
                Storage     = StorageByMachine.GetAllItems(dbItem.Id)
            };
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
                if(dbItem.Introduced.Year == year)
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