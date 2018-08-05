/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Company.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage companies.
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
        ///     Gets all companies
        /// </summary>
        /// <param name="entries">All companies</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetCompanies(out List<Company> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all companies...");
            #endif

            try
            {
                const string SQL = "SELECT * from companies";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = CompaniesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting companies.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the specified number of companies since the specified start
        /// </summary>
        /// <param name="entries">List of companies</param>
        /// <param name="start">Start of query</param>
        /// <param name="count">How many entries to retrieve</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetCompanies(out List<Company> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} companies from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM companies LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = CompaniesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting companies.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets all companies that start with the specified letter
        /// </summary>
        /// <param name="entries">All companies</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetCompanies(out List<Company> entries, char startingLetter)
        {
            if((startingLetter < 'a' || startingLetter > 'z') && (startingLetter < 'A' || startingLetter > 'Z'))
                return GetCompanies(out entries);

            #if DEBUG
            Console.WriteLine("Getting all companies that start with {0}...");
            #endif

            try
            {
                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = $"SELECT * FROM companies WHERE name LIKE '{startingLetter}%'";
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = CompaniesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting companies.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets all companies from the specified country
        /// </summary>
        /// <param name="entries">All companies</param>
        /// <returns><c>true</c> if <see cref="entries" /> is correct, <c>false</c> otherwise</returns>
        public bool GetCompanies(out List<Company> entries, int countryCode)
        {
            #if DEBUG
            Console.WriteLine("Getting all companies that start with {0}...");
            #endif

            try
            {
                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = $"SELECT * from companies WHERE country = '{countryCode}'";
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = CompaniesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting companies.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets company by specified id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Company with specified id, <c>null</c> if not found or error</returns>
        public Company GetCompany(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting company with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from companies WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Company> entries = CompaniesFromDataTable(dataSet.Tables[0]);

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
        ///     Counts the number of companies in the database
        /// </summary>
        /// <returns>Entries in database</returns>
        public long CountCompanies()
        {
            #if DEBUG
            Console.WriteLine("Counting companies...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM companies";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        /// <summary>
        ///     Adds a new company to the database
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="id">ID of added entry</param>
        /// <returns><c>true</c> if added correctly, <c>false</c> otherwise</returns>
        public bool AddCompany(Company entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding company `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandCompany(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL =
                "INSERT INTO companies (name, founded, website, twitter, facebook, sold, sold_to, "          +
                "address, city, province, postal_code, country, status) VALUES (@name, @founded, @website, " +
                "@twitter, @facebook, @sold, @sold_to, @address, @city, @province, @postal_code, "           +
                "@country, status)";

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
        ///     Updated a company in the database
        /// </summary>
        /// <param name="entry">Updated entry</param>
        /// <returns><c>true</c> if updated correctly, <c>false</c> otherwise</returns>
        public bool UpdateCompany(Company entry)
        {
            #if DEBUG
            Console.WriteLine("Updating company `{0}`...", entry.Name);
            #endif

            IDbCommand     dbcmd = GetCommandCompany(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql =
                "UPDATE companies SET name = @name, founded = @founded, website = @website, twitter = @twitter, " +
                "facebook = @facebook, sold = @sold, sold_to = @sold_to, address = @address, city = @city, "      +
                "province = @province, postal_code = @postal_code, country = @country, status = @status "         +
                $"WHERE id = {entry.Id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        /// <summary>
        ///     Removes a company from the database
        /// </summary>
        /// <param name="id">ID of entry to remove</param>
        /// <returns><c>true</c> if removed correctly, <c>false</c> otherwise</returns>
        public bool RemoveCompany(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing company widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM companies WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandCompany(Company entry)
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

            param1.ParameterName  = "@name";
            param2.ParameterName  = "@founded";
            param3.ParameterName  = "@website";
            param4.ParameterName  = "@twitter";
            param5.ParameterName  = "@facebook";
            param6.ParameterName  = "@sold";
            param7.ParameterName  = "@sold_to";
            param8.ParameterName  = "@address";
            param9.ParameterName  = "@city";
            param10.ParameterName = "@province";
            param11.ParameterName = "@postal_code";
            param12.ParameterName = "@country";
            param13.ParameterName = "@status";

            param1.DbType  = DbType.String;
            param2.DbType  = DbType.DateTime;
            param3.DbType  = DbType.String;
            param4.DbType  = DbType.String;
            param5.DbType  = DbType.String;
            param6.DbType  = DbType.DateTime;
            param7.DbType  = DbType.UInt32;
            param8.DbType  = DbType.String;
            param9.DbType  = DbType.String;
            param10.DbType = DbType.String;
            param11.DbType = DbType.String;
            param12.DbType = DbType.UInt16;
            param13.DbType = DbType.UInt32;

            param1.Value = entry.Name;
            param2.Value = entry.Founded;
            param3.Value = entry.Website;
            param4.Value = entry.Twitter;
            param5.Value = entry.Facebook;
            if(entry.SoldTo != null)
            {
                param6.Value = entry.Sold;
                param7.Value = entry.SoldTo.Id;
            }
            else
            {
                param6.Value = null;
                param7.Value = null;
            }

            param8.Value  = entry.Address;
            param9.Value  = entry.City;
            param10.Value = entry.Province;
            param11.Value = entry.PostalCode;
            if(entry.Country != null) param12.Value = entry.Country.Id;
            else param12.Value                      = null;
            param13.Value = entry.Status;

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

        List<Company> CompaniesFromDataTable(DataTable dataTable)
        {
            List<Company> entries = new List<Company>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Company entry = new Company
                {
                    Id         = (int)dataRow["id"],
                    Name       = (string)dataRow["name"],
                    Website    = dataRow["website"]     == DBNull.Value ? null : (string)dataRow["website"],
                    Twitter    = dataRow["twitter"]     == DBNull.Value ? null : (string)dataRow["twitter"],
                    Facebook   = dataRow["facebook"]    == DBNull.Value ? null : (string)dataRow["facebook"],
                    Address    = dataRow["address"]     == DBNull.Value ? null : (string)dataRow["address"],
                    City       = dataRow["city"]        == DBNull.Value ? null : (string)dataRow["city"],
                    Province   = dataRow["province"]    == DBNull.Value ? null : (string)dataRow["province"],
                    PostalCode = dataRow["postal_code"] == DBNull.Value ? null : (string)dataRow["postal_code"],
                    Status     = (CompanyStatus)dataRow["status"],
                    Founded =
                        dataRow["founded"] == DBNull.Value
                            ? DateTime.MinValue
                            : Convert.ToDateTime(dataRow["founded"].ToString()),
                    Sold =
                        dataRow["sold"] == DBNull.Value
                            ? DateTime.MinValue
                            : Convert.ToDateTime(dataRow["sold"].ToString()),
                    SoldTo  = dataRow["sold_to"] == DBNull.Value ? null : GetCompany((int)dataRow["sold_to"]),
                    Country = dataRow["country"] == DBNull.Value ? null : GetIso3166((short)dataRow["country"])
                };

                if(GetCompanyLogosByCompany(out List<CompanyLogo> logos, entry.Id))
                {
                    entry.Logos = logos.ToArray();
                    if(entry.Logos != null && entry.Logos.Length > 0)
                        if(entry.Logos.Length > 1)
                        {
                            int currentYear = 0;
                            foreach(CompanyLogo logo in entry.Logos)
                            {
                                if(logo.Year <= currentYear) continue;

                                entry.LastLogo = logo;
                                currentYear    = logo.Year;
                            }
                        }
                        else
                            entry.LastLogo = entry.Logos[0];
                }

                if(GetCompanyDescriptionsByCompany(out List<CompanyDescription> descriptions, entry.Id))
                    if(descriptions != null && descriptions.Count > 0)
                        entry.Description = descriptions[0].Text;

                entries.Add(entry);
            }

            return entries;
        }
    }
}