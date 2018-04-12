using System;
using System.Collections.Generic;
using System.Data;
using Cicm.Database.Schemas;
using Console = System.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
        public bool GetForbiddens(out List<Forbidden> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all forbiddens...");
            #endif

            try
            {
                const string SQL = "SELECT * from forbidden";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ForbiddensFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting forbiddens.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public bool GetForbiddens(out List<Forbidden> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} forbiddens from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM forbidden LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = ForbiddensFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting forbiddens.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public Forbidden GetForbidden(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting forbidden with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from forbidden WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Forbidden> entries = ForbiddensFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting forbidden.");
                Console.WriteLine(ex);
                return null;
            }
        }

        public long CountForbiddens()
        {
            #if DEBUG
            Console.WriteLine("Counting forbiddens...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM forbidden";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        public bool AddForbidden(Forbidden entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding forbidden `{0}`...", entry.browser);
            #endif

            IDbCommand     dbcmd = GetCommandForbidden(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO forbidden (browser, date, ip, referer)" +
                               " VALUES (@browser, @date, @ip, @referer)";

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

        public bool UpdateForbidden(Forbidden entry)
        {
            #if DEBUG
            Console.WriteLine("Updating forbidden `{0}`...", entry.browser);
            #endif

            IDbCommand     dbcmd = GetCommandForbidden(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE forbidden SET browser = @browser, date = @date, ip = @ip, referer = @referer " +
                         $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        public bool RemoveForbidden(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing forbidden widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM forbidden WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandForbidden(Forbidden entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();
            IDbDataParameter param3 = dbcmd.CreateParameter();
            IDbDataParameter param4 = dbcmd.CreateParameter();

            param1.ParameterName = "@browser";
            param2.ParameterName = "@date";
            param3.ParameterName = "@ip";
            param4.ParameterName = "@referer";

            param1.DbType = DbType.String;
            param2.DbType = DbType.String;
            param3.DbType = DbType.String;
            param4.DbType = DbType.String;

            param1.Value = entry.browser;
            param2.Value = entry.date;
            param3.Value = entry.ip;
            param4.Value = entry.referer;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);
            dbcmd.Parameters.Add(param3);
            dbcmd.Parameters.Add(param4);

            return dbcmd;
        }

        static List<Forbidden> ForbiddensFromDataTable(DataTable dataTable)
        {
            List<Forbidden> entries = new List<Forbidden>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Forbidden entry = new Forbidden
                {
                    id      = int.Parse(dataRow["id"].ToString()),
                    browser = dataRow["browser"].ToString(),
                    date    = dataRow["date"].ToString(),
                    ip      = dataRow["ip"].ToString(),
                    referer = dataRow["referer"].ToString()
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}