/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : OwnedConsole.cs
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

namespace Cicm.Database
{
    public partial class Operations
    {
        /// <summary>
        ///     Gets all owned computers
        /// </summary>
        /// <param name="entries">All owned computers</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetOwnedComputers(out List<OwnedComputer> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all owned computers...");
            #endif

            try
            {
                const string SQL = "SELECT * from owned_computers";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = OwnedComputersFromDataTable(dataSet.Tables[0]);

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
        public bool GetOwnedComputers(out List<OwnedComputer> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} owned computers from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM owned_computers LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = OwnedComputersFromDataTable(dataSet.Tables[0]);

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
        public OwnedComputer GetOwnedComputer(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting owned computer with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from owned_computers WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<OwnedComputer> entries = OwnedComputersFromDataTable(dataSet.Tables[0]);

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
        public long CountOwnedComputers()
        {
            #if DEBUG
            Console.WriteLine("Counting owned computers...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM owned_computers";
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
        public bool AddOwnedComputer(OwnedComputer entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding owned computer `{0}`...", entry.ComputerId);
            #endif

            IDbCommand     dbcmd = GetCommandOwnedComputer(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL =
                "INSERT INTO owned_computers (db_id, date, status, trade, boxed, manuals, cpu1, mhz1, cpu2, mhz2, ram, vram, rigid, disk1, cap1, disk2, cap2)" +
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
        public bool UpdateOwnedComputer(OwnedComputer entry)
        {
            #if DEBUG
            Console.WriteLine("Updating computer `{0}`...", entry.ComputerId);
            #endif

            IDbCommand     dbcmd = GetCommandOwnedComputer(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql =
                "UPDATE owned_computers SET db_id = @db_id, date = @date, status = @status, trade = @trade, boxed = @boxed, manuals = @manuals, cpu1 = @cpu1"     +
                "mhz1 = @mhz1, cpu2 = @cpu2, mhz2 = @mhz2, ram = @ram, vram = @vram, rigid = @rigid, disk1 = @disk1, cap1 = @cap1, disk2 = @disk2, cap2 = @cap2 " +
                $"WHERE id = {entry.Id}";

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
        public bool RemoveOwnedComputer(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing owned computer widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM owned_computers WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandOwnedComputer(OwnedComputer entry)
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

            param1.Value  = entry.ComputerId;
            param2.Value  = entry.Acquired;
            param3.Value  = entry.Status;
            param4.Value  = entry.Trade;
            param5.Value  = entry.Boxed;
            param6.Value  = entry.Manuals;
            param7.Value  = entry.Cpu1;
            param8.Value  = entry.Mhz1;
            param9.Value  = entry.Cpu2;
            param10.Value = entry.Mhz2;
            param11.Value = entry.Ram;
            param12.Value = entry.Vram;
            param13.Value = entry.Rigid;

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

            return dbcmd;
        }

        static List<OwnedComputer> OwnedComputersFromDataTable(DataTable dataTable)
        {
            List<OwnedComputer> entries = new List<OwnedComputer>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                OwnedComputer entry = new OwnedComputer
                {
                    Id         = (int)dataRow["id"],
                    ComputerId = (int)dataRow["db_id"],
                    Acquired   = dataRow["date"].ToString(),
                    Status     = (StatusType)dataRow["status"],
                    Trade      = (int)dataRow["trade"]   > 0,
                    Boxed      = (int)dataRow["boxed"]   > 0,
                    Manuals    = (int)dataRow["manuals"] > 0,
                    Cpu1       = (int)dataRow["cpu1"],
                    Mhz1       = (float)dataRow["mhz1"],
                    Cpu2       = (int)dataRow["cpu1"],
                    Mhz2       = (float)dataRow["mhz2"],
                    Ram        = (int)dataRow["ram"],
                    Vram       = (int)dataRow["vram"],
                    Rigid      = (string)dataRow["rigid"]
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}