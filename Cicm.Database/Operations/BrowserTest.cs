/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : BrowserTest.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage browser tests.
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
        ///     Gets all browser tests
        /// </summary>
        /// <param name="entries">All browser tests</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetBrowserTests(out List<BrowserTest> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all browser tests...");
            #endif

            try
            {
                const string SQL = "SELECT * from browser_test";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = BrowserTestsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting browser tests.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of browser tests since the specified start
        /// </summary>
        /// <param name="entries">List of browser tests</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetBrowserTests(out List<BrowserTest> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} browser tests from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM browser_test LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = BrowserTestsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting browser tests.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets browser test by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Browser test with specified id, <c>null</c> if not found or error</returns>
        public BrowserTest GetBrowserTest(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting browser test with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from browser_test WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<BrowserTest> entries = BrowserTestsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting browser test.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of browser tests in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountBrowserTests()
        {
            #if DEBUG
            Console.WriteLine("Counting browser tests...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM browser_test";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new browser test to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddBrowserTest(BrowserTest entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding browser test `{0}`...", entry.UserAgent);
            #endif

            IDbCommand     dbcmd = GetCommandBrowserTest(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL =
                "INSERT INTO browser_test (idstring, browser, version, os, platform, gif87, gif89, jpeg, png, pngt, agif, table, colors, js, frames, flash)" +
                " VALUES (@idstring, @browser, @version, @os, @platform, @gif87, @gif89, @jpeg, @png, @pngt, @agif, @table, @colors, @js, @frames, @flash)";

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
        ///     Updated a browser test in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateBrowserTest(BrowserTest entry)
        {
            #if DEBUG
            Console.WriteLine("Updating browser test `{0}`...", entry.UserAgent);
            #endif

            IDbCommand     dbcmd = GetCommandBrowserTest(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql =
                "UPDATE browser_test SET idstring = @idstring, browser = @browser, version = @version, os = @os, platform = @platform, gif87 = @gif87, "              +
                "gif89 = @gif89, jpeg = @jpeg, png = @png, pngt = @pngt, agif = @agif, table = @table, colors = @colors, js = @js, frames = @frames, flash = @flash " +
                $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a browser test from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveBrowserTest(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing browser test widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM browser_test WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandBrowserTest(BrowserTest entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1  = dbcmd.CreateParameter();
            IDbDataParameter param2  = dbcmd.CreateParameter();
            IDbDataParameter param3  = dbcmd.CreateParameter();
            IDbDataParameter param4  = dbcmd.CreateParameter();
            IDbDataParameter param5  = dbcmd.CreateParameter();
            IDbDataParameter param6  = dbcmd.CreateParameter();
            IDbDataParameter param7  = dbcmd.CreateParameter();
            IDbDataParameter param8  = dbcmd.CreateParameter();
            IDbDataParameter param9  = dbcmd.CreateParameter();
            IDbDataParameter param10 = dbcmd.CreateParameter();
            IDbDataParameter param11 = dbcmd.CreateParameter();
            IDbDataParameter param12 = dbcmd.CreateParameter();
            IDbDataParameter param13 = dbcmd.CreateParameter();
            IDbDataParameter param14 = dbcmd.CreateParameter();
            IDbDataParameter param15 = dbcmd.CreateParameter();
            IDbDataParameter param16 = dbcmd.CreateParameter();

            param1.ParameterName  = "@idstring";
            param2.ParameterName  = "@browser";
            param3.ParameterName  = "@version";
            param4.ParameterName  = "@os";
            param5.ParameterName  = "@platform";
            param6.ParameterName  = "@gif87";
            param7.ParameterName  = "@gif89";
            param8.ParameterName  = "@jpeg";
            param9.ParameterName  = "@png";
            param10.ParameterName = "@pngt";
            param11.ParameterName = "@agif";
            param12.ParameterName = "@table";
            param13.ParameterName = "@colors";
            param14.ParameterName = "@js";
            param15.ParameterName = "@frames";
            param16.ParameterName = "@flash";

            param1.DbType  = DbType.String;
            param2.DbType  = DbType.String;
            param3.DbType  = DbType.String;
            param4.DbType  = DbType.String;
            param5.DbType  = DbType.String;
            param6.DbType  = DbType.Boolean;
            param7.DbType  = DbType.Boolean;
            param8.DbType  = DbType.Boolean;
            param9.DbType  = DbType.Boolean;
            param10.DbType = DbType.Boolean;
            param11.DbType = DbType.Boolean;
            param12.DbType = DbType.Boolean;
            param13.DbType = DbType.Boolean;
            param14.DbType = DbType.Boolean;
            param15.DbType = DbType.Boolean;
            param16.DbType = DbType.Boolean;

            param1.Value  = entry.UserAgent;
            param2.Value  = entry.Name;
            param3.Value  = entry.Version;
            param4.Value  = entry.OperatingSystem;
            param5.Value  = entry.Architecture;
            param6.Value  = entry.Gif87;
            param7.Value  = entry.Gif89;
            param8.Value  = entry.Jpeg;
            param9.Value  = entry.Png;
            param10.Value = entry.AlphaPng;
            param11.Value = entry.AnimatedGif;
            param12.Value = entry.Tables;
            param13.Value = entry.Color;
            param14.Value = entry.Js;
            param15.Value = entry.Frames;
            param16.Value = entry.Flash;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);
            dbcmd.Parameters.Add(param3);
            dbcmd.Parameters.Add(param4);
            dbcmd.Parameters.Add(param5);
            dbcmd.Parameters.Add(param6);
            dbcmd.Parameters.Add(param7);
            dbcmd.Parameters.Add(param8);
            dbcmd.Parameters.Add(param9);
            dbcmd.Parameters.Add(param10);
            dbcmd.Parameters.Add(param11);
            dbcmd.Parameters.Add(param12);
            dbcmd.Parameters.Add(param13);
            dbcmd.Parameters.Add(param14);
            dbcmd.Parameters.Add(param15);
            dbcmd.Parameters.Add(param16);

            return dbcmd;
        }

        static List<BrowserTest> BrowserTestsFromDataTable(DataTable dataTable)
        {
            List<BrowserTest> entries = new List<BrowserTest>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                BrowserTest entry = new BrowserTest
                {
                    Id              = ushort.Parse(dataRow["id"].ToString()),
                    UserAgent       = dataRow["idstring"].ToString(),
                    Name            = dataRow["browser"].ToString(),
                    Version         = dataRow["version"].ToString(),
                    OperatingSystem = dataRow["os"].ToString(),
                    Architecture    = dataRow["platform"].ToString(),
                    Gif87           = int.Parse(dataRow["gif87"].ToString()) > 0,
                    Gif89           = int.Parse(dataRow["gif89"].ToString()) > 0,
                    Jpeg            = int.Parse(dataRow["jpeg"].ToString()) > 0,
                    Png             = int.Parse(dataRow["png"].ToString()) > 0,
                    AlphaPng        = int.Parse(dataRow["pngt"].ToString()) > 0,
                    AnimatedGif     = int.Parse(dataRow["agif"].ToString()) > 0,
                    Tables          = int.Parse(dataRow["table"].ToString()) > 0,
                    Color           = int.Parse(dataRow["colors"].ToString()) > 0,
                    Js              = int.Parse(dataRow["js"].ToString()) > 0,
                    Frames          = int.Parse(dataRow["frames"].ToString()) > 0,
                    Flash           = int.Parse(dataRow["flash"].ToString()) > 0
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}