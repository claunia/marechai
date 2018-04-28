/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Resolution.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage Resolutions.
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
        ///     Gets all Resolutions
        /// </summary>
        /// <param name="entries">All Resolutions</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetResolutions(out List<Resolution> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all Resolutions...");
            #endif

            try
            {
                const string SQL = "SELECT * from resolutions";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ResolutionsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting Resolutions.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of Resolutions since the specified start
        /// </summary>
        /// <param name="entries">List of Resolutions</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetResolutions(out List<Resolution> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} Resolutions from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM resolutions LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ResolutionsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting Resolutions.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets Resolution by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Resolution with specified id, <c>null</c> if not found or error</returns>
        public Resolution GetResolution(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting Resolution with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from resolutions WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Resolution> entries = ResolutionsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting Resolution.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Gets Resolution by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Resolution with specified id, <c>null</c> if not found or error</returns>
        public Resolution GetResolution(int width, int height)
        {
            #if DEBUG
            Console.WriteLine("Getting first resolution of {0}x{1}...", width, height);
            #endif

            try
            {
                string sql = $"SELECT * from resolutions WHERE width = {width} AND height = {height}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Resolution> entries = ResolutionsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting Resolution.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Gets Resolution by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Resolution with specified id, <c>null</c> if not found or error</returns>
        public Resolution GetResolution(int width, int height, long colors)
        {
            #if DEBUG
            Console.WriteLine("Getting first resolution of {0}x{1} with {2} colors...", width, height, colors);
            #endif

            try
            {
                string sql =
                    $"SELECT * from resolutions WHERE width = {width} AND height = {height} AND colors = {colors}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Resolution> entries = ResolutionsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting Resolution.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Gets Resolution by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Resolution with specified id, <c>null</c> if not found or error</returns>
        public Resolution GetResolution(int width, int height, long colors, long palette)
        {
            #if DEBUG
            Console.WriteLine("Getting first resolution of {0}x{1} with {2} colors from a palette of {3} colors...",
                              width, height, colors, palette);
            #endif

            try
            {
                string sql =
                    $"SELECT * from resolutions WHERE width = {width} AND height = {height} AND colors = {colors} AND palette = {palette}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Resolution> entries = ResolutionsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting Resolution.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of Resolutions in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountResolutions()
        {
            #if DEBUG
            Console.WriteLine("Counting resolutions...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM resolutions";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new Resolution to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddResolution(Resolution entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding Resolution...");
            #endif

            IDbCommand     dbcmd = GetCommandResolution(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL =
                "INSERT INTO resolutions (width, height, colors, palette, chars) VALUES (@width, @height, @colors, " +
                "@palette, @chars)";

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
        ///     Updated a Resolution in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateResolution(Resolution entry)
        {
            #if DEBUG
            Console.WriteLine("Updating Resolution `{0}`...", entry.Id);
            #endif

            IDbCommand     dbcmd = GetCommandResolution(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql =
                "UPDATE resolutions SET width = @width, height = @height, colors = @colors, palette = @palette, " +
                "chars = @chars "                                                                                 +
                $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a Resolution from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveResolution(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing Resolution widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM resolutions WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandResolution(Resolution entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();
            IDbDataParameter param3 = dbcmd.CreateParameter();
            IDbDataParameter param4 = dbcmd.CreateParameter();
            IDbDataParameter param5 = dbcmd.CreateParameter();

            param1.ParameterName = "@width";
            param2.ParameterName = "@height";
            param3.ParameterName = "@colors";
            param4.ParameterName = "@palette";
            param5.ParameterName = "@chars";

            param1.DbType = DbType.Int32;
            param2.DbType = DbType.Int32;
            param3.DbType = DbType.Int64;
            param4.DbType = DbType.Int64;
            param5.DbType = DbType.Boolean;

            param1.Value = entry.Width;
            param2.Value = entry.Height;
            param3.Value = entry.Colors  == 0 ? (object)null : entry.Colors;
            param4.Value = entry.Palette == 0 ? (object)null : entry.Palette;
            param5.Value = entry.Chars;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);
            dbcmd.Parameters.Add(param3);
            dbcmd.Parameters.Add(param4);
            dbcmd.Parameters.Add(param5);

            return dbcmd;
        }

        static List<Resolution> ResolutionsFromDataTable(DataTable dataTable)
        {
            List<Resolution> entries = new List<Resolution>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Resolution entry = new Resolution
                {
                    Id      = (int)dataRow["id"],
                    Width   = (int)dataRow["width"],
                    Height  = (int)dataRow["height"],
                    Colors  = dataRow["colors"]  == DBNull.Value ? 0 : (long)dataRow["colors"],
                    Palette = dataRow["palette"] == DBNull.Value ? 0 : (long)dataRow["palette"],
                    Chars   = (bool)dataRow["chars"]
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}