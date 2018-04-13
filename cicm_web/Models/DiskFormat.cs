/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : DiskFormat.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Disk format model
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
    public class DiskFormat
    {
        public string Description;
        public int    Id;

        public static DiskFormat GetItem(int id)
        {
            Cicm.Database.Schemas.DiskFormat dbItem = Program.Database?.Operations.GetDiskFormat(id);
            return dbItem == null ? null : new DiskFormat {Description = dbItem.Description, Id = dbItem.Id};
        }

        public static DiskFormat[] GetAllItems()
        {
            List<Cicm.Database.Schemas.DiskFormat> dbItems = null;
            bool?                                  result  = Program.Database?.Operations.GetDiskFormats(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<DiskFormat> items = new List<DiskFormat>();

            foreach(Cicm.Database.Schemas.DiskFormat dbItem in dbItems)
                items.Add(new DiskFormat {Id = dbItem.Id, Description = dbItem.Description});

            return items.ToArray();
        }
    }
}