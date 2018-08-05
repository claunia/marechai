/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : MemoryByMachine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage memory.
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
        ///     Gets all memory in machine
        /// </summary>
        /// <param name="entries">All CPUs</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetMemoryByMachine(out List<MemoryByMachine> entries, int machineId)
        {
            #if DEBUG
            Console.WriteLine("Getting all memory synths for machine {0}...", machineId);
            #endif

            try
            {
                string sql = $"SELECT * FROM memory_by_machine WHERE machine = {machineId}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MemoryByMachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting memory by machine.");
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
        public bool GetMachinesByMemory(out List<MemoryByMachine> entries, int gpuId)
        {
            #if DEBUG
            Console.WriteLine("Getting all machines with memory synth {0}...", gpuId);
            #endif

            try
            {
                string sql = $"SELECT * FROM memory_by_machine WHERE gpu = {gpuId}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MemoryByMachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting machines by memory synth.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        IDbCommand GetCommandMemoryByMachine(MemoryByMachine entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();
            IDbDataParameter param3 = dbcmd.CreateParameter();
            IDbDataParameter param4 = dbcmd.CreateParameter();
            IDbDataParameter param5 = dbcmd.CreateParameter();

            param1.ParameterName = "@machine";
            param2.ParameterName = "@type";
            param3.ParameterName = "@usage";
            param4.ParameterName = "@size";
            param5.ParameterName = "@speed";

            param1.DbType = DbType.Int32;
            param2.DbType = DbType.Int32;
            param3.DbType = DbType.Int32;
            param4.DbType = DbType.Int64;
            param5.DbType = DbType.Double;

            param1.Value = entry.Machine;
            param2.Value = entry.Type;
            param3.Value = entry.Usage;
            param4.Value = entry.Size  == 0 ? (object)null : entry.Size;
            param5.Value = entry.Speed <= 0 ? (object)null : entry.Speed;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);
            dbcmd.Parameters.Add(param3);
            dbcmd.Parameters.Add(param4);
            dbcmd.Parameters.Add(param5);

            return dbcmd;
        }

        static List<MemoryByMachine> MemoryByMachinesFromDataTable(DataTable dataTable)
        {
            List<MemoryByMachine> entries = new List<MemoryByMachine>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                MemoryByMachine entry = new MemoryByMachine
                {
                    Machine = (int)dataRow["machine"],
                    Type    = (MemoryType)dataRow["type"],
                    Usage   = (MemoryUsage)dataRow["usage"],
                    Size    = dataRow["size"]  == DBNull.Value ? 0 : (long)dataRow["size"],
                    Speed   = dataRow["speed"] == DBNull.Value ? 0 : (double)dataRow["speed"]
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}