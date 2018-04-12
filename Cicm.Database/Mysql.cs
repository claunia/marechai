/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Mysql.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements database interface for MySql.Data.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/
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