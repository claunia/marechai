/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : OwnConsole.cs
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
    public class OwnConsole
    {
        public DateTime   Acquired;
        public bool       Boxed;
        public Console    Console;
        public int        Id;
        public bool       Manuals;
        public StatusType Status;
        public bool       Trade;

        public static OwnConsole[] GetAllItems()
        {
            List<Cicm.Database.Schemas.OwnConsole> dbItems = null;
            bool?                                  result  = Program.Database?.Operations.GetOwnConsoles(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            return dbItems.OrderByDescending(i => i.Id).Select(TransformItem) as OwnConsole[];
        }

        public static OwnConsole GetItem(int id)
        {
            Cicm.Database.Schemas.OwnConsole dbItem = Program.Database?.Operations.GetOwnConsole(id);
            return dbItem == null ? null : TransformItem(dbItem);
        }

        static OwnConsole TransformItem(Cicm.Database.Schemas.OwnConsole dbItem)
        {
            Console console = Console.GetItem(dbItem.ConsoleId);

            return console == null
                       ? null
                       : new OwnConsole
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