using System;
using System.Collections.Generic;
using System.Data;
using Cicm.Database.Schemas;
using Console = System.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
        public bool GetGpus(out List<Gpu> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all GPUs...");
            #endif

            try
            {
                const string SQL = "SELECT * from gpus";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = GpusFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting GPUs.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public bool GetGpus(out List<Gpu> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} GPUs from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM gpus LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = GpusFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting GPUs.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public Gpu GetGpu(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting GPU with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from gpus WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Gpu> entries = GpusFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting GPU.");
                Console.WriteLine(ex);
                return null;
            }
        }

        public long CountGpus()
        {
            #if DEBUG
            Console.WriteLine("Counting gpus...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM gpus";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        public bool AddGpu(Gpu entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding GPU `{0}`...", entry.gpu);
            #endif

            IDbCommand     dbcmd = GetCommandGpu(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO gpus (GPU)" + " VALUES (@GPU)";

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

        public bool UpdateGpu(Gpu entry)
        {
            #if DEBUG
            Console.WriteLine("Updating GPU `{0}`...", entry.gpu);
            #endif

            IDbCommand     dbcmd = GetCommandGpu(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE gpus SET GPU = @GPU " + $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        public bool RemoveGpu(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing GPU widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM gpus WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandGpu(Gpu entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();

            param1.ParameterName = "@GPU";

            param1.DbType = DbType.String;

            param1.Value = entry.gpu;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<Gpu> GpusFromDataTable(DataTable dataTable)
        {
            List<Gpu> entries = new List<Gpu>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Gpu entry = new Gpu {id = int.Parse(dataRow["id"].ToString()), gpu = dataRow["GPU"].ToString()};

                entries.Add(entry);
            }

            return entries;
        }
    }
}