/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : StorageByMachine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Storage by machine model
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
using Cicm.Database;

namespace cicm_web.Models
{
    public class StorageByMachine
    {
        /// <summary>Capacity in bytes</summary>
        public long Capacity;
        /// <summary>Storage interface</summary>
        public StorageInterface Interface;
        /// <summary>Storage type</summary>
        public StorageType Type;

        public static StorageByMachine[] GetAllItems(int machineId)
        {
            List<Cicm.Database.Schemas.StorageByMachine> dbItems = null;
            bool? result =
                Program.Database?.Operations.GetStorageByMachines(out dbItems, machineId);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<StorageByMachine> items = new List<StorageByMachine>();

            foreach(Cicm.Database.Schemas.StorageByMachine dbItem in dbItems)
                items.Add(new StorageByMachine
                {
                    Type      = dbItem.Type,
                    Interface = dbItem.Interface,
                    Capacity  = dbItem.Capacity
                });

            return items.ToArray();
        }
    }
}