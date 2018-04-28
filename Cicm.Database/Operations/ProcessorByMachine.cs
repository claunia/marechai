/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : ProcessorByMachine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage processors.
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
        ///     Gets all processors in machine
        /// </summary>
        /// <param name="entries">All CPUs</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetProcessorsByMachines(out List<ProcessorByMachine> entries, int machineId)
        {
            #if DEBUG
            Console.WriteLine("Getting all processors for machine {0}...", machineId);
            #endif

            try
            {
                string sql = $"SELECT * FROM processors_by_machine WHERE machine = {machineId}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ProcessorByMachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting processors by machine.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets all machines with specified processor
        /// </summary>
        /// <param name="entries">All CPUs</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetMachinesByProcessor(out List<ProcessorByMachine> entries, int processorId)
        {
            #if DEBUG
            Console.WriteLine("Getting all machines with processor {0}...", processorId);
            #endif

            try
            {
                string sql = $"SELECT * FROM processors_by_machine WHERE processor = {processorId}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ProcessorByMachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting machines by processor.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        IDbCommand GetCommandProcessorByMachine(ProcessorByMachine entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();
            IDbDataParameter param3 = dbcmd.CreateParameter();

            param1.ParameterName = "@processor";
            param2.ParameterName = "@machine";
            param3.ParameterName = "@speed";

            param1.DbType = DbType.Int32;
            param2.DbType = DbType.Int32;
            param3.DbType = DbType.Single;

            param1.Value = entry.Processor;
            param2.Value = entry.Machine;
            param3.Value = entry.Speed <= 0 ? (object)null : entry.Speed;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<ProcessorByMachine> ProcessorByMachinesFromDataTable(DataTable dataTable)
        {
            List<ProcessorByMachine> entries = new List<ProcessorByMachine>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                ProcessorByMachine entry = new ProcessorByMachine
                {
                    Machine   = (int)dataRow["machine"],
                    Processor = (int)dataRow["processor"],
                    Speed     = dataRow["speed"] == DBNull.Value ? 0 : (float)dataRow["speed"]
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}