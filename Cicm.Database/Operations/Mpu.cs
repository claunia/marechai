﻿using System;
using System.Collections.Generic;
using System.Data;
using Cicm.Database.Schemas;
using Console = System.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
        public bool GetMpus(out List<Mpu> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all MPUs...");
            #endif

            try
            {
                const string SQL = "SELECT * from mpus";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MpusFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting MPUs.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public bool GetMpus(out List<Mpu> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} MPUs from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM mpus LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = MpusFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting MPUs.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public Mpu GetMpu(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting MPU with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from mpus WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Mpu> entries = MpusFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting MPU.");
                Console.WriteLine(ex);
                return null;
            }
        }

        public long CountMpus()
        {
            #if DEBUG
            Console.WriteLine("Counting mpus...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM mpus";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        public bool AddMpu(Mpu entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding MPU `{0}`...", entry.mpu);
            #endif

            IDbCommand     dbcmd = GetCommandMpu(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO mpus (MPU)" + " VALUES (@MPU)";

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

        public bool UpdateMpu(Mpu entry)
        {
            #if DEBUG
            Console.WriteLine("Updating MPU `{0}`...", entry.mpu);
            #endif

            IDbCommand     dbcmd = GetCommandMpu(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE mpus SET MPU = @MPU " + $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        public bool RemoveMpu(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing MPU widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM mpus WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandMpu(Mpu entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();

            param1.ParameterName = "@MPU";

            param1.DbType = DbType.String;

            param1.Value = entry.mpu;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<Mpu> MpusFromDataTable(DataTable dataTable)
        {
            List<Mpu> entries = new List<Mpu>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Mpu entry = new Mpu {id = int.Parse(dataRow["id"].ToString()), mpu = dataRow["MPU"].ToString()};

                entries.Add(entry);
            }

            return entries;
        }
    }
}