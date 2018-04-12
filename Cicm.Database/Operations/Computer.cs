﻿/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Computer.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage computers.
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
        ///     Gets all computers
        /// </summary>
        /// <param name="entries">All computers</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetComputers(out List<Computer> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all computers...");
            #endif

            try
            {
                const string SQL = "SELECT * from computers";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ComputersFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting computers.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of computers since the specified start
        /// </summary>
        /// <param name="entries">List of computers</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetComputers(out List<Computer> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} computers from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM computers LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ComputersFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting computers.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets computer by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Computer with specified id, <c>null</c> if not found or error</returns>
        public Computer GetComputer(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting computer with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from computers WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Computer> entries = ComputersFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting computer.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of computers in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountComputers()
        {
            #if DEBUG
            Console.WriteLine("Counting computers...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM computers";
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
        public bool AddComputer(Computer entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding computer `{0}`...", entry.model);
            #endif

            IDbCommand     dbcmd = GetCommandComputer(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL =
                "INSERT INTO computers (company, year, model, cpu1, mhz1, cpu2, mhz2, bits, ram, rom, gpu, vram, colors, res, spu, mpu, sound_channels, music_channels, hdd1, hdd2, hdd3, disk1, cap1, disk2, cap2, comment)" +
                " VALUES (@company, @year, @model, @cpu1, @mhz1, @cpu2, @mhz2, @bits, @ram, @rom, @gpu, @vram, @colors, @res, @spu, @mpu, @sound_channels, @music_channels, @hdd1, @hdd2, @hdd3, @disk1, @cap1, @disk2, @cap2, @comment)";

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
        ///     Updated a computer in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateComputer(Computer entry)
        {
            #if DEBUG
            Console.WriteLine("Updating computer `{0}`...", entry.model);
            #endif

            IDbCommand     dbcmd = GetCommandComputer(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql =
                "UPDATE computers SET company = @company, year = @year, model = @model, cpu1 = @cpu1, mhz1 = @mhz1, cpu2 = @cpu2, "                                                                             +
                "mhz2 = @mhz2, bits = @bits, ram = @ram, rom = @rom, gpu = @gpu, vram = @vram, colors = @colors, res = @res, spu = @spu, mpu = @mpu "                                                           +
                "sound_channels = @sound_channels, music_channels = @music_channels, hdd1 = @hdd1, hdd2 = @hdd2, hdd3 = @hdd3, disk1 = @disk1, cap1 = @cap1, disk2 = @disk2, cap2 = @cap2, comment = @comment " +
                $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a computer from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveComputer(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing computer widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM computers WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandComputer(Computer entry)
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
            IDbDataParameter param17 = dbcmd.CreateParameter();
            IDbDataParameter param18 = dbcmd.CreateParameter();
            IDbDataParameter param19 = dbcmd.CreateParameter();
            IDbDataParameter param20 = dbcmd.CreateParameter();
            IDbDataParameter param21 = dbcmd.CreateParameter();
            IDbDataParameter param22 = dbcmd.CreateParameter();
            IDbDataParameter param23 = dbcmd.CreateParameter();
            IDbDataParameter param24 = dbcmd.CreateParameter();
            IDbDataParameter param25 = dbcmd.CreateParameter();
            IDbDataParameter param26 = dbcmd.CreateParameter();

            param1.ParameterName  = "@company";
            param2.ParameterName  = "@year";
            param3.ParameterName  = "@model";
            param4.ParameterName  = "@cpu1";
            param5.ParameterName  = "@mhz1";
            param6.ParameterName  = "@cpu2";
            param7.ParameterName  = "@mhz2";
            param8.ParameterName  = "@bits";
            param9.ParameterName  = "@ram";
            param10.ParameterName = "@rom";
            param11.ParameterName = "@gpu";
            param12.ParameterName = "@vram";
            param13.ParameterName = "@colors";
            param14.ParameterName = "@res";
            param15.ParameterName = "@spu";
            param16.ParameterName = "@mpu";
            param17.ParameterName = "@sound_channels";
            param18.ParameterName = "@music_channels";
            param19.ParameterName = "@hdd1";
            param20.ParameterName = "@hdd2";
            param21.ParameterName = "@hdd3";
            param22.ParameterName = "@disk1";
            param23.ParameterName = "@cap1";
            param24.ParameterName = "@disk2";
            param25.ParameterName = "@cap2";
            param26.ParameterName = "@comment";

            param1.DbType  = DbType.Int32;
            param2.DbType  = DbType.Int32;
            param3.DbType  = DbType.String;
            param4.DbType  = DbType.Int32;
            param5.DbType  = DbType.Double;
            param6.DbType  = DbType.Int32;
            param7.DbType  = DbType.Double;
            param8.DbType  = DbType.Int32;
            param9.DbType  = DbType.Int32;
            param10.DbType = DbType.Int32;
            param11.DbType = DbType.Int32;
            param12.DbType = DbType.Int32;
            param13.DbType = DbType.Int32;
            param14.DbType = DbType.String;
            param15.DbType = DbType.Int32;
            param16.DbType = DbType.Int32;
            param17.DbType = DbType.Int32;
            param18.DbType = DbType.Int32;
            param19.DbType = DbType.Int32;
            param20.DbType = DbType.Int32;
            param21.DbType = DbType.Int32;
            param22.DbType = DbType.Int32;
            param23.DbType = DbType.String;
            param24.DbType = DbType.Int32;
            param25.DbType = DbType.String;
            param26.DbType = DbType.String;

            param1.Value  = entry.company;
            param2.Value  = entry.year;
            param3.Value  = entry.model;
            param4.Value  = entry.cpu1;
            param5.Value  = entry.mhz1;
            param6.Value  = entry.cpu2;
            param7.Value  = entry.mhz2;
            param8.Value  = entry.bits;
            param9.Value  = entry.ram;
            param10.Value = entry.rom;
            param11.Value = entry.gpu;
            param12.Value = entry.vram;
            param13.Value = entry.colors;
            param14.Value = entry.res;
            param15.Value = entry.spu;
            param16.Value = entry.mpu;
            param17.Value = entry.sound_channels;
            param18.Value = entry.music_channels;
            param19.Value = entry.hdd1;
            param20.Value = entry.hdd2;
            param21.Value = entry.hdd3;
            param22.Value = entry.disk1;
            param23.Value = entry.cap1;
            param24.Value = entry.disk2;
            param25.Value = entry.cap2;
            param26.Value = entry.comment;

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
            dbcmd.Parameters.Add(param21);
            dbcmd.Parameters.Add(param22);
            dbcmd.Parameters.Add(param23);
            dbcmd.Parameters.Add(param24);
            dbcmd.Parameters.Add(param25);
            dbcmd.Parameters.Add(param26);

            return dbcmd;
        }

        static List<Computer> ComputersFromDataTable(DataTable dataTable)
        {
            List<Computer> entries = new List<Computer>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Computer entry = new Computer
                {
                    id             = int.Parse(dataRow["id"].ToString()),
                    company        = int.Parse(dataRow["company"].ToString()),
                    year           = int.Parse(dataRow["year"].ToString()),
                    model          = dataRow["model"].ToString(),
                    cpu1           = int.Parse(dataRow["cpu1"].ToString()),
                    mhz1           = float.Parse(dataRow["mhz1"].ToString()),
                    cpu2           = int.Parse(dataRow["cpu2"].ToString()),
                    mhz2           = float.Parse(dataRow["mhz2"].ToString()),
                    bits           = int.Parse(dataRow["bits"].ToString()),
                    ram            = int.Parse(dataRow["ram"].ToString()),
                    rom            = int.Parse(dataRow["rom"].ToString()),
                    gpu            = int.Parse(dataRow["gpu"].ToString()),
                    vram           = int.Parse(dataRow["vram"].ToString()),
                    colors         = int.Parse(dataRow["colors"].ToString()),
                    res            = dataRow["res"].ToString(),
                    spu            = int.Parse(dataRow["spu"].ToString()),
                    mpu            = int.Parse(dataRow["mpu"].ToString()),
                    sound_channels = int.Parse(dataRow["sound_channels"].ToString()),
                    music_channels = int.Parse(dataRow["music_channels"].ToString()),
                    hdd1           = int.Parse(dataRow["hdd1"].ToString()),
                    hdd2           = int.Parse(dataRow["hdd2"].ToString()),
                    hdd3           = int.Parse(dataRow["hdd3"].ToString()),
                    disk1          = int.Parse(dataRow["disk1"].ToString()),
                    cap1           = dataRow["cap1"].ToString(),
                    disk2          = int.Parse(dataRow["disk2"].ToString()),
                    cap2           = dataRow["cap2"].ToString(),
                    comment        = dataRow["comment"].ToString()
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}