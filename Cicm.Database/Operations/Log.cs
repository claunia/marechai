/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Log.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage log entries.
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
        ///     Gets all log entries
        /// </summary>
        /// <param name="entries">All log entries</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetLogs(out List<Log> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all logs...");
            #endif

            try
            {
                const string SQL = "SELECT * from log";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = LogsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting logs.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of log entries since the specified start
        /// </summary>
        /// <param name="entries">List of log entries</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetLogs(out List<Log> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} logs from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM log LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = LogsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting logs.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets log entry by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Log entry with specified id, <c>null</c> if not found or error</returns>
        public Log GetLog(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting log with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from log WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Log> entries = LogsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting log.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of log entries in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountLogs()
        {
            #if DEBUG
            Console.WriteLine("Counting logs...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM log";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new access log entry to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddLog(Log entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding log `{0}`...", entry.UserAgent);
            #endif

            IDbCommand     dbcmd = GetCommandLog(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO log (browser, date, ip, referer)" +
                               " VALUES (@browser, @date, @ip, @referer)";

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
        ///     Updated an access log in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateLog(Log entry)
        {
            #if DEBUG
            Console.WriteLine("Updating log `{0}`...", entry.UserAgent);
            #endif

            IDbCommand     dbcmd = GetCommandLog(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE log SET browser = @browser, date = @date, ip = @ip, referer = @referer " +
                         $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes an access log entry from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveLog(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing log widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM log WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandLog(Log entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();
            IDbDataParameter param3 = dbcmd.CreateParameter();
            IDbDataParameter param4 = dbcmd.CreateParameter();

            param1.ParameterName = "@browser";
            param2.ParameterName = "@date";
            param3.ParameterName = "@ip";
            param4.ParameterName = "@referer";

            param1.DbType = DbType.String;
            param2.DbType = DbType.String;
            param3.DbType = DbType.String;
            param4.DbType = DbType.String;

            param1.Value = entry.UserAgent;
            param2.Value = entry.Date;
            param3.Value = entry.Ip;
            param4.Value = entry.Referer;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);
            dbcmd.Parameters.Add(param3);
            dbcmd.Parameters.Add(param4);

            return dbcmd;
        }

        static List<Log> LogsFromDataTable(DataTable dataTable)
        {
            List<Log> entries = new List<Log>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Log entry = new Log
                {
                    Id        = int.Parse(dataRow["id"].ToString()),
                    UserAgent = dataRow["browser"].ToString(),
                    Date      = dataRow["date"].ToString(),
                    Ip        = dataRow["ip"].ToString(),
                    Referer   = dataRow["referer"].ToString()
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}