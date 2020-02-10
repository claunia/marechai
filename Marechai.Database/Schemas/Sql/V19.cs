/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : V19.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SQL queries to create the database version 19.
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
    public static class V19
    {
        public static readonly string Admins = V18.Admins;

        public static readonly string BrowserTests = V18.BrowserTests;

        public static readonly string MarechaiDb = "CREATE TABLE `marechai_db` (\n"                      +
                                               "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                               "`version` int(11) NOT NULL,\n"                   +
                                               "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                               "PRIMARY KEY (`id`));\n"                          +
                                               "INSERT INTO marechai_db (version) VALUES ('19');";

        public static readonly string Companies = V18.Companies;

        public static readonly string Machines = "CREATE TABLE `machines` (;\n"               +
                                                 "`id` int(11) NOT NULL AUTO_INCREMENT,;\n"   +
                                                 "`company` int(11) NOT NULL DEFAULT '0',;\n" +
                                                 "`year` int(11) NOT NULL DEFAULT '0',;\n"    +
                                                 "`model` char(50) NOT NULL DEFAULT '',;\n"   +
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
                                                 "KEY `idx_machines_hdd1` (`hdd1`),;\n"       +
                                                 "KEY `idx_machines_hdd2` (`hdd2`),;\n"       +
                                                 "KEY `idx_machines_hdd3` (`hdd3`),;\n"       +
                                                 "KEY `idx_machines_disk1` (`disk1`),;\n"     +
                                                 "KEY `idx_machines_disk2` (`disk2`),;\n"     +
                                                 "KEY `idx_machines_cap1` (`cap1`),;\n"       +
                                                 "KEY `idx_machines_cap2` (`cap2`),;\n"       +
                                                 "KEY `idx_machines_type` (`type`));";

        public static readonly string DiskFormats = V18.DiskFormats;

        public static readonly string Forbidden = V18.Forbidden;

        public static readonly string Gpus = V18.Gpus;

        public static readonly string Logs = V18.Logs;

        public static readonly string MoneyDonations = V18.MoneyDonations;

        public static readonly string News = V18.News;

        public static readonly string OwnedComputers = V18.OwnedComputers;

        public static readonly string OwnedConsoles = V18.OwnedConsoles;

        public static readonly string Processors = V18.Processors;

        public static readonly string SoundSynths = V18.SoundSynths;

        public static readonly string MachinesForeignKeys = V18.MachinesForeignKeys;

        public static readonly string Iso3166Numeric = V18.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V18.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V18.CompaniesForeignKeys;

        public static readonly string CompanyLogos = V18.CompanyLogos;

        public static readonly string CompanyDescriptions = V18.CompanyDescriptions;

        public static readonly string InstructionSets = V18.InstructionSets;

        public static readonly string InstructionSetExtensions = V18.InstructionSetExtensions;

        public static readonly string InstructionSetExtensionsByProcessor = V18.InstructionSetExtensionsByProcessor;

        public static readonly string ProcessorsByMachine = V18.ProcessorsByMachine;

        public static readonly string GpusByMachine = V18.GpusByMachine;

        public static readonly string SoundByMachine = V18.SoundByMachine;

        public static readonly string MemoryByMachine = V18.MemoryByMachine;

        public static readonly string Resolutions =
            "CREATE TABLE `resolutions` (\n"                                               +
            "`id` INT NOT NULL AUTO_INCREMENT,\n"                                          +
            "`width` INT NOT NULL DEFAULT 0,\n"                                            +
            "`height` INT NOT NULL DEFAULT 0,\n"                                           +
            "`colors` BIGINT DEFAULT NULL,\n"                                              +
            "`palette` BIGINT DEFAULT NULL,\n"                                             +
            "`chars` BOOL NOT NULL DEFAULT 0,\n"                                           + "PRIMARY KEY (`id`),\n" +
            "KEY `idx_resolutions_width` (`width`),\n"                                     +
            "KEY `idx_resolutions_height` (`height`),\n"                                   +
            "KEY `idx_resolutions_colors` (`colors`),\n"                                   +
            "KEY `idx_resolutions_palette` (`palette`),\n"                                 +
            "INDEX `idx_resolutions_resolution` (`width`,`height`),\n"                     +
            "INDEX `idx_resolutions_resolution_with_color` (`width`,`height`,`colors`),\n" +
            "INDEX `idx_resolutions_resolution_with_color_and_palette` (`width`,`height`,`colors`,`palette`));";

        public static readonly string ResolutionsByGpu =
            "CREATE TABLE `resolutions_by_gpu` (\n"                                                                                      +
            "`gpu` INT NOT NULL,\n"                                                                                                      +
            "`resolution` INT NOT NULL,\n"                                                                                               +
            "KEY `idx_resolutions_by_gpu_gpu` (`gpu`),\n"                                                                                +
            "KEY `idx_resolutions_by_gpu_resolution` (`resolution`),\n"                                                                  +
            "CONSTRAINT `fk_resolutions_by_gpu_gpu` FOREIGN KEY (`gpu`) REFERENCES `gpus` (`id`) ON UPDATE CASCADE ON DELETE CASCADE,\n" +
            "CONSTRAINT `fk_resolutions_by_gpu_resolution` FOREIGN KEY (`resolution`) REFERENCES `resolutions` (`id`) ON UPDATE CASCADE ON DELETE CASCADE);";
    }
}