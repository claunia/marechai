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
    public static class V12
    {
        public static readonly string Admins = V11.Admins;

        public static readonly string BrowserTests = V11.BrowserTests;

        public static readonly string MarechaiDb = "CREATE TABLE `marechai_db` (\n"                  +
                                                   "`id` int(11) NOT NULL AUTO_INCREMENT,\n"         +
                                                   "`version` int(11) NOT NULL,\n"                   +
                                                   "`updated` datetime DEFAULT CURRENT_TIMESTAMP,\n" +
                                                   "PRIMARY KEY (`id`)\n"                            + ");\n" +
                                                   "INSERT INTO marechai_db (version) VALUES ('12');";

        public static readonly string Companies = V11.Companies;

        public static readonly string Computers = "CREATE TABLE `computers` (\n" +
                                                  "`id` int(11) NOT NULL AUTO_INCREMENT,\n" +
                                                  "`company` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`year` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`model` char(50) NOT NULL DEFAULT '',\n" +
                                                  "`cpu1` int(11) DEFAULT NULL,\n" + "`mhz1` int(11) DEFAULT NULL,\n" +
                                                  "`cpu2` int(11) DEFAULT NULL,\n" +
                                                  "`mhz2` decimal(11,2) DEFAULT NULL,\n" +
                                                  "`bits` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`ram` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`rom` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`gpu` int(11) DEFAULT NULL,\n" +
                                                  "`vram` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`colors` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`res` char(10) NOT NULL DEFAULT '',\n" +
                                                  "`sound_synth` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`music_synth` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`sound_channels` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`music_channels` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`hdd1` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`hdd2` int(11) DEFAULT NULL,\n" + "`hdd3` int(11) DEFAULT NULL,\n" +
                                                  "`disk1` int(11) NOT NULL DEFAULT '0',\n" +
                                                  "`cap1` char(25) NOT NULL DEFAULT '0',\n" +
                                                  "`disk2` int(11) DEFAULT NULL,\n" +
                                                  "`cap2` char(25) DEFAULT NULL,\n" + "PRIMARY KEY (`id`),\n" +
                                                  "KEY `idx_computers_company` (`company`),\n" +
                                                  "KEY `idx_computers_year` (`year`),\n" +
                                                  "KEY `idx_computers_model` (`model`),\n" +
                                                  "KEY `idx_computers_cpu1` (`cpu1`),\n" +
                                                  "KEY `idx_computers_cpu2` (`cpu2`),\n" +
                                                  "KEY `idx_computers_mhz1` (`mhz1`),\n" +
                                                  "KEY `idx_computers_mhz2` (`mhz2`),\n" +
                                                  "KEY `idx_computers_bits` (`bits`),\n" +
                                                  "KEY `idx_computers_ram` (`ram`),\n" +
                                                  "KEY `idx_computers_rom` (`rom`),\n" +
                                                  "KEY `idx_computers_gpu` (`gpu`),\n" +
                                                  "KEY `idx_computers_vram` (`vram`),\n" +
                                                  "KEY `idx_computers_colors` (`colors`),\n" +
                                                  "KEY `idx_computers_res` (`res`),\n" +
                                                  "KEY `idx_computers_sound_synth` (`sound_synth`),\n" +
                                                  "KEY `idx_computers_music_synth` (`music_synth`),\n" +
                                                  "KEY `idx_computers_hdd1` (`hdd1`),\n" +
                                                  "KEY `idx_computers_hdd2` (`hdd2`),\n" +
                                                  "KEY `idx_computers_hdd3` (`hdd3`),\n" +
                                                  "KEY `idx_computers_disk1` (`disk1`),\n" +
                                                  "KEY `idx_computers_disk2` (`disk2`),\n" +
                                                  "KEY `idx_computers_cap1` (`cap1`),\n" +
                                                  "KEY `idx_computers_cap2` (`cap2`))";

        public static readonly string Consoles = "CREATE TABLE `consoles` (\n" +
                                                 "`id` int(11) NOT NULL AUTO_INCREMENT,\n" +
                                                 "`company` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`model` char(50) NOT NULL DEFAULT '',\n" +
                                                 "`year` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`cpu1` int(11) DEFAULT NULL,\n" + "`mhz1` int(11) DEFAULT NULL,\n" +
                                                 "`cpu2` int(11) DEFAULT NULL,\n" +
                                                 "`mhz2` decimal(11,2) DEFAULT NULL,\n" +
                                                 "`bits` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`ram` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`rom` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`gpu` int(11) DEFAULT NULL,\n" +
                                                 "`vram` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`res` char(11) NOT NULL DEFAULT '',\n" +
                                                 "`colors` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`palette` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`sound_synth` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`schannels` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`music_synth` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`mchannels` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`format` int(11) NOT NULL DEFAULT '0',\n" +
                                                 "`cap` int(11) NOT NULL DEFAULT '0',\n" + "PRIMARY KEY (`id`),\n" +
                                                 "KEY `idx_consoles_company` (`company`),\n" +
                                                 "KEY `idx_consoles_year` (`year`),\n" +
                                                 "KEY `idx_consoles_model` (`model`),\n" +
                                                 "KEY `idx_consoles_cpu1` (`cpu1`),\n" +
                                                 "KEY `idx_consoles_cpu2` (`cpu2`),\n" +
                                                 "KEY `idx_consoles_mhz1` (`mhz1`),\n" +
                                                 "KEY `idx_consoles_mhz2` (`mhz2`),\n" +
                                                 "KEY `idx_consoles_bits` (`bits`),\n" +
                                                 "KEY `idx_consoles_ram` (`ram`),\n" +
                                                 "KEY `idx_consoles_rom` (`rom`),\n" +
                                                 "KEY `idx_consoles_gpu` (`gpu`),\n" +
                                                 "KEY `idx_consoles_vram` (`vram`),\n" +
                                                 "KEY `idx_consoles_colors` (`colors`),\n" +
                                                 "KEY `idx_consoles_res` (`res`),\n" +
                                                 "KEY `idx_consoles_sound_synth` (`sound_synth`),\n" +
                                                 "KEY `idx_consoles_music_synth` (`music_synth`),\n" +
                                                 "KEY `idx_consoles_palette` (`palette`),\n" +
                                                 "KEY `idx_consoles_format` (`format`),\n" +
                                                 "KEY `idx_consoles_cap` (`cap`));";

        public static readonly string DiskFormats = V11.DiskFormats;

        public static readonly string Forbidden = V11.Forbidden;

        public static readonly string Gpus = V11.Gpus + "\n" +
                                             $"INSERT INTO `gpus` (`id`, `name`) VALUES ({Operations.DB_NONE}, `DB_NONE`);\n" +
                                             $"INSERT INTO `gpus` (`id`, `name`) VALUES ({Operations.DB_SOFTWARE}, `DB_FRAMEBUFFER`);";

        public static readonly string Logs = V11.Logs;

        public static readonly string MoneyDonations = V11.MoneyDonations;

        public static readonly string MusicSynths = V11.MusicSynths;

        public static readonly string News = V11.News;

        public static readonly string OwnedComputers = V11.OwnedComputers;

        public static readonly string OwnedConsoles = V11.OwnedConsoles;

        public static readonly string Processors = V11.Processors;

        public static readonly string SoundSynths = V11.SoundSynths;

        public static readonly string ComputersForeignKeys = V11.ComputersForeignKeys;

        public static readonly string ConsolesForeignKeys = V11.ConsolesForeignKeys;

        public static readonly string Iso3166Numeric = V11.Iso3166Numeric;

        public static readonly string Iso3166NumericValues = V11.Iso3166NumericValues;

        public static readonly string CompaniesForeignKeys = V11.CompaniesForeignKeys;

        public static readonly string CompanyLogos = V11.CompanyLogos;

        public static readonly string CompanyDescriptions = V11.CompanyDescriptions;

        public static readonly string InstructionSets = V11.InstructionSets;

        public static readonly string InstructionSetExtensions = V11.InstructionSetExtensions;

        public static readonly string InstructionSetExtensionsByProcessor = V11.InstructionSetExtensionsByProcessor;
    }
}