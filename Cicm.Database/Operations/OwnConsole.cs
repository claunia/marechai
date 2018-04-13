/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : OwnConsole.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage owned consoles.
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
        ///     Gets all owned consoles
        /// </summary>
        /// <param name="entries">All owned consoles</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetOwnConsoles(out List<OwnConsole> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all owned consoles...");
            #endif

            try
            {
                const string SQL = "SELECT * from own_consoles";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = OwnConsolesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting owned consoles.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of owned consoles since the specified start
        /// </summary>
        /// <param name="entries">List of owned consoles</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetOwnConsoles(out List<OwnConsole> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} owned consoles from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM own_consoles LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = OwnConsolesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting owned consoles.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets owned console by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Owned console with specified id, <c>null</c> if not found or error</returns>
        public OwnConsole GetOwnConsole(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting owned console with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from own_consoles WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<OwnConsole> entries = OwnConsolesFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting owned console.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of owned consoles in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountOwnConsoles()
        {
            #if DEBUG
            Console.WriteLine("Counting owned consoles...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM own_consoles";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new owned console to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddOwnConsole(OwnConsole entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding owned console `{0}`...", entry.ConsoleId);
            #endif

            IDbCommand     dbcmd = GetCommandOwnConsole(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO own_consoles (db_id, date, status, trade, boxed, manuals)" +
                               " VALUES (@db_id, @date, @status, @trade, @boxed, @manuals)";

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
        ///     Updated an owned console in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateOwnConsole(OwnConsole entry)
        {
            #if DEBUG
            Console.WriteLine("Updating console `{0}`...", entry.ConsoleId);
            #endif

            IDbCommand     dbcmd = GetCommandOwnConsole(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql =
                "UPDATE own_consoles SET db_id = @db_id, date = @date, status = @status, trade = @trade, boxed = @boxed, manuals = @manuals " +
                $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes an owned console from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveOwnConsole(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing owned console widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM own_consoles WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandOwnConsole(OwnConsole entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();
            IDbDataParameter param3 = dbcmd.CreateParameter();
            IDbDataParameter param4 = dbcmd.CreateParameter();
            IDbDataParameter param5 = dbcmd.CreateParameter();
            IDbDataParameter param6 = dbcmd.CreateParameter();

            param1.ParameterName = "@db_id";
            param2.ParameterName = "@date";
            param3.ParameterName = "@status";
            param4.ParameterName = "@trade";
            param5.ParameterName = "@boxed";
            param6.ParameterName = "@manuals";

            param1.DbType = DbType.Int32;
            param2.DbType = DbType.String;
            param3.DbType = DbType.Int32;
            param4.DbType = DbType.Boolean;
            param5.DbType = DbType.Boolean;
            param6.DbType = DbType.Boolean;

            param1.Value = entry.ConsoleId;
            param2.Value = entry.Acquired;
            param3.Value = entry.Status;
            param4.Value = entry.Trade;
            param5.Value = entry.Boxed;
            param6.Value = entry.Manuals;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);
            dbcmd.Parameters.Add(param3);
            dbcmd.Parameters.Add(param4);
            dbcmd.Parameters.Add(param5);
            dbcmd.Parameters.Add(param6);

            return dbcmd;
        }

        static List<OwnConsole> OwnConsolesFromDataTable(DataTable dataTable)
        {
            List<OwnConsole> entries = new List<OwnConsole>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                OwnConsole entry = new OwnConsole
                {
                    Id        = int.Parse(dataRow["id"].ToString()),
                    ConsoleId = int.Parse(dataRow["db_id"].ToString()),
                    Acquired  = dataRow["date"].ToString(),
                    Status    = (StatusType)int.Parse(dataRow["status"].ToString()),
                    Trade     = bool.Parse(dataRow["trade"].ToString()),
                    Boxed     = bool.Parse(dataRow["boxed"].ToString()),
                    Manuals   = bool.Parse(dataRow["manuals"].ToString())
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}