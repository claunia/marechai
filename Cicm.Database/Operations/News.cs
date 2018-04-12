using System;
using System.Collections.Generic;
using System.Data;
using Cicm.Database.Schemas;
using Console = System.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
        public bool GetNews(out List<News> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all news...");
            #endif

            try
            {
                const string SQL = "SELECT * from news";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = NewsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting news.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public bool GetNews(out List<News> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} news from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM news LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = NewsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting news.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public News GetNews(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting news with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from news WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<News> entries = NewsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting news.");
                Console.WriteLine(ex);
                return null;
            }
        }

        public long CountNews()
        {
            #if DEBUG
            Console.WriteLine("Counting news...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM news";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        public bool AddNews(News entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding news `{0}`...", entry.date);
            #endif

            IDbCommand     dbcmd = GetCommandNews(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO news (date, type, added_id)" + " VALUES (@date, @type, @added_id)";

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

        public bool UpdateNews(News entry)
        {
            #if DEBUG
            Console.WriteLine("Updating news `{0}`...", entry.date);
            #endif

            IDbCommand     dbcmd = GetCommandNews(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE news SET date = @date, type = @type, added_id = @added_id " + $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        public bool RemoveNews(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing news widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM news WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandNews(News entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();
            IDbDataParameter param3 = dbcmd.CreateParameter();

            param1.ParameterName = "@date";
            param2.ParameterName = "@type";
            param3.ParameterName = "@added_id";

            param1.DbType = DbType.String;
            param2.DbType = DbType.Int32;
            param3.DbType = DbType.Int32;

            param1.Value = entry.date;
            param2.Value = entry.type;
            param3.Value = entry.added_id;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);
            dbcmd.Parameters.Add(param3);

            return dbcmd;
        }

        static List<News> NewsFromDataTable(DataTable dataTable)
        {
            List<News> entries = new List<News>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                News entry = new News
                {
                    id       = int.Parse(dataRow["id"].ToString()),
                    date     = dataRow["date"].ToString(),
                    type     = int.Parse(dataRow["type"].ToString()),
                    added_id = int.Parse(dataRow["added_id"].ToString())
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}