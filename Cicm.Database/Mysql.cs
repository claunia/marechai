using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Cicm.Database
{
    public class Mysql : IDbCore
    {
        MySqlConnection connection;

        public Mysql(string server, string user, string database, ushort port, string password)
        {
            string connectionString =
                $"server={server};user={user};database={database};port={port};password={password}";

            connection = new MySqlConnection(connectionString);
        }

        public Operations Operations { get; private set; }
        public long LastInsertRowId
        {
            get
            {
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT LAST_INSERT_ID()";
                IDataReader reader = command.ExecuteReader();

                if(reader == null || !reader.Read()) return 0;

                long id = reader.GetInt64(0);
                reader.Close();
                return id;
            }
        }

        public bool OpenDb(string server, string user, string database, string password, ushort port = 3306)
        {
            try
            {
                string connectionString =
                    $"server={server};user={user};database={database};port={port};password={password}";

                connection = new MySqlConnection(connectionString);

                Operations = new Operations(connection, this);

                bool res = Operations.UpdateDatabase();

                if(res) return true;

                connection = null;
                return false;
            }
            catch(MySqlException ex)
            {
                Console.WriteLine("Error opening database.");
                Console.WriteLine(ex);
                connection = null;
                return false;
            }
        }

        public void CloseDb()
        {
            connection?.Close();
            connection = null;
        }

        public bool CreateDb(string database, string server, string user, string password, ushort port = 3306)
        {
            try
            {
                string connectionString =
                    $"server={server};user={user};database={database};port={port};password={password}";

                connection = new MySqlConnection(connectionString);

                IDbCommand command = connection.CreateCommand();
                command.CommandText = $"CREATE DATABASE `{database}`;";
                command.ExecuteNonQuery();
                command.CommandText = $"USE `{database}`;";
                command.ExecuteNonQuery();

                Operations = new Operations(connection, this);

                bool res = Operations.InitializeNewDatabase();

                if(res) return true;

                connection = null;
                return false;
            }
            catch(MySqlException ex)
            {
                Console.WriteLine("Error opening database.");
                Console.WriteLine(ex);
                connection = null;
                return false;
            }
        }

        public IDbDataAdapter GetNewDataAdapter()
        {
            return new MySqlDataAdapter();
        }

        ~Mysql()
        {
            CloseDb();
        }
    }
}