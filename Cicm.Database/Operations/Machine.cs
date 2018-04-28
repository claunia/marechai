/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Machine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage machines.
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
        ///     Gets all machines
        /// </summary>
        /// <param name="entries">All machines</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetMachines(out List<Machine> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all machines...");
            #endif

            try
            {
                const string SQL = "SELECT * from machines";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting machines.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets all machines from specified company
        /// </summary>
        /// <param name="entries">All machines</param>
        /// <param name="company">Company id</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetMachines(out List<Machine> entries, int company)
        {
            #if DEBUG
            Console.WriteLine("Getting all machines from company id {0}...", company);
            #endif

            try
            {
                string sql = $"SELECT * from machines WHERE company = '{company}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting machines.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of machines since the specified start
        /// </summary>
        /// <param name="entries">List of machines</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetMachines(out List<Machine> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} machines from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM machines LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MachinesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting machines.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets machine by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Machine with specified id, <c>null</c> if not found or error</returns>
        public Machine GetMachine(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting machine with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from machines WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Machine> entries = MachinesFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting machine.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of machines in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountMachines()
        {
            #if DEBUG
            Console.WriteLine("Counting machines...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM machines";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new administrator to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddMachine(Machine entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding machine `{0}`...", entry.Model);
            #endif

            IDbCommand     dbcmd = GetCommandMachine(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL =
                "INSERT INTO machines (company, year, model, ram, rom, vram, colors, res, sound_synth, music_synth, " +
                "sound_channels, music_channels, hdd1, hdd2, hdd3, disk1, cap1, disk2, cap2, type)"                        +
                " VALUES (@company, @year, @model, @ram, @rom, @vram, @colors, @res, @sound_synth, @music_synth, "   +
                "@sound_channels, @music_channels, @hdd1, @hdd2, @hdd3, @disk1, @cap1, @disk2, @cap2, @type)";

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
        ///     Updated a machine in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateMachine(Machine entry)
        {
            #if DEBUG
            Console.WriteLine("Updating machine `{0}`...", entry.Model);
            #endif

            IDbCommand     dbcmd = GetCommandMachine(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql =
                "UPDATE machines SET company = @company, year = @year, model = @model, ram = @ram, rom = @rom, "         +
                "vram = @vram, colors = @colors, res = @res, sound_synth = @sound_synth, "                   +
                "music_synth = @music_synth, sound_channels = @sound_channels, music_channels = @music_channels, "       +
                "hdd1 = @hdd1, hdd2 = @hdd2, hdd3 = @hdd3, disk1 = @disk1, cap1 = @cap1, disk2 = @disk2, cap2 = @cap2, " +
                "type = @type "                                                                                          +
                $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a machine from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveMachine(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing machine widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM machines WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandMachine(Machine entry)
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
            IDbDataParameter param9 = dbcmd.CreateParameter();
            IDbDataParameter param10 = dbcmd.CreateParameter();
            IDbDataParameter param11 = dbcmd.CreateParameter();
            IDbDataParameter param12 = dbcmd.CreateParameter();
            IDbDataParameter param13 = dbcmd.CreateParameter();
            IDbDataParameter param14 = dbcmd.CreateParameter();
            IDbDataParameter param15 = dbcmd.CreateParameter();
            IDbDataParameter param16 = dbcmd.CreateParameter();
            IDbDataParameter param17 = dbcmd.CreateParameter();
            IDbDataParameter param18 = dbcmd.CreateParameter();
            IDbDataParameter param19 = dbcmd.CreateParameter();
            IDbDataParameter param20 = dbcmd.CreateParameter();

            param1.ParameterName  = "@company";
            param2.ParameterName  = "@year";
            param3.ParameterName  = "@model";
            param4.ParameterName  = "@ram";
            param5.ParameterName  = "@rom";
            param6.ParameterName  = "@vram";
            param7.ParameterName  = "@colors";
            param8.ParameterName  = "@res";
            param9.ParameterName = "@sound_synth";
            param10.ParameterName = "@music_synth";
            param11.ParameterName = "@sound_channels";
            param12.ParameterName = "@music_channels";
            param13.ParameterName = "@hdd1";
            param14.ParameterName = "@hdd2";
            param15.ParameterName = "@hdd3";
            param16.ParameterName = "@disk1";
            param17.ParameterName = "@cap1";
            param18.ParameterName = "@disk2";
            param19.ParameterName = "@cap2";
            param20.ParameterName = "@type";

            param1.DbType  = DbType.Int32;
            param2.DbType  = DbType.Int32;
            param3.DbType  = DbType.String;
            param4.DbType  = DbType.Int32;
            param5.DbType  = DbType.Int32;
            param6.DbType  = DbType.Int32;
            param7.DbType  = DbType.Int32;
            param8.DbType  = DbType.String;
            param9.DbType = DbType.Int32;
            param10.DbType = DbType.Int32;
            param11.DbType = DbType.Int32;
            param12.DbType = DbType.Int32;
            param13.DbType = DbType.Int32;
            param14.DbType = DbType.Int32;
            param15.DbType = DbType.Int32;
            param16.DbType = DbType.Int32;
            param17.DbType = DbType.String;
            param18.DbType = DbType.Int32;
            param19.DbType = DbType.String;
            param20.DbType = DbType.Int32;

            param1.Value  = entry.Company;
            param2.Value  = entry.Year;
            param3.Value  = entry.Model;
            param4.Value  = entry.Ram;
            param5.Value  = entry.Rom;
            param6.Value  = entry.Vram;
            param7.Value  = entry.Colors;
            param8.Value  = entry.Resolution;
            param9.Value = entry.SoundSynth;
            param10.Value = entry.MusicSynth;
            param11.Value = entry.SoundChannels;
            param12.Value = entry.MusicChannels;
            param13.Value = entry.Hdd1;
            param14.Value = entry.Hdd2;
            param15.Value = entry.Hdd3;
            param16.Value = entry.Disk1;
            param17.Value = entry.Cap1;
            param18.Value = entry.Disk2;
            param19.Value = entry.Cap2;
            param20.Value = entry.Type;

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
            dbcmd.Parameters.Add(param17);
            dbcmd.Parameters.Add(param18);
            dbcmd.Parameters.Add(param19);
            dbcmd.Parameters.Add(param20);

            return dbcmd;
        }

        static List<Machine> MachinesFromDataTable(DataTable dataTable)
        {
            List<Machine> entries = new List<Machine>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Machine entry = new Machine
                {
                    Id            = (int)dataRow["id"],
                    Company       = (int)dataRow["company"],
                    Year          = (int)dataRow["year"],
                    Model         = (string)dataRow["model"],
                    Ram           = (int)dataRow["ram"],
                    Rom           = (int)dataRow["rom"],
                    Vram          = (int)dataRow["vram"],
                    Colors        = (int)dataRow["colors"],
                    Resolution    = (string)dataRow["res"],
                    SoundSynth    = (int)dataRow["sound_synth"],
                    MusicSynth    = (int)dataRow["music_synth"],
                    SoundChannels = (int)dataRow["sound_channels"],
                    MusicChannels = (int)dataRow["music_channels"],
                    Hdd1          = (int)dataRow["hdd1"],
                    Hdd2          = dataRow["hdd2"] == DBNull.Value ? 0 : (int)dataRow["hdd2"],
                    Hdd3          = dataRow["hdd3"] == DBNull.Value ? 0 : (int)dataRow["hdd3"],
                    Disk1         = (int)dataRow["disk1"],
                    Cap1          = (string)dataRow["cap1"],
                    Disk2         = dataRow["disk2"] == DBNull.Value ? 0 : (int)dataRow["disk2"],
                    Cap2          = dataRow["cap2"]  == DBNull.Value ? null : (string)dataRow["cap2"],
                    Type          = (MachineType)dataRow["type"]
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}