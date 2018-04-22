/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : InstructionSet.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage instruction_set_extensions.
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
using Console = System.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
        /// <summary>
        ///     Gets all instruction set extensions
        /// </summary>
        /// <param name="entries">All instruction set extensions</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetInstructionSetExtensions(out List<InstructionSetExtension> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all instruction set extensions...");
            #endif

            try
            {
                const string SQL = "SELECT * from instruction_set_extensions";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = InstructionSetExtensionsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting instruction set extensions.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of instruction set extensions since the specified start
        /// </summary>
        /// <param name="entries">List of instruction set extensions</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetInstructionSetExtensions(out List<InstructionSetExtension> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} instruction set extensions from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM instruction_set_extensions LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = InstructionSetExtensionsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting instruction set extensions.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets instruction set extension by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>InstructionSet with specified id, <c>null</c> if not found or error</returns>
        public InstructionSetExtension GetInstructionSetExtension(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting instruction set extension with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from instruction_set_extensions WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<InstructionSetExtension> entries = InstructionSetExtensionsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting instruction set extension.");
                Console.WriteLine(ex);
                return null;
            }
        }

        public List<InstructionSetExtension> GetInstructionSetExtensions(int processor)
        {
            #if DEBUG
            Console.WriteLine("Getting instruction set extension for processor id {0}...", processor);
            #endif

            try
            {
                string sql =
                    $"SELECT * from instruction_set_extensions_by_processor WHERE processor_id = '{processor}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                if(dataSet.Tables[0].Rows.Count == 0) return null;

                List<InstructionSetExtension> entries = new List<InstructionSetExtension>();
                foreach(DataRow dataRow in dataSet.Tables[0].Rows)
                {
                    if(!int.TryParse(dataRow["extension_id"].ToString(), out int extensionId)) continue;
                    if(extensionId == 0) continue;

                    entries.Add(GetInstructionSetExtension(extensionId));
                }

                return entries;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting instruction set extension.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of instruction set extensions in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountInstructionSetExtensions()
        {
            #if DEBUG
            Console.WriteLine("Counting instruction_set_extensions...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM instruction_set_extensions";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new instruction set extension to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddInstructionSetExtension(InstructionSetExtension entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding instruction set extension `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandInstructionSetExtension(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO instruction_set_extensions (extension) VALUES (@extension)";

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
        ///     Updated a instruction set extension in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateInstructionSetExtension(InstructionSetExtension entry)
        {
            #if DEBUG
            Console.WriteLine("Updating instruction set extension `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandInstructionSetExtension(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE instruction_set_extensions SET extension = @extension " + $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a instruction set extension from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveInstructionSetExtension(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing instruction set extension widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM instruction_set_extensions WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandInstructionSetExtension(InstructionSetExtension entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();

            param1.ParameterName = "@extension";

            param1.DbType = DbType.String;

            param1.Value = entry.Name;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<InstructionSetExtension> InstructionSetExtensionsFromDataTable(DataTable dataTable)
        {
            List<InstructionSetExtension> entries = new List<InstructionSetExtension>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                InstructionSetExtension entry =
                    new InstructionSetExtension {Id = (int)dataRow["id"], Name = (string)dataRow["extension"]};

                entries.Add(entry);
            }

            return entries;
        }
    }
}