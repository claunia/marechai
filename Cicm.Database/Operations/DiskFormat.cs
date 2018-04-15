/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : DiskFormat.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage disk formats.
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
        ///     Gets all disk formats
        /// </summary>
        /// <param name="entries">All disk formats</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetDiskFormats(out List<DiskFormat> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all disk formats...");
            #endif

            try
            {
                const string SQL = "SELECT * from disk_formats";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = DiskFormatsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting disk formats.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of disk formats since the specified start
        /// </summary>
        /// <param name="entries">List of disk formats</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetDiskFormats(out List<DiskFormat> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} disk formats from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM disk_formats LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = DiskFormatsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting disk formats.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets disk format by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Disk format with specified id, <c>null</c> if not found or error</returns>
        public DiskFormat GetDiskFormat(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting disk format with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from disk_formats WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<DiskFormat> entries = DiskFormatsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting disk format.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of disk formats in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountDiskFormats()
        {
            #if DEBUG
            Console.WriteLine("Counting disk formats...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM disk_formats";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new disk format to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddDiskFormat(DiskFormat entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding disk format `{0}`...", entry.Description);
            #endif

            IDbCommand     dbcmd = GetCommandDiskFormat(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO disk_formats (description)" + " VALUES (@description)";

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
        ///     Updated a disk format in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateDiskFormat(DiskFormat entry)
        {
            #if DEBUG
            Console.WriteLine("Updating disk format `{0}`...", entry.Description);
            #endif

            IDbCommand     dbcmd = GetCommandDiskFormat(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE disk_formats SET description = @description " + $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a disk format from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveDiskFormat(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing disk format widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM disk_formats WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandDiskFormat(DiskFormat entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();

            param1.ParameterName = "@description";

            param1.DbType = DbType.String;

            param1.Value = entry.Description;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<DiskFormat> DiskFormatsFromDataTable(DataTable dataTable)
        {
            List<DiskFormat> entries = new List<DiskFormat>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                DiskFormat entry = new DiskFormat
                {
                    Id          = int.Parse(dataRow["id"].ToString()),
                    Description = dataRow["description"].ToString()
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}