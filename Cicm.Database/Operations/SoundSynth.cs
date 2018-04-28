/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : SoundSynth.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage sound synthetizers.
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
        ///     Gets all sound synthetizers
        /// </summary>
        /// <param name="entries">All sound synthetizers</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetSoundSynths(out List<SoundSynth> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all sound synthetizers...");
            #endif

            try
            {
                const string SQL = "SELECT * from sound_synths";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = SoundSynthFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting sound synthetizers.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of sound synthetizers since the specified start
        /// </summary>
        /// <param name="entries">List of sound synthetizers</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetSoundSynths(out List<SoundSynth> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} sound synthetizers from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM sound_synths LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = SoundSynthFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting sound synthetizers.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets sound synthetizer by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Sound synthetizer with specified id, <c>null</c> if not found or error</returns>
        public SoundSynth GetSoundSynth(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting sound synthetizer with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from sound_synths WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<SoundSynth> entries = SoundSynthFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting sound synthetizer.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of sound synthetizers in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountSoundSynths()
        {
            #if DEBUG
            Console.WriteLine("Counting sound synthetizers...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM sound_synths";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new sound synthetizer to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddSoundSynth(SoundSynth entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding sound synthetizer `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandSoundSynth(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO sound_synths (name, company, model_code, introduced, voices, frequency, " +
                               "depth, square_wave, white_noise, type) VALUES (@name, @company, @model_code, "        +
                               "@introduced, @voices, @frequency, @depth, @square_wave, @white_noise, @type)";

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
        ///     Updated a sound synthetizer in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateSoundSynth(SoundSynth entry)
        {
            #if DEBUG
            Console.WriteLine("Updating sound synthetizer `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandSoundSynth(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE sound_synths SET name = @name, company = @company, model_code = @model_code, " +
                         "introduced = @introduced, voices = @voices, frequency = @frequency, depth = @depth, " +
                         "square_wave = @square_wave, white_noise = @white_noise, type = @type, "               +
                         $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a sound synthetizer from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveSoundSynth(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing sound synthetizer widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM sound_synths WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandSoundSynth(SoundSynth entry)
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

            param1.ParameterName  = "@name";
            param2.ParameterName  = "@company";
            param3.ParameterName  = "@model_code";
            param4.ParameterName  = "@introduced";
            param5.ParameterName  = "@voices";
            param6.ParameterName  = "@frequency";
            param7.ParameterName  = "@depth";
            param8.ParameterName  = "@square_wave";
            param9.ParameterName  = "@white_noise";
            param10.ParameterName = "@type";

            param1.DbType  = DbType.String;
            param2.DbType  = DbType.Int32;
            param3.DbType  = DbType.String;
            param4.DbType  = DbType.DateTime;
            param5.DbType  = DbType.Int32;
            param6.DbType  = DbType.Double;
            param7.DbType  = DbType.Int32;
            param8.DbType  = DbType.Int32;
            param9.DbType  = DbType.Int32;
            param10.DbType = DbType.Int32;

            param1.Value  = entry.Name;
            param2.Value  = entry.Company?.Id;
            param3.Value  = entry.ModelCode;
            param4.Value  = entry.Introduced == DateTime.MinValue ? (object)null : entry.Introduced;
            param5.Value  = entry.Voices     == 0 ? (object)null : entry.Voices;
            param6.Value  = entry.Frequency  <= 0 ? (object)null : entry.Frequency;
            param7.Value  = entry.Depth      == 0 ? (object)null : entry.Depth;
            param8.Value  = entry.SquareWave == 0 ? (object)null : entry.SquareWave;
            param9.Value  = entry.WhiteNoise == 0 ? (object)null : entry.WhiteNoise;
            param10.Value = entry.Type       == 0 ? (object)null : entry.Type;

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

            return dbcmd;
        }

        List<SoundSynth> SoundSynthFromDataTable(DataTable dataTable)
        {
            List<SoundSynth> entries = new List<SoundSynth>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                SoundSynth entry = new SoundSynth
                {
                    Id         = (int)dataRow["id"],
                    Name       = (string)dataRow["name"],
                    ModelCode  = dataRow["model_code"]  == DBNull.Value ? null : (string)dataRow["model_code"],
                    Voices     = dataRow["voices"]      == DBNull.Value ? 0 : (int)dataRow["voices"],
                    Frequency  = dataRow["frequency"]   == DBNull.Value ? 0 : (double)dataRow["frequency"],
                    Depth      = dataRow["depth"]       == DBNull.Value ? 0 : (int)dataRow["depth"],
                    SquareWave = dataRow["square_wave"] == DBNull.Value ? 0 : (int)dataRow["square_wave"],
                    WhiteNoise = dataRow["white_noise"] == DBNull.Value ? 0 : (int)dataRow["white_noise"],
                    Type       = dataRow["type"]        == DBNull.Value ? 0 : (int)dataRow["type"],
                    Company    = dataRow["company"]     == DBNull.Value ? null : GetCompany((int)dataRow["company"]),
                    Introduced = dataRow["introduced"] == DBNull.Value
                                     ? DateTime.MinValue
                                     : Convert.ToDateTime(dataRow["introduced"])
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}