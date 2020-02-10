/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : V22.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SQL queries to create the database version 22.
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

namespace Marechai.Database.Schemas.Sql
{
    public static class V22
    {
        public static readonly string Admins = V21.Admins;

        public static readonly string BrowserTests = V21.BrowserTests;

        public static readonly string CicmDb = "CREATE TABLE `cicm_db` (\n"                      +
                                               "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                               "`version` int(11) NOT NULL,\n"                   +
                                               "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                               "PRIMARY KEY (`id`));\n"                          +
                                               "INSERT INTO cicm_db (version) VALUES ('22');";

        public static readonly string Companies = V21.Companies;

        public static readonly string Machines = "CREATE TABLE `machines` (\n"                     +
                                                 "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                                 "`company` int(11) NOT NULL DEFAULT '0',\n"       +
                                                 "`name` varchar(255) NOT NULL,\n"                 +
                                                 "`type` int(11) NOT NULL DEFAULT '0',\n"          +
                                                 "`introduced` datetime DEFAULT NULL,\n"           +
                                                 "`family` int(11) DEFAULT NULL,\n"                +
                                                 "`model` varchar(50) DEFAULT NULL,\n"             +
                                                 "PRIMARY KEY (`id`),\n"                           +
                                                 "KEY `idx_machines_company` (`company`),\n"       +
                                                 "KEY `idx_machines_type` (`type`),\n"             +
                                                 "KEY `idx_machines_introduced` (`introduced`),\n" +
                                                 "KEY `idx_machines_family` (`family`),\n"         +
                                                 "KEY `idx_machines_name` (`name`),\n"             +
                                                 "KEY `idx_machines_model` (`model`));";

        public static readonly string Forbidden = V21.Forbidden;

        public static readonly string Gpus = V21.Gpus;

        public static readonly string Logs = V21.Logs;

        public static readonly string MoneyDonations = V21.MoneyDonations;

        public static readonly string News = V21.News;

        public static readonly string OwnedComputers = V21.OwnedComputers;

        public static readonly string OwnedConsoles = V21.OwnedConsoles;

        public static readonly string Processors = V21.Processors;

        public static readonly string SoundSynths = V21.SoundSynths;

        public static readonly string MachinesForeignKeys =
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_company` (company) REFERENCES `companies` (`id`) ON UPDATE CASCADE;\n" +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_family` (family) REFERENCES `machine_families` (`id`) ON UPDATE CASCADE;";

        public static readonly string Iso3166Numeric = V21.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V21.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V21.CompaniesForeignKeys;

        public static readonly string CompanyLogos = V21.CompanyLogos;

        public static readonly string CompanyDescriptions = V21.CompanyDescriptions;

        public static readonly string InstructionSets = V21.InstructionSets;

        public static readonly string InstructionSetExtensions = V21.InstructionSetExtensions;

        public static readonly string InstructionSetExtensionsByProcessor = V21.InstructionSetExtensionsByProcessor;

        public static readonly string ProcessorsByMachine = V21.ProcessorsByMachine;

        public static readonly string GpusByMachine = V21.GpusByMachine;

        public static readonly string SoundByMachine = V21.SoundByMachine;

        public static readonly string MemoryByMachine = V21.MemoryByMachine;

        public static readonly string Resolutions = V21.Resolutions;

        public static readonly string ResolutionsByGpu = V21.ResolutionsByGpu;

        public static readonly string StorageByMachine = V21.StorageByMachine;

        public static readonly string MachineFamilies = "CREATE TABLE `machine_families` (\n"               +
                                                        "`id` INT NOT NULL AUTO_INCREMENT,\n"               +
                                                        "`company` INT NOT NULL,\n"                         +
                                                        "`name` VARCHAR(255) NOT NULL,\n"                   +
                                                        "PRIMARY KEY (`id`),\n"                             +
                                                        "KEY `idx_machine_families_company` (`company`),\n" +
                                                        "KEY `idx_machine_families_name` (`name`),\n"       +
                                                        "CONSTRAINT `fk_machine_families_company` FOREIGN KEY (`company`) REFERENCES `companies` (`id`) ON UPDATE CASCADE ON DELETE CASCADE);";
    }
}