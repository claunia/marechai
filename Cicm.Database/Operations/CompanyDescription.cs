/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : CompanyDescription.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage company descriptions.
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
        ///     Gets all company descriptions
        /// </summary>
        /// <param name="entries">All company descriptions</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetCompanyDescriptions(out List<CompanyDescription> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all company descriptions...");
            #endif

            try
            {
                const string SQL = "SELECT * from company_descriptions";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = CompanyDescriptionsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting company descriptions.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of company descriptions since the specified start
        /// </summary>
        /// <param name="entries">List of company_descriptions</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetCompanyDescriptions(out List<CompanyDescription> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} company descriptions from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM company_descriptions LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = CompanyDescriptionsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting company descriptions.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets company description by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>CompanyDescription with specified id, <c>null</c> if not found or error</returns>
        public CompanyDescription GetCompanyDescription(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting company description with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from company_descriptions WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<CompanyDescription> entries = CompanyDescriptionsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting company.");
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Gets company description by specified id
        /// </summary>
        /// <param name="entries">List of company_descriptions</param>
        /// <param name="company">Company id</param>
        /// <returns>CompanyDescription with specified id, <c>null</c> if not found or error</returns>
        public bool GetCompanyDescriptionsByCompany(out List<CompanyDescription> entries, int company)
        {
            #if DEBUG
            Console.WriteLine("Getting company descriptions for company {0}...", company);
            #endif

            try
            {
                string sql = $"SELECT * FROM company_descriptions WHERE company_id = {company}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = CompanyDescriptionsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting company descriptions.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Counts the number of company descriptions in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountCompanyDescriptions()
        {
            #if DEBUG
            Console.WriteLine("Counting company descriptions...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM company_descriptions";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new company description to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddCompanyDescription(CompanyDescription entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding description for company id `{0}`...", entry.CompanyId);
            #endif

            IDbCommand     dbcmd = GetCommandCompanyDescription(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL =
                "INSERT INTO company_descriptions (company_id, text) VALUES (@company_id, @text)";

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
        ///     Updated a company description in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateCompanyDescription(CompanyDescription entry)
        {
            #if DEBUG
            Console.WriteLine("Updating company description id `{0}`...", entry.Id);
            #endif

            IDbCommand     dbcmd = GetCommandCompanyDescription(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE company_descriptions SET company_id = @company_id, text = @text " +
                         $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a company description from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveCompanyDescription(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing company description with id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM company_descriptions WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandCompanyDescription(CompanyDescription entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();

            param1.ParameterName = "@company_id";
            param2.ParameterName = "@text";

            param1.DbType = DbType.String;
            param2.DbType = DbType.String;

            param1.Value = entry.CompanyId;
            param2.Value = entry.Text;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);

            return dbcmd;
        }

        static List<CompanyDescription> CompanyDescriptionsFromDataTable(DataTable dataTable)
        {
            List<CompanyDescription> entries = new List<CompanyDescription>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                CompanyDescription entry = new CompanyDescription
                {
                    Id        = int.Parse(dataRow["id"].ToString()),
                    CompanyId = int.Parse(dataRow["company_id"].ToString()),
                    Text      = dataRow["text"].ToString()
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}