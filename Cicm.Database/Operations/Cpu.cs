using System;
using System.Collections.Generic;
using System.Data;
using Cicm.Database.Schemas;
using Console = System.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
        public bool GetCpus(out List<Cpu> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all CPUs...");
            #endif

            try
            {
                const string SQL = "SELECT * from cpu";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = CpusFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting CPUs.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public bool GetCpus(out List<Cpu> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} CPUs from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM cpu LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = CpusFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting CPUs.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public Cpu GetCpu(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting CPU with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from cpu WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Cpu> entries = CpusFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting CPU.");
                Console.WriteLine(ex);
                return null;
            }
        }

        public long CountCpus()
        {
            #if DEBUG
            Console.WriteLine("Counting CPUs...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM cpu";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        public bool AddCpu(Cpu entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding CPU `{0}`...", entry.cpu);
            #endif

            IDbCommand     dbcmd = GetCommandCpu(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO cpu (cpu)" + " VALUES (@cpu)";

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

        public bool UpdateCpu(Cpu entry)
        {
            #if DEBUG
            Console.WriteLine("Updating CPU `{0}`...", entry.cpu);
            #endif

            IDbCommand     dbcmd = GetCommandCpu(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE cpu SET cpu = @cpu " + $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        public bool RemoveCpu(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing CPU widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM cpu WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandCpu(Cpu entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();

            param1.ParameterName = "@cpu";

            param1.DbType = DbType.String;

            param1.Value = entry.cpu;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<Cpu> CpusFromDataTable(DataTable dataTable)
        {
            List<Cpu> entries = new List<Cpu>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Cpu entry = new Cpu {id = int.Parse(dataRow["id"].ToString()), cpu = dataRow["cpu"].ToString()};

                entries.Add(entry);
            }

            return entries;
        }
    }
}