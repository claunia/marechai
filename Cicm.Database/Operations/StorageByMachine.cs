/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : StorageByMachine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage storage.
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
        ///     Gets all storage in machine
        /// </summary>
        /// <param name="entries">All CPUs</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetStorageByMachines(out List<StorageByMachine> entries, int machineId)
        {
            #if DEBUG
            Console.WriteLine("Getting all storage for machine {0}...", machineId);
            #endif

            try
            {
                string sql = $"SELECT * FROM storage_by_machine WHERE machine = {machineId}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = StorageByMachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting storage by machine.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        IDbCommand GetCommandStorageByMachine(StorageByMachine entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();
            IDbDataParameter param3 = dbcmd.CreateParameter();
            IDbDataParameter param4 = dbcmd.CreateParameter();

            param1.ParameterName = "@machine";
            param2.ParameterName = "@type";
            param3.ParameterName = "@interface";
            param4.ParameterName = "@capacity";

            param1.DbType = DbType.Int32;
            param2.DbType = DbType.Int32;
            param3.DbType = DbType.Int32;
            param4.DbType = DbType.Int64;

            param1.Value = entry.Machine;
            param2.Value = entry.Type;
            param3.Value = entry.Interface;
            param4.Value = entry.Capacity == 0 ? (object)null : entry.Capacity;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);
            dbcmd.Parameters.Add(param3);
            dbcmd.Parameters.Add(param4);

            return dbcmd;
        }

        static List<StorageByMachine> StorageByMachinesFromDataTable(DataTable dataTable)
        {
            List<StorageByMachine> entries = new List<StorageByMachine>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                StorageByMachine entry = new StorageByMachine
                {
                    Machine   = (int)dataRow["machine"],
                    Type      = (StorageType)dataRow["type"],
                    Interface = (StorageInterface)dataRow["interface"],
                    Capacity  = dataRow["capacity"] == DBNull.Value ? 0 : (long)dataRow["capacity"]
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}