/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : OwnConsole.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage owned computers.
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
        ///     Gets all owned computers
        /// </summary>
        /// <param name="entries">All owned computers</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetOwnOwnComputers(out List<OwnComputer> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all owned computers...");
            #endif

            try
            {
                const string SQL = "SELECT * from own_computer";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = OwnOwnComputersFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting owned computers.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of owned computers since the specified start
        /// </summary>
        /// <param name="entries">List of owned computers</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetOwnOwnComputers(out List<OwnComputer> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} owned computers from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM own_computer LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = OwnOwnComputersFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting owned computers.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets owned computer by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Owned computer with specified id, <c>null</c> if not found or error</returns>
        public OwnComputer GetOwnComputer(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting owned computer with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from own_computer WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<OwnComputer> entries = OwnOwnComputersFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting owned computer.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of owned computers in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountOwnOwnComputers()
        {
            #if DEBUG
            Console.WriteLine("Counting owned computers...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM own_computer";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new owned computer to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddOwnComputer(OwnComputer entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding owned computer `{0}`...", entry.db_id);
            #endif

            IDbCommand     dbcmd = GetCommandOwnComputer(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL =
                "INSERT INTO own_computer (db_id, date, status, trade, boxed, manuals, cpu1, mhz1, cpu2, mhz2, ram, vram, rigid, disk1, cap1, disk2, cap2)" +
                " VALUES (@db_id, @date, @status, @trade, @boxed, @manuals, @cpu1, @mhz1, @cpu2, @mhz2, @ram, @vram, @rigid, @disk1, @cap1, @disk2, @cap2)";

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
        ///     Updated an owned computer in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateOwnComputer(OwnComputer entry)
        {
            #if DEBUG
            Console.WriteLine("Updating computer `{0}`...", entry.db_id);
            #endif

            IDbCommand     dbcmd = GetCommandOwnComputer(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql =
                "UPDATE own_computer SET db_id = @db_id, date = @date, status = @status, trade = @trade, boxed = @boxed, manuals = @manuals, cpu1 = @cpu1"        +
                "mhz1 = @mhz1, cpu2 = @cpu2, mhz2 = @mhz2, ram = @ram, vram = @vram, rigid = @rigid, disk1 = @disk1, cap1 = @cap1, disk2 = @disk2, cap2 = @cap2 " +
                $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes an owned computer from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveOwnComputer(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing owned computer widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM own_computer WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandOwnComputer(OwnComputer entry)
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

            param1.ParameterName  = "@db_id";
            param2.ParameterName  = "@date";
            param3.ParameterName  = "@status";
            param4.ParameterName  = "@trade";
            param5.ParameterName  = "@boxed";
            param6.ParameterName  = "@manuals";
            param7.ParameterName  = "@cpu1";
            param8.ParameterName  = "@mhz1";
            param9.ParameterName  = "@cpu2";
            param10.ParameterName = "@mhz2";
            param11.ParameterName = "@ram";
            param12.ParameterName = "@vram";
            param13.ParameterName = "@rigid";
            param14.ParameterName = "@disk1";
            param15.ParameterName = "@cap1";
            param16.ParameterName = "@disk2";
            param17.ParameterName = "@cap2";

            param1.DbType  = DbType.Int32;
            param2.DbType  = DbType.String;
            param3.DbType  = DbType.Int32;
            param4.DbType  = DbType.Boolean;
            param5.DbType  = DbType.Boolean;
            param6.DbType  = DbType.Boolean;
            param7.DbType  = DbType.Int32;
            param8.DbType  = DbType.Double;
            param9.DbType  = DbType.Int32;
            param10.DbType = DbType.Double;
            param11.DbType = DbType.Int32;
            param12.DbType = DbType.Int32;
            param13.DbType = DbType.String;
            param14.DbType = DbType.Int32;
            param15.DbType = DbType.Int32;
            param16.DbType = DbType.Int32;
            param17.DbType = DbType.Int32;

            param1.Value  = entry.db_id;
            param2.Value  = entry.date;
            param3.Value  = entry.status;
            param4.Value  = entry.trade;
            param5.Value  = entry.boxed;
            param6.Value  = entry.manuals;
            param7.Value  = entry.cpu1;
            param8.Value  = entry.mhz1;
            param9.Value  = entry.cpu2;
            param10.Value = entry.mhz2;
            param11.Value = entry.ram;
            param12.Value = entry.vram;
            param13.Value = entry.rigid;
            param14.Value = entry.disk1;
            param15.Value = entry.cap1;
            param16.Value = entry.disk2;
            param17.Value = entry.cap2;

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

            return dbcmd;
        }

        static List<OwnComputer> OwnOwnComputersFromDataTable(DataTable dataTable)
        {
            List<OwnComputer> entries = new List<OwnComputer>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                OwnComputer entry = new OwnComputer
                {
                    id      = int.Parse(dataRow["id"].ToString()),
                    db_id   = int.Parse(dataRow["db_id"].ToString()),
                    date    = dataRow["date"].ToString(),
                    status  = int.Parse(dataRow["status"].ToString()),
                    trade   = bool.Parse(dataRow["trade"].ToString()),
                    boxed   = bool.Parse(dataRow["boxed"].ToString()),
                    manuals = bool.Parse(dataRow["manuals"].ToString()),
                    cpu1    = int.Parse(dataRow["cpu1"].ToString()),
                    mhz1    = float.Parse(dataRow["mhz1"].ToString()),
                    cpu2    = int.Parse(dataRow["cpu1"].ToString()),
                    mhz2    = float.Parse(dataRow["mhz2"].ToString()),
                    ram     = int.Parse(dataRow["ram"].ToString()),
                    vram    = int.Parse(dataRow["vram"].ToString()),
                    rigid   = dataRow["rigid"].ToString(),
                    disk1   = int.Parse(dataRow["disk1"].ToString()),
                    cap1    = int.Parse(dataRow["cap1"].ToString()),
                    disk2   = int.Parse(dataRow["disk2"].ToString()),
                    cap2    = int.Parse(dataRow["cap2"].ToString())
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}