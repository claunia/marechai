/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Iso3166.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage iso3166_1_numeric.
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
        ///     Gets all ISO 3166-1 codes
        /// </summary>
        /// <param name="entries">All ISO 3166-1 codes</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetIso3166(out List<Iso3166> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all ISO 3166-1 codes...");
            #endif

            try
            {
                const string SQL = "SELECT * from iso3166_1_numeric";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = Iso3166FromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting ISO 3166-1 codes.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of ISO 3166-1 codes since the specified start
        /// </summary>
        /// <param name="entries">List of ISO 3166-1 codes</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetIso3166(out List<Iso3166> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} ISO 3166-1 codes from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM iso3166_1_numeric LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = Iso3166FromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting ISO 3166-1 codes.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets ISO 3166-1 code by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>ISO 3166-1 code with specified id, <c>null</c> if not found or error</returns>
        public Iso3166 GetIso3166(short id)
        {
            #if DEBUG
            Console.WriteLine("Getting ISO 3166-1 code with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from iso3166_1_numeric WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Iso3166> entries = Iso3166FromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting ISO 3166-1 code.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of ISO 3166-1 codes in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountIso3166()
        {
            #if DEBUG
            Console.WriteLine("Counting ISO 3166-1 codes...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM iso3166_1_numeric";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new ISO 3166-1 code to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddIso3166(Iso3166 entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding ISO 3166-1 code `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandIso3166(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO iso3166_1_numeric (name)" + " VALUES (@name)";

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
        ///     Updates an ISO 3166-1 code in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateIso3166(Iso3166 entry)
        {
            #if DEBUG
            Console.WriteLine("Updating ISO 3166-1 code `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandIso3166(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE iso3166_1_numeric SET name = @name " + $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes an ISO 3166-1 code from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveIso3166(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing ISO 3166-1 code `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM iso3166_1_numeric WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandIso3166(Iso3166 entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();

            param1.ParameterName = "@name";

            param1.DbType = DbType.String;

            param1.Value = entry.Name;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<Iso3166> Iso3166FromDataTable(DataTable dataTable)
        {
            List<Iso3166> entries = new List<Iso3166>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Iso3166 entry = new Iso3166 {Id = (short)dataRow["id"], Name = (string)dataRow["name"]};

                entries.Add(entry);
            }

            return entries;
        }
    }
}