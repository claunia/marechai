/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : V15.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SQL queries to create the database version 15.
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
    public static class V15
    {
        public static readonly string Admins = V14.Admins;

        public static readonly string BrowserTests = V14.BrowserTests;

        public static readonly string MarechaiDb = "CREATE TABLE `marechai_db` (\n"                      +
                                               "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                               "`version` int(11) NOT NULL,\n"                   +
                                               "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                               "PRIMARY KEY (`id`)\n"                            + ");\n" +
                                               "INSERT INTO marechai_db (version) VALUES ('15');";

        public static readonly string Companies = V14.Companies;

        public static readonly string Machines = "CREATE TABLE `machines` (;\n"                       +
                                                 "`id` int(11) NOT NULL AUTO_INCREMENT,;\n"           +
                                                 "`company` int(11) NOT NULL DEFAULT '0',;\n"         +
                                                 "`year` int(11) NOT NULL DEFAULT '0',;\n"            +
                                                 "`model` char(50) NOT NULL DEFAULT '',;\n"           +
                                                 "`ram` int(11) NOT NULL DEFAULT '0',;\n"             +
                                                 "`rom` int(11) NOT NULL DEFAULT '0',;\n"             +
                                                 "`gpu` int(11) DEFAULT NULL,;\n"                     +
                                                 "`vram` int(11) NOT NULL DEFAULT '0',;\n"            +
                                                 "`colors` int(11) NOT NULL DEFAULT '0',;\n"          +
                                                 "`res` char(10) NOT NULL DEFAULT '',;\n"             +
                                                 "`sound_synth` int(11) NOT NULL DEFAULT '0',;\n"     +
                                                 "`music_synth` int(11) NOT NULL DEFAULT '0',;\n"     +
                                                 "`sound_channels` int(11) NOT NULL DEFAULT '0',;\n"  +
                                                 "`music_channels` int(11) NOT NULL DEFAULT '0',;\n"  +
                                                 "`hdd1` int(11) NOT NULL DEFAULT '0',;\n"            +
                                                 "`hdd2` int(11) DEFAULT NULL,;\n"                    +
                                                 "`hdd3` int(11) DEFAULT NULL,;\n"                    +
                                                 "`disk1` int(11) NOT NULL DEFAULT '0',;\n"           +
                                                 "`cap1` char(25) NOT NULL DEFAULT '0',;\n"           +
                                                 "`disk2` int(11) DEFAULT NULL,;\n"                   +
                                                 "`cap2` char(25) DEFAULT NULL,;\n"                   +
                                                 "`type` int(11) NOT NULL DEFAULT '0',;\n"            +
                                                 "PRIMARY KEY (`id`),;\n"                             +
                                                 "KEY `idx_machines_company` (`company`),;\n"         +
                                                 "KEY `idx_machines_year` (`year`),;\n"               +
                                                 "KEY `idx_machines_model` (`model`),;\n"             +
                                                 "KEY `idx_machines_ram` (`ram`),;\n"                 +
                                                 "KEY `idx_machines_rom` (`rom`),;\n"                 +
                                                 "KEY `idx_machines_gpu` (`gpu`),;\n"                 +
                                                 "KEY `idx_machines_vram` (`vram`),;\n"               +
                                                 "KEY `idx_machines_colors` (`colors`),;\n"           +
                                                 "KEY `idx_machines_res` (`res`),;\n"                 +
                                                 "KEY `idx_machines_sound_synth` (`sound_synth`),;\n" +
                                                 "KEY `idx_machines_music_synth` (`music_synth`),;\n" +
                                                 "KEY `idx_machines_hdd1` (`hdd1`),;\n"               +
                                                 "KEY `idx_machines_hdd2` (`hdd2`),;\n"               +
                                                 "KEY `idx_machines_hdd3` (`hdd3`),;\n"               +
                                                 "KEY `idx_machines_disk1` (`disk1`),;\n"             +
                                                 "KEY `idx_machines_disk2` (`disk2`),;\n"             +
                                                 "KEY `idx_machines_cap1` (`cap1`),;\n"               +
                                                 "KEY `idx_machines_cap2` (`cap2`),;\n"               +
                                                 "KEY `idx_machines_type` (`type`));";

        public static readonly string DiskFormats = V14.DiskFormats;

        public static readonly string Forbidden = V14.Forbidden;

        public static readonly string Gpus = V14.Gpus;

        public static readonly string Logs = V14.Logs;

        public static readonly string MoneyDonations = V14.MoneyDonations;

        public static readonly string News = V14.News;

        public static readonly string OwnedComputers = V14.OwnedComputers;

        public static readonly string OwnedConsoles = V14.OwnedConsoles;

        public static readonly string Processors = V14.Processors;

        public static readonly string SoundSynths = V14.SoundSynths;

        public static readonly string MachinesForeignKeys =
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_company` (company) REFERENCES `companies` (`id`) ON UPDATE CASCADE;\n"            +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_disk1` (disk1) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"             +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_disk2` (disk2) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"             +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_gpu` (gpu) REFERENCES `gpus` (`id`) ON UPDATE CASCADE;\n"                         +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_hdd1` (hdd1) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"               +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_hdd2` (hdd2) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"               +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_hdd3` (hdd3) REFERENCES `disk_formats` (`id`) ON UPDATE CASCADE;\n"               +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_music_synth` (music_synth) REFERENCES `sound_synths` (`id`) ON UPDATE CASCADE;\n" +
            "ALTER TABLE `machines` ADD FOREIGN KEY `fk_machines_sound_synth` (sound_synth) REFERENCES `sound_synths` (`id`) ON UPDATE CASCADE;";

        public static readonly string Iso3166Numeric = V14.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V14.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V14.CompaniesForeignKeys;

        public static readonly string CompanyLogos = V14.CompanyLogos;

        public static readonly string CompanyDescriptions = V14.CompanyDescriptions;

        public static readonly string InstructionSets = V14.InstructionSets;

        public static readonly string InstructionSetExtensions = V14.InstructionSetExtensions;

        public static readonly string InstructionSetExtensionsByProcessor = V14.InstructionSetExtensionsByProcessor;

        public static readonly string ProcessorsByMachine =
            "CREATE TABLE `processors_by_machine` (\n"                                                                                                  +
            "`processor` INT NOT NULL, \n"                                                                                                              +
            "`machine` INT NOT NULL,\n"                                                                                                                 +
            "`speed` FLOAT DEFAULT NULL, \n"                                                                                                            +
            "KEY `idx_processors_by_machine_processor` (`processor`),\n"                                                                                +
            "KEY `idx_processors_by_machine_machine` (`machine`),\n"                                                                                    +
            "KEY `idx_processors_by_machine_speed` (`speed`),\n"                                                                                        +
            "CONSTRAINT `fk_processors_by_machine_machine` FOREIGN KEY (`machine`) REFERENCES `machines` (`id`) ON UPDATE CASCADE ON DELETE CASCADE,\n" +
            "CONSTRAINT `fk_processors_by_machine_processor` FOREIGN KEY (`processor`) REFERENCES `processors` (`id`) ON UPDATE CASCADE ON DELETE CASCADE);";
    }
}