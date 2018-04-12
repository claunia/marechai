using System;
using System.Collections.Generic;
using System.Data;
using Cicm.Database.Schemas;
using Console = System.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
        public bool GetOwnOwnConsoles(out List<OwnConsole> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all owned consoles...");
            #endif

            try
            {
                const string SQL = "SELECT * from own_consoles";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = OwnOwnConsolesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting owned consoles.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public bool GetOwnOwnConsoles(out List<OwnConsole> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} owned consoles from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM own_consoles LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = OwnOwnConsolesFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting owned consoles.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public OwnConsole GetOwnConsole(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting owned console with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from own_consoles WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<OwnConsole> entries = OwnOwnConsolesFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting owned console.");
                Console.WriteLine(ex);
                return null;
            }
        }

        public long CountOwnOwnConsoles()
        {
            #if DEBUG
            Console.WriteLine("Counting owned consoles...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM own_consoles";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        public bool AddOwnConsole(OwnConsole entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding owned console `{0}`...", entry.db_id);
            #endif

            IDbCommand     dbcmd = GetCommandOwnConsole(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO own_consoles (db_id, date, status, trade, boxed, manuals)" +
                               " VALUES (@db_id, @date, @status, @trade, @boxed, @manuals)";

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

        public bool UpdateOwnConsole(OwnConsole entry)
        {
            #if DEBUG
            Console.WriteLine("Updating console `{0}`...", entry.db_id);
            #endif

            IDbCommand     dbcmd = GetCommandOwnConsole(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql =
                "UPDATE own_consoles SET db_id = @db_id, date = @date, status = @status, trade = @trade, boxed = @boxed, manuals = @manuals " +
                $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        public bool RemoveOwnConsole(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing owned console widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM own_consoles WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandOwnConsole(OwnConsole entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();
            IDbDataParameter param3 = dbcmd.CreateParameter();
            IDbDataParameter param4 = dbcmd.CreateParameter();
            IDbDataParameter param5 = dbcmd.CreateParameter();
            IDbDataParameter param6 = dbcmd.CreateParameter();

            param1.ParameterName = "@db_id";
            param2.ParameterName = "@date";
            param3.ParameterName = "@status";
            param4.ParameterName = "@trade";
            param5.ParameterName = "@boxed";
            param6.ParameterName = "@manuals";

            param1.DbType = DbType.Int32;
            param2.DbType = DbType.String;
            param3.DbType = DbType.Int32;
            param4.DbType = DbType.Boolean;
            param5.DbType = DbType.Boolean;
            param6.DbType = DbType.Boolean;

            param1.Value = entry.db_id;
            param2.Value = entry.date;
            param3.Value = entry.status;
            param4.Value = entry.trade;
            param5.Value = entry.boxed;
            param6.Value = entry.manuals;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);
            dbcmd.Parameters.Add(param3);
            dbcmd.Parameters.Add(param4);
            dbcmd.Parameters.Add(param5);
            dbcmd.Parameters.Add(param6);

            return dbcmd;
        }

        static List<OwnConsole> OwnOwnConsolesFromDataTable(DataTable dataTable)
        {
            List<OwnConsole> entries = new List<OwnConsole>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                OwnConsole entry = new OwnConsole
                {
                    id      = int.Parse(dataRow["id"].ToString()),
                    db_id   = int.Parse(dataRow["db_id"].ToString()),
                    date    = dataRow["date"].ToString(),
                    status  = int.Parse(dataRow["status"].ToString()),
                    trade   = bool.Parse(dataRow["trade"].ToString()),
                    boxed   = bool.Parse(dataRow["boxed"].ToString()),
                    manuals = bool.Parse(dataRow["manuals"].ToString())
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}