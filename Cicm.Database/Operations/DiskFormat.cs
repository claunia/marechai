using System;
using System.Collections.Generic;
using System.Data;
using Cicm.Database.Schemas;
using Console = System.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
        public bool GetDiskFormats(out List<DiskFormat> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all disk formats...");
            #endif

            try
            {
                const string SQL = "SELECT * from Formatos_de_disco";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = DiskFormatsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting disk formats.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public bool GetDiskFormats(out List<DiskFormat> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} disk formats from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM Formatos_de_disco LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = DiskFormatsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting disk formats.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public DiskFormat GetDiskFormat(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting disk format with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from Formatos_de_disco WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<DiskFormat> entries = DiskFormatsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting disk format.");
                Console.WriteLine(ex);
                return null;
            }
        }

        public long CountDiskFormats()
        {
            #if DEBUG
            Console.WriteLine("Counting disk formats...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM Formatos_de_disco";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        public bool AddDiskFormat(DiskFormat entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding disk format `{0}`...", entry.Format);
            #endif

            IDbCommand     dbcmd = GetCommandDiskFormat(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO Formatos_de_disco (Format)" + " VALUES (@Format)";

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

        public bool UpdateDiskFormat(DiskFormat entry)
        {
            #if DEBUG
            Console.WriteLine("Updating disk format `{0}`...", entry.Format);
            #endif

            IDbCommand     dbcmd = GetCommandDiskFormat(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE Formatos_de_disco SET Format = @Format " + $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        public bool RemoveDiskFormat(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing disk format widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM Formatos_de_disco WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandDiskFormat(DiskFormat entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();

            param1.ParameterName = "@Format";

            param1.DbType = DbType.String;

            param1.Value = entry.Format;

            dbcmd.Parameters.Add(param1);

            return dbcmd;
        }

        static List<DiskFormat> DiskFormatsFromDataTable(DataTable dataTable)
        {
            List<DiskFormat> entries = new List<DiskFormat>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                DiskFormat entry = new DiskFormat
                {
                    id     = int.Parse(dataRow["id"].ToString()),
                    Format = dataRow["Format"].ToString()
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}