using System;
using System.Collections.Generic;
using System.Data;
using Cicm.Database.Schemas;
using Console = System.Console;

namespace Cicm.Database
{
    public partial class Operations
    {
        public bool GetAdmins(out List<Admin> entries)
        {
            #if DEBUG
            Console.WriteLine("Getting all admins...");
            #endif

            try
            {
                const string SQL = "SELECT * from admin";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = SQL;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = AdminsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting admins.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public bool GetAdmins(out List<Admin> entries, ulong start, ulong count)
        {
            #if DEBUG
            Console.WriteLine("Getting {0} admins from {1}...", count, start);
            #endif

            try
            {
                string sql = $"SELECT * FROM admin LIMIT {start}, {count}";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                entries = AdminsFromDataTable(dataSet.Tables[0]);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting admins.");
                Console.WriteLine(ex);
                entries = null;
                return false;
            }
        }

        public Admin GetAdmin(int id)
        {
            #if DEBUG
            Console.WriteLine("Getting admin with id {0}...", id);
            #endif

            try
            {
                string sql = $"SELECT * from admin WHERE id = '{id}'";

                IDbCommand     dbCmd       = dbCon.CreateCommand();
                IDbDataAdapter dataAdapter = dbCore.GetNewDataAdapter();
                dbCmd.CommandText = sql;
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand = dbCmd;
                dataAdapter.Fill(dataSet);

                List<Admin> entries = AdminsFromDataTable(dataSet.Tables[0]);

                return entries == null || entries.Count == 0 ? null : entries[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting admin.");
                Console.WriteLine(ex);
                return null;
            }
        }

        public long CountAdmins()
        {
            #if DEBUG
            Console.WriteLine("Counting admins...");
            #endif

            IDbCommand dbcmd = dbCon.CreateCommand();
            dbcmd.CommandText = "SELECT COUNT(*) FROM admin";
            object count = dbcmd.ExecuteScalar();
            dbcmd.Dispose();
            try { return Convert.ToInt64(count); }
            catch { return 0; }
        }

        public bool AddAdmin(Admin entry, out long id)
        {
            #if DEBUG
            Console.Write("Adding admin `{0}`...", entry.user);
            #endif

            IDbCommand     dbcmd = GetCommandAdmin(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            const string SQL = "INSERT INTO admin (user, password)" + " VALUES (@user, @password)";

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

        public bool UpdateAdmin(Admin entry)
        {
            #if DEBUG
            Console.WriteLine("Updating admin `{0}`...", entry.user);
            #endif

            IDbCommand     dbcmd = GetCommandAdmin(entry);
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = "UPDATE admin SET user = @user, password = @password " + $"WHERE id = {entry.id}";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        public bool RemoveAdmin(long id)
        {
            #if DEBUG
            Console.WriteLine("Removing admin widh id `{0}`...", id);
            #endif

            IDbCommand     dbcmd = dbCon.CreateCommand();
            IDbTransaction trans = dbCon.BeginTransaction();
            dbcmd.Transaction = trans;

            string sql = $"DELETE FROM admin WHERE id = '{id}';";

            dbcmd.CommandText = sql;

            dbcmd.ExecuteNonQuery();
            trans.Commit();
            dbcmd.Dispose();

            return true;
        }

        IDbCommand GetCommandAdmin(Admin entry)
        {
            IDbCommand dbcmd = dbCon.CreateCommand();

            IDbDataParameter param1 = dbcmd.CreateParameter();
            IDbDataParameter param2 = dbcmd.CreateParameter();

            param1.ParameterName = "@user";
            param2.ParameterName = "@password";

            param1.DbType = DbType.String;
            param2.DbType = DbType.String;

            param1.Value = entry.user;
            param2.Value = entry.password;

            dbcmd.Parameters.Add(param1);
            dbcmd.Parameters.Add(param2);

            return dbcmd;
        }

        static List<Admin> AdminsFromDataTable(DataTable dataTable)
        {
            List<Admin> entries = new List<Admin>();

            foreach(DataRow dataRow in dataTable.Rows)
            {
                Admin entry = new Admin
                {
                    id       = int.Parse(dataRow["id"].ToString()),
                    user     = dataRow["user"].ToString(),
                    password = dataRow["password"].ToString()
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}