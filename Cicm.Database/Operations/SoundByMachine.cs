/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : SoundByMachine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage sound.
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
using System.Data;
using Cicm.Database.Schemas;

namespace Cicm.Database
{
    public partial class Operations
    {
        /// <summary>
        ///     Gets all sound in machine
        /// </summary>
        /// <param name="entries">All CPUs</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetSoundsByMachines(out List<SoundByMachine> entries, int machineId)
        {
            #if DEBUG
            Console.WriteLine("Getting all sound synths for machine {0}...", machineId);
            #endif

            try
            {
                string sql = $"SELECT * FROM sound_by_machine WHERE machine = {machineId}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = SoundByMachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting sound by machine.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets all machines with specified gpu
        /// </summary>
        /// <param name="entries">All CPUs</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetMachinesBySound(out List<SoundByMachine> entries, int gpuId)
        {
            #if DEBUG
            Console.WriteLine("Getting all machines with sound synth {0}...", gpuId);
            #endif

            try
            {
                string sql = $"SELECT * FROM sound_by_machine WHERE gpu = {gpuId}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = SoundByMachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting machines by sound synth.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        IDbCommand GetCommandSoundByMachine(SoundByMachine entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();

            param1.ParameterName = "@sound_synth";
            param2.ParameterName = "@machine";

            param1.DbType = DbType.Int32;
            param2.DbType = DbType.Int32;

            param1.Value = entry.SoundSynth;
            param2.Value = entry.Machine;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<SoundByMachine> SoundByMachinesFromDataTable(DataTable dataTable)
        {
            List<SoundByMachine> entries = new List<SoundByMachine>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                SoundByMachine entry = new SoundByMachine
                {
                    Machine    = (int)dataRow["machine"],
                    SoundSynth = (int)dataRow["sound_synth"]
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}