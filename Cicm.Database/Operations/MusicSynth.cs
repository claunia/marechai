/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : MusicSynth.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage music synthetizer.
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
        ///     Gets all music synthetizers
        /// </summary>
        /// <param name="entries">All music synthetizers</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetMusicSynths(out List<MusicSynth> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all music synthetizers...");
            #endif

            try
            {
                const string SQL = "SELECT * from music_synths";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MusicSynthFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting music synthetizers.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of music synthetizers since the specified start
        /// </summary>
        /// <param name="entries">List of music synthetizers</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetMusicSynths(out List<MusicSynth> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} music synthetizers from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM music_synths LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MusicSynthFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting music synthetizers.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets music synthetizer by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>music synthetizer with specified id, <c>null</c> if not found or error</returns>
        public MusicSynth GetMusicSynth(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting music synthetizer with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from music_synths WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<MusicSynth> entries = MusicSynthFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting music synthetizer.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of music synthetizers in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountMusicSynths()
        {
            #if DEBUG
            Console.WriteLine("Counting mpus...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM music_synths";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new music synthetizer to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddMusicSynth(MusicSynth entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding music synthetizer `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandMusicSynth(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO music_synths (name)" + " VALUES (@name)";

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
        ///     Updated an music synthetizer in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateMusicSynth(MusicSynth entry)
        {
            #if DEBUG
            Console.WriteLine("Updating music synthetizer `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandMusicSynth(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE music_synths SET name = @name " + $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        public bool RemoveMusicSynth(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing music synthetizer widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM music_synths WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandMusicSynth(MusicSynth entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();

            param1.ParameterName = "@name";

            param1.DbType = DbType.String;

            param1.Value = entry.Name;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<MusicSynth> MusicSynthFromDataTable(DataTable dataTable)
        {
            List<MusicSynth> entries = new List<MusicSynth>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                MusicSynth entry = new MusicSynth
                {
                    Id   = int.Parse(dataRow["id"].ToString()),
                    Name = dataRow["name"].ToString()
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}