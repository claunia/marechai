﻿/******************************************************************************
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
                dbCmd.CommandText = V21.Admins;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `browser_tests`");
                dbCmd.CommandText = V21.BrowserTests;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `cicm_db`");
                dbCmd.CommandText = V21.CicmDb;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `companies`");
                dbCmd.CommandText = V21.Companies;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `machines`");
                dbCmd.CommandText = V21.Machines;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `forbidden`");
                dbCmd.CommandText = V21.Forbidden;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `gpus`");
                dbCmd.CommandText = V21.Gpus;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `log`");
                dbCmd.CommandText = V21.Logs;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `money_donations`");
                dbCmd.CommandText = V21.MoneyDonations;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `news`");
                dbCmd.CommandText = V21.News;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `owned_computers`");
                dbCmd.CommandText = V21.OwnedComputers;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `owned_consoles`");
                dbCmd.CommandText = V21.OwnedConsoles;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `instruction_sets`");
                dbCmd.CommandText = V21.InstructionSets;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `instruction_set_extensions`");
                dbCmd.CommandText = V21.InstructionSetExtensions;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `processors`");
                dbCmd.CommandText = V21.Processors;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `instruction_set_extensions_by_processor`");
                dbCmd.CommandText = V21.InstructionSetExtensionsByProcessor;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `sound_synths`");
                dbCmd.CommandText = V21.SoundSynths;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `iso3166_1_numeric`");
                dbCmd.CommandText = V21.Iso3166Numeric;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Filling table `iso3166_1_numeric`");
                dbCmd.CommandText = V21.Iso3166NumericValues;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating foreign keys for table `companies`");
                dbCmd.CommandText = V21.CompaniesForeignKeys;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating foreign keys for table `machines`");
                dbCmd.CommandText = V21.MachinesForeignKeys;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `company_logos`");
                dbCmd.CommandText = V21.CompanyLogos;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `company_descriptions`");
                dbCmd.CommandText = V21.CompanyDescriptions;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `processors_by_machine`");
                dbCmd.CommandText = V21.ProcessorsByMachine;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `gpus_by_machine`");
                dbCmd.CommandText = V21.GpusByMachine;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `sound_by_machine`");
                dbCmd.CommandText = V21.SoundByMachine;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `memory_by_machine`");
                dbCmd.CommandText = V21.MemoryByMachine;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `resolutions`");
                dbCmd.CommandText = V21.Resolutions;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `resolutions_by_gpu`");
                dbCmd.CommandText = V21.ResolutionsByGpu;
                dbCmd.ExecuteNonQuery();

                Console.WriteLine("Creating table `storage_by_machine`");
                dbCmd.CommandText = V21.StorageByMachine;
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