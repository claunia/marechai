using System;
using System.Collections.Generic;
using System.Data;
using Cicm.Database.Schemas;
using Console = System.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
        public bool GetCompanies(out List<Company> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all companies...");
            #endif

            try
            {
                const string SQL = "SELECT * from Companias";

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

        public bool GetCompanies(out List<Company> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} companies from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM Companias LIMIT {start}, {count}";

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

        public Company GetCompany(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting company with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from Companias WHERE id = '{id}'";

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

        public long CountCompanies()
        {
            #if DEBUG
            Console.WriteLine("Counting companies...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM Companias";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        public bool AddCompany(Company entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding company `{0}`...", entry.Compania);
            #endif

            IDbCommand     dbcmd = GetCommandCompany(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO Companias (Compania)" + " VALUES (@Compania)";

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

        public bool UpdateCompany(Company entry)
        {
            #if DEBUG
            Console.WriteLine("Updating company `{0}`...", entry.Compania);
            #endif

            IDbCommand     dbcmd = GetCommandCompany(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE Companias SET Compania = @Compania " + $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        public bool RemoveCompany(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing company widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM Companias WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandCompany(Company entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();

            param1.ParameterName = "@Compania";

            param1.DbType = DbType.String;

            param1.Value = entry.Compania;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<Company> CompaniesFromDataTable(DataTable dataTable)
        {
            List<Company> entries = new List<Company>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Company entry = new Company
                {
                    id       = int.Parse(dataRow["id"].ToString()),
                    Compania = dataRow["Compania"].ToString()
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}