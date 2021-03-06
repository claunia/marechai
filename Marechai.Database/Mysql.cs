﻿/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System;
using System.Data;
using MySqlConnector;

namespace Marechai.Database
{
    public class Mysql : IDbCore
    {
        MySqlConnection connection;

        /// <summary>Database operations</summary>
        public Operations Operations { get; private set; }

        /// <summary>Last inserted row's ID</summary>
        public long LastInsertRowId
        {
            get
            {
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT LAST_INSERT_ID()";
                IDataReader reader = command.ExecuteReader();

                if(reader == null ||
                   !reader.Read())
                    return 0;

                long id = reader.GetInt64(0);
                reader.Close();

                return id;
            }
        }

        /// <summary>Opens an existing database</summary>
        /// <param name="server">Server</param>
        /// <param name="user">User</param>
        /// <param name="database">Database name</param>
        /// <param name="password">Password</param>
        /// <param name="port">Port</param>
        /// <returns><c>true</c> if database opened correctly, <c>false</c> otherwise</returns>
        public bool OpenDb(string server, string user, string database, string password, ushort port = 3306)
        {
            try
            {
                string connectionString =
                    $"server={server};user={user};database={database};port={port};password={password}";

                connection = new MySqlConnection(connectionString);
                connection.Open();

                Operations = new Operations(connection, this);

                bool res = Operations.UpdateDatabase();

                if(res)
                    return true;

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

        /// <summary>Closes the database</summary>
        public void CloseDb()
        {
            connection?.Close();
            connection = null;
        }

        /// <summary>Gets a data adapter for the opened database</summary>
        /// <returns>Data adapter</returns>
        public IDbDataAdapter GetNewDataAdapter() => new MySqlDataAdapter();

        public bool TableExists(string tableName)
        {
            MySqlCommand cmd = connection.CreateCommand();

            cmd.CommandText =
                $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{connection.Database}' AND table_name = '{tableName}'";

            MySqlDataReader reader = cmd.ExecuteReader();

            reader.Read();

            int count = reader.GetInt32(0);
            reader.Close();

            return count > 0;
        }

        ~Mysql() => CloseDb();
    }
}