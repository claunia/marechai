/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : SoundSynth.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Digital Sound Synthetizer model
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

namespace cicm_web.Models
{
    public class SoundSynth
    {
        public Company  Company;
        public int      Depth;
        public double   Frequency;
        public int      Id;
        public DateTime Introduced;
        public string   ModelCode;
        public string   Name;
        public int      SquareWave;
        public int      Type;
        public int      Voices;
        public int      WhiteNoise;

        public static SoundSynth GetItem(int id)
        {
            Cicm.Database.Schemas.SoundSynth dbItem = Program.Database?.Operations.GetSoundSynth(id);
            return dbItem == null
                       ? null
                       : new SoundSynth
                       {
                           Name       = dbItem.Name,
                           Id         = dbItem.Id,
                           Company    = dbItem.Company == null ? null : Company.GetItem(dbItem.Company.Id),
                           ModelCode  = dbItem.ModelCode,
                           Introduced = dbItem.Introduced,
                           Voices     = dbItem.Voices,
                           Frequency  = dbItem.Frequency,
                           Depth      = dbItem.Depth,
                           SquareWave = dbItem.SquareWave,
                           WhiteNoise = dbItem.WhiteNoise,
                           Type       = dbItem.Type
                       };
        }

        public static SoundSynth[] GetAllItems()
        {
            List<Cicm.Database.Schemas.SoundSynth> dbItems = null;
            bool?                                  result  = Program.Database?.Operations.GetSoundSynths(out dbItems);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<SoundSynth> items = new List<SoundSynth>();

            foreach(Cicm.Database.Schemas.SoundSynth dbItem in dbItems)
                items.Add(new SoundSynth
                {
                    Name       = dbItem.Name,
                    Id         = dbItem.Id,
                    Company    = dbItem.Company == null ? null : Company.GetItem(dbItem.Company.Id),
                    ModelCode  = dbItem.ModelCode,
                    Introduced = dbItem.Introduced,
                    Voices     = dbItem.Voices,
                    Frequency  = dbItem.Frequency,
                    Depth      = dbItem.Depth,
                    SquareWave = dbItem.SquareWave,
                    WhiteNoise = dbItem.WhiteNoise,
                    Type       = dbItem.Type
                });

            return items.ToArray();
        }
    }
}