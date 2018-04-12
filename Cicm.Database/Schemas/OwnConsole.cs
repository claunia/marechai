/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : OwnConsole.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     High level representation of an owned console.
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

namespace Cicm.Database.Schemas
{
    /// <summary>Owned videogame console</summary>
    public class OwnConsole
    {
        /// <summary>Box present in collection</summary>
        public bool boxed;
        /// <summary>Adquired date</summary>
        public string date;
        /// <summary>Videogame console's ID</summary>
        public int db_id;
        /// <summary>ID</summary>
        public int id;
        /// <summary>Original manuals present in collection</summary>
        public bool manuals;
        /// <summary>Status</summary>
        public int status;
        /// <summary>Available for trade</summary>
        public bool trade;
    }
}