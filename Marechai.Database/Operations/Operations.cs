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

using System.Data;

namespace Marechai.Database
{
    public partial class Operations
    {
        /// <summary>Last known database version</summary>
        const int DB_VERSION = 24;
        public const int DbVersionEntityFramework = 1984;
        /// <summary>The column with this value indicates there is no item of this type.</summary>
        public const int DB_NONE = -1;
        /// <summary>
        ///     This value indicates there's no processing unit, but a direct memory connection (a framebuffer or sound
        ///     buffer).
        /// </summary>
        public const int DB_SOFTWARE = -2;

        readonly IDbConnection dbCon;
        readonly IDbCore       dbCore;

        public Operations(IDbConnection connection, IDbCore core)
        {
            dbCon  = connection;
            dbCore = core;
        }
    }
}