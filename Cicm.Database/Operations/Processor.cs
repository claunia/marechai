/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Processor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage processors.
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
        ///     Gets all processors
        /// </summary>
        /// <param name="entries">All CPUs</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetProcessors(out List<Processor> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all processors...");
            #endif

            try
            {
                const string SQL = "SELECT * from processors";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ProcessorsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting processors.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of processors since the specified start
        /// </summary>
        /// <param name="entries">List of processors</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetProcessors(out List<Processor> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} processors from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM processors LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ProcessorsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting processors.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets processor by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Processor with specified id, <c>null</c> if not found or error</returns>
        public Processor GetProcessor(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting processor with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from processors WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Processor> entries = ProcessorsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting processor.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of processors in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountProcessors()
        {
            #if DEBUG
            Console.WriteLine("Counting processors...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM processors";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new processor to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddProcessor(Processor entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding processor `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandProcessor(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO processors (name)" + " VALUES (@name)";

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
        ///     Updated a processor in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateProcessor(Processor entry)
        {
            #if DEBUG
            Console.WriteLine("Updating processor `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandProcessor(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE processors SET name = @name, company = @company, model_code = @model_code,"           +
                         "introduced = @introduced, instruction_set = @instruction_set, speed = @speed, "              +
                         "package = @package, GPRs = @GPRs, GPR_size = @GPR_size, FPRs = @FPRs, FPR_size = @FPR_size," +
                         "cores = @cores, threads_per_core = @threads_per_core, process = @process,"                   +
                         "process_nm = @process_nm, die_size = @die_size, transistors = @transistors,"                 +
                         "data_bus = @data_bus, addr_bus = @addr_bus, SIMD_registers = @SIMD_registers,"               +
                         "SIMD_size = @SIMD_size, L1_instruction = @L1_instruction, L1_data = @L1_data, L2 = @L2,"     +
                         "L3 = @L3 "                                                                                   +
                         $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a processor from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveProcessor(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing processor widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM processors WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandProcessor(Processor entry)
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

            param1.ParameterName  = "@name";
            param2.ParameterName  = "@company";
            param3.ParameterName  = "@model_code";
            param4.ParameterName  = "@introduced";
            param5.ParameterName  = "@instruction_set";
            param6.ParameterName  = "@speed";
            param7.ParameterName  = "@package";
            param8.ParameterName  = "@GPRs";
            param9.ParameterName  = "@GPR_size";
            param10.ParameterName = "@FPRs";
            param11.ParameterName = "@FPR_size";
            param12.ParameterName = "@cores";
            param13.ParameterName = "@threads_per_core";
            param14.ParameterName = "@process";
            param15.ParameterName = "@process_nm";
            param16.ParameterName = "@die_size";
            param17.ParameterName = "@transistors";
            param18.ParameterName = "@addr_bus";
            param19.ParameterName = "@data_bus";
            param20.ParameterName = "@SIMD_registers";
            param21.ParameterName = "@SIMD_size";
            param22.ParameterName = "@L1_instruction";
            param23.ParameterName = "@L1_data";
            param24.ParameterName = "@L2";
            param25.ParameterName = "@L3";

            param1.DbType  = DbType.String;
            param2.DbType  = DbType.Int32;
            param3.DbType  = DbType.String;
            param4.DbType  = DbType.DateTime;
            param5.DbType  = DbType.Int32;
            param6.DbType  = DbType.Double;
            param7.DbType  = DbType.String;
            param8.DbType  = DbType.Int32;
            param9.DbType  = DbType.Int32;
            param10.DbType = DbType.Int32;
            param11.DbType = DbType.Int32;
            param12.DbType = DbType.Int32;
            param13.DbType = DbType.Int32;
            param14.DbType = DbType.String;
            param15.DbType = DbType.Double;
            param16.DbType = DbType.Double;
            param17.DbType = DbType.UInt64;
            param18.DbType = DbType.Int32;
            param19.DbType = DbType.Int32;
            param20.DbType = DbType.Int32;
            param21.DbType = DbType.Int32;
            param22.DbType = DbType.Double;
            param23.DbType = DbType.Double;
            param24.DbType = DbType.Double;
            param25.DbType = DbType.Double;

            param1.Value  = entry.Name;
            param2.Value  = entry.Company == null ? null : (object)entry.Company.Id;
            param3.Value  = entry.ModelCode;
            param4.Value  = entry.Introduced == DateTime.MinValue ? null : (object)entry.Introduced;
            param5.Value  = entry.InstructionSet?.Name;
            param6.Value  = entry.Speed;
            param7.Value  = entry.Package;
            param8.Value  = entry.Gpr;
            param9.Value  = entry.GprSize;
            param10.Value = entry.Fpr;
            param11.Value = entry.FprSize;
            param12.Value = entry.Cores;
            param13.Value = entry.ThreadsPerCore;
            param14.Value = entry.Process;
            param15.Value = entry.ProcessNm;
            param16.Value = entry.DieSize;
            param17.Value = entry.Transistors;
            param18.Value = entry.AddressBus;
            param19.Value = entry.DataBus;
            param20.Value = entry.Simd;
            param21.Value = entry.SimdSize;
            param22.Value = entry.L1Instruction;
            param23.Value = entry.L1Data;
            param24.Value = entry.L2;
            param25.Value = entry.L3;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        List<Processor> ProcessorsFromDataTable(DataTable dataTable)
        {
            List<Processor> entries = new List<Processor>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Processor entry = new Processor
                {
                    Id             = int.Parse(dataRow["id"].ToString()),
                    Name           = dataRow["name"].ToString(),
                    ModelCode      = dataRow["model_code"].ToString(),
                    Speed          = Convert.ToDouble(dataRow["speed"].ToString()),
                    Package        = dataRow["package"].ToString(),
                    Gpr            = Convert.ToInt32(dataRow["GPRs"].ToString()),
                    GprSize        = Convert.ToInt32(dataRow["GPR_size"].ToString()),
                    Fpr            = Convert.ToInt32(dataRow["FPRs"].ToString()),
                    FprSize        = Convert.ToInt32(dataRow["FPR_size"].ToString()),
                    Cores          = Convert.ToInt32(dataRow["cores"].ToString()),
                    ThreadsPerCore = Convert.ToInt32(dataRow["threads_per_core"].ToString()),
                    Process        = dataRow["process"].ToString(),
                    ProcessNm      = Convert.ToSingle(dataRow["process_nm"].ToString()),
                    DieSize        = Convert.ToSingle(dataRow["die_size"].ToString()),
                    Transistors    = Convert.ToUInt64(dataRow["transistors"].ToString()),
                    AddressBus     = Convert.ToInt32(dataRow["addr_bus"].ToString()),
                    DataBus        = Convert.ToInt32(dataRow["data_bus"].ToString()),
                    Simd           = Convert.ToInt32(dataRow["SIMD_registers"].ToString()),
                    SimdSize       = Convert.ToInt32(dataRow["SIMD_size"].ToString()),
                    L1Instruction  = Convert.ToSingle(dataRow["L1_instruction"].ToString()),
                    L1Data         = Convert.ToSingle(dataRow["L1_data"].ToString()),
                    L2             = Convert.ToSingle(dataRow["L2"].ToString()),
                    L3             = Convert.ToSingle(dataRow["L3"].ToString())
                };

                if(!string.IsNullOrEmpty(dataRow["company"].ToString()))
                    entry.Company = GetCompany(Convert.ToInt32(dataRow["company"].ToString()));

                if(!string.IsNullOrEmpty(dataRow["introduced"].ToString()))
                    entry.Introduced = Convert.ToDateTime(dataRow["introduced"].ToString());

                if(!string.IsNullOrEmpty(dataRow["instruction_set"].ToString()))
                    entry.InstructionSet = GetInstructionSet(Convert.ToInt32(dataRow["instruction_set"].ToString()));

                entries.Add(entry);
            }

            return entries;
        }
    }
}