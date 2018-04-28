/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : OwnedConsole.cs
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
    public class OwnedConsole
    {
        public DateTime   Acquired;
        public bool       Boxed;
        public Machine    Console;
        public int        Id;
        public bool       Manuals;
        public StatusType Status;
        public bool       Trade;

        public static OwnedConsole[] GetAllItems()
        {
            List<Cicm.Database.Schemas.OwnedConsole> dbItems = null;
            bool? result =
                Program.Database?.Operations.GetOwnedConsoles(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            return dbItems.OrderByDescending(i => i.Id).Select(TransformItem) as OwnedConsole[];
        }

        public static OwnedConsole GetItem(int id)
        {
            Cicm.Database.Schemas.OwnedConsole dbItem = Program.Database?.Operations.GetOwnedConsole(id);
            return dbItem == null ? null : TransformItem(dbItem);
        }

        static OwnedConsole TransformItem(Cicm.Database.Schemas.OwnedConsole dbItem)
        {
            Machine console = Machine.GetItem(dbItem.ConsoleId);

            return console == null
                       ? null
                       : new OwnedConsole
                       {
                           Acquired =
                               DateTime.ParseExact(dbItem.Acquired, "yyyy/MM/dd HH:mm:ss",
                                                   CultureInfo.InvariantCulture),
                           Boxed   = dbItem.Boxed,
                           Console = console,
                           Id      = dbItem.Id,
                           Manuals = dbItem.Manuals,
                           Status  = dbItem.Status,
                           Trade   = dbItem.Trade
                       };
        }
    }
}