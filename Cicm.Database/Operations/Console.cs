/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Console.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage videogame consoles.
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
using Console = Cicm.Database.Schemas.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
        /// <summary>
        ///     Gets all consoles
        /// </summary>
        /// <param name="entries">All consoles</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetConsoles(out List<Console> entries)
        {
            #if DEBUG
            System.Console.WriteLine("Getting all consoles...");
            #endif

            try
            {
                const string SQL = "SELECT * from consoles";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ConsolesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                System.Console.WriteLine("Error getting consoles.");
                System.Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of videogame consoles since the specified start
        /// </summary>
        /// <param name="entries">List of videogame consoles</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetConsoles(out List<Console> entries, ulong start, ulong count)
        {
            #if DEBUG
            System.Console.WriteLine("Getting {0} consoles from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM consoles LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ConsolesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                System.Console.WriteLine("Error getting consoles.");
                System.Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets videogame console by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Videogame console with specified id, <c>null</c> if not found or error</returns>
        public Console GetConsole(int id)
        {
            #if DEBUG
            System.Console.WriteLine("Getting console with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from consoles WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Console> entries = ConsolesFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                System.Console.WriteLine("Error getting console.");
                System.Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of videogame consoles in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountConsoles()
        {
            #if DEBUG
            System.Console.WriteLine("Counting consoles...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM consoles";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new videogame console to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddConsole(Console entry, out long id)
        {
            #if DEBUG
            System.Console.Write("Adding console `{0}`...", entry.name);
            #endif

            IDbCommand     dbcmd = GetCommandConsole(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL =
                "INSERT INTO consoles (company, year, name, cpu1, mhz1, cpu2, mhz2, bits, ram, rom, gpu, vram, colors, res, spu, mpu, schannels, mchannels, palette, format, cap, comments)" +
                " VALUES (@company, @year, @name, @cpu1, @mhz1, @cpu2, @mhz2, @bits, @ram, @rom, @gpu, @vram, @colors, @res, @spu, @mpu, @schannels, @mchannels, @palette, @format, @cap, @comments)";

            dbcmd.CommandText = SQL;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            id = dbCore.LastInsertRowId;

            #if DEBUG
            System.Console.WriteLine(" id {0}", id);
            #endif

            return true;
        }

        /// <summary>
        ///     Updated an videogame console in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateConsole(Console entry)
        {
            #if DEBUG
            System.Console.WriteLine("Updating console `{0}`...", entry.name);
            #endif

            IDbCommand     dbcmd = GetCommandConsole(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql =
                "UPDATE consoles SET company = @company, year = @year, name = @name, cpu1 = @cpu1, mhz1 = @mhz1, cpu2 = @cpu2, "                      +
                "mhz2 = @mhz2, bits = @bits, ram = @ram, rom = @rom, gpu = @gpu, vram = @vram, colors = @colors, res = @res, spu = @spu, mpu = @mpu " +
                "schannels = @schannels, mchannels = @mchannels, palette = @palette, format = @format, cap = @cap, comments = @comments "             +
                $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a videogame console from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveConsole(long id)
        {
            #if DEBUG
            System.Console.WriteLine("Removing console widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM consoles WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandConsole(Console entry)
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

            param1.ParameterName  = "@company";
            param2.ParameterName  = "@year";
            param3.ParameterName  = "@name";
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
            param17.ParameterName = "@schannels";
            param18.ParameterName = "@mchannels";
            param19.ParameterName = "@palette";
            param20.ParameterName = "@format";
            param21.ParameterName = "@cap";
            param22.ParameterName = "@comments";

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
            param22.DbType = DbType.String;

            param1.Value  = entry.company;
            param2.Value  = entry.year;
            param3.Value  = entry.name;
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
            param17.Value = entry.schannels;
            param18.Value = entry.mchannels;
            param19.Value = entry.palette;
            param20.Value = entry.format;
            param21.Value = entry.cap;
            param22.Value = entry.comments;

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

            return dbcmd;
        }

        static List<Console> ConsolesFromDataTable(DataTable dataTable)
        {
            List<Console> entries = new List<Console>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Console entry = new Console
                {
                    id        = int.Parse(dataRow["id"].ToString()),
                    company   = int.Parse(dataRow["company"].ToString()),
                    year      = int.Parse(dataRow["year"].ToString()),
                    name      = dataRow["name"].ToString(),
                    cpu1      = int.Parse(dataRow["cpu1"].ToString()),
                    mhz1      = float.Parse(dataRow["mhz1"].ToString()),
                    cpu2      = int.Parse(dataRow["cpu2"].ToString()),
                    mhz2      = float.Parse(dataRow["mhz2"].ToString()),
                    bits      = int.Parse(dataRow["bits"].ToString()),
                    ram       = int.Parse(dataRow["ram"].ToString()),
                    rom       = int.Parse(dataRow["rom"].ToString()),
                    gpu       = int.Parse(dataRow["gpu"].ToString()),
                    vram      = int.Parse(dataRow["vram"].ToString()),
                    colors    = int.Parse(dataRow["colors"].ToString()),
                    res       = dataRow["res"].ToString(),
                    spu       = int.Parse(dataRow["spu"].ToString()),
                    mpu       = int.Parse(dataRow["mpu"].ToString()),
                    schannels = int.Parse(dataRow["schannels"].ToString()),
                    mchannels = int.Parse(dataRow["mchannels"].ToString()),
                    palette   = int.Parse(dataRow["palette"].ToString()),
                    format    = int.Parse(dataRow["format"].ToString()),
                    cap       = int.Parse(dataRow["cap"].ToString()),
                    comments  = dataRow["comments"].ToString()
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}