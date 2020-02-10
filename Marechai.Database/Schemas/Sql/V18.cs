/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : V18.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SQL queries to create the database version 18.
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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

namespace Marechai.Database.Schemas.Sql
{
    public static class V18
    {
        public static readonly string Admins = V17.Admins;

        public static readonly string BrowserTests = V17.BrowserTests;

        public static readonly string MarechaiDb = "CREATE TABLE `marechai_db` (\n"                      +
                                               "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                               "`version` int(11) NOT NULL,\n"                   +
                                               "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                               "PRIMARY KEY (`id`));\n"                          +
                                               "INSERT INTO marechai_db (version) VALUES ('18');";

        public static readonly string Companies = V17.Companies;

        public static readonly string Machines = "CREATE TABLE `machines` (;\n"               +
                                                 "`id` int(11) NOT NULL AUTO_INCREMENT,;\n"   +
                                                 "`company` int(11) NOT NULL DEFAULT '0',;\n" +
                                                 "`year` int(11) NOT NULL DEFAULT '0',;\n"    +
                                                 "`model` char(50) NOT NULL DEFAULT '',;\n"   +
                                                 "`colors` int(11) NOT NULL DEFAULT '0',;\n"  +
                                                 "`res` char(10) NOT NULL DEFAULT '',;\n"     +
                                                 "`hdd1` int(11) NOT NULL DEFAULT '0',;\n"    +
                                                 "`hdd2` int(11) DEFAULT NULL,;\n"            +
                                                 "`hdd3` int(11) DEFAULT NULL,;\n"            +
                                                 "`disk1` int(11) NOT NULL DEFAULT '0',;\n"   +
                                                 "`cap1` char(25) NOT NULL DEFAULT '0',;\n"   +
                                                 "`disk2` int(11) DEFAULT NULL,;\n"           +
                                                 "`cap2` char(25) DEFAULT NULL,;\n"           +
                                                 "`type` int(11) NOT NULL DEFAULT '0',;\n"    +
                                                 "PRIMARY KEY (`id`),;\n"                     +
                                                 "KEY `idx_machines_company` (`company`),;\n" +
                                                 "KEY `idx_machines_year` (`year`),;\n"       +
                                                 "KEY `idx_machines_model` (`model`),;\n"     +
                                                 "KEY `idx_machines_colors` (`colors`),;\n"   +
                                                 "KEY `idx_machines_res` (`res`),;\n"         +
                                                 "KEY `idx_machines_hdd1` (`hdd1`),;\n"       +
                                                 "KEY `idx_machines_hdd2` (`hdd2`),;\n"       +
                                                 "KEY `idx_machines_hdd3` (`hdd3`),;\n"       +
                                                 "KEY `idx_machines_disk1` (`disk1`),;\n"     +
                                                 "KEY `idx_machines_disk2` (`disk2`),;\n"     +
                                                 "KEY `idx_machines_cap1` (`cap1`),;\n"       +
                                                 "KEY `idx_machines_cap2` (`cap2`),;\n"       +
                                                 "KEY `idx_machines_type` (`type`));";

        public static readonly string DiskFormats = V17.DiskFormats;

        public static readonly string Forbidden = V17.Forbidden;

        public static readonly string Gpus = V17.Gpus;

        public static readonly string Logs = V17.Logs;

        public static readonly string MoneyDonations = V17.MoneyDonations;

        public static readonly string News = V17.News;

        public static readonly string OwnedComputers = V17.OwnedComputers;

        public static readonly string OwnedConsoles = V17.OwnedConsoles;

        public static readonly string Processors = V17.Processors;

        public static readonly string SoundSynths = V17.SoundSynths;

        public static readonly string MachinesForeignKeys = V17.MachinesForeignKeys;

        public static readonly string Iso3166Numeric = V17.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V17.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V17.CompaniesForeignKeys;

        public static readonly string CompanyLogos = V17.CompanyLogos;

        public static readonly string CompanyDescriptions = V17.CompanyDescriptions;

        public static readonly string InstructionSets = V17.InstructionSets;

        public static readonly string InstructionSetExtensions = V17.InstructionSetExtensions;

        public static readonly string InstructionSetExtensionsByProcessor = V17.InstructionSetExtensionsByProcessor;

        public static readonly string ProcessorsByMachine = V17.ProcessorsByMachine;

        public static readonly string GpusByMachine = V17.GpusByMachine;

        public static readonly string SoundByMachine = V17.SoundByMachine;

        public static readonly string MemoryByMachine = "CREATE TABLE `memory_by_machine` (\n"               +
                                                        "`machine` INT NOT NULL,\n"                          +
                                                        "`type` INT NOT NULL DEFAULT 0,\n"                   +
                                                        "`usage` INT NOT NULL DEFAULT 0,\n"                  +
                                                        "`size` BIGINT DEFAULT NULL,\n"                      +
                                                        "`speed` DOUBLE DEFAULT NULL,\n"                     +
                                                        "KEY `idx_memory_by_machine_machine` (`machine`),\n" +
                                                        "KEY `idx_memory_by_machine_type` (`type`),\n"       +
                                                        "KEY `idx_memory_by_machine_usage` (`usage`),\n"     +
                                                        "KEY `idx_memory_by_machine_size` (`size`),\n"       +
                                                        "KEY `idx_memory_by_machine_speed` (`speed`),\n"     +
                                                        "CONSTRAINT `fk_memory_by_machine_machine` FOREIGN KEY (`machine`) REFERENCES `machines` (`id`) ON UPDATE CASCADE ON DELETE CASCADE);";
    }
}