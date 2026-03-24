/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
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
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.Services;

public class SoundSynthsService(Marechai.ApiClient.Client client)
{
    public async Task<List<SoundSynthDto>> GetAllAsync()
    {
        try
        {
            List<SoundSynthDto>? synths = await client.SoundSynths.GetAsync();

            return synths ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoundSynthDto?> GetByIdAsync(int id)
    {
        try
        {
            return await client.SoundSynths[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<MachineDto>> GetMachinesBySoundSynthAsync(int soundSynthId)
    {
        try
        {
            List<SoundSynthByMachineDto>? junctions =
                await client.SoundSynthsByMachine.BySoundSynth[soundSynthId].GetAsync();

            if(junctions is null || junctions.Count == 0) return [];

            var machines = new List<MachineDto>();

            foreach(SoundSynthByMachineDto junction in junctions)
            {
                if(!junction.MachineId.HasValue) continue;

                try
                {
                    MachineDto? machine = await client.Machines[junction.MachineId.Value].Full.GetAsync();

                    if(machine != null) machines.Add(machine);
                }
                catch
                {
                    // Skip machines that fail to load
                }
            }

            return machines;
        }
        catch
        {
            return [];
        }
    }
}
