/******************************************************************************
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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

namespace Marechai.Database.Schemas.Sql
{
    public static class V20
    {
        public static readonly string Admins = V19.Admins;

        public static readonly string BrowserTests = V19.BrowserTests;

        public static readonly string MarechaiDb = "CREATE TABLE `marechai_db` (\n"                  +
                                                   "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                                   "`version` int(11) NOT NULL,\n"                   +
                                                   "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                                   "PRIMARY KEY (`id`));\n"                          +
                                                   "INSERT INTO marechai_db (version) VALUES ('20');";

        public static readonly string Companies = V19.Companies;

        public static readonly string Machines = "CREATE TABLE `machines` (;\n" +
                                                 "`id` int(11) NOT NULL AUTO_INCREMENT,;\n" +
                                                 "`company` int(11) NOT NULL DEFAULT '0',;\n" +
                                                 "`year` int(11) NOT NULL DEFAULT '0',;\n" +
                                                 "`model` char(50) NOT NULL DEFAULT '',;\n" +
                                                 "`type` int(11) NOT NULL DEFAULT '0',;\n" + "PRIMARY KEY (`id`),;\n" +
                                                 "KEY `idx_machines_company` (`company`),;\n" +
                                                 "KEY `idx_machines_year` (`year`),;\n" +
                                                 "KEY `idx_machines_model` (`model`),;\n" +
                                                 "KEY `idx_machines_type` (`type`));";

        public static readonly string Forbidden = V19.Forbidden;

        public static readonly string Gpus = V19.Gpus;

        public static readonly string Logs = V19.Logs;

        public static readonly string MoneyDonations = V19.MoneyDonations;

        public static readonly string News = V19.News;

        public static readonly string OwnedComputers = V19.OwnedComputers;

        public static readonly string OwnedConsoles = V19.OwnedConsoles;

        public static readonly string Processors = V19.Processors;

        public static readonly string SoundSynths = V19.SoundSynths;

        public static readonly string MachinesForeignKeys =
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_company` (company) REFERENCES `companies` (`id`) ON UPDATE CASCADE;";

        public static readonly string Iso3166Numeric = V19.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V19.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V19.CompaniesForeignKeys;

        public static readonly string CompanyLogos = V19.CompanyLogos;

        public static readonly string CompanyDescriptions = V19.CompanyDescriptions;

        public static readonly string InstructionSets = V19.InstructionSets;

        public static readonly string InstructionSetExtensions = V19.InstructionSetExtensions;

        public static readonly string InstructionSetExtensionsByProcessor = V19.InstructionSetExtensionsByProcessor;

        public static readonly string ProcessorsByMachine = V19.ProcessorsByMachine;

        public static readonly string GpusByMachine = V19.GpusByMachine;

        public static readonly string SoundByMachine = V19.SoundByMachine;

        public static readonly string MemoryByMachine = V19.MemoryByMachine;

        public static readonly string Resolutions = V19.Resolutions;

        public static readonly string ResolutionsByGpu = V19.ResolutionsByGpu;

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