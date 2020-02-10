/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : V10.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SQL queries to create the database version 10.
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
    public static class V10
    {
        public static readonly string Admins = V9.Admins;

        public static readonly string BrowserTests = V9.BrowserTests;

        public static readonly string MarechaiDb = "CREATE TABLE `marechai_db` (\n"                  +
                                                   "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                                   "`version` int(11) NOT NULL,\n"                   +
                                                   "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                                   "PRIMARY KEY (`id`)\n"                            + ");\n" +
                                                   "INSERT INTO marechai_db (version) VALUES ('10');";

        public static readonly string Companies = V9.Companies;

        public static readonly string Computers = V9.Computers;

        public static readonly string Consoles = V9.Consoles;

        public static readonly string DiskFormats = V9.DiskFormats;

        public static readonly string Forbidden = V9.Forbidden;

        public static readonly string Gpus = V9.Gpus;

        public static readonly string Logs = V9.Logs;

        public static readonly string MoneyDonations = V9.MoneyDonations;

        public static readonly string MusicSynths = V9.MusicSynths;

        public static readonly string News = V9.News;

        public static readonly string OwnedComputers = V9.OwnedComputers;

        public static readonly string OwnedConsoles = V9.OwnedConsoles;

        public static readonly string Processors =
            "CREATE TABLE `processors` (\n"                                                                                 +
            "`id` int(11) NOT NULL AUTO_INCREMENT,\n"                                                                       +
            "`name` char(50) NOT NULL DEFAULT '',\n"                                                                        +
            "`company` int(11) DEFAULT NULL,\n"                                                                             +
            "`model_code` varchar(45) DEFAULT NULL,\n"                                                                      +
            "`introduced` datetime DEFAULT NULL,\n"                                                                         +
            "`instruction_set` int(11) DEFAULT NULL,\n"                                                                     +
            "`speed` double DEFAULT NULL,\n"                                                                                +
            "`package` varchar(45) DEFAULT NULL,\n"                                                                         +
            "`GPRs` int(11) DEFAULT NULL,\n"                                                                                +
            "`GPR_size` int(11) DEFAULT NULL,\n"                                                                            +
            "`FPRs` int(11) DEFAULT NULL,\n"                                                                                +
            "`FPR_size` int(11) DEFAULT NULL,\n"                                                                            +
            "`cores` int(11) DEFAULT NULL,\n"                                                                               +
            "`threads_per_core` int(11) DEFAULT NULL,\n"                                                                    +
            "`process` varchar(45) DEFAULT NULL,\n"                                                                         +
            "`process_nm` float DEFAULT NULL,\n"                                                                            +
            "`die_size` float DEFAULT NULL,\n"                                                                              +
            "`transistors` bigint(20) DEFAULT NULL,\n"                                                                      +
            "`data_bus` int(11) DEFAULT NULL,\n"                                                                            +
            "`addr_bus` int(11) DEFAULT NULL,\n"                                                                            +
            "`SIMD_registers` int(11) DEFAULT NULL,\n"                                                                      +
            "`SIMD_size` int(11) DEFAULT NULL,\n"                                                                           +
            "`L1_instruction` float DEFAULT NULL,\n"                                                                        +
            "`L1_data` float DEFAULT NULL,\n"                                                                               +
            "`L2` float DEFAULT NULL,\n"                                                                                    +
            "`L3` float DEFAULT NULL,\n"                                                                                    +
            "PRIMARY KEY (`id`),\n"                                                                                         +
            "KEY `idx_processors_name` (`name`),\n"                                                                         +
            "KEY `idx_processors_company` (`company`),\n"                                                                   +
            "KEY `idx_processors_model_code` (`model_code`),\n"                                                             +
            "KEY `idx_processors_introduced` (`introduced`),\n"                                                             +
            "KEY `idx_processors_instruction_set` (`instruction_set`),\n"                                                   +
            "KEY `idx_processors_speed` (`speed`),\n"                                                                       +
            "KEY `idx_processors_package` (`package`),\n"                                                                   +
            "KEY `idx_processors_GPRs` (`GPRs`),\n"                                                                         +
            "KEY `idx_processors_GPR_size` (`GPR_size`),\n"                                                                 +
            "KEY `idx_processors_FPRs` (`FPRs`),\n"                                                                         +
            "KEY `idx_processors_FPR_size` (`FPR_size`),\n"                                                                 +
            "KEY `idx_processors_cores` (`cores`),\n"                                                                       +
            "KEY `idx_processors_threads_per_core` (`threads_per_core`),\n"                                                 +
            "KEY `idx_processors_process` (`process`),\n"                                                                   +
            "KEY `idx_processors_process_nm` (`process_nm`),\n"                                                             +
            "KEY `idx_processors_die_size` (`die_size`),\n"                                                                 +
            "KEY `idx_processors_transistors` (`transistors`),\n"                                                           +
            "KEY `idx_processors_data_bus` (`data_bus`),\n"                                                                 +
            "KEY `idx_processors_addr_bus` (`addr_bus`),\n"                                                                 +
            "KEY `idx_processors_SIMD_registers` (`SIMD_registers`),\n"                                                     +
            "KEY `idx_processors_SIMD_size` (`SIMD_size`),\n"                                                               +
            "KEY `idx_processors_L1_instruction` (`L1_instruction`),\n"                                                     +
            "KEY `idx_processors_L1_data` (`L1_data`),\n"                                                                   +
            "KEY `idx_processors_L2` (`L2`),\n"                                                                             +
            "KEY `idx_processors_L3` (`L3`),\n"                                                                             +
            "CONSTRAINT `fk_processors_company` FOREIGN KEY (`company`) REFERENCES `companies` (`id`) ON UPDATE CASCADE,\n" +
            "CONSTRAINT `fk_processors_instruction_set` FOREIGN KEY (`instruction_set`) REFERENCES `instruction_sets` (`id`) ON UPDATE CASCADE);";

        public static readonly string SoundSynths = V9.SoundSynths;

        public static readonly string ComputersForeignKeys = V9.ComputersForeignKeys;

        public static readonly string ConsolesForeignKeys = V9.ConsolesForeignKeys;

        public static readonly string Iso3166Numeric = V9.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V9.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V9.CompaniesForeignKeys;

        public static readonly string CompanyLogos = V9.CompanyLogos;

        public static readonly string CompanyDescriptions = V9.CompanyDescriptions;

        public static readonly string InstructionSets = "CREATE TABLE IF NOT EXISTS `instruction_sets` (\n" +
                                                        "`id` INT NOT NULL AUTO_INCREMENT,\n"               +
                                                        "`instruction_set` VARCHAR(45) NOT NULL,\n"         +
                                                        "PRIMARY KEY (`id`));";

        public static readonly string InstructionSetExtensions =
            "CREATE TABLE IF NOT EXISTS `instruction_set_extensions` (\n" + "`id` INT NOT NULL AUTO_INCREMENT,\n" +
            "`extension` VARCHAR(45) NOT NULL,\n"                         + "PRIMARY KEY (`id`));";

        public static readonly string InstructionSetExtensionsByProcessor =
            "CREATE TABLE IF NOT EXISTS `instruction_set_extensions_by_processor` (\n" +
            "`id` INT           NOT NULL AUTO_INCREMENT,\n"                            +
            "`processor_id` INT NOT NULL,\n"                                           +
            "`extension_id` INT     NOT NULL,\n"                                       +
            "PRIMARY                KEY (`id`, `processor_id`, `extension_id`),\n"     +
            "INDEX `idx_setextension_processor` (`processor_id` ASC),\n"               +
            "INDEX `idx_setextension_extension` (`extension_id` ASC),\n"               +
            "CONSTRAINT `fk_extension_processor_id`\n"                                 +
            "FOREIGN KEY (`processor_id`)\n"                                           +
            "REFERENCES `processors` (`id`)\n"                                         + "ON DELETE RESTRICT\n" +
            "ON UPDATE CASCADE,\n"                                                     +
            "CONSTRAINT `fk_extension_extension_id`\n"                                 + "FOREIGN KEY (`extension_id`)\n" +
            "REFERENCES `instruction_set_extensions` (`id`)\n"                         +
            "ON DELETE RESTRICT\n"                                                     + "ON UPDATE CASCADE);";
    }
}