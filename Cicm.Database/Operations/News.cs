/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : News.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage site news.
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
        ///     Gets all site news
        /// </summary>
        /// <param name="entries">All site news</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetNews(out List<News> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all news...");
            #endif

            try
            {
                const string SQL = "SELECT * from news";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = NewsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting news.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of site news since the specified start
        /// </summary>
        /// <param name="entries">List of site news</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetNews(out List<News> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} news from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM news LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = NewsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting news.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets site news by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Site news with specified id, <c>null</c> if not found or error</returns>
        public News GetNews(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting news with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from news WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<News> entries = NewsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting news.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of site news in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountNews()
        {
            #if DEBUG
            Console.WriteLine("Counting news...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM news";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new site news to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddNews(News entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding news `{0}`...", entry.Date);
            #endif

            IDbCommand     dbcmd = GetCommandNews(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO news (date, type, added_id)" + " VALUES (@date, @type, @added_id)";

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
        ///     Updated a site news in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateNews(News entry)
        {
            #if DEBUG
            Console.WriteLine("Updating news `{0}`...", entry.Date);
            #endif

            IDbCommand     dbcmd = GetCommandNews(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE news SET date = @date, type = @type, added_id = @added_id " + $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a site news entry from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveNews(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing news widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM news WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandNews(News entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();
            IDbDataParameter param3 = dbcmd.CreateParameter();

            param1.ParameterName = "@date";
            param2.ParameterName = "@type";
            param3.ParameterName = "@added_id";

            param1.DbType = DbType.String;
            param2.DbType = DbType.Int32;
            param3.DbType = DbType.Int32;

            param1.Value = entry.Date;
            param2.Value = entry.Type;
            param3.Value = entry.AffectedId;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);
            dbcmd.Parameters.Add(param3);

            return dbcmd;
        }

        static List<News> NewsFromDataTable(DataTable dataTable)
        {
            List<News> entries = new List<News>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                News entry = new News
                {
                    Id         = int.Parse(dataRow["id"].ToString()),
                    Date       = dataRow["date"].ToString(),
                    Type       = (NewsType)int.Parse(dataRow["type"].ToString()),
                    AffectedId = int.Parse(dataRow["added_id"].ToString())
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}