/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Money Donation.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operations to manage money donations.
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
        public bool GetMoneyDonations(out List<MoneyDonation> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all money donations...");
            #endif

            try
            {
                const string SQL = "SELECT * from money_donation";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MoneyDonationsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting money donations.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public bool GetMoneyDonations(out List<MoneyDonation> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} money donations from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM money_donation LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MoneyDonationsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting money donations.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public MoneyDonation GetMoneyDonation(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting money_donation with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from money_donation WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<MoneyDonation> entries = MoneyDonationsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting money_donation.");
                Console.WriteLine(ex);
                return null;
            }
        }

        public long CountMoneyDonations()
        {
            #if DEBUG
            Console.WriteLine("Counting money donations...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM money_donation";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        public bool AddMoneyDonation(MoneyDonation entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding money_donation `{0}` for `{1}`...", entry.donator, entry.quantity);
            #endif

            IDbCommand     dbcmd = GetCommandMoneyDonation(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO money_donation (donator, quantity)" + " VALUES (@donator, @quantity)";

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

        public bool UpdateMoneyDonation(MoneyDonation entry)
        {
            #if DEBUG
            Console.WriteLine("Updating money_donation `{0}`...", entry.donator);
            #endif

            IDbCommand     dbcmd = GetCommandMoneyDonation(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE money_donation SET donator = @donator, quantity = @quantity " +
                         $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        public bool RemoveMoneyDonation(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing money_donation widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM money_donation WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandMoneyDonation(MoneyDonation entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();

            param1.ParameterName = "@donator";
            param2.ParameterName = "@quantity";

            param1.DbType = DbType.String;
            param2.DbType = DbType.Double;

            param1.Value = entry.donator;
            param2.Value = entry.quantity;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);

            return dbcmd;
        }

        static List<MoneyDonation> MoneyDonationsFromDataTable(DataTable dataTable)
        {
            List<MoneyDonation> entries = new List<MoneyDonation>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                MoneyDonation entry = new MoneyDonation
                {
                    id       = int.Parse(dataRow["id"].ToString()),
                    donator  = dataRow["browser"].ToString(),
                    quantity = float.Parse(dataRow["date"].ToString())
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}