/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : MusicSynth.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Music Synthetizer model
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
    public class MusicSynth
    {
        public int    Id;
        public string Name;

        public static MusicSynth GetItem(int id)
        {
            Cicm.Database.Schemas.MusicSynth dbItem = Program.Database?.Operations.GetMusicSynth(id);
            return dbItem == null ? null : new MusicSynth {Name = dbItem.Name, Id = dbItem.Id};
        }

        public static MusicSynth[] GetAllItems()
        {
            List<Cicm.Database.Schemas.MusicSynth> dbItems = null;
            bool?                                  result  = Program.Database?.Operations.GetMusicSynths(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<MusicSynth> items = new List<MusicSynth>();

            foreach(Cicm.Database.Schemas.MusicSynth dbItem in dbItems)
                items.Add(new MusicSynth {Id = dbItem.Id, Name = dbItem.Name});

            return items.ToArray();
        }
    }
}