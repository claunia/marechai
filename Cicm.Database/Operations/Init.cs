/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Init.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains operation to initialize a new database.
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
using Cicm.Database.Schemas.Sql;

namespace Cicm.Database
{
    public partial class Operations
    {
        /// <summary>
        ///     Initializes tables in the database
        /// </summary>
        /// <returns><c>true</c> if initialized correctly, <c>false</c> otherwise</returns>
        public bool InitializeNewDatabase()
        {
            Console.WriteLine("Creating new database version {0}", DB_VERSION);

            try
            {
                IDbCommand dbCmd = dbCon.CreateCommand();

                Console.WriteLine("Creating table `admins`");
                dbCmd.CommandText = V4.Admins;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `browser_tests`");
                dbCmd.CommandText = V4.BrowserTests;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `cicm_db`");
                dbCmd.CommandText = V4.CicmDb;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `companies`");
                dbCmd.CommandText = V4.Companies;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `computers`");
                dbCmd.CommandText = V4.Computers;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `consoles`");
                dbCmd.CommandText = V4.Consoles;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `disk_formats`");
                dbCmd.CommandText = V4.DiskFormats;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `forbidden`");
                dbCmd.CommandText = V4.Forbidden;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `gpus`");
                dbCmd.CommandText = V4.Gpus;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `log`");
                dbCmd.CommandText = V4.Logs;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `money_donations`");
                dbCmd.CommandText = V4.MoneyDonations;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `music_synths`");
                dbCmd.CommandText = V4.MusicSynths;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `news`");
                dbCmd.CommandText = V4.News;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `owned_computers`");
                dbCmd.CommandText = V4.OwnedComputers;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `owned_consoles`");
                dbCmd.CommandText = V4.OwnedConsoles;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `processors`");
                dbCmd.CommandText = V4.Processors;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `sound_synths`");
                dbCmd.CommandText = V4.SoundSynths;
                dbCmd.ExecuteNonQuery();

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error creating database.");
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}