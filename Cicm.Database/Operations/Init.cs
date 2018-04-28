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
                dbCmd.CommandText = V19.Admins;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `browser_tests`");
                dbCmd.CommandText = V19.BrowserTests;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `cicm_db`");
                dbCmd.CommandText = V19.CicmDb;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `companies`");
                dbCmd.CommandText = V19.Companies;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `machines`");
                dbCmd.CommandText = V19.Machines;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `disk_formats`");
                dbCmd.CommandText = V19.DiskFormats;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `forbidden`");
                dbCmd.CommandText = V19.Forbidden;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `gpus`");
                dbCmd.CommandText = V19.Gpus;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `log`");
                dbCmd.CommandText = V19.Logs;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `money_donations`");
                dbCmd.CommandText = V19.MoneyDonations;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `news`");
                dbCmd.CommandText = V19.News;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `owned_computers`");
                dbCmd.CommandText = V19.OwnedComputers;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `owned_consoles`");
                dbCmd.CommandText = V19.OwnedConsoles;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `instruction_sets`");
                dbCmd.CommandText = V19.InstructionSets;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `instruction_set_extensions`");
                dbCmd.CommandText = V19.InstructionSetExtensions;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `processors`");
                dbCmd.CommandText = V19.Processors;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `instruction_set_extensions_by_processor`");
                dbCmd.CommandText = V19.InstructionSetExtensionsByProcessor;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `sound_synths`");
                dbCmd.CommandText = V19.SoundSynths;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `iso3166_1_numeric`");
                dbCmd.CommandText = V19.Iso3166Numeric;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Filling table `iso3166_1_numeric`");
                dbCmd.CommandText = V19.Iso3166NumericValues;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating foreign keys for table `companies`");
                dbCmd.CommandText = V19.CompaniesForeignKeys;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating foreign keys for table `machines`");
                dbCmd.CommandText = V19.MachinesForeignKeys;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `company_logos`");
                dbCmd.CommandText = V19.CompanyLogos;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `company_descriptions`");
                dbCmd.CommandText = V19.CompanyDescriptions;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `processors_by_machine`");
                dbCmd.CommandText = V19.ProcessorsByMachine;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `gpus_by_machine`");
                dbCmd.CommandText = V19.GpusByMachine;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `sound_by_machine`");
                dbCmd.CommandText = V19.SoundByMachine;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `memory_by_machine`");
                dbCmd.CommandText = V19.MemoryByMachine;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `resolutions`");
                dbCmd.CommandText = V19.Resolutions;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `resolutions_by_gpu`");
                dbCmd.CommandText = V19.ResolutionsByGpu;
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