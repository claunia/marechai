/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : MachineFamily.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage machine_families.
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
        ///     Gets all machine_families
        /// </summary>
        /// <param name="entries">All machine_families</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetMachineFamilies(out List<MachineFamily> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all machine_families...");
            #endif

            try
            {
                const string SQL = "SELECT * from machine_families";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MachineFamiliesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting machine_families.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets all machine_families from specified company
        /// </summary>
        /// <param name="entries">All machine_families</param>
        /// <param name="company">Company id</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetMachineFamilies(out List<MachineFamily> entries, int company)
        {
            #if DEBUG
            Console.WriteLine("Getting all machine_families from company id {0}...", company);
            #endif

            try
            {
                string sql = $"SELECT * from machine_families WHERE company = '{company}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MachineFamiliesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting machine_families.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of machine_families since the specified start
        /// </summary>
        /// <param name="entries">List of machine_families</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetMachineFamilies(out List<MachineFamily> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} machine_families from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM machine_families LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MachineFamiliesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting machine_families.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets machine_families by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>MachineFamily with specified id, <c>null</c> if not found or error</returns>
        public MachineFamily GetMachineFamily(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting machine_families with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from machine_families WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<MachineFamily> entries = MachineFamiliesFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting machine_families.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of machine_families in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountMachineFamilies()
        {
            #if DEBUG
            Console.WriteLine("Counting machine_families...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM machine_families";
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
        public bool AddMachineFamily(MachineFamily entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding machine_families `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandMachineFamily(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO machine_families (company, name) VALUES (@company, @name)";

            dbcmd.CommandText = SQL;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            id = dbCore.LastInsertRowId;

            #if DEBUG
            Console.WriteLine(" id {0}", id);
            #endif

            return true;
        }

        /// <summary>
        ///     Updated a machine_families in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateMachineFamily(MachineFamily entry)
        {
            #if DEBUG
            Console.WriteLine("Updating machine_families `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandMachineFamily(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE machine_families SET company = @company, name = @name " + $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a machine_families from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveMachineFamily(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing machine_families widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM machine_families WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandMachineFamily(MachineFamily entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();

            param1.ParameterName = "@company";
            param2.ParameterName = "@name";

            param1.DbType = DbType.Int32;
            param2.DbType = DbType.String;

            param1.Value = entry.Company;
            param2.Value = entry.Name;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);

            return dbcmd;
        }

        static List<MachineFamily> MachineFamiliesFromDataTable(DataTable dataTable)
        {
            List<MachineFamily> entries = new List<MachineFamily>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                MachineFamily entry = new MachineFamily
                {
                    Id      = (int)dataRow["id"],
                    Company = (int)dataRow["company"],
                    Name    = (string)dataRow["name"]
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}