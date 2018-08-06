/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : IDbCore.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines database interface using System.Data.
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

using System.Data;

namespace Cicm.Database
{
    /// <summary>Interface to database</summary>
    public interface IDbCore
    {
        /// <summary>Database operations</summary>
        Operations Operations { get; }

        /// <summary>Last inserted row's ID</summary>
        long LastInsertRowId { get; }

        /// <summary>
        ///     Opens an existing database
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="user">User</param>
        /// <param name="database">Database name</param>
        /// <param name="password">Password</param>
        /// <param name="port">Port</param>
        /// <returns><c>true</c> if database opened correctly, <c>false</c> otherwise</returns>
        bool OpenDb(string server, string user, string database, string password, ushort port);

        /// <summary>
        ///     Closes the database
        /// </summary>
        void CloseDb();

        /// <summary>
        ///     Gets a data adapter for the opened database
        /// </summary>
        /// <returns>Data adapter</returns>
        IDbDataAdapter GetNewDataAdapter();

        bool TableExists(string tableName);
    }
}