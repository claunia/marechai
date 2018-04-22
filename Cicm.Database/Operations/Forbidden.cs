/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Forbidden.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage forbidden accesses.
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
        ///     Gets all forbidden accesses
        /// </summary>
        /// <param name="entries">All forbidden accesses</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetForbiddens(out List<Forbidden> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all forbiddens...");
            #endif

            try
            {
                const string SQL = "SELECT * from forbidden";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ForbiddensFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting forbiddens.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of forbidden accesses since the specified start
        /// </summary>
        /// <param name="entries">List of forbidden accesses</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetForbiddens(out List<Forbidden> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} forbiddens from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM forbidden LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ForbiddensFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting forbiddens.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets forbidden entry by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Forbidden entry with specified id, <c>null</c> if not found or error</returns>
        public Forbidden GetForbidden(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting forbidden with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from forbidden WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Forbidden> entries = ForbiddensFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting forbidden.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of Forbidden accesses in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountForbiddens()
        {
            #if DEBUG
            Console.WriteLine("Counting forbiddens...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM forbidden";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new forbidden to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddForbidden(Forbidden entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding forbidden `{0}`...", entry.UserAgent);
            #endif

            IDbCommand     dbcmd = GetCommandForbidden(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO forbidden (browser, date, ip, referer)" +
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
        ///     Updated a forbidden access in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateForbidden(Forbidden entry)
        {
            #if DEBUG
            Console.WriteLine("Updating forbidden `{0}`...", entry.UserAgent);
            #endif

            IDbCommand     dbcmd = GetCommandForbidden(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE forbidden SET browser = @browser, date = @date, ip = @ip, referer = @referer " +
                         $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a forbidden access from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveForbidden(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing forbidden widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM forbidden WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandForbidden(Forbidden entry)
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

        static List<Forbidden> ForbiddensFromDataTable(DataTable dataTable)
        {
            List<Forbidden> entries = new List<Forbidden>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Forbidden entry = new Forbidden
                {
                    Id        = (int)dataRow["id"],
                    UserAgent = (string)dataRow["browser"],
                    Date      = (string)dataRow["date"],
                    Ip        = (string)dataRow["ip"],
                    Referer   = (string)dataRow["referer"]
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}