/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : GpuByMachine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Gpu by machine model
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
    public class SoundByMachine
    {
        public SoundSynth SoundSynth;

        public static SoundByMachine[] GetAllItems(int machineId)
        {
            List<Cicm.Database.Schemas.SoundByMachine> dbItems = null;
            bool? result =
                Program.Database?.Operations.GetSoundsByMachines(out dbItems, machineId);
            if(result == null || result.Value == false || dbItems == null) return null;

            List<SoundByMachine> items = new List<SoundByMachine>();

            foreach(Cicm.Database.Schemas.SoundByMachine dbItem in dbItems)
                items.Add(new SoundByMachine {SoundSynth = SoundSynth.GetItem(dbItem.SoundSynth)});

            return items.ToArray();
        }
    }
}