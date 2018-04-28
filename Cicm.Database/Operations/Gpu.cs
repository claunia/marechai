/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Gpu.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage GPUs.
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
        ///     Gets all GPUs
        /// </summary>
        /// <param name="entries">All GPUs</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetGpus(out List<Gpu> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all GPUs...");
            #endif

            try
            {
                const string SQL = "SELECT * from gpus";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = GpusFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting GPUs.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of GPUs since the specified start
        /// </summary>
        /// <param name="entries">List of GPUs</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetGpus(out List<Gpu> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} GPUs from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM gpus LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = GpusFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting GPUs.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets GPU by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>GPU with specified id, <c>null</c> if not found or error</returns>
        public Gpu GetGpu(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting GPU with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from gpus WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Gpu> entries = GpusFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting GPU.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of GPUs in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountGpus()
        {
            #if DEBUG
            Console.WriteLine("Counting gpus...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM gpus";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new GPU to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddGpu(Gpu entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding GPU `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandGpu(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL =
                "INSERT INTO gpus (name, company, model_code, introduced, package, process, process_nm, " +
                "die_size, transistors)"                                                                  +
                " VALUES (@name, @company, @model_code, @introduced, @package, "                          +
                "@process, @process_nm, @die_size, @transistors)";

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
        ///     Updated a GPU in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateGpu(Gpu entry)
        {
            #if DEBUG
            Console.WriteLine("Updating GPU `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandGpu(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql =
                "UPDATE gpus SET name = @name, company = @company, model_code = @model_code, "                 +
                "introduced = @introduced, package = @package, process = @process, process_nm = @process_nm, " +
                "die_size = @die_size, transistors = @transistors "                                            +
                $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a GPU from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveGpu(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing GPU widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM gpus WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandGpu(Gpu entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();
            IDbDataParameter param3 = dbcmd.CreateParameter();
            IDbDataParameter param4 = dbcmd.CreateParameter();
            IDbDataParameter param5 = dbcmd.CreateParameter();
            IDbDataParameter param6 = dbcmd.CreateParameter();
            IDbDataParameter param7 = dbcmd.CreateParameter();
            IDbDataParameter param8 = dbcmd.CreateParameter();
            IDbDataParameter param9 = dbcmd.CreateParameter();

            param1.ParameterName = "@name";
            param2.ParameterName = "@company";
            param3.ParameterName = "@model_code";
            param4.ParameterName = "@introduced";
            param5.ParameterName = "@package";
            param6.ParameterName = "@process";
            param7.ParameterName = "@process_nm";
            param8.ParameterName = "@die_size";
            param9.ParameterName = "@transistors";

            param1.DbType = DbType.String;
            param2.DbType = DbType.Int32;
            param3.DbType = DbType.String;
            param4.DbType = DbType.DateTime;
            param5.DbType = DbType.String;
            param6.DbType = DbType.String;
            param7.DbType = DbType.Double;
            param8.DbType = DbType.Double;
            param9.DbType = DbType.UInt64;

            param1.Value = entry.Name;
            param2.Value = entry.Company == null ? null : (object)entry.Company.Id;
            param3.Value = entry.ModelCode;
            param4.Value = entry.Introduced == DateTime.MinValue ? null : (object)entry.Introduced;
            param5.Value = entry.Package;
            param6.Value = entry.Process;
            param7.Value = entry.ProcessNm;
            param8.Value = entry.DieSize;
            param9.Value = entry.Transistors;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);
            dbcmd.Parameters.Add(param3);
            dbcmd.Parameters.Add(param4);
            dbcmd.Parameters.Add(param5);
            dbcmd.Parameters.Add(param6);
            dbcmd.Parameters.Add(param7);
            dbcmd.Parameters.Add(param8);
            dbcmd.Parameters.Add(param9);

            return dbcmd;
        }

        List<Gpu> GpusFromDataTable(DataTable dataTable)
        {
            List<Gpu> entries = new List<Gpu>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Gpu entry = new Gpu
                {
                    Id          = (int)dataRow["id"],
                    Name        = (string)dataRow["name"],
                    ModelCode   = dataRow["model_code"]  == DBNull.Value ? null : (string)dataRow["model_code"],
                    Package     = dataRow["package"]     == DBNull.Value ? null : (string)dataRow["package"],
                    Process     = dataRow["process"]     == DBNull.Value ? null : (string)dataRow["process"],
                    ProcessNm   = dataRow["process_nm"]  == DBNull.Value ? 0 : (float)dataRow["process_nm"],
                    DieSize     = dataRow["die_size"]    == DBNull.Value ? 0 : (float)dataRow["die_size"],
                    Transistors = dataRow["transistors"] == DBNull.Value ? 0 : (long)dataRow["transistors"],
                    Company     = dataRow["company"]     == DBNull.Value ? null : GetCompany((int)dataRow["company"]),
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