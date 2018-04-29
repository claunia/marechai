/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : V21.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SQL queries to create the database version 7.
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

namespace Cicm.Database.Schemas.Sql
{
    public static class V21
    {
        public static readonly string Admins = V20.Admins;

        public static readonly string BrowserTests = V20.BrowserTests;

        public static readonly string CicmDb = "CREATE TABLE `cicm_db` (\n"                      +
                                               "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                               "`version` int(11) NOT NULL,\n"                   +
                                               "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                               "PRIMARY KEY (`id`));\n"                          +
                                               "INSERT INTO cicm_db (version) VALUES ('21');";

        public static readonly string Companies = V20.Companies;

        public static readonly string Machines = "CREATE TABLE `machines` (;\n"                     +
                                                 "`id` int(11) NOT NULL AUTO_INCREMENT,;\n"         +
                                                 "`company` int(11) NOT NULL DEFAULT '0',;\n"       +
                                                 "`introduced` DATETIME NULL,;\n"                   +
                                                 "`model` char(50) NOT NULL DEFAULT '',;\n"         +
                                                 "`type` int(11) NOT NULL DEFAULT '0',;\n"          +
                                                 "PRIMARY KEY (`id`),;\n"                           +
                                                 "KEY `idx_machines_company` (`company`),;\n"       +
                                                 "KEY `idx_machines_introduced` (`introduced`),;\n" +
                                                 "KEY `idx_machines_model` (`model`),;\n"           +
                                                 "KEY `idx_machines_type` (`type`));";

        public static readonly string Forbidden = V20.Forbidden;

        public static readonly string Gpus = V20.Gpus;

        public static readonly string Logs = V20.Logs;

        public static readonly string MoneyDonations = V20.MoneyDonations;

        public static readonly string News = V20.News;

        public static readonly string OwnedComputers = V20.OwnedComputers;

        public static readonly string OwnedConsoles = V20.OwnedConsoles;

        public static readonly string Processors = V20.Processors;

        public static readonly string SoundSynths = V20.SoundSynths;

        public static readonly string MachinesForeignKeys =
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_company` (company) REFERENCES `companies` (`id`) ON UPDATE CASCADE;";

        public static readonly string Iso3166Numeric = V20.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V20.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V20.CompaniesForeignKeys;

        public static readonly string CompanyLogos = V20.CompanyLogos;

        public static readonly string CompanyDescriptions = V20.CompanyDescriptions;

        public static readonly string InstructionSets = V20.InstructionSets;

        public static readonly string InstructionSetExtensions = V20.InstructionSetExtensions;

        public static readonly string InstructionSetExtensionsByProcessor = V20.InstructionSetExtensionsByProcessor;

        public static readonly string ProcessorsByMachine = V20.ProcessorsByMachine;

        public static readonly string GpusByMachine = V20.GpusByMachine;

        public static readonly string SoundByMachine = V20.SoundByMachine;

        public static readonly string MemoryByMachine = V20.MemoryByMachine;

        public static readonly string Resolutions = V20.Resolutions;

        public static readonly string ResolutionsByGpu = V20.ResolutionsByGpu;

        public static readonly string StorageByMachine = "CREATE TABLE `storage_by_machine` (\n"        +
                                                         "`machine` INT NOT NULL,\n"                    +
                                                         "`type` INT NOT NULL DEFAULT 0,\n"             +
                                                         "`interface` INT NOT NULL DEFAULT 0,\n"        +
                                                         "`capacity` BIGINT DEFAULT NULL,\n"            +
                                                         "KEY `idx_storage_machine` (`machine`),\n"     +
                                                         "KEY `idx_storage_type` (`type`),\n"           +
                                                         "KEY `idx_storage_interface` (`interface`),\n" +
                                                         "KEY `idx_storage_capacity` (`capacity`),\n"   +
                                                         "CONSTRAINT `fk_storage_by_machine_machine` FOREIGN KEY (`machine`) REFERENCES `machines` (`id`) ON UPDATE CASCADE ON DELETE CASCADE);";
    }
}