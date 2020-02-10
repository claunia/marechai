/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : V17.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SQL queries to create the database version 17.
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
    public static class V17
    {
        public static readonly string Admins = V16.Admins;

        public static readonly string BrowserTests = V16.BrowserTests;

        public static readonly string CicmDb = "CREATE TABLE `cicm_db` (\n"                      +
                                               "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                               "`version` int(11) NOT NULL,\n"                   +
                                               "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                               "PRIMARY KEY (`id`));\n"                          +
                                               "INSERT INTO cicm_db (version) VALUES ('17');";

        public static readonly string Companies = V16.Companies;

        public static readonly string Machines = "CREATE TABLE `machines` (;\n"               +
                                                 "`id` int(11) NOT NULL AUTO_INCREMENT,;\n"   +
                                                 "`company` int(11) NOT NULL DEFAULT '0',;\n" +
                                                 "`year` int(11) NOT NULL DEFAULT '0',;\n"    +
                                                 "`model` char(50) NOT NULL DEFAULT '',;\n"   +
                                                 "`ram` int(11) NOT NULL DEFAULT '0',;\n"     +
                                                 "`rom` int(11) NOT NULL DEFAULT '0',;\n"     +
                                                 "`vram` int(11) NOT NULL DEFAULT '0',;\n"    +
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
                                                 "KEY `idx_machines_ram` (`ram`),;\n"         +
                                                 "KEY `idx_machines_rom` (`rom`),;\n"         +
                                                 "KEY `idx_machines_vram` (`vram`),;\n"       +
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

        public static readonly string DiskFormats = V16.DiskFormats;

        public static readonly string Forbidden = V16.Forbidden;

        public static readonly string Gpus = V16.Gpus;

        public static readonly string Logs = V16.Logs;

        public static readonly string MoneyDonations = V16.MoneyDonations;

        public static readonly string News = V16.News;

        public static readonly string OwnedComputers = V16.OwnedComputers;

        public static readonly string OwnedConsoles = V16.OwnedConsoles;

        public static readonly string Processors = V16.Processors;

        public static readonly string SoundSynths = V16.SoundSynths;

        public static readonly string MachinesForeignKeys =
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_company` (company) REFERENCES `companies` (`id`) ON UPDATE CASCADE;\n" +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_disk1` (disk1) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"  +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_disk2` (disk2) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"  +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_hdd1` (hdd1) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"    +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_hdd2` (hdd2) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"    +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_hdd3` (hdd3) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;";

        public static readonly string Iso3166Numeric = V16.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V16.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V16.CompaniesForeignKeys;

        public static readonly string CompanyLogos = V16.CompanyLogos;

        public static readonly string CompanyDescriptions = V16.CompanyDescriptions;

        public static readonly string InstructionSets = V16.InstructionSets;

        public static readonly string InstructionSetExtensions = V16.InstructionSetExtensions;

        public static readonly string InstructionSetExtensionsByProcessor = V16.InstructionSetExtensionsByProcessor;

        public static readonly string ProcessorsByMachine = V16.ProcessorsByMachine;

        public static readonly string GpusByMachine = V16.GpusByMachine;

        public static readonly string SoundByMachine =
            "CREATE TABLE `sound_by_machine` (\n"                                                                                                  +
            "`sound_synth` INT NOT NULL, \n"                                                                                                       +
            "`machine` INT NOT NULL,\n"                                                                                                            +
            "KEY `idx_sound_by_machine_sound_synth` (`sound_synth`),\n"                                                                            +
            "KEY `idx_sound_by_machine_machine` (`machine`),\n"                                                                                    +
            "CONSTRAINT `fk_sound_by_machine_machine` FOREIGN KEY (`machine`) REFERENCES `machines` (`id`) ON UPDATE CASCADE ON DELETE CASCADE,\n" +
            "CONSTRAINT `fk_sound_by_machine_sound_synth` FOREIGN KEY (`sound_synth`) REFERENCES `sound_synths` (`id`) ON UPDATE CASCADE ON DELETE CASCADE);";
    }
}