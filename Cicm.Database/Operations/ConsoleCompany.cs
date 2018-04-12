/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : ConsoleCompany.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage console manufacturers.
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
        ///     Gets all console companies
        /// </summary>
        /// <param name="entries">All console companies</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetConsoleCompanies(out List<ConsoleCompany> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all console companies...");
            #endif

            try
            {
                const string SQL = "SELECT * from console_company";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ConsoleCompaniesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting console companies.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of console companies since the specified start
        /// </summary>
        /// <param name="entries">List of console companies</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetConsoleCompanies(out List<ConsoleCompany> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} console companies from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM console_company LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ConsoleCompaniesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting console companies.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets console company by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Console company with specified id, <c>null</c> if not found or error</returns>
        public ConsoleCompany GetConsoleCompany(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting console company with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from console_company WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<ConsoleCompany> entries = ConsoleCompaniesFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting console company.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Counts the number of console companies in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountConsoleCompanies()
        {
            #if DEBUG
            Console.WriteLine("Counting console companies...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM console_company";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new console company to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddConsoleCompany(ConsoleCompany entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding console company `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandConsoleCompany(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO console_company (company)" + " VALUES (@company)";

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
        ///     Updated a console company in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateConsoleCompany(ConsoleCompany entry)
        {
            #if DEBUG
            Console.WriteLine("Updating console company `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandConsoleCompany(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE console_company SET company = @company " + $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a console company from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveConsoleCompany(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing console company widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM console_company WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandConsoleCompany(ConsoleCompany entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();

            param1.ParameterName = "@company";

            param1.DbType = DbType.String;

            param1.Value = entry.Name;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<ConsoleCompany> ConsoleCompaniesFromDataTable(DataTable dataTable)
        {
            List<ConsoleCompany> entries = new List<ConsoleCompany>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                ConsoleCompany entry = new ConsoleCompany
                {
                    Id   = int.Parse(dataRow["id"].ToString()),
                    Name = dataRow["company"].ToString()
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}