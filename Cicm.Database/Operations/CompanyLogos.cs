/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : CompanyLogo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage company logos.
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
        ///     Gets all company logos
        /// </summary>
        /// <param name="entries">All company logos</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetCompanyLogos(out List<CompanyLogo> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all company logos...");
            #endif

            try
            {
                const string SQL = "SELECT * from company_logos";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = CompanyLogosFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting company logos.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of company logos since the specified start
        /// </summary>
        /// <param name="entries">List of company_logos</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetCompanyLogos(out List<CompanyLogo> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} company logos from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM company_logos LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = CompanyLogosFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting company logos.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets company logo by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>CompanyLogo with specified id, <c>null</c> if not found or error</returns>
        public CompanyLogo GetCompanyLogo(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting company logo with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from company_logos WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<CompanyLogo> entries = CompanyLogosFromDataTable(dataSet.Tables[0]);

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
        ///     Gets company logo by specified id
        /// </summary>
        /// <param name="entries">List of company_logos</param>
        /// <param name="company">Company id</param>
        /// <returns>CompanyLogo with specified id, <c>null</c> if not found or error</returns>
        public bool GetCompanyLogosByCompany(out List<CompanyLogo> entries, int company)
        {
            #if DEBUG
            Console.WriteLine("Getting company logos for company {0}...", company);
            #endif

            try
            {
                string sql = $"SELECT * FROM company_logos WHERE company_id = {company}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = CompanyLogosFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting company logos.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Counts the number of company logos in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountCompanyLogos()
        {
            #if DEBUG
            Console.WriteLine("Counting company logos...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM company_logos";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new company logo to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddCompanyLogo(CompanyLogo entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding logo for company id `{0}`...", entry.CompanyId);
            #endif

            IDbCommand     dbcmd = GetCommandCompanyLogo(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL =
                "INSERT INTO company_logos (company_id, year, logo_guid) VALUES (@company_id, @year, @logo_guid)";

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
        ///     Updated a company logo in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateCompanyLogo(CompanyLogo entry)
        {
            #if DEBUG
            Console.WriteLine("Updating company logo id `{0}`...", entry.Id);
            #endif

            IDbCommand     dbcmd = GetCommandCompanyLogo(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE company_logos SET company_id = @company_id, year = @year, logo_guid = @logo_guid " +
                         $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a company logo from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveCompanyLogo(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing company logo with id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM company_logos WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandCompanyLogo(CompanyLogo entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();
            IDbDataParameter param3 = dbcmd.CreateParameter();

            param1.ParameterName = "@company_id";
            param2.ParameterName = "@year";
            param3.ParameterName = "@logo_guid";

            param1.DbType = DbType.String;
            param2.DbType = DbType.String;
            param3.DbType = DbType.Guid;

            param1.Value = entry.CompanyId;
            param2.Value = entry.Year;
            param3.Value = entry.Guid;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);
            dbcmd.Parameters.Add(param3);

            return dbcmd;
        }

        static List<CompanyLogo> CompanyLogosFromDataTable(DataTable dataTable)
        {
            List<CompanyLogo> entries = new List<CompanyLogo>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                CompanyLogo entry = new CompanyLogo
                {
                    Id        = int.Parse(dataRow["id"].ToString()),
                    CompanyId = int.Parse(dataRow["company_id"].ToString()),
                    Year      = int.Parse(dataRow["year"].ToString()),
                    Guid      = Guid.Parse(dataRow["logo_guid"].ToString())
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}