/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Computer.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage computers.
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
        ///     Gets all computers
        /// </summary>
        /// <param name="entries">All computers</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetComputers(out List<Machine> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all computers...");
            #endif

            try
            {
                string sql = $"SELECT * FROM machines WHERE type = '{(int)MachineType.Computer}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting computers.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets all computers from specified company
        /// </summary>
        /// <param name="entries">All computers</param>
        /// <param name="company">Company id</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetComputers(out List<Machine> entries, int company)
        {
            #if DEBUG
            Console.WriteLine("Getting all computers from company id {0}...", company);
            #endif

            try
            {
                string sql =
                    $"SELECT * FROM machines WHERE company = '{company}' AND type = '{(int)MachineType.Computer}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting computers.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of computers since the specified start
        /// </summary>
        /// <param name="entries">List of computers</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetComputers(out List<Machine> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} computers from {1}...", count, start);
            #endif

            try
            {
                string sql =
                    $"SELECT * FROM machines LIMIT {start}, {count} WHERE type = '{(int)MachineType.Computer}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting computers.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets computer by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Computer with specified id, <c>null</c> if not found or error</returns>
        public Machine GetComputer(int id)
        {
            return GetMachine(id);
        }

        /// <summary>
        ///     Counts the number of computers in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountComputers()
        {
            #if DEBUG
            Console.WriteLine("Counting computers...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = $"SELECT COUNT(*) FROM computers WHERE type = '{(int)MachineType.Computer}'";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new administrator to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddComputer(Machine entry, out long id)
        {
            entry.Type = MachineType.Computer;
            return AddMachine(entry, out id);
        }

        /// <summary>
        ///     Updated a computer in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateComputer(Machine entry)
        {
            return UpdateMachine(entry);
        }

        /// <summary>
        ///     Removes a computer from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveComputer(long id)
        {
            return RemoveMachine(id);
        }
    }
}