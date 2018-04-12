using System;
using System.Collections.Generic;
using System.Data;
using Cicm.Database.Schemas;
using Console = System.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
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

        public bool AddConsoleCompany(ConsoleCompany entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding console company `{0}`...", entry.company);
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

        public bool UpdateConsoleCompany(ConsoleCompany entry)
        {
            #if DEBUG
            Console.WriteLine("Updating console company `{0}`...", entry.company);
            #endif

            IDbCommand     dbcmd = GetCommandConsoleCompany(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE console_company SET company = @company " + $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

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

            param1.Value = entry.company;

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
                    id      = int.Parse(dataRow["id"].ToString()),
                    company = dataRow["company"].ToString()
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}